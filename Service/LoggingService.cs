using hr_crm.Data;
using hr_crm.Entities;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Service
{
    public class LoggingService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public LoggingService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task CreateLog(int? userId, string? userName, string action, string? details)
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
            var entry = new LogEntry
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            db.Logs.Add(entry);
            await db.SaveChangesAsync();
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
