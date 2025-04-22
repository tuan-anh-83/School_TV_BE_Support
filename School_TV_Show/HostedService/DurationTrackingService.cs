using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.HostedServices
{
    public class DurationTrackingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DurationTrackingService> _logger;

        public DurationTrackingService(IServiceProvider serviceProvider, ILogger<DurationTrackingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DurationTrackingService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var videoRepo = scope.ServiceProvider.GetRequiredService<IVideoHistoryRepo>();
                    var channelRepo = scope.ServiceProvider.GetRequiredService<ISchoolChannelRepo>();

                    var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);

                    var recentVideos = await videoRepo.GetVideosUploadedAfterAsync(tenMinutesAgo);

                    foreach (var video in recentVideos)
                    {
                        if (video.Program?.SchoolChannelID != null && video.StreamAt.HasValue)
                        {
                            var channel = await channelRepo.GetByIdAsync(video.Program.SchoolChannelID);

                            if (channel != null)
                            {
                                var elapsedTime = DateTime.UtcNow - video.StreamAt.Value;
                                double minutes = Math.Ceiling(elapsedTime.TotalMinutes);

                                if (channel.TotalDuration >= minutes)
                                {
                                    channel.TotalDuration -= (int)minutes;
                                    await channelRepo.UpdateAsync(channel);

                                    _logger.LogInformation($"Deducted {minutes} mins from SchoolChannel {channel.SchoolChannelID}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DurationTrackingService");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("DurationTrackingService stopped.");
        }
    }
}
