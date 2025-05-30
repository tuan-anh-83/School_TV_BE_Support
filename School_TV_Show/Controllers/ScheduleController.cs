using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Repos;
using School_TV_Show.DTO;
using School_TV_Show.Helpers;
using Services;
using Services.Hubs;
using System.Globalization;


namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly IVideoHistoryService _videoHistoryService;
        private readonly CloudflareSettings _cloudflareSettings;
        private readonly IProgramService _programService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IProgramFollowRepo _programFollowRepository;
        private readonly ISchoolChannelFollowRepo _schoolChannelFollowRepository;
        private readonly INotificationService _notificationService;
        private readonly IPackageService _packageService;
        private readonly ICloudflareUploadService _cloudflareUploadService;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public ScheduleController(
            IScheduleService scheduleService,
            IVideoHistoryService videoHistoryService,
            IProgramService programService,
            IHubContext<NotificationHub> hubContext,
            IProgramFollowRepo programFollowRepository,
            ISchoolChannelFollowRepo schoolChannelFollowRepository,
            INotificationService notificationService,
            IPackageService packageService,
            ICloudflareUploadService cloudflareUploadService,
            IOptions<CloudflareSettings> cloudflareOptions)
        {
            _scheduleService = scheduleService;
            _videoHistoryService = videoHistoryService;
            _programService = programService;
            _hubContext = hubContext;
            _programFollowRepository = programFollowRepository;
            _schoolChannelFollowRepository = schoolChannelFollowRepository;
            _notificationService = notificationService;
            _packageService = packageService;
            _cloudflareUploadService = cloudflareUploadService;
            _cloudflareSettings = cloudflareOptions.Value;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromForm] CreateScheduleRequest request)
        {
            DateTime startDate = DateTime.ParseExact(request.StartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(request.EndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input", ModelState));

            var currentPackage = await _packageService.GetCurrentPackageAndDurationByProgramIdAsync(request.ProgramID);

            if (currentPackage == null)
                return NotFound(new { error = "Chưa có gói đăng kí, vui lòng đăng kí gói để tạo quảng cáo." });

            if (currentPackage.RemainingMinutes <= 0 || currentPackage.ExpiredAt < TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone))
                return BadRequest(new { error = "Your package was expired." });

            var program = await _programService.GetProgramByIdAsync(request.ProgramID);
            if (program == null)
                return NotFound(new ApiResponse(false, "Program not found"));

            bool isOverlap = await _scheduleService.IsScheduleOverlappingAsync(program.SchoolChannelID, startDate, endDate);
            if (isOverlap)
                return Conflict(new ApiResponse(false, "Schedule time overlaps with another program on the same school channel."));

            string? thumbnail = await _cloudflareUploadService.UploadImageAsync(request.ThumbnailFile); 

            var schedule = new Schedule
            {
                ProgramID = request.ProgramID,
                StartTime = startDate,
                EndTime = endDate,
                IsReplay = request.IsReplay,
                Status = "Pending",
                Thumbnail = thumbnail ?? string.Empty
            };

            var created = await _scheduleService.CreateScheduleAsync(schedule);
            var programFollowers = await _programFollowRepository.GetFollowersByProgramIdAsync(request.ProgramID);
            var channelFollowers = await _schoolChannelFollowRepository.GetFollowersByChannelIdAsync(program.SchoolChannelID);

            var allFollowerIds = programFollowers
                .Select(f => f.AccountID)
                .Concat(channelFollowers.Select(f => f.AccountID))
                .Distinct();

            foreach (var accountId in allFollowerIds)
            {
                var notification = new Notification
                {
                    ProgramID = request.ProgramID,
                    SchoolChannelID = program.SchoolChannelID,
                    AccountID = accountId,
                    Title = "New Schedule",
                    Message = $"A new schedule has been added for {program.ProgramName}.",
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.CreateNotificationAsync(notification);

                await _hubContext.Clients.User(accountId.ToString())
                    .SendAsync("ReceiveNotification", notification);
            }

            return Ok(new ApiResponse(true, "Schedule created", new
            {
                scheduleId = created.ScheduleID,
                startTime = created.StartTime,
                endTime = created.EndTime,
                status = created.Status
            }));
        }


        [HttpPost("replay-from-video")]
        public async Task<IActionResult> CreateReplayScheduleFromVideo([FromForm] CreateReplayScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input"));

            var video = await _videoHistoryService.GetVideoByIdAsync(request.VideoHistoryId);
            if (video == null || video.ProgramID == null)
                return NotFound(new ApiResponse(false, "Video not found or missing ProgramID"));

            string? thumbnail = await _cloudflareUploadService.UploadImageAsync(request.ThumbnailFile);

            var schedule = new Schedule
            {
                ProgramID = video.ProgramID.Value,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = "Ready",
                IsReplay = true,
                VideoHistoryID = video.VideoHistoryID,
                Thumbnail = thumbnail ?? string.Empty
            };

            var created = await _scheduleService.CreateScheduleAsync(schedule);

            var iframeUrl = string.IsNullOrEmpty(video.CloudflareStreamId)
                ? null
                : $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{video.CloudflareStreamId}/iframe";

            return Ok(new ApiResponse(true, "Replay schedule created", new
            {
                scheduleId = created.ScheduleID,
                videoId = video.VideoHistoryID,
                playbackUrl = video.PlaybackUrl,
                mp4Url = video.MP4Url,
                iframeUrl = iframeUrl,
                duration = video.Duration,
                description = video.Description,
                program = video.Program?.ProgramName ?? "No program",
                channel = video.Program?.SchoolChannel?.Name ?? "No channel",
                startTime = created.StartTime,
                endTime = created.EndTime,
                thumbnail = thumbnail ?? string.Empty
            }));
        }

        [HttpGet("live-now")]
        public async Task<IActionResult> GetLiveNowSchedules()
        {
            var schedules = await _scheduleService.GetLiveNowSchedulesAsync();
            return Ok(schedules.Select(s => new
            {
                s.ScheduleID,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                s.Status,
                s.IsReplay,
                s.Thumbnail,
                Program = new
                {
                    s.Program.ProgramID,
                    s.Program.Title,
                    SchoolChannel = new
                    {
                        s.Program.SchoolChannel.SchoolChannelID,
                        s.Program.SchoolChannel.Name
                    }
                }
            }));
        }

        [Authorize(Roles = "SchoolOwner")]
        [HttpGet("by-channel/{id}")]
        public async Task<IActionResult> GetSchedulesBySchoolId(int id)
        {
            var schedules = await _scheduleService.GetSchedulesBySchoolIdAsync(id);
            return Ok(schedules.Select(s => new
            {
                s.ScheduleID,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                s.Status,
                s.IsReplay,
                s.Thumbnail,
                Title = s.Program.Title,
                ProgramID = s.Program.ProgramID
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("suitable")]
        public async Task<IActionResult> GetSuitableSchedules()
        {
            var schedules = await _scheduleService.GetSuitableSchedulesAsync(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone));
            return Ok(schedules.Select(s => new
            {
                s.ScheduleID,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                s.Status,
                s.IsReplay,
                Program = new
                {
                    s.Program.ProgramID,
                    s.Program.Title,
                    SchoolChannel = new
                    {
                        s.Program.SchoolChannel.SchoolChannelID,
                        s.Program.SchoolChannel.Name,
                        s.Program.SchoolChannel.AccountID
                    }
                },
                AdLiveStreams = s.AdLiveStreams.Select(ad => new {
                    ad.AdLiveStreamID,
                    ad.AdScheduleID,
                    ad.PlayAt,
                    ad.Duration,
                    ad.IsPlayed
                })
            }));
        }

        [HttpGet("by-program/{programId}")]
        public async Task<IActionResult> GetSchedulesByProgramId(int programId)
        {
            var schedules = await _scheduleService.GetSchedulesByProgramIdAsync(programId);

            var result = schedules.Select(s => new
            {
                s.ScheduleID,
                s.StartTime,
                s.EndTime,
                s.LiveStreamStarted,
                s.LiveStreamEnded,
                s.Thumbnail
            });

            return Ok(new ApiResponse(true, "Schedules for program", result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var schedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
                return NotFound(new ApiResponse(false, "Schedule not found"));

            return Ok(new ApiResponse(true, "Schedule found", schedule));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromForm] UpdateScheduleRequest request)
        {
            DateTime startDate = DateTime.ParseExact(request.StartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(request.EndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input"));

            var existingSchedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (existingSchedule == null)
                return NotFound(new ApiResponse(false, "Schedule not found"));

            if (!existingSchedule.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ApiResponse(false, "Only 'Pending' schedules can be updated"));

            string? thumbnail = await _cloudflareUploadService.UploadImageAsync(request.ThumbnailFile);

            existingSchedule.StartTime = startDate;
            existingSchedule.EndTime = endDate;
            existingSchedule.Thumbnail = thumbnail ?? string.Empty;

            var updated = await _scheduleService.UpdateScheduleAsync(existingSchedule);
            return updated
                ? Ok(new ApiResponse(true, "Schedule updated successfully"))
                : StatusCode(500, new ApiResponse(false, "Failed to update schedule"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
                return NotFound(new ApiResponse(false, "Schedule not found"));

            if (!schedule.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ApiResponse(false, "Only 'Pending' schedules can be deleted"));

            var deleted = await _scheduleService.DeleteScheduleAsync(id);
            return deleted
                ? Ok(new ApiResponse(true, "Schedule deleted successfully"))
                : StatusCode(500, new ApiResponse(false, "Failed to delete schedule"));
        }

        [HttpGet("by-channel-and-date")]
        public async Task<IActionResult> GetSchedulesByChannelAndDate([FromQuery] int channelId, [FromQuery] DateTime date)
        {
            if (channelId <= 0)
                return BadRequest(new ApiResponse(false, "Invalid channel ID"));

            var schedules = await _scheduleService.GetSchedulesByChannelAndDateAsync(channelId, date);
            var result = new List<object>();

            foreach (var schedule in schedules)
            {
                var videoInfo = schedule.IsReplay
                    ? await GetReplayVideoInfoAsync(schedule)
                    : await GetLiveVideoInfoAsync(schedule);

                result.Add(new
                {
                    schedule.ScheduleID,
                    schedule.StartTime,
                    schedule.EndTime,
                    schedule.Status,
                    schedule.IsReplay,
                    schedule.LiveStreamStarted,
                    schedule.LiveStreamEnded,
                    schedule.ProgramID,
                    schedule.Thumbnail,
                    videoHistoryIdFromSchedule = schedule.VideoHistoryID,
                    videoHistoryPlaybackUrl = videoInfo.PlaybackUrl,
                    videoHistoryId = videoInfo.VideoHistoryID,
                    iframeUrl = videoInfo.IframeUrl,
                    duration = videoInfo.Duration,
                    description = videoInfo.Description,
                    mp4Url = videoInfo.MP4Url,
                    program = new
                    {
                        schedule.Program?.ProgramID,
                        schedule.Program?.ProgramName,
                        schedule.Program?.Title,
                        channel = schedule.Program?.SchoolChannel?.Name
                    }
                });
            }

            return Ok(new ApiResponse(true, "Schedules for channel and date", result));
        }

        private async Task<VideoInfo> GetReplayVideoInfoAsync(Schedule schedule)
        {
            var video = schedule.VideoHistoryID.HasValue
                ? await _videoHistoryService.GetVideoByIdAsync(schedule.VideoHistoryID.Value)
                : await _videoHistoryService.GetReplayVideoByProgramAndTimeAsync(schedule.ProgramID, schedule.StartTime, schedule.EndTime);

            if (video == null) return new VideoInfo();

            return new VideoInfo
            {
                IframeUrl = !string.IsNullOrEmpty(video.CloudflareStreamId)
                    ? $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{video.CloudflareStreamId}/iframe"
                    : null,
                PlaybackUrl = video.PlaybackUrl,
                MP4Url = video.MP4Url,
                Duration = video.Duration,
                Description = video.Description,
                VideoHistoryID = video.VideoHistoryID,
                Thumbnail = schedule.Thumbnail
            };
        }

        private async Task<VideoInfo> GetLiveVideoInfoAsync(Schedule schedule)
        {
            if (schedule.LiveStreamEnded && schedule.VideoHistoryID.HasValue)
            {
                var video = await _videoHistoryService.GetVideoByIdAsync(schedule.VideoHistoryID.Value);
                if (video != null)
                {
                    return new VideoInfo
                    {
                        IframeUrl = !string.IsNullOrEmpty(video.CloudflareStreamId)
                            ? $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{video.CloudflareStreamId}/iframe"
                            : null,
                        PlaybackUrl = video.PlaybackUrl,
                        MP4Url = video.MP4Url,
                        Duration = video.Duration,
                        Description = video.Description,
                        VideoHistoryID = video.VideoHistoryID,
                        Thumbnail = schedule.Thumbnail
                    };
                }
            }

            return new VideoInfo
            {
                Thumbnail = schedule.Thumbnail,
                IframeUrl = !string.IsNullOrEmpty(schedule.Program?.CloudflareStreamId)
                    ? $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{schedule.Program.CloudflareStreamId}/iframe"
                    : null
            };
        }

        [HttpGet("timeline")]
        public async Task<IActionResult> GetSchedulesTimeline()
        {
            var result = await _scheduleService.GetSchedulesGroupedTimelineAsync();
            return Ok(new ApiResponse(true, "Schedule timeline", result));
        }

        [HttpGet("by-date")]
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        public async Task<IActionResult> GetSchedulesByDate([FromQuery] DateTime date)
        {
            var schedules = await _scheduleService.GetSchedulesByDateAsync(date);
            var result = schedules.Select(s => new
            {
                s.ScheduleID,
                s.StartTime,
                s.EndTime,
                s.Status,
                s.IsReplay,
                s.ProgramID,
                s.Thumbnail,
                Program = new
                {
                    s.Program?.ProgramID,
                    s.Program?.ProgramName,
                    s.Program?.Title,
                    s.Program?.SchoolChannel?.Name
                }
            });

            return Ok(new ApiResponse(true, "Schedules by date", result));
        }
    }
}
