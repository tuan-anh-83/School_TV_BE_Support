using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            var expiredOrders = await orderService.GetPendingOrdersOlderThanAsync(TimeSpan.FromMinutes(5));

            foreach (var order in expiredOrders)
            {
                try
                {
                    var httpClient = httpClientFactory.CreateClient();

                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://api-merchant.payos.vn/v2/payment-requests/{order.OrderCode}");
                    request.Headers.Add("x-client-id", "c8f279c5-3703-4413-b5b8-1f856e7066f5");
                    request.Headers.Add("x-api-key", "6b8a5e5e-8e32-4b0b-b9a5-a3075d7075f8");

                    var response = await httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"❌ Failed to fetch PayOS status for Order {order.OrderID}. HTTP {(int)response.StatusCode}");
                        order.Status = "Failed";
                    }
                    else
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonDocument.Parse(json);
                        var status = result.RootElement
                            .GetProperty("data")
                            .GetProperty("status")
                            .GetString();

                        if (status == "PAID")
                        {
                            _logger.LogInformation($"✅ Order {order.OrderID} was paid on PayOS. Marking as 'Completed'.");
                            order.Status = "Completed";
                        }
                        else
                        {
                            _logger.LogWarning($"⚠️ Order {order.OrderID} was not paid (status: {status}). Marking as 'Failed'.");
                            order.Status = "Failed";
                        }
                    }

                    await orderService.UpdateOrderAsync(order);
                    _logger.LogWarning($"✅ Updated status for order: {order.OrderID}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Error while checking PayOS for Order {order.OrderID}");
                }
            }
        }
    }
}
