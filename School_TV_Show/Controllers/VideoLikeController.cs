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
    public class VideoLikeController : ControllerBase
    {
        private readonly IVideoLikeService _videoLikeService;
        private readonly IVideoHistoryService _videoService;
        private readonly ILogger<VideoLikeController> _logger;
        private readonly IHubContext<LiveStreamHub> _hubContext;

        public VideoLikeController(
            IVideoLikeService videoLikeService,
            IVideoHistoryService videoService,
            ILogger<VideoLikeController> logger,
            IHubContext<LiveStreamHub> hubContext)
        {
            _videoLikeService = videoLikeService;
            _videoService = videoService;
            _logger = logger;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllVideoLikes()
        {
            try
            {
                var videoLikes = await _videoLikeService.GetAllVideoLikesAsync();
                return Ok(videoLikes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video likes");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActiveVideoLikes()
        {
            try
            {
                var videoLikes = await _videoLikeService.GetAllVideoLikesAsync();
                return Ok(videoLikes.Where(v => v.Quantity > 0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving positive video likes");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVideoLikeById(int id)
        {
            try
            {
                var videoLike = await _videoLikeService.GetVideoLikeByIdAsync(id);
                if (videoLike == null)
                    return NotFound("Video like not found");

                return Ok(videoLike);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video like by id");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpGet("total/{videoHistoryId}")]
        public async Task<IActionResult> GetTotalLikesForVideo(int videoHistoryId)
        {
            try
            {
                var totalLikes = await _videoLikeService.GetTotalLikesForVideoAsync(videoHistoryId);
                return Ok(new { TotalLikes = totalLikes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total likes for video");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpPost]
        public async Task<IActionResult> AddVideoLike([FromBody] CreateVideoLikeRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid account identifier.");

            var videoLike = new VideoLike
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
                var result = await _videoLikeService.AddVideoLikeAsync(videoLike);
                if (!result)
                    return BadRequest("Invalid VideoHistoryID or AccountID.");

                var totalLikes = await _videoLikeService.GetTotalLikesForVideoAsync(video.VideoHistoryID);
                await _hubContext.Clients.Group(video.CloudflareStreamId)
                    .SendAsync("LikeUpdated", new { videoId = video.VideoHistoryID, totalLikes });

                return CreatedAtAction(nameof(GetVideoLikeById), new { id = videoLike.LikeID }, videoLike);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating video like");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVideoLike(int id, [FromBody] UpdateVideoLikeRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var videoLike = await _videoLikeService.GetVideoLikeByIdAsync(id);
            if (videoLike == null)
                return NotFound("Video like not found");

            videoLike.Quantity = request.Quantity;

            try
            {
                var result = await _videoLikeService.UpdateVideoLikeAsync(videoLike);
                if (!result)
                    return BadRequest("Invalid update operation");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating video like");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideoLike(int id)
        {
            try
            {
                var result = await _videoLikeService.DeleteVideoLikeAsync(id);
                if (!result)
                    return NotFound("Video like not found");

                return Ok(new { message = "Video like deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video like");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("totalLikes")]
        public async Task<IActionResult> GetTotalLikes()
        {
            var totalLikes = await _videoLikeService.GetTotalLikesAsync();
            return Ok(new { totalLikes });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("likesByVideo/{videoHistoryId}")]
        public async Task<IActionResult> GetLikesByVideo(int videoHistoryId)
        {
            var likes = await _videoLikeService.GetLikesByVideoIdAsync(videoHistoryId);
            return Ok(new { videoHistoryId, likes });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("likesPerVideo")]
        public async Task<IActionResult> GetLikesPerVideo()
        {
            var likesPerVideo = await _videoLikeService.GetLikesPerVideoAsync();
            return Ok(likesPerVideo);
        }
    }
}
