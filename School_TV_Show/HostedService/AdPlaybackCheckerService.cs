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
                var adLiveStreamRepo = scope.ServiceProvider.GetRequiredService<IAdLiveStreamRepo>();
                var programFollowRepo = scope.ServiceProvider.GetRequiredService<IProgramFollowRepo>();

                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

                // 1. Lấy các quảng cáo đến hạn phát trong vòng 1 phút
                var dueAds = await adLiveStreamRepo.GetDueAds(now.AddMinutes(1));

                foreach (var ad in dueAds)
                {
                    try
                    {
                        if (ad.AdSchedule != null && ad.Schedule?.ProgramID != null)
                        {
                            // 2. Lấy danh sách followers của chương trình
                            var followers = await programFollowRepo.GetByProgramIdAsync(ad.Schedule.ProgramID);

                            foreach (var follower in followers)
                            {
                                // 3. Gửi quảng cáo cho từng follower (theo account ID group)
                                await _hubContext.Clients.Group(follower.AccountID.ToString())
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
                        }

                        _logger.LogInformation($"✅ Gửi quảng cáo ID {ad.AdLiveStreamID} thành công.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"❌ Lỗi khi xử lý quảng cáo ID {ad.AdLiveStreamID}");
                    }
                }

                // 4. Delay 10s để lặp lại
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
