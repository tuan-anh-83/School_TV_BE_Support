
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using BOs.Models;
using Services.Email;
using Services.Hubs;
using Services;
using Repos;

namespace BLL.Services.LiveStream.Implements
{
    public class LiveStreamScheduler : BackgroundService
    {
        private readonly ILogger<LiveStreamScheduler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<LiveStreamHub> _hubContext;
        private readonly CloudflareSettings _cloudflareSettings;

        public LiveStreamScheduler(
            ILogger<LiveStreamScheduler> logger,
            IServiceScopeFactory scopeFactory,
            IHubContext<LiveStreamHub> hubContext,
            IOptions<CloudflareSettings> cloudflareOptions)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _cloudflareSettings = cloudflareOptions.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LiveStreamScheduler is starting.");

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            while (!stoppingToken.IsCancellationRequested)
            {
                var utcNow = DateTime.UtcNow;
                var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, vietnamTimeZone);
                _logger.LogInformation("[Scheduler Tick] Vietnam time now: {CurrentTime}", localNow);

                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ILiveStreamRepo>();
                var streamService = scope.ServiceProvider.GetRequiredService<ILiveStreamService>();
                var adService = scope.ServiceProvider.GetRequiredService<IAdScheduleService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var videoRepo = scope.ServiceProvider.GetRequiredService<IVideoHistoryRepo>();

                try
                {
                    var pending = await repository.GetPendingSchedulesAsync(utcNow.AddMinutes(5));
                    _logger.LogInformation("Pending schedules to prepare: {Count}", pending.Count);

                    foreach (var s in pending)
                    {
                        s.Status = "Ready";
                        _logger.LogInformation("Schedule {ScheduleID} marked as READY.", s.ScheduleID);

                        var program = s.Program ?? await repository.GetProgramByIdAsync(s.ProgramID);
                        if (program == null)
                        {
                            _logger.LogWarning("Program not found for ScheduleID {ScheduleID}", s.ScheduleID);
                            continue;
                        }

                        var school = program.SchoolChannel ?? await repository.GetSchoolChannelByIdAsync(program.SchoolChannelID);
                        if (school == null)
                        {
                            _logger.LogWarning("SchoolChannel not found for ProgramID {ProgramID}", program.ProgramID);
                            continue;
                        }

                        try
                        {
                            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                            var programFollowService = scope.ServiceProvider.GetRequiredService<IProgramFollowService>();
                           var schoolFollowService = scope.ServiceProvider.GetRequiredService<ISchoolChannelFollowService>();

                            var programFollowers = await programFollowService.GetByProgramIdAsync(program.ProgramID);
                             var schoolFollowers = await schoolFollowService.GetFollowersBySchoolChannelIdAsync(school.SchoolChannelID);
                             var notifiedAccountIds = programFollowers.Select(f => f.AccountID)
                                                     .Union(schoolFollowers.Select(f => f.AccountID))
                                                     .Distinct();

                            foreach (var accId in notifiedAccountIds)
                            {
                                var noti = new Notification
                                {
                                    AccountID = accId,
                                    Title = $"📺 Livestream from {school.Name} is about to start!",
                                    Message = $"The program {program.ProgramName} will start at {s.StartTime.ToLocalTime():HH:mm}",
                                    Content = $"The program {program.ProgramName} will start at {s.StartTime.ToLocalTime():HH:mm}",
                                    CreatedAt = DateTime.UtcNow,
                                    IsRead = false
                                };
                                await notificationService.CreateNotificationAsync(noti);
                                await _hubContext.Clients.User(accId.ToString())
                                    .SendAsync("ReceiveNotification", new { title = noti.Title, content = noti.Content });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending notification to followers for ScheduleID {ScheduleID}", s.ScheduleID);
                        }
                    }

                    var lateSchedules = await repository.GetLateStartCandidatesAsync(utcNow);
                    foreach (var s in lateSchedules)
                    {
                        if (!s.LiveStreamStarted && s.Status == "Ready" && utcNow >= s.StartTime.AddMinutes(5))
                        {
                            s.Status = "LateStart";
                            repository.UpdateSchedule(s);
                            _logger.LogWarning("Schedule {ScheduleID} marked as LateStart at {CurrentTime}.", s.ScheduleID, localNow);
                        }
                    }

                    var toStart = await repository.GetReadySchedulesAsync(utcNow.AddMinutes(5));
                    _logger.LogInformation("Schedules to start: {Count}", toStart.Count);

                    foreach (var schedule in toStart)
                    {
                        if (schedule.IsReplay)
                        {
                            if (utcNow >= schedule.EndTime)
                            {
                                var replayVideo = await videoRepo.GetReplayVideoByProgramAndTimeAsync(
                                    schedule.ProgramID,
                                    schedule.StartTime,
                                    schedule.EndTime
                                );

                                if (replayVideo != null)
                                {
                                    schedule.VideoHistoryID = replayVideo.VideoHistoryID;
                                }

                                schedule.Status = "Ended";
                                schedule.LiveStreamStarted = true;
                                schedule.LiveStreamEnded = true;

                                repository.UpdateSchedule(schedule);
                                await repository.SaveChangesAsync();

                                _logger.LogInformation("Replay schedule {ScheduleID} marked as Ended.", schedule.ScheduleID);

                                string playbackUrl = replayVideo?.PlaybackUrl;
                                string iframeUrl = !string.IsNullOrEmpty(replayVideo?.CloudflareStreamId)
                                    ? $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{replayVideo.CloudflareStreamId}/iframe"
                                    : null;

                                await _hubContext.Clients.All.SendAsync("StreamEnded", new
                                {
                                    scheduleId = schedule.ScheduleID,
                                    videoId = replayVideo?.VideoHistoryID,
                                    playbackUrl = playbackUrl,
                                    iframeUrl = iframeUrl
                                });
                            }
                            else
                            {
                                schedule.Status = "Ready";
                                schedule.LiveStreamStarted = true;
                                schedule.LiveStreamEnded = true;

                                repository.UpdateSchedule(schedule);
                                await repository.SaveChangesAsync();

                                _logger.LogInformation("Replay schedule {ScheduleID} marked as Ready (still playing).", schedule.ScheduleID);
                            }
                            continue;
                        }

                        var existingVideo = await repository.GetVideoHistoryByProgramIdAsync(schedule.ProgramID);
                        if (existingVideo == null)
                        {
                            var program = schedule.Program ?? await repository.GetProgramByIdAsync(schedule.ProgramID);
                            schedule.Program = program;

                            if (!string.IsNullOrEmpty(program.CloudflareStreamId))
                            {
                                bool stillExists = await streamService.CheckLiveInputExistsAsync(program.CloudflareStreamId);
                                if (!stillExists)
                                {
                                    program.CloudflareStreamId = null;
                                    await repository.UpdateProgramAsync(program);
                                    _logger.LogWarning("[Input Check] Old Cloudflare input not found, cleared CloudflareStreamId for ProgramID={0}", program.ProgramID);
                                }
                            }

                            var video = new VideoHistory
                            {
                                ProgramID = program.ProgramID,
                                Description = $"Scheduled stream for program {program.ProgramName}",
                                CreatedAt = utcNow,
                                UpdatedAt = utcNow,
                                StreamAt = schedule.StartTime,
                                Status = true,
                                Type = "Live",
                                CloudflareStreamId = program.CloudflareStreamId
                            };

                            bool created = await streamService.StartLiveStreamAsync(video);
                            if (created)
                            {
                                await repository.SaveChangesAsync();
                                schedule.VideoHistoryID = video.VideoHistoryID;
                                schedule.Status = schedule.Status != "LateStart" ? "Ready" : "LateStart";
                                schedule.LiveStreamStarted = false;

                                var streamerEmail = program.SchoolChannel?.Email;
                                if (!string.IsNullOrEmpty(streamerEmail))
                                {
                                    var schoolName = program.SchoolChannel?.Name ?? "School TV Show";
                                    await emailService.SendStreamKeyEmailAsync(
                                        streamerEmail,
                                        video.URL,
                                        schedule.StartTime,
                                        schedule.EndTime,
                                        schoolName
                                    );

                                    _logger.LogInformation("Stream URL created and email sent to {Email} for ScheduleID {ScheduleID}.",
                                        streamerEmail, schedule.ScheduleID);
                                }
                                else
                                {
                                    _logger.LogWarning("No email found for ScheduleID {ScheduleID}'s streamer.", schedule.ScheduleID);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Failed to create stream URL for ScheduleID {ScheduleID}", schedule.ScheduleID);
                            }
                        }
                    }

                    await repository.SaveChangesAsync();

                    var waitingSchedules = await repository.GetWaitingToStartStreamsAsync();
                    foreach (var schedule in waitingSchedules)
                    {
                        var videoHistory = await repository.GetVideoHistoryByProgramIdAsync(schedule.ProgramID);
                        if (videoHistory == null) continue;

                        bool streamerStarted = await streamService.CheckStreamerStartedAsync(videoHistory.CloudflareStreamId);
                        if (streamerStarted && !schedule.LiveStreamStarted)
                        {
                            schedule.LiveStreamStarted = true;
                            schedule.Status = "Live";
                            repository.UpdateSchedule(schedule);
                            _logger.LogInformation("Streamer started broadcasting, ScheduleID {ScheduleID} marked LIVE.", schedule.ScheduleID);

                            await _hubContext.Clients.All.SendAsync("StreamStarted", new
                            {
                                scheduleId = schedule.ScheduleID,
                                videoId = videoHistory.VideoHistoryID,
                                url = videoHistory.URL,
                                playbackUrl = videoHistory.PlaybackUrl
                            });
                        }
                    }

                    await repository.SaveChangesAsync();

                    var liveSchedules = await repository.GetLiveSchedulesAsync();
                    foreach (var schedule in liveSchedules)
                    {
                        var videoHistory = await repository.GetVideoHistoryByProgramIdAsync(schedule.ProgramID);
                        if (videoHistory == null) continue;

                        bool isStillStreaming = await streamService.CheckStreamerStartedAsync(videoHistory.CloudflareStreamId);
                        if (!isStillStreaming && !schedule.LiveStreamEnded)
                        {
                            if (utcNow < schedule.StartTime)
                            {
                                _logger.LogInformation("Skip early end check for ScheduleID {ScheduleID} - not yet time to stream (StartTime = {StartTime})", schedule.ScheduleID, schedule.StartTime);
                                continue;
                            }

                            if (utcNow < schedule.StartTime.AddMinutes(5))
                            {
                                _logger.LogInformation("Skip early end check for ScheduleID {ScheduleID} - within 5-minute grace after StartTime {StartTime}", schedule.ScheduleID, schedule.StartTime);
                                continue;
                            }

                            if (utcNow >= schedule.EndTime)
                            {
                                var success = await streamService.EndStreamAndReturnLinksAsync(videoHistory);
                                if (success)
                                {
                                    schedule.Status = "EndedEarly";
                                    schedule.LiveStreamEnded = true;

                                    if (!schedule.LiveStreamStarted)
                                    {
                                        await videoRepo.DeleteVideoAsync(videoHistory.VideoHistoryID);
                                        _logger.LogWarning("No livestream detected for ScheduleID {ScheduleID}. VideoHistory was deleted.", schedule.ScheduleID);
                                    }
                                    else
                                    {
                                        schedule.VideoHistoryID = videoHistory.VideoHistoryID;
                                    }

                                    await repository.SaveChangesAsync();
                                }
                                else
                                {
                                    _logger.LogWarning("Failed to end and save stream for ScheduleID {ScheduleID}", schedule.ScheduleID);
                                }
                            }
                            else
                            {
                                _logger.LogInformation("Skip ending ScheduleID {ScheduleID} early - stream not active, but still within scheduled time.", schedule.ScheduleID);
                            }
                        }
                    }

                    var overdueSchedules = await repository.GetOverdueSchedulesAsync(utcNow);
                    foreach (var schedule in overdueSchedules)
                    {
                        if (schedule.LiveStreamStarted && !schedule.LiveStreamEnded)
                        {
                            var videoHistory = await repository.GetVideoHistoryByProgramIdAsync(schedule.ProgramID);
                            if (videoHistory == null) continue;

                            var success = await streamService.EndStreamAndReturnLinksAsync(videoHistory);
                            if (success)
                            {
                                schedule.Status = "Ended";
                                schedule.LiveStreamEnded = true;
                                schedule.VideoHistoryID = videoHistory.VideoHistoryID;
                                await repository.SaveChangesAsync();

                                _logger.LogInformation("Force-ended overdue livestream for ScheduleID {ScheduleID}, recordings saved.", schedule.ScheduleID);

                                await _hubContext.Clients.All.SendAsync("StreamEnded", new
                                {
                                    scheduleId = schedule.ScheduleID,
                                    videoId = videoHistory.VideoHistoryID
                                });
                            }
                            else
                            {
                                _logger.LogError("Failed to force-end livestream or save recording for ScheduleID {ScheduleID}", schedule.ScheduleID);
                            }
                        }
                    }

                    await CheckAndMarkEndedEarlySchedulesAsync(repository, streamService, utcNow);
                    await repository.SaveChangesAsync();

                    var expiredVideos = await videoRepo.GetExpiredUploadedVideosAsync(utcNow);
                    foreach (var video in expiredVideos)
                    {
                        video.Status = false;
                        video.UpdatedAt = utcNow;
                        _logger.LogInformation("[Expire] Uploaded video {VideoID} marked as inactive.", video.VideoHistoryID);
                    }
                    await repository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during LiveStreamScheduler tick");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("LiveStreamScheduler stopped.");
        }
        private async Task CheckAndMarkEndedEarlySchedulesAsync(ILiveStreamRepo repository, ILiveStreamService streamService, DateTime now)
        {
            var lateSchedules = await repository.GetLateStartSchedulesPastEndTimeAsync(now);

            foreach (var schedule in lateSchedules)
            {
                if (!schedule.LiveStreamEnded && schedule.Status == "LateStart")
                {
                    var video = await repository.GetVideoHistoryByProgramIdAsync(schedule.ProgramID);

                    schedule.Status = "EndedEarly";
                    schedule.LiveStreamEnded = true;
                    schedule.VideoHistoryID = video?.VideoHistoryID;
                    await repository.UpdateAsync(schedule);

                    if (video != null && !string.IsNullOrEmpty(video.CloudflareStreamId))
                    {
                        try
                        {
                            var deleted = await streamService.EndLiveStreamAsync(video);
                            if (deleted)
                            {
                                _logger.LogInformation("[Auto-End] Cloudflare input deleted for LateStart ScheduleID {ScheduleID}, StreamID {StreamID}.",
                                    schedule.ScheduleID, video.VideoHistoryID);
                            }
                            else
                            {
                                _logger.LogWarning("[Auto-End] Failed to delete Cloudflare input for LateStart ScheduleID {ScheduleID}, StreamID {StreamID}.",
                                    schedule.ScheduleID, video.VideoHistoryID);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[Auto-End] Error deleting Cloudflare input for ScheduleID {ScheduleID}", schedule.ScheduleID);
                        }
                    }

                    _logger.LogInformation("[Auto-End] Schedule {ScheduleID} marked as EndedEarly (no stream detected).", schedule.ScheduleID);
                }
            }
        }
    }
}
