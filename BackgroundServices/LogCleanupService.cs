using hr_crm.Data;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.BackgroundServices
{
    public class LogCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LogCleanupService> _logger;

        public LogCleanupService(IServiceScopeFactory scopeFactory, ILogger<LogCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Log cleanup service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldLogs(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while cleaning up logs");
                }

                // Run once every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CleanupOldLogs(CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow.AddDays(-7);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var oldLogs = await db.Logs
                .Where(l => l.Timestamp < cutoff)
                .ToListAsync(cancellationToken);

            if (oldLogs.Any())
            {
                _logger.LogInformation("Deleting {Count} log entries older than {Cutoff}", oldLogs.Count, cutoff);
                db.Logs.RemoveRange(oldLogs);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogDebug("No old logs to delete. Cutoff={Cutoff}", cutoff);
            }
        }
    }
}
