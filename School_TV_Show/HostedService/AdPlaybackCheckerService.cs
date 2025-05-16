using BOs.Models;
using Microsoft.AspNetCore.SignalR;
using Repos;
using Services.Hubs;

namespace School_TV_Show.HostedService
{
    public class AdPlaybackCheckerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<AdPlaybackCheckerService> _logger;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public AdPlaybackCheckerService(
               IServiceScopeFactory scopeFactory,
               IHubContext<NotificationHub> hubContext,
               ILogger<AdPlaybackCheckerService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

                await PrepareAds(scope, now);
                await DisableAndCalculateForOldAds(scope, now);

                // 4. Delay 10s để lặp lại
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task PrepareAds(IServiceScope scope, DateTime now)
        {
            var adLiveStreamRepo = scope.ServiceProvider.GetRequiredService<IAdLiveStreamRepo>();
            var programFollowRepo = scope.ServiceProvider.GetRequiredService<IProgramFollowRepo>();

            // 1. Lấy các quảng cáo đến hạn phát trong vòng 2 phút
            var dueAds = await adLiveStreamRepo.GetDueAds(now.AddMinutes(2));

            foreach (var ad in dueAds)
            {
                try
                {
                    if (ad.AdSchedule != null && ad.Schedule?.ProgramID != null)
                    {
                        // 2. Gửi quảng cáo cho những ai đang trong buổi live theo schedule ID
                        await _hubContext.Clients.Group(ad.ScheduleID.ToString())
                            .SendAsync("Ad", new
                            {
                                adScheduleId = ad.AdScheduleID,
                                adLiveStreamId = ad.AdLiveStreamID,
                                videoUrl = ad.AdSchedule?.VideoUrl,
                                startTime = ad.PlayAt,
                                endTime = ad.AdSchedule != null ? ad.PlayAt.AddSeconds(ad.AdSchedule.DurationSeconds) : ad.PlayAt.AddSeconds(10),
                                title = ad.AdSchedule?.Title,
                                ownerId = ad.AdSchedule?.AccountID
                            });
                    }

                    _logger.LogInformation($"✅ Gửi quảng cáo ID {ad.AdLiveStreamID} thành công.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Lỗi khi xử lý quảng cáo ID {ad.AdLiveStreamID}");
                }
            }
        }

        private async Task DisableAndCalculateForOldAds(IServiceScope scope, DateTime now)
        {
            try
            {
                var adLiveStreamRepo = scope.ServiceProvider.GetRequiredService<IAdLiveStreamRepo>();
                var accountPackageRepo = scope.ServiceProvider.GetRequiredService<IAccountPackageRepo>();

                var expiredAds = await adLiveStreamRepo.GetExpiredAds(now);
                var accountPackagesCache = new Dictionary<int, AccountPackage>();

                if (expiredAds.Any())
                {
                    foreach (var expiredAd in expiredAds)
                    {
                        await adLiveStreamRepo.UpdateStatusAlternative(expiredAd.AdLiveStreamID);
                        _logger.LogInformation($"⏹ Đã vô hiệu hóa quảng cáo #{expiredAd.AdLiveStreamID}");

                        if (expiredAd.AdSchedule != null)
                        {
                            int accountId = expiredAd.AdSchedule.AccountID;

                            if (!accountPackagesCache.TryGetValue(accountId, out var accountPackage))
                            {
                                accountPackage = await accountPackageRepo.GetActiveAccountPackageAsync(accountId);
                                if (accountPackage == null)
                                {
                                    _logger.LogWarning($"⚠️ Account {accountId} chưa có gói.");
                                    continue;
                                }

                                accountPackagesCache[accountId] = accountPackage;
                            }

                            accountPackage.MinutesUsed += expiredAd.AdSchedule.DurationSeconds / 60.0;
                            accountPackage.RemainingMinutes = accountPackage.TotalMinutesAllowed - accountPackage.MinutesUsed;

                            await accountPackageRepo.UpdateAccountPackageAsync(accountPackage);

                            _logger.LogInformation($"⏱ Cộng {expiredAd.AdSchedule.DurationSeconds / 60.0} phút vào gói của AccountID = {expiredAd.AdSchedule.AccountID}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Lỗi khi xử lý danh sách quảng cáo cũ: {ex}");
            }
        }
    }
}
