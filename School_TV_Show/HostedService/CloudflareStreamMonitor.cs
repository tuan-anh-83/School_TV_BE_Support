using BOs.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Repos;
using School_TV_Show.DTO;
using Services;
using Services.Hubs;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace School_TV_Show.HostedService
{
    public class CloudflareStreamMonitor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CloudflareStreamMonitor> _logger;
        private readonly CloudflareSettings _cloudflareSettings;
        private readonly IHubContext<LiveStreamHub> _hubContext;
        private readonly IHubContext<NotificationHub> _notiHubContext;
        public static TaskCompletionSource<bool> StartupCompleted = new();

        public CloudflareStreamMonitor(
            IServiceScopeFactory scopeFactory,
            ILogger<CloudflareStreamMonitor> logger,
            IHttpClientFactory httpClientFactory,
            IHubContext<LiveStreamHub> hubContext,
            IHubContext<NotificationHub> notiHubContext,
            IOptions<CloudflareSettings> cloudflareSettings
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _cloudflareSettings = cloudflareSettings.Value;
            _hubContext = hubContext;
            _notiHubContext = notiHubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartupCompleted.TrySetResult(true);

            _logger.LogInformation("📡 CloudflareStreamMonitor started.");
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope(); // ✅ mỗi vòng loop là 1 scope mới
                var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

                try
                {
                    var videoRepo = scope.ServiceProvider.GetRequiredService<IVideoHistoryRepo>();
                    var videos = await videoRepo.GetActiveUnconfirmedStreamsAsync();

                    foreach (var video in videos)
                    {
                        if (string.IsNullOrEmpty(video.CloudflareStreamId)) continue;

                        var httpClient = CreateCloudflareHttpClient(scope);
                        var (uid, streamState) = await GetStreamStateAsync(httpClient, video.CloudflareStreamId, stoppingToken);

                        if (streamState == "connected")
                        {
                            await MarkVideoAsLiveAsync(uid, video, scope, localNow);
                            await MarkScheduleAsLiveAsync(video, scope, localNow);
                            _logger.LogInformation("Stream is starting!");
                        }

                        if(video.Status && video.StreamAt <= localNow)
                        {
                            await MarkVideoAsLiveAsync(null, video, scope, localNow);
                            await MarkScheduleAsLiveAsync(video, scope, localNow);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // Wating for checking expired

                    var activeVideos = await videoRepo.GetActiveStreamsAsync();
                    foreach (var video in activeVideos)
                    {
                        if (string.IsNullOrEmpty(video.CloudflareStreamId)) continue;

                        var httpClient = CreateCloudflareHttpClient(scope);
                        var (uid, streamState) = await GetStreamStateAsync(httpClient, video.CloudflareStreamId, stoppingToken);

                        if (streamState == "connected")
                        {
                            await CheckAndStopOverdueSchedulesAsync(video, scope, stoppingToken, localNow);
                            await CheckAndEndStreamsForExpiredPackagesAsync(video, scope, localNow);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // Wating for checking expired

                    await MarkScheduleLateStartAsync(scope, localNow);
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // Wating for checking past time

/*                    await CheckAndMarkEndedEarlySchedulesAsync(scope, localNow);*/
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🔥 Error in CloudflareStreamMonitor loop.");
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // check per 2ms
            }
        }

        private HttpClient CreateCloudflareHttpClient(IServiceScope scope)
        {
            var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareSettings.ApiToken);
            return client;
        }

        private async Task<(string? Uid, string? State)> GetStreamStateAsync(HttpClient client, string streamId, CancellationToken token)
        {
            var url = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/live_inputs/{streamId}";
            var response = await client.GetAsync(url, token);
            if (!response.IsSuccessStatusCode) return (null, null);

            var json = await response.Content.ReadAsStringAsync(token);
            var result = JsonSerializer.Deserialize<CloudflareLiveInputResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (
                result?.Result?.Uid,
                result?.Result?.Status?.Current?.State
            );
        }

        private async Task NotifyToFollower(VideoHistory video, IServiceScope scope, DateTime now)
        {
            var programFollowRepo = scope.ServiceProvider.GetRequiredService<IProgramFollowRepo>();
            var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepo>();

            if (video.ProgramID == null)
            {
                _logger.LogWarning($"⚠️ VideoID {video.VideoHistoryID} does not have a ProgramID. Skipping notify.");
                return;
            }

            var programFollowers = await programFollowRepo.GetByProgramIdAsync(video.ProgramID.Value);

            if (programFollowers.Count() > 0)
            {
                foreach (var follower in programFollowers)
                {
                    var noti = new Notification
                    {
                        AccountID = follower.AccountID,
                        Title = $"📺 Starting live stream!",
                        Message = $"The program is in streaming now. Let join!",
                        Content = $"The program is in streaming now. Let join!",
                        CreatedAt = now,
                        IsRead = false
                    };
                    await notificationRepo.AddAsync(noti);
                    await _notiHubContext.Clients.Group(follower.AccountID.ToString())
                    .SendAsync("ReceiveNotification", new { title = noti.Title, content = noti.Content });
                }
            }
        }

        private async Task MarkVideoAsLiveAsync(string? uid, VideoHistory video, IServiceScope scope, DateTime now)
        {
            var videoRepo = scope.ServiceProvider.GetRequiredService<IVideoHistoryRepo>();

            video.Type = "Live";
            video.UpdatedAt = now;
            
            if(!string.IsNullOrEmpty(uid))
            {
                video.MP4Url = $"https://customer-nohgu8m8j4ms2pjk.cloudflarestream.com/{uid}/downloads/default.mp4";
            }

            await videoRepo.UpdateVideoAsync(video);
            _logger.LogInformation($"✅ Marked VideoID {video.VideoHistoryID} as Live");
        }

        private async Task MarkScheduleLateStartAsync(IServiceScope scope, DateTime now)
        {
            var liveStreamRepo = scope.ServiceProvider.GetRequiredService<ILiveStreamRepo>();
            var lateSchedules = await liveStreamRepo.GetLateStartCandidatesAsync(now);

            foreach (var s in lateSchedules)
            {
                if (!s.LiveStreamStarted && s.Status == "Ready" && now >= s.StartTime.AddMinutes(5))
                {
                    s.Status = "LateStart";
                    liveStreamRepo.UpdateSchedule(s);
                    _logger.LogWarning("Schedule {ScheduleID} marked as LateStart at {CurrentTime}.", s.ScheduleID, now);
                }
            }
            await liveStreamRepo.SaveChangesAsync();
        }

        private async Task MarkScheduleAsLiveAsync(VideoHistory video, IServiceScope scope, DateTime now)
        {
            if (video.ProgramID == null)
            {
                _logger.LogWarning($"⚠️ VideoID {video.VideoHistoryID} does not have a ProgramID. Skipping schedule update.");
                return;
            }

            var scheduleRepo = scope.ServiceProvider.GetRequiredService<IScheduleRepo>();
            var schedule = await scheduleRepo.GetScheduleByProgramIdAsync(video.ProgramID.Value);

            if (schedule != null && schedule.Status != "Live")
            {
                schedule.VideoHistoryID = video.VideoHistoryID;
                schedule.Status = now > schedule.StartTime.AddMinutes(5) ? "LateStart" : "Live";
                schedule.LiveStreamStarted = true;

                await scheduleRepo.UpdateScheduleAsync(schedule);
                _logger.LogInformation($"🎬 Marked ScheduleID {schedule.ScheduleID} as Live (from VideoID {video.VideoHistoryID})");

                await NotifyToFollower(video, scope, now);
            }
        }

        private async Task CheckAndStopOverdueSchedulesAsync(VideoHistory video, IServiceScope scope, CancellationToken cancellationToken, DateTime now)
        {
            var scheduleRepo = scope.ServiceProvider.GetRequiredService<IScheduleRepo>();
            var liveStreamService = scope.ServiceProvider.GetRequiredService<ILiveStreamService>();
            var liveStreamRepo = scope.ServiceProvider.GetRequiredService<ILiveStreamRepo>();
            var packageRepo = scope.ServiceProvider.GetRequiredService<IPackageRepo>();

            if (video.ProgramID == null) return;

            var schedule = await scheduleRepo.GetActiveScheduleByProgramIdAsync(video.ProgramID.Value);

            if (schedule == null || schedule.LiveStreamEnded || schedule.EndTime > now) return;

            if(video.Status && video.Duration > 0 && video.StreamAt.HasValue)
            {
                DateTime end = video.StreamAt.Value.AddSeconds(video.Duration.Value);
                if(now >= end)
                {
                    video.Type = "Recorded";
                    video.UpdatedAt = now;
                    var updated = await liveStreamRepo.UpdateVideoHistoryAsync(video);

                    schedule.LiveStreamEnded = true;
                    schedule.Status = "EndedEarly";
                    schedule.VideoHistoryID = video.VideoHistoryID;
                    await scheduleRepo.UpdateScheduleAsync(schedule);

                    await _hubContext.Clients.All.SendAsync("StreamEnded", new
                    {
                        scheduleId = schedule.ScheduleID,
                        videoId = video.VideoHistoryID
                    });

                    Console.WriteLine($"✅ Ended overdue schedule stream: ProgramID {schedule.ProgramID}");
                    _logger.LogInformation($"✅ Ended overdue schedule stream: ProgramID {schedule.ProgramID}");
                }
                return;
            }

            var success = await liveStreamService.EndStreamAndReturnLinksAsync(video);
            if (success)
            {
                schedule.LiveStreamEnded = true;
                schedule.Status = "EndedEarly";
                schedule.VideoHistoryID = video.VideoHistoryID;
                await scheduleRepo.UpdateScheduleAsync(schedule);

                await _hubContext.Clients.All.SendAsync("StreamEnded", new
                {
                    scheduleId = schedule.ScheduleID,
                    videoId = video.VideoHistoryID
                });

                Console.WriteLine($"✅ Ended overdue schedule stream: ProgramID {schedule.ProgramID}");
            }
        }

        private async Task CheckAndEndStreamsForExpiredPackagesAsync(VideoHistory video, IServiceScope scope, DateTime now)
        {
            var scheduleRepo = scope.ServiceProvider.GetRequiredService<IScheduleRepo>();
            var videoRepo = scope.ServiceProvider.GetRequiredService<IVideoHistoryRepo>();
            var packageRepo = scope.ServiceProvider.GetRequiredService<IPackageRepo>();
            var liveStreamService = scope.ServiceProvider.GetRequiredService<ILiveStreamService>();

            if (video.ProgramID == null) return;

            var accountPackage = await packageRepo.GetCurrentPackageAndDurationByProgramIdAsync(video.ProgramID.Value);

            var expired = accountPackage == null || accountPackage.ExpiredAt < now || accountPackage.RemainingMinutes <= 0;
            if (!expired) return;

            var success = await liveStreamService.EndStreamAndReturnLinksAsync(video);
            if (success)
            {
                var schedule = await scheduleRepo.GetActiveScheduleByProgramIdAsync(video.ProgramID.Value);
                if (schedule != null)
                {
                    schedule.Status = "Ended";
                    schedule.LiveStreamEnded = true;
                    schedule.VideoHistoryID = video.VideoHistoryID;
                    await scheduleRepo.UpdateScheduleAsync(schedule);
                }

                video.Status = false;
                await videoRepo.UpdateVideoAsync(video);

                _logger.LogInformation("🔒 Ended stream due to expired package for AccountID {AccountID}", accountPackage?.AccountID);

                await _hubContext.Clients.All.SendAsync("StreamEnded", new
                {
                    scheduleId = schedule?.ScheduleID,
                    videoId = video.VideoHistoryID
                });
            }
        }

        private async Task CheckAndMarkEndedEarlySchedulesAsync(IServiceScope scope, DateTime now)
        {
            var liveStreamRepo = scope.ServiceProvider.GetRequiredService<ILiveStreamRepo>();
            var videoHistoryRepo = scope.ServiceProvider.GetRequiredService<IVideoHistoryRepo>();

            var lateSchedules = await liveStreamRepo.GetSchedulesPastEndTimeAsync(now);

            foreach (var schedule in lateSchedules)
            {
                VideoHistory? video = null;
                if (schedule.VideoHistoryID != null)
                {
                    video = await videoHistoryRepo.GetVideoByIdAsync(schedule.VideoHistoryID.Value);
                }
                else
                {
                    video = await liveStreamRepo.GetVideoHistoryByProgramIdAsync(schedule.ProgramID, schedule.StartTime);
                }

                schedule.Status = "Ended";
                schedule.LiveStreamEnded = true;
                await liveStreamRepo.UpdateAsync(schedule);
                _logger.LogInformation($"Updated past schedule - ID: {schedule.ScheduleID}");

                if (video != null && !string.IsNullOrEmpty(video.CloudflareStreamId))
                {
                    video.Type = "Recorded";
                    if (video.Duration == null) video.Duration = 0;
                    await liveStreamRepo.UpdateVideoHistoryAsync(video);
                    _logger.LogInformation($"Updated past video history - ID: {video.VideoHistoryID}");
                }

                _logger.LogInformation("[Auto-End] Schedule {ScheduleID} marked as EndedEarly (no stream detected).", schedule.ScheduleID);
            }
        }
    }
}
