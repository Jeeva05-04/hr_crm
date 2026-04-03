using hr_crm.Data;
using hr_crm.Entities;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Service
{
    public class LoggingService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly int _retentionDays;

        public LoggingService(IDbContextFactory<AppDbContext> dbFactory, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _dbFactory = dbFactory;
            // read retention from configuration if present, default to 7 days
            var configured = configuration["LoggingRetentionDays"];
            if (!int.TryParse(configured, out _retentionDays)) _retentionDays = 7;
        }

        public async Task CreateLog(int? userId, string? userName, string action, string? details,
            int? statusCode = null, int? durationMs = null, string? controllerName = null, string? actionName = null,
            string? userAgent = null, string? correlationId = null)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            // If incoming userName is missing or numeric-only, attempt to resolve a friendly name from other tables
            if (string.IsNullOrWhiteSpace(userName) || userName.All(char.IsDigit))
            {
                if (userId.HasValue)
                {
                    try
                    {
                        // Try to get name from Payroll (most reliable snapshot of employee name)
                        var payroll = await db.Payrolls
                            .Where(p => p.UserId == userId.Value && !string.IsNullOrEmpty(p.UserName))
                            .OrderByDescending(p => p.CreatedDate)
                            .FirstOrDefaultAsync();
                        if (payroll != null)
                            userName = payroll.UserName;
                    }
                    catch { /* non-fatal */ }
                }
            }
            // Append correlation id to details (so it's searchable), then sanitize details and username before storing
            var combinedDetails = details;
            if (!string.IsNullOrEmpty(correlationId))
            {
                combinedDetails = string.IsNullOrEmpty(combinedDetails) ? $"CorrelationId={correlationId}" : combinedDetails + $"; CorrelationId={correlationId}";
            }

            var sanitizedDetails = Sanitize(combinedDetails);
            var sanitizedUserName = SanitizeUserName(userName);

            var entry = new LogEntry
            {
                UserId = userId,
                UserName = sanitizedUserName,
                Action = action,
                Details = sanitizedDetails,
                Timestamp = DateTime.UtcNow,
                StatusCode = statusCode,
                DurationMs = durationMs,
                ControllerName = controllerName,
                ActionName = actionName,
                UserAgent = userAgent
            };

            db.Logs.Add(entry);
            await db.SaveChangesAsync();

            // Cleanup old logs immediately after creating a log entry to ensure retention enforcement
            try
            {
                var cutoff = DateTime.UtcNow.AddDays(-_retentionDays);
                var oldLogs = await db.Logs.Where(l => l.Timestamp < cutoff).ToListAsync();
                if (oldLogs.Any())
                {
                    db.Logs.RemoveRange(oldLogs);
                    await db.SaveChangesAsync();
                }
            }
            catch { /* non-fatal - don't break logging when cleanup fails */ }
        }

        // Basic sanitization to mask sensitive patterns (emails, long digit sequences, common keys)
        private string? Sanitize(string? input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string output = input;

            try
            {
                // Mask emails
                output = System.Text.RegularExpressions.Regex.Replace(output,
                    "([a-zA-Z0-9._%+-]+)@([a-zA-Z0-9.-]+\\.[a-zA-Z]{2,})",
                    "[REDACTED_EMAIL]", System.Text.RegularExpressions.RegexOptions.Compiled);

                // Mask long digit sequences (account numbers, card numbers) of length >= 8
                output = System.Text.RegularExpressions.Regex.Replace(output,
                    "\\b\\d{8,}\\b",
                    m => new string('*', Math.Max(4, m.Value.Length - 4)) + m.Value.Substring(m.Value.Length - 4),
                    System.Text.RegularExpressions.RegexOptions.Compiled);

                // Mask typical sensitive keys
                output = System.Text.RegularExpressions.Regex.Replace(output,
                    "(?i)(password|pwd|ssn|bankaccount|accountnumber)\\s*[:=]\\s*[^;\\n\\r]+",
                    "[REDACTED]",
                    System.Text.RegularExpressions.RegexOptions.Compiled);

                // Trim to a safe maximum length
                if (output.Length > 4000) output = output.Substring(0, 4000) + "...";
            }
            catch
            {
                output = input.Length > 1000 ? input.Substring(0, 1000) + "..." : input;
            }

            return output;
        }

        private string? SanitizeUserName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            var s = name.Trim();
            if (s.Length > 200) s = s.Substring(0, 200);
            return s;
        }

        public async Task<List<LogEntry>> GetAll()
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.Logs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<LogEntry?> GetById(int id)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.Logs.FindAsync(id);
        }
    }
}
