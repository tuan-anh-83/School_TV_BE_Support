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
    public class ShareController : ControllerBase
    {
        private readonly IShareService _shareService;
        private readonly IVideoHistoryService _videoService;
        private readonly ILogger<ShareController> _logger;
        private readonly IHubContext<LiveStreamHub> _hubContext;

        public ShareController(
            IShareService shareService,
            IVideoHistoryService videoService,
            ILogger<ShareController> logger,
            IHubContext<LiveStreamHub> hubContext)
        {
            _shareService = shareService;
            _videoService = videoService;
            _logger = logger;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllShares()
        {
            try
            {
                var shares = await _shareService.GetAllSharesAsync();
                return Ok(shares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shares");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActiveShares()
        {
            try
            {
                var shares = await _shareService.GetAllActiveSharesAsync();
                return Ok(shares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all shares (including those with Quantity = 0)");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShareById(int id)
        {
            try
            {
                var share = await _shareService.GetShareByIdAsync(id);
                if (share == null)
                    return NotFound("Share not found");

                return Ok(share);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving share by id");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpGet("total/{videoHistoryId}")]
        public async Task<IActionResult> GetTotalSharesForVideo(int videoHistoryId)
        {
            try
            {
                var totalShares = await _shareService.GetTotalSharesForVideoAsync(videoHistoryId);
                return Ok(new { TotalShares = totalShares });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total shares for video");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpPost]
        public async Task<IActionResult> AddShare([FromBody] CreateShareRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid user.");

            var share = new Share
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
                var result = await _shareService.AddShareAsync(share);
                if (!result)
                    return BadRequest("Invalid VideoHistoryID or AccountID.");

                var totalShares = await _shareService.GetTotalSharesForVideoAsync(video.VideoHistoryID);
                await _hubContext.Clients.Group(video.CloudflareStreamId)
                    .SendAsync("ShareUpdated", new { videoId = video.VideoHistoryID, totalShares });

                return CreatedAtAction(nameof(GetShareById), new { id = share.ShareID }, share);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating share");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShare(int id, [FromBody] UpdateShareRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var share = await _shareService.GetShareByIdAsync(id);
            if (share == null)
                return NotFound("Share not found");

            if (share.Quantity <= 0)
                return BadRequest("Shares with zero or negative quantity cannot be updated.");

            try
            {
                var result = await _shareService.UpdateShareAsync(share);
                if (!result)
                    return BadRequest("Invalid VideoHistoryID or AccountID.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating share");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShare(int id)
        {
            try
            {
                var result = await _shareService.DeleteShareAsync(id);
                if (!result)
                    return NotFound("Share not found");

                return Ok(new { message = "Share deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting share");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("total-shares")]
        public async Task<IActionResult> GetTotalShares()
        {
            try
            {
                int totalShares = await _shareService.GetTotalSharesAsync();
                return Ok(new { TotalShares = totalShares });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total shares.");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("shares-per-video")]
        public async Task<IActionResult> GetSharesPerVideo()
        {
            try
            {
                Dictionary<int, int> sharesPerVideo = await _shareService.GetSharesPerVideoAsync();
                return Ok(sharesPerVideo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shares per video.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
