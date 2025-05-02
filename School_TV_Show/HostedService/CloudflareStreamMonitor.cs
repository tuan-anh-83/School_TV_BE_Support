using BOs.Models;
using Microsoft.Identity.Client;
using Repos;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;
using School_TV_Show.DTO;
using Microsoft.AspNetCore.SignalR;
using Services.Hubs;
using Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace School_TV_Show.HostedService
{
    public class CloudflareStreamMonitor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CloudflareStreamMonitor> _logger;
        private readonly CloudflareSettings _cloudflareSettings;
        private readonly IHubContext<LiveStreamHub> _hubContext;

        public CloudflareStreamMonitor(
            IServiceScopeFactory scopeFactory,
            ILogger<CloudflareStreamMonitor> logger,
            IHttpClientFactory httpClientFactory,
            IHubContext<LiveStreamHub> hubContext,
            IOptions<CloudflareSettings> cloudflareSettings
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _cloudflareSettings = cloudflareSettings.Value;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
                        var streamState = await GetStreamStateAsync(httpClient, video.CloudflareStreamId, stoppingToken);

                        if (streamState == "connected")
                        {
                            await MarkVideoAsLiveAsync(video, scope, localNow);
                            await MarkScheduleAsLiveAsync(video, scope, localNow);
                        } 
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Wating for checking expired

                    var activeVideos = await videoRepo.GetActiveStreamsAsync();
                    foreach (var video in activeVideos)
                    {
                        if (string.IsNullOrEmpty(video.CloudflareStreamId)) continue;

                        var httpClient = CreateCloudflareHttpClient(scope);
                        var streamState = await GetStreamStateAsync(httpClient, video.CloudflareStreamId, stoppingToken);

                        if (streamState == "connected")
                        {
                            await CheckAndStopOverdueSchedulesAsync(video, scope, stoppingToken, localNow);
                            await CheckAndEndStreamsForExpiredPackagesAsync(video, scope, localNow);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Wating for checking expired

                    await MarkScheduleLateStartAsync(scope, localNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🔥 Error in CloudflareStreamMonitor loop.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // check per 5ms
            }
        }

        private HttpClient CreateCloudflareHttpClient(IServiceScope scope)
        {
            var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareSettings.ApiToken);
            return client;
        }

        private async Task<string?> GetStreamStateAsync(HttpClient client, string streamId, CancellationToken token)
        {
            var url = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/live_inputs/{streamId}";
            var response = await client.GetAsync(url, token);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(token);
            var result = JsonSerializer.Deserialize<CloudflareLiveInputResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Result?.Status?.Current?.State;
        }

        private async Task MarkVideoAsLiveAsync(VideoHistory video, IServiceScope scope, DateTime now)
        {
            var videoRepo = scope.ServiceProvider.GetRequiredService<IVideoHistoryRepo>();

            video.Type = "Live";
            video.UpdatedAt = now;

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
                schedule.Status = now > schedule.StartTime.AddMinutes(5) ? "LateStart" : "Live";
                schedule.LiveStreamStarted = true;

                await scheduleRepo.UpdateScheduleAsync(schedule);
                _logger.LogInformation($"🎬 Marked ScheduleID {schedule.ScheduleID} as Live (from VideoID {video.VideoHistoryID})");
                await _hubContext.Clients.All.SendAsync("StreamStarted", new
                {
                    scheduleId = schedule.ScheduleID,
                    videoId = video.VideoHistoryID,
                    url = video.URL,
                    playbackUrl = video.PlaybackUrl
                });
            }
        }

        private async Task CheckAndStopOverdueSchedulesAsync(VideoHistory video, IServiceScope scope, CancellationToken cancellationToken, DateTime now)
        {
            var scheduleRepo = scope.ServiceProvider.GetRequiredService<IScheduleRepo>();
            var liveStreamService = scope.ServiceProvider.GetRequiredService<ILiveStreamService>();

            if (video.ProgramID == null) return;

            var schedule = await scheduleRepo.GetActiveScheduleByProgramIdAsync(video.ProgramID.Value);

            if (schedule == null || schedule.LiveStreamEnded || schedule.EndTime > now) return;

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

            var expired = accountPackage == null || accountPackage.ExpiredAt < now || accountPackage.RemainingHours <= 0;
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
    }
}
