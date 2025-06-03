using BOs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repos;
using School_TV_Show.HostedService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.HostedServices
{
    public class DurationTrackingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CloudflareSettings _cloudflareSettings;
        private readonly ILogger<DurationTrackingService> _logger;
        private readonly HttpClient _httpClient;

        public DurationTrackingService(
            IServiceProvider serviceProvider, 
            ILogger<DurationTrackingService> logger,
            IOptions<CloudflareSettings> cloudflareSettings,
            IHttpClientFactory httpClientFactory
        )
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClientFactory.CreateClient();
            _cloudflareSettings = cloudflareSettings.Value;
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _cloudflareSettings.ApiToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for CloudflareStreamMonitor to be ready...");
            await CloudflareStreamMonitor.StartupCompleted.Task;

            _logger.LogInformation("DurationTrackingService started after Cloudflare is ready.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var videoRepo = scope.ServiceProvider.GetRequiredService<IVideoHistoryRepo>();
                    var channelRepo = scope.ServiceProvider.GetRequiredService<ISchoolChannelRepo>();

                    await TrackDuration(videoRepo, channelRepo);
                    await SetDownloadForVideos(videoRepo, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DurationTrackingService");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("DurationTrackingService stopped.");
        }

        private async Task SetDownloadForVideos(IVideoHistoryRepo videoHistoryRepo, CancellationToken stoppingToken)
        {
            var videos = await videoHistoryRepo.GetUpcomingVideosWithoutDownloadUrlAsync();

            foreach (var video in videos)
            {
                if (stoppingToken.IsCancellationRequested) break;

                var detailsUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/{video.CloudflareStreamId}";
                var response = await _httpClient.GetAsync(detailsUrl, stoppingToken);
                if (!response.IsSuccessStatusCode) continue;

                var json = await response.Content.ReadAsStringAsync(stoppingToken);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement.GetProperty("result");

                if (root.TryGetProperty("readyToStream", out var readyProp) && readyProp.GetBoolean())
                {
                    _logger.LogInformation("Video {Id} is ready. Creating download link...", video.VideoHistoryID);

                    var downloadUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/{video.CloudflareStreamId}/downloads";
                    var downloadResponse = await _httpClient.PostAsync(downloadUrl, null, stoppingToken);

                    if (downloadResponse.IsSuccessStatusCode)
                    {
                        var result = await downloadResponse.Content.ReadAsStringAsync(stoppingToken);
                        using var downloadDoc = JsonDocument.Parse(result);

                        var mp4Url = downloadDoc.RootElement
                            .GetProperty("result")
                            .GetProperty("default")
                            .GetProperty("url")
                            .GetString();

                        await videoHistoryRepo.UpdateMp4UrlAsync(video.VideoHistoryID, mp4Url ?? string.Empty);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create download URL for video {Id}", video.VideoHistoryID);
                    }
                }
            }
        }

        private async Task TrackDuration(IVideoHistoryRepo videoHistoryRepo, ISchoolChannelRepo schoolChannelRepo)
        {
            /*var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);

            var recentVideos = await videoHistoryRepo.GetVideosUploadedAfterAsync(tenMinutesAgo);

            foreach (var video in recentVideos)
            {
                if (video.Program?.SchoolChannelID != null && video.StreamAt.HasValue)
                {
                    var channel = await schoolChannelRepo.GetByIdAsync(video.Program.SchoolChannelID);

                    if (channel != null)
                    {
                        var elapsedTime = DateTime.UtcNow - video.StreamAt.Value;
                        double minutes = Math.Ceiling(elapsedTime.TotalMinutes);

                        if (channel.TotalDuration >= minutes)
                        {
                            channel.TotalDuration -= (int)minutes;
                            await schoolChannelRepo.UpdateAsync(channel);

                            _logger.LogInformation($"Deducted {minutes} mins from SchoolChannel {channel.SchoolChannelID}");
                        }
                    }
                }
            }*/
        }
    }
}
