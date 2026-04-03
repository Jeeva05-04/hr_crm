using hr_crm.Service;
using hr_crm.Service.Interface;

namespace hr_crm.BackgroundServices
{
    public class PayrollAutoGenerationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PayrollAutoGenerationService> _logger;

        public PayrollAutoGenerationService(
            IServiceScopeFactory scopeFactory,
            ILogger<PayrollAutoGenerationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payroll Auto-Generation Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                // Run on the 1st of every month at 00:05 UTC
                if (now.Day == 1 && now.Hour == 0 && now.Minute >= 5 && now.Minute < 10)
                {
                    await RunPayrollGeneration();
                }

                // Check every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task RunPayrollGeneration()
        {
            _logger.LogInformation("Auto payroll generation started at {Time}", DateTime.UtcNow);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var payrollService = scope.ServiceProvider.GetRequiredService<IPayrollService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                var loggingService = scope.ServiceProvider.GetService<hr_crm.Service.LoggingService>();

                // Log start of auto-generation
                if (loggingService != null)
                {
                    await loggingService.CreateLog(null, "System", "AutoPayroll/Start", "Auto payroll generation started.");
                }
                var (generated, skipped) = await payrollService.AutoGeneratePayrollForAllAsync();

                _logger.LogInformation(
                    "Auto payroll done. Generated: {Generated}, Skipped: {Skipped}",
                    generated, skipped);
                if (loggingService != null)
                {
                    await loggingService.CreateLog(null, "System", "AutoPayroll/Complete",
                        $"Auto payroll completed. Generated: {generated}, Skipped: {skipped}");
                }

                // Notify all employees whose payroll was generated
                if (generated > 0)
                {
                    var allConfigs = await payrollService.GetAllSalaryConfigsAsync();
                    var monthLabel = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                                        .ToString("MMMM yyyy");

                    foreach (var config in allConfigs)
                    {
                        await notificationService.CreateNotification(
                            config.UserId,
                            "Payslip Generated",
                            $"Your payslip for {monthLabel} has been generated. Please check your payroll section.",
                            "Payroll",
                            0
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during auto payroll generation");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var loggingService = scope.ServiceProvider.GetService<hr_crm.Service.LoggingService>();
                    if (loggingService != null)
                    {
                        await loggingService.CreateLog(null, "System", "AutoPayroll/Error", ex.Message);
                    }
                }
                catch { }
            }
        }
    }
}
