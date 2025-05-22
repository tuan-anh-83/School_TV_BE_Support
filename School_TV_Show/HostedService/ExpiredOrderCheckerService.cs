using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.HostedServices
{
    public class ExpiredOrderCheckerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiredOrderCheckerService> _logger;

        public ExpiredOrderCheckerService(
            IServiceScopeFactory scopeFactory, 
            ILogger<ExpiredOrderCheckerService> logger
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                await MarkExpiredOrdersAsFailedAsync();

                // Delay 10s để lặp lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task MarkExpiredOrdersAsFailedAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepo>();
            var accountPackageRepo = scope.ServiceProvider.GetRequiredService<IAccountPackageRepo>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();

            var expiredOrders = await orderRepo.GetPendingOrdersOlderThanAsync(now.AddMinutes(2.5));

            foreach (var order in expiredOrders)
            {
                try
                {
                    string[] clientIds = {
                        "c8f279c5-3703-4413-b5b8-1f856e7066f5",
                        "dfa1d0da-865f-445a-8717-e4c4f17a169d"
                    };

                    string[] apiKeys = {
                        "6b8a5e5e-8e32-4b0b-b9a5-a3075d7075f8",
                        "7d961009-05bd-4d3b-843d-1f0e81a625b0"
                    };

                    string? status = null;
                    bool success = false;

                    for (int i = 0; i < clientIds.Length; i++)
                    {
                        try
                        {
                            var httpClient = httpClientFactory.CreateClient();
                            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api-merchant.payos.vn/v2/payment-requests/{order.OrderCode}");
                            request.Headers.Add("x-client-id", clientIds[i]);
                            request.Headers.Add("x-api-key", apiKeys[i]);

                            var response = await httpClient.SendAsync(request);

                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogWarning($"⚠️ Channel {i + 1} failed with status code {(int)response.StatusCode}");
                                continue;
                            }

                            var json = await response.Content.ReadAsStringAsync();
                            var result = JsonDocument.Parse(json);

                            if (result.RootElement.TryGetProperty("data", out var dataProp) &&
                                dataProp.TryGetProperty("status", out var statusProp))
                            {
                                status = statusProp.GetString();
                                success = true;
                                break;
                            }

                            _logger.LogWarning($"⚠️ Channel {i + 1} did not return 'data.status'");
                        }
                        catch (Exception exInner)
                        {
                            _logger.LogWarning(exInner, $"⚠️ Exception when calling PayOS channel {i + 1}");
                        }
                    }

                    if (!success || string.IsNullOrEmpty(status))
                    {
                        if ((now - order.CreatedAt).TotalMinutes > 5)
                        {
                            _logger.LogWarning($"⚠️ Could not retrieve payment status for order {order.OrderID}. Marking as 'Failed'.");
                            order.Status = "Failed";
                        }
                        else
                        {
                            _logger.LogInformation($"⏳ Order {order.OrderID} is too recent (less than 5 minutes), skipping failure marking.");
                        }
                    }
                    else if (status == "PAID")
                    {
                        _logger.LogInformation($"✅ Order {order.OrderID} was paid on PayOS. Marking as 'Completed'.");
                        order.Status = "Completed";

                        if (order.OrderDetails.Count() > 0)
                        {
                            var orderDetail = order.OrderDetails.FirstOrDefault();
                            var accountPackage = await accountPackageRepo.GetActiveAccountPackageAsync(order.AccountID);

                            if (accountPackage != null)
                            {
                                accountPackage.TotalMinutesAllowed += orderDetail?.Package.TimeDuration ?? 0;
                                accountPackage.RemainingMinutes += orderDetail?.Package.TimeDuration ?? 0;
                                accountPackage.ExpiredAt = accountPackage.ExpiredAt != null
                                    ? accountPackage.ExpiredAt.Value.AddDays(orderDetail?.Package.Duration ?? 0)
                                    : now.AddDays(orderDetail?.Package.Duration ?? 0);

                                await accountPackageRepo.UpdateAccountPackageAsync(accountPackage);
                            }
                            else if (orderDetail != null)
                            {
                                await accountPackageRepo.CreateAccountPackageAsync(new BOs.Models.AccountPackage
                                {
                                    AccountID = order.AccountID,
                                    PackageID = orderDetail.Package.PackageID,
                                    TotalMinutesAllowed = orderDetail.Package.TimeDuration,
                                    MinutesUsed = 0,
                                    RemainingMinutes = orderDetail.Package.TimeDuration,
                                    StartDate = now,
                                    ExpiredAt = now.AddDays(orderDetail.Package.Duration)
                                });
                            }
                        }
                    }
                    else
                    {
                        if ((now - order.CreatedAt).TotalMinutes > 5)
                        {
                            _logger.LogWarning($"⚠️ Order {order.OrderID} was not paid (status: {status}). Marking as 'Failed'.");
                            order.Status = "Failed";
                        }
                        else
                        {
                            _logger.LogInformation($"⏳ Order {order.OrderID} is too recent (less than 5 minutes), skipping failure marking.");
                        }
                    }

                    await orderRepo.UpdateOrderAsync(order);
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
