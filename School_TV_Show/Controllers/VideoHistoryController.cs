using BOs.Models;
using Microsoft.AspNetCore.Authorization;
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
    public class VideoHistoryController : ControllerBase
    {
        private readonly IVideoHistoryService _videoService;
        private readonly ILogger<VideoHistoryController> _logger;
        private readonly CloudflareSettings _cloudflareSettings;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly IProgramFollowRepo _programFollowRepository;
        private readonly IFollowRepo _schoolChannelFollowRepository;
        private readonly IProgramService _programService;

        public VideoHistoryController(
            IVideoHistoryService videoService,
            IOptions<CloudflareSettings> cloudflareOptions,
            ILogger<VideoHistoryController> logger,
            IHubContext<NotificationHub> hubContext,
            INotificationService notificationService,
            IProgramFollowRepo programFollowRepository,
            IFollowRepo schoolChannelFollowRepository,
            IProgramService programService)
        {
            _videoService = videoService;
            _cloudflareSettings = cloudflareOptions.Value;
            _logger = logger;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _programFollowRepository = programFollowRepository;
            _schoolChannelFollowRepository = schoolChannelFollowRepository;
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
            var filteredVideos = videos.Where(video => video.Status);
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
        public async Task<IActionResult> UploadVideoToCloudflare([FromForm] UploadVideoHistoryRequest request)
        {
            if (request.VideoFile == null || request.VideoFile.Length == 0)
                return BadRequest(new { message = "No video file provided." });

            var videoHistory = new VideoHistory
            {
                ProgramID = request.ProgramID,
                Type = request.Type,
                Description = request.Description,
                Status = true,
                StreamAt = request.StreamAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _videoService.AddVideoWithCloudflareAsync(request.VideoFile, videoHistory);

            if (!result)
                return StatusCode(500, new { message = "Failed to upload video to Cloudflare." });

            var program = videoHistory.Program ?? await _programService.GetProgramByIdAsync(videoHistory.ProgramID ?? 0);
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
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.CreateNotificationAsync(notification);
                await _hubContext.Clients.User(accountId.ToString())
                    .SendAsync("ReceiveNotification", notification);
            }

            // === Auto create replay schedule if StreamAt & Duration exist ===
            if (videoHistory.Duration != null && videoHistory.StreamAt != null)
            {
                var endTime = videoHistory.StreamAt.Value.AddSeconds(videoHistory.Duration.Value);

                var schedule = new Schedule
                {
                    ProgramID = videoHistory.ProgramID ?? 0,
                    StartTime = videoHistory.StreamAt.Value,
                    EndTime = endTime,
                    IsReplay = true,
                    Status = "Pending",
                    VideoHistoryID = videoHistory.VideoHistoryID
                };

                using var scope = HttpContext.RequestServices.CreateScope();
                var scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
                await scheduleService.CreateScheduleAsync(schedule);
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
