using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Repos;
using School_TV_Show.DTO;
using Services;
using Services.Hubs;


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

        public ScheduleController(
            IScheduleService scheduleService,
            IVideoHistoryService videoHistoryService,
            IProgramService programService,
            IHubContext<NotificationHub> hubContext,
            IProgramFollowRepo programFollowRepository,
            ISchoolChannelFollowRepo schoolChannelFollowRepository,
            INotificationService notificationService,
            IOptions<CloudflareSettings> cloudflareOptions)
        {
            _scheduleService = scheduleService;
            _videoHistoryService = videoHistoryService;
            _programService = programService;
            _hubContext = hubContext;
            _programFollowRepository = programFollowRepository;
            _schoolChannelFollowRepository = schoolChannelFollowRepository;
            _notificationService = notificationService;
            _cloudflareSettings = cloudflareOptions.Value;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input", ModelState));

            var program = await _programService.GetProgramByIdAsync(request.ProgramID);
            if (program == null)
                return NotFound(new ApiResponse(false, "Program not found"));

            bool isOverlap = await _scheduleService.IsScheduleOverlappingAsync(program.SchoolChannelID, request.StartTime, request.EndTime);
            if (isOverlap)
                return Conflict(new ApiResponse(false, "Schedule time overlaps with another program on the same school channel."));

            var schedule = new Schedule
            {
                ProgramID = request.ProgramID,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsReplay = request.IsReplay,
                Status = "Pending"
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
        public async Task<IActionResult> CreateReplayScheduleFromVideo([FromBody] CreateReplayScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input"));

            var video = await _videoHistoryService.GetVideoByIdAsync(request.VideoHistoryId);
            if (video == null || video.ProgramID == null)
                return NotFound(new ApiResponse(false, "Video not found or missing ProgramID"));

            var schedule = new Schedule
            {
                ProgramID = video.ProgramID.Value,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = "Ready",
                IsReplay = true,
                VideoHistoryID = video.VideoHistoryID
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
                endTime = created.EndTime
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
                s.LiveStreamEnded
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
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input"));

            var existingSchedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (existingSchedule == null)
                return NotFound(new ApiResponse(false, "Schedule not found"));

            if (!existingSchedule.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ApiResponse(false, "Only 'Pending' schedules can be updated"));

            existingSchedule.StartTime = request.StartTime;
            existingSchedule.EndTime = request.EndTime;

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
                string? iframeUrl = null;
                string? playbackUrl = null;
                string? mp4Url = null;
                double? duration = null;
                string? description = null;
                int? videoHistoryId = null;

                if (schedule.IsReplay)
                {
                    VideoHistory? video = null;
                    if (schedule.VideoHistoryID.HasValue)
                    {
                        video = await _videoHistoryService.GetVideoByIdAsync(schedule.VideoHistoryID.Value);
                    }
                    else
                    {
                        video = await _videoHistoryService.GetReplayVideoByProgramAndTimeAsync(
                            schedule.ProgramID, schedule.StartTime, schedule.EndTime);
                    }

                    if (video != null)
                    {
                        iframeUrl = !string.IsNullOrEmpty(video.CloudflareStreamId)
                            ? $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{video.CloudflareStreamId}/iframe"
                            : null;

                        playbackUrl = video.PlaybackUrl;
                        mp4Url = video.MP4Url;
                        duration = video.Duration;
                        description = video.Description;
                        videoHistoryId = video.VideoHistoryID;
                    }
                }
                else
                {
                    if (schedule.LiveStreamEnded && schedule.VideoHistoryID.HasValue)
                    {
                        var video = await _videoHistoryService.GetVideoByIdAsync(schedule.VideoHistoryID.Value);
                        if (video != null && !string.IsNullOrEmpty(video.CloudflareStreamId))
                        {
                            iframeUrl = $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{video.CloudflareStreamId}/iframe";
                            playbackUrl = video.PlaybackUrl;
                            mp4Url = video.MP4Url;
                            duration = video.Duration;
                            description = video.Description;
                            videoHistoryId = video.VideoHistoryID;
                        }
                    }
                    else
                    {
                        iframeUrl = !string.IsNullOrEmpty(schedule.Program?.CloudflareStreamId)
                            ? $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{schedule.Program.CloudflareStreamId}/iframe"
                            : null;
                    }
                }

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
                    videoHistoryIdFromSchedule = schedule.VideoHistoryID,
                    videoHistoryId,
                    iframeUrl,
                    duration,
                    description,
                    mp4Url,
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
