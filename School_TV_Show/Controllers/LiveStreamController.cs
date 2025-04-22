using Microsoft.AspNetCore.Mvc;

using Services;
using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using School_TV_Show.DTO;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/livestreams")]
    public class LiveStreamController : ControllerBase
    {
        private readonly ILiveStreamService _liveStreamService;
        private readonly IVideoHistoryService _videoHistoryService;
        private readonly ILogger<LiveStreamController> _logger;

        public LiveStreamController(
            ILiveStreamService liveStreamService,
            IVideoHistoryService videoHistoryService,
            ILogger<LiveStreamController> logger)
        {
            _liveStreamService = liveStreamService;
            _videoHistoryService = videoHistoryService;
            _logger = logger;
        }


        [HttpPost("start")]
        public async Task<IActionResult> StartLiveStream([FromBody] LiveStreamStartRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stream = new VideoHistory
            {
                ProgramID = request.ProgramID,
                Description = request.Description,
                Status = true,
                Type = "Live",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                StreamAt = DateTime.UtcNow
            };

            var success = await _liveStreamService.StartLiveStreamAsync(stream);

            if (!success)
                return StatusCode(500, "Failed to start live stream.");

            return Ok(new
            {
                message = "Live stream started successfully.",
                streamId = stream.VideoHistoryID,
                stream.URL,
                stream.PlaybackUrl
            });
        }
        [HttpPost("finalize/{id}")]
        public async Task<IActionResult> FinalizeStreamAndGetLinks(int id)
        {
            var stream = await _liveStreamService.GetLiveStreamByIdAsync(id);
            if (stream == null || !stream.Status)
                return NotFound("Live stream not found or already ended.");

            var result = await _liveStreamService.EndStreamAndReturnLinksAsync(stream);
            if (!result)
                return StatusCode(500, "Failed to finalize and fetch stream links.");

            return Ok(new
            {
                message = "Stream finalized successfully.",
                playbackUrl = stream.PlaybackUrl,
                mp4Url = stream.MP4Url
            });
        }

        [HttpPost("disable-input/{id}")]
        public async Task<IActionResult> DisableStreamInput(int id)
        {
            var stream = await _liveStreamService.GetLiveStreamByIdAsync(id);
            if (stream == null || !stream.Status)
                return NotFound("Live stream not found or already ended.");

            var result = await _liveStreamService.EndLiveStreamAsync(stream);
            if (!result)
                return StatusCode(500, "Failed to disable live stream input.");

            return Ok(new
            {
                message = "Live stream input disabled. Cloudflare input removed.",
                streamId = stream.VideoHistoryID
            });
        }
        [HttpPost("{videoHistoryId}/end")]
        [Authorize(Roles = "Admin,SchoolOwner")]
        public async Task<IActionResult> EndLivestreamNow(int videoHistoryId)
        {
            var video = await _videoHistoryService.GetVideoByIdAsync(videoHistoryId);
            if (video == null || !video.Status)
                return NotFound(new { message = "Live video not found or already ended." });

            var success = await _liveStreamService.EndStreamAndReturnLinksAsync(video);

            if (!success)
                return StatusCode(500, new { message = "Failed to end livestream or save recorded video." });

            return Ok(new
            {
                message = "Livestream ended and video saved successfully.",
                video.VideoHistoryID,
                video.MP4Url,
                video.PlaybackUrl
            });
        }


        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveLiveStreams()
        {
            var streams = await _liveStreamService.GetActiveLiveStreamsAsync();
            var response = streams.Select(s => new
            {
                s.VideoHistoryID,
                s.Description,
                s.URL,
                s.PlaybackUrl,
                s.Type,
                s.StreamAt
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLiveStreamById(int id)
        {
            var stream = await _liveStreamService.GetLiveStreamByIdAsync(id);
            if (stream == null)
                return NotFound("Live stream not found.");

            return Ok(new
            {
                stream.VideoHistoryID,
                stream.Description,
                stream.URL,
                stream.PlaybackUrl,
                stream.Type,
                stream.StreamAt
            });
        }

        [HttpPost("{id}/like")]
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        public async Task<IActionResult> LikeStream(int id)
        {
            var userId = GetUserIdFromToken();

            var like = new VideoLike
            {
                VideoHistoryID = id,
                AccountID = userId,
                Quantity = 1
            };

            var result = await _liveStreamService.AddLikeAsync(like);
            return result ? Ok(new { message = "Like recorded successfully." }) : StatusCode(500, "Failed to record like.");
        }

        [HttpPost("{id}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> ViewStream(int id)
        {
            var view = new VideoView
            {
                VideoHistoryID = id,
                Quantity = 1
            };

            var result = await _liveStreamService.AddViewAsync(view);
            return result ? Ok(new { message = "View recorded successfully." }) : StatusCode(500, "Failed to record view.");
        }

        [HttpPost("{id}/share")]
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        public async Task<IActionResult> ShareStream(int id)
        {
            var userId = GetUserIdFromToken();

            var share = new Share
            {
                VideoHistoryID = id,
                AccountID = userId,
                Quantity = 1
            };

            var result = await _liveStreamService.AddShareAsync(share);
            return result ? Ok(new { message = "Share recorded successfully." }) : StatusCode(500, "Failed to record share.");
        }

        private int GetUserIdFromToken()
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return accountIdClaim != null ? int.Parse(accountIdClaim.Value) : 0;
        }
    }
}
