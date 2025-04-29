using BOs.Models;
using DAOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IPaymentRepo _paymentRepo;
        private readonly IAccountPackageRepo _accountPackageRepo;
        private readonly IPaymentHistoryService _paymentHistoryService;
        private readonly IPackageService _packageService;
        private readonly string _checksumKey;
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentService> _logger;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public PaymentService(
            IOrderService orderService,
            IOrderDetailService orderDetailService,
            IPaymentRepo paymentRepo,
            IAccountPackageRepo accountPackageRepo,
            IConfiguration configuration,
            IPaymentHistoryService paymentHistoryService,
            IPackageService packageService,
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentService> logger)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _paymentRepo = paymentRepo;
            _accountPackageRepo = accountPackageRepo;
            _paymentHistoryService = paymentHistoryService;
            _packageService = packageService;
            _checksumKey = configuration["Environment:PAYOS_CHECKSUM_KEY"];
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<bool> HandlePaymentWebhookAsync(PayOSWebhookRequest request)
        {
            try
            {
                long orderCode = request.data.orderCode;
                _logger.LogInformation($"🔄 Processing payment webhook for OrderCode {orderCode}");

                var order = await _orderService.GetOrderByOrderCodeAsync(orderCode);
                if (order == null)
                {
                    _logger.LogError($"❌ Order with OrderCode {orderCode} not found.");
                    return false;
                }

                var payment = await _paymentRepo.GetPaymentByOrderIdAsync(order.OrderID);
                if (payment == null)
                {
                    payment = new Payment
                    {
                        OrderID = order.OrderID,
                        Amount = request.data.amount,
                        PaymentDate = DateTime.UtcNow,
                        PaymentMethod = "PayOS",
                        Status = request.data.code == "00" ? "Completed" : "Failed"
                    };

                    var paymentUpdated = await _paymentRepo.UpdatePaymentAsync(payment);

                    if (paymentUpdated != null && paymentUpdated.Status == "Completed")
                    {
                        var package = await _packageService.GetCurrentPackageAndDurationByAccountIdAsync(order.AccountID);

                        if(package.HasValue && package.Value.Item1 != null)
                        {
                            var currentPackage = await _accountPackageRepo.GetActiveAccountPackageAsync(order.AccountID);

                            if (currentPackage != null)
                            {
                                // Update remaining time
                                currentPackage.RemainingHours = package.Value.Item1.Duration;
                                currentPackage.TotalHoursAllowed += package.Value.Item1.Duration;
                                currentPackage.ExpiredAt = currentPackage.ExpiredAt != null ? 
                                    currentPackage.ExpiredAt.Value.AddDays(package.Value.Item1.Duration) :
                                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).AddDays(package.Value.Item1.Duration);

                                await _accountPackageRepo.UpdateAccountPackageAsync(currentPackage);
                            }
                            else
                            {
                                await _accountPackageRepo.CreateAccountPackageAsync(new AccountPackage
                                {
                                    AccountPackageID = 0,
                                    AccountID = order.AccountID,
                                    PackageID = package.Value.Item1.PackageID,
                                    TotalHoursAllowed = package.Value.Item1.Duration,
                                    HoursUsed = 0,
                                    RemainingHours = package.Value.Item1.Duration,
                                    StartDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                                    ExpiredAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).AddDays(package.Value.Item1.Duration)
                                });
                            }
                            
                        }
                    }
                    _logger.LogInformation($"✅ New payment record created for Order {order.OrderID}");
                }
                else
                {
                    if (payment.Status == "Completed" && request.data.code == "00")
                    {
                        _logger.LogWarning($"⚠️ Payment for Order {order.OrderID} is already completed. Skipping update.");
                        return true;
                    }

                    payment.Status = request.data.code == "00" ? "Completed" : "Failed";
                    await _paymentRepo.UpdatePaymentAsync(payment);
                    _logger.LogInformation($"✅ Payment record updated for Order {order.OrderID}");
                }

                await _paymentHistoryService.AddPaymentHistoryAsync(payment);
                _logger.LogInformation($"📜 Payment history recorded for Payment {payment.PaymentID}");

                order.Status = request.data.code == "00" ? "Completed" : "Failed";
                _logger.LogInformation($"✅ Order {order.OrderID} status updated to '{order.Status}'");

                await _orderService.UpdateOrderAsync(order);

                if (order.Status == "Completed")
                {
                    using var scope = _scopeFactory.CreateScope();

                    var orderDetailService = scope.ServiceProvider.GetRequiredService<IOrderDetailService>();
                    var orderDetails = await orderDetailService.GetOrderDetailsByOrderIdAsync(order.OrderID);
                    var firstDetail = orderDetails.FirstOrDefault();
                    if (firstDetail != null && firstDetail.Package != null)
                    {
                        int packageDuration = firstDetail.Package.Duration;

                        var schoolChannelDao = scope.ServiceProvider.GetRequiredService<SchoolChannelDAO>();
                        var schoolChannel = await schoolChannelDao.GetByIdAsync(order.AccountID);
                        if (schoolChannel != null)
                        {
                            schoolChannel.TotalDuration = (schoolChannel.TotalDuration ?? 0) + packageDuration;
                            await schoolChannelDao.UpdateAsync(schoolChannel);
                            _logger.LogInformation($"🕒 Updated SchoolChannel {schoolChannel.SchoolChannelID} TotalDuration to {schoolChannel.TotalDuration}");
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error processing payment webhook: {ex.Message}");
                return false;
            }
        }

        public bool VerifySignature(PayOSWebhookRequest request)
        {
            try
            {
                _logger.LogInformation($"🔍 Verifying Signature for Order {request.data.orderCode}...");

                var sortedData = new SortedDictionary<string, string>();
                var dataProperties = request.data.GetType().GetProperties();
                foreach (var prop in dataProperties)
                {
                    var value = prop.GetValue(request.data, null)?.ToString() ?? "";
                    if (value.StartsWith("[") || value.StartsWith("{"))
                    {
                        value = JsonSerializer.Serialize(value);
                    }
                    sortedData[prop.Name] = value;
                }

                string dataString = string.Join("&", sortedData.Select(kv => $"{kv.Key}={kv.Value}"));
                _logger.LogInformation($"📝 Data String for Signature: {dataString}");

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_checksumKey.Trim()));
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
                string computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

                string receivedSignature = request.signature?.Trim().ToLower();
                bool isValid = computedSignature == receivedSignature;

                _logger.LogInformation($"🔐 Computed Signature: {computedSignature}");
                _logger.LogInformation($"📩 Received Signature: {receivedSignature}");
                _logger.LogInformation($"✅ Signature Match: {isValid}");

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Signature Verification Error: {ex.Message}");
                return false;
            }
        }
    }
}
