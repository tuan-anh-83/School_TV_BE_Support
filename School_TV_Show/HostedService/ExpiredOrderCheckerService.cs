using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.HostedServices
{
    public class ExpiredOrderCheckerService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiredOrderCheckerService> _logger;
        private Timer _timer;

        public ExpiredOrderCheckerService(IServiceScopeFactory scopeFactory, ILogger<ExpiredOrderCheckerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("⏳ Starting Expired Order Checker Service...");
            _timer = new Timer(async _ => await MarkExpiredOrdersAsFailedAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("⏹️ Stopping Expired Order Checker Service...");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async Task MarkExpiredOrdersAsFailedAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            var expiredOrders = await orderService.GetPendingOrdersOlderThanAsync(TimeSpan.FromMinutes(5));

            foreach (var order in expiredOrders)
            {
                _logger.LogWarning($"⚠️ Order {order.OrderID} has exceeded 5 minutes without payment. Marking as 'Failed'.");
                order.Status = "Failed";
                await orderService.UpdateOrderAsync(order);
            }
        }
    }
}
