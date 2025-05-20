using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
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
    public class VideoHistoryController : ControllerBase
    {
        private readonly IVideoHistoryService _videoService;
        private readonly ILogger<VideoHistoryController> _logger;
        private readonly CloudflareSettings _cloudflareSettings;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly IProgramFollowRepo _programFollowRepository;
        private readonly IFollowRepo _schoolChannelFollowRepository;
        private readonly IScheduleService _scheduleService;
        private readonly IProgramService _programService;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public VideoHistoryController(
            IVideoHistoryService videoService,
            IOptions<CloudflareSettings> cloudflareOptions,
            ILogger<VideoHistoryController> logger,
            IHubContext<NotificationHub> hubContext,
            INotificationService notificationService,
            IProgramFollowRepo programFollowRepository,
            IFollowRepo schoolChannelFollowRepository,
            IScheduleService scheduleService,
            IProgramService programService)
        {
            _videoService = videoService;
            _cloudflareSettings = cloudflareOptions.Value;
            _logger = logger;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _programFollowRepository = programFollowRepository;
            _schoolChannelFollowRepository = schoolChannelFollowRepository;
            _scheduleService = scheduleService;
            _programService = programService;
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllVideos()
        {
            var videos = await _videoService.GetAllVideosAsync();
            return Ok(videos);
        }
        [HttpGet("program/{programId}/videos")]
        [Authorize(Roles = "SchoolOwner,Admin")]
        public async Task<IActionResult> GetVideosByProgramId(int programId)
        {
            var videos = await _videoService.GetVideosByProgramIdAsync(programId);
            var result = videos.Select(v => new
            {
                v.VideoHistoryID,
                v.Description,
                v.Type,
                v.URL,
                v.PlaybackUrl,
                v.MP4Url,
                v.Duration,
                v.CreatedAt,
                v.Status,
                v.ProgramID
            });

            return Ok(result);
        }

        [HttpGet("active")]
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        public async Task<IActionResult> GetAllActiveVideos()
        {
            var videos = await _videoService.GetAllVideosAsync();
            var filteredVideos = videos.Where(video => video.Type == "Recorded");
            return Ok(filteredVideos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetVideoById(int id)
        {
            var video = await _videoService.GetVideoByIdAsync(id);
            if (video == null)
                return NotFound(new { message = "Video not found" });

            return Ok(video);
        }

        [HttpPost("UploadCloudflare")]
        [Authorize(Roles = "SchoolOwner")]
        [RequestSizeLimit(1_000_000_000)]
        public async Task<IActionResult> UploadVideoToCloudflare([FromForm] UploadVideoHistoryRequest request)
        {
            DateTime streamAt = DateTime.ParseExact(request.StreamAt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (request.VideoFile == null || request.VideoFile.Length == 0)
                return BadRequest(new { message = "No video file provided." });

            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { message });
            }

            if(await _scheduleService.CheckIsInSchedule(streamAt))
            {
                return BadRequest(new { message = "Đã có lịch phát trong thời gian này." });
            }

            var program = new BOs.Models.Program
            {
                SchoolChannelID = request.SchoolChannelId,
                ProgramName = request.ProgramName,
                Title = request.ProgramTitle,
                Status = "Active",
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
            };

            var programResult = await _programService.CreateProgramAsync(program);

            if (programResult == null)
                return StatusCode(500, new { message = "Failed to create program." });

            var videoHistory = new VideoHistory
            {
                ProgramID = request.ProgramID == null ? program.ProgramID : request.ProgramID,
                Type = request.Type,
                Description = request.Description,
                Status = true,
                StreamAt = streamAt,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            var result = await _videoService.AddVideoWithCloudflareAsync(request.VideoFile, videoHistory);

            if (result == null)
                return StatusCode(500, new { message = "Failed to upload video to Cloudflare." });

            int schoolChannelId = program.SchoolChannelID;

            var programFollowers = await _programFollowRepository.GetFollowersByProgramIdAsync(program.ProgramID);
            var channelFollowers = await _schoolChannelFollowRepository.GetFollowersByChannelIdAsync(schoolChannelId);

            var allFollowerIds = programFollowers.Select(f => f.AccountID)
                .Concat(channelFollowers.Select(f => f.AccountID))
                .Distinct();

            foreach (var accountId in allFollowerIds)
            {
                var notification = new Notification
                {
                    ProgramID = program.ProgramID,
                    SchoolChannelID = schoolChannelId,
                    AccountID = accountId,
                    Title = "New Video Uploaded",
                    Message = $"A new video has been uploaded to {program.ProgramName}.",
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
                };

                await _notificationService.CreateNotificationAsync(notification);
                await _hubContext.Clients.User(accountId.ToString())
                    .SendAsync("ReceiveNotification", notification);
            }

            // === Auto create replay schedule if StreamAt & Duration exist ===
            if (result.Duration != null && result.StreamAt != null)
            {
                var endTime = videoHistory.StreamAt.Value.AddSeconds(result.Duration.Value);

                var schedule = new Schedule
                {
                    ProgramID = program.ProgramID,
                    StartTime = result.StreamAt.Value,
                    EndTime = endTime,
                    IsReplay = true,
                    Status = "Pending",
                    Thumbnail = string.IsNullOrEmpty(result.CloudflareStreamId) ? "https://www.keytechinc.com/wp-content/uploads/2022/01/video-thumbnail.jpg" : $"https://videodelivery.net/{result.CloudflareStreamId}/thumbnails/thumbnail.jpg",
                    VideoHistoryID = result.VideoHistoryID
                };

                await _scheduleService.CreateScheduleAsync(schedule);
            }

            return Ok(new
            {
                message = "Video uploaded successfully.",
                data = new
                {
                    videoId = videoHistory.VideoHistoryID,
                    programId = videoHistory.ProgramID,
                    playbackUrl = videoHistory.PlaybackUrl,
                    mp4Url = videoHistory.MP4Url,
                    iframeUrl = $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{videoHistory.CloudflareStreamId}/iframe",
                    duration = videoHistory.Duration
                }
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<IActionResult> UpdateVideo(int id, [FromBody] UpdateVideoHistoryRequestDTO request)
        {
            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { message });
            }

            var videoHistory = await _videoService.GetVideoByIdAsync(id);
            if (videoHistory == null)
                return NotFound(new { message = "Video history not found" });

            videoHistory.URL = request.URL;
            videoHistory.Type = request.Type;
            videoHistory.Description = request.Description;
            videoHistory.ProgramID = request.ProgramID;
            videoHistory.UpdatedAt = DateTime.UtcNow;

            var result = await _videoService.UpdateVideoAsync(videoHistory);
            if (!result)
                return StatusCode(500, "Error updating video");

            return Ok(new { message = "Video updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SchoolOwner,Admin")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var result = await _videoService.DeleteVideoAsync(id);
            if (!result)
                return NotFound(new { message = "Video not found" });

            return Ok(new { message = "Video deleted successfully" });
        }

        [HttpGet("{id}/playback")]
        public async Task<IActionResult> GetVideoPlaybackUrl(int id)
        {
            var video = await _videoService.GetVideoByIdAsync(id);
            if (video == null || string.IsNullOrEmpty(video.PlaybackUrl))
                return NotFound(new { message = "Playback URL not found." });

            return Ok(new
            {
                video.VideoHistoryID,
                video.PlaybackUrl
            });
        }

        [HttpGet("program/{programId}/latest-live")]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<IActionResult> GetLatestLiveByProgramId(int programId)
        {
            var video = await _videoService.GetLatestLiveStreamByProgramIdAsync(programId);
            if (video == null || string.IsNullOrEmpty(video.URL))
                return NotFound(new { message = "No active livestream found for this program." });

            return Ok(new
            {
                video.VideoHistoryID,
                video.URL,
                video.PlaybackUrl,
                video.Type,
                video.Status,
                video.CreatedAt
            });
        }

        [HttpGet("by-date")]
        public async Task<IActionResult> GetVideosByDate([FromQuery] DateTime date)
        {
            var videos = await _videoService.GetVideosByDateAsync(date);

            var result = videos.Select(v => new
            {
                v.VideoHistoryID,
                v.Description,
                v.Type,
                v.URL,
                v.PlaybackUrl,
                v.MP4Url,
                v.Duration,
                CreatedAt = v.CreatedAt.ToString("HH:mm:ss"),
                v.Program?.ProgramName,
                v.ProgramID
            });

            return Ok(result);
        }
    }
}
