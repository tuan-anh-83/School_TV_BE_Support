using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.Hubs;
using Services;
using System.Security.Claims;
using School_TV_Show.DTO;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoViewController : ControllerBase
    {
        private readonly IVideoViewService _videoViewService;
        private readonly IVideoHistoryService _videoService;
        private readonly ILogger<VideoViewController> _logger;
        private readonly IHubContext<LiveStreamHub> _hubContext;

        public VideoViewController(
            IVideoViewService videoViewService,
            IVideoHistoryService videoService,
            ILogger<VideoViewController> logger,
            IHubContext<LiveStreamHub> hubContext)
        {
            _videoViewService = videoViewService;
            _videoService = videoService;
            _logger = logger;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllVideoViews()
        {
            try
            {
                var videoViews = await _videoViewService.GetAllVideoViewsAsync();
                return Ok(videoViews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video views");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActiveVideoViews()
        {
            try
            {
                var videoLikes = await _videoViewService.GetAllVideoViewsAsync();
                var positiveVideoLikes = videoLikes.Where(v => v.Quantity > 0);
                return Ok(positiveVideoLikes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving positive video likes");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVideoViewById(int id)
        {
            try
            {
                var videoView = await _videoViewService.GetVideoViewByIdAsync(id);
                if (videoView == null)
                    return NotFound("Video view not found");

                return Ok(videoView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video view by id");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpGet("total/{videoHistoryId}")]
        public async Task<IActionResult> GetTotalViewsForVideo(int videoHistoryId)
        {
            try
            {
                var totalViews = await _videoViewService.GetTotalViewsForVideoAsync(videoHistoryId);
                return Ok(new { TotalViews = totalViews });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total views for video");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpPost]
        public async Task<IActionResult> AddVideoView([FromBody] CreateVideoViewRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized();

            var videoView = new VideoView
            {
                VideoHistoryID = request.VideoHistoryID,
                AccountID = accountId,
                Quantity = 1
            };


            var video = await _videoService.GetVideoByIdAsync(request.VideoHistoryID);
            if (video == null || !video.Status)
                return BadRequest("The specified video does not exist or is not active.");

            try
            {
                var result = await _videoViewService.AddVideoViewAsync(videoView);
                if (!result)
                    return BadRequest("Invalid VideoHistoryID.");

                var totalViews = await _videoViewService.GetTotalViewsForVideoAsync(video.VideoHistoryID);
                await _hubContext.Clients.Group(video.CloudflareStreamId)
                    .SendAsync("ViewerCountUpdated", video.CloudflareStreamId, totalViews);

                return CreatedAtAction(nameof(GetVideoViewById), new { id = videoView.ViewID }, videoView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating video view");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVideoView(int id, [FromBody] UpdateVideoViewRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var videoView = await _videoViewService.GetVideoViewByIdAsync(id);
            if (videoView == null)
                return NotFound("Video view not found");

            videoView.Quantity = request.Quantity;

            try
            {
                var result = await _videoViewService.UpdateVideoViewAsync(videoView);
                if (!result)
                    return BadRequest("Invalid VideoHistoryID.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating video view");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideoView(int id)
        {
            try
            {
                var result = await _videoViewService.DeleteVideoViewAsync(id);
                if (!result)
                    return NotFound("Video view not found");

                return Ok(new { message = "Video view deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video view");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("total-views")]
        public async Task<IActionResult> GetTotalViews()
        {
            try
            {
                int totalViews = await _videoViewService.GetTotalViewsAsync();
                return Ok(new { TotalViews = totalViews });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total views for dashboard.");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("views-per-video")]
        public async Task<IActionResult> GetViewsPerVideo()
        {
            try
            {
                Dictionary<int, int> viewsPerVideo = await _videoViewService.GetViewsPerVideoAsync();
                return Ok(viewsPerVideo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving views per video for dashboard.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
