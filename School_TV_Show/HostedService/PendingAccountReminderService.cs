using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.HostedServices
{
    public class PendingAccountReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PendingAccountReminderService> _logger;
        private readonly TimeSpan _reminderInterval = TimeSpan.FromHours(1);

        public PendingAccountReminderService(
            IServiceProvider serviceProvider,
            ILogger<PendingAccountReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PendingAccountReminderService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendPendingAccountRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in background reminder task.");
                }

                await Task.Delay(_reminderInterval, stoppingToken);
            }

            _logger.LogInformation("PendingAccountReminderService stopped.");
        }

        private async Task SendPendingAccountRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            DateTime threshold = DateTime.UtcNow.AddHours(-1);

            try
            {
                var pendingAccounts = await accountService.GetPendingAccountsAsync(threshold);

                foreach (var account in pendingAccounts)
                {
                    await emailService.SendOtpReminderEmailAsync(account.Email);
                    _logger.LogInformation("Sent OTP reminder to: {Email}", account.Email);
                }

                _logger.LogInformation("Reminder process completed. Total reminders sent: {Count}", pendingAccounts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending OTP reminders.");
            }
        }
    }
}
