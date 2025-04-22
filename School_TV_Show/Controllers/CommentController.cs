using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using School_TV_Show.DTO;
using Services;
using Services.Hubs;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IVideoHistoryService _videoHistoryService;
        private readonly IHubContext<LiveStreamHub> _hubContext;
        private readonly ILogger<CommentController> _logger;

        public CommentController(
            ICommentService commentService,
            IVideoHistoryService videoHistoryService,
            IHubContext<LiveStreamHub> hubContext,
            ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _videoHistoryService = videoHistoryService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllComments()
        {
            try
            {
                var comments = await _commentService.GetAllCommentsAsync();
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActiveComments()
        {
            try
            {
                var comments = await _commentService.GetAllActiveCommentsAsync();
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all comments (including deleted)");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "SchoolOwner,Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                if (comment == null)
                    return NotFound("Comment not found");

                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment by id");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpGet("video/{videoHistoryId}")]
        public async Task<IActionResult> GetCommentsByVideoHistoryId(int videoHistoryId)
        {
            try
            {
                var comments = await _commentService.GetCommentsWithAccountByVideoHistoryIdAsync(videoHistoryId);
                var result = comments.Select(c => new
                {
                    CommentID = c.CommentID,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for video");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpGet("video/{videoHistoryId}/total")]
        public async Task<IActionResult> GetTotalCommentsForVideo(int videoHistoryId)
        {
            try
            {
                int total = await _commentService.GetTotalCommentsForVideoAsync(videoHistoryId);
                return Ok(new { videoHistoryId, totalComments = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total comment quantity for video");
                return StatusCode(500, "Internal server error");
            }
        }
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid account identifier.");

            var video = await _videoHistoryService.GetVideoByIdAsync(request.VideoHistoryID);
            if (video == null || !video.Status)
                return BadRequest("The specified video does not exist or is not active.");

            var comment = new Comment
            {
                VideoHistoryID = request.VideoHistoryID,
                AccountID = accountId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                Quantity = 1,
                Status = "Active"
            };

            try
            {
                var saved = await _commentService.AddCommentAsync(comment);
                if (!saved)
                    return BadRequest("Failed to save comment.");

                await _hubContext.Clients.Group(video.CloudflareStreamId).SendAsync("NewComment", new
                {
                    videoId = video.VideoHistoryID,
                    commentId = comment.CommentID,
                    content = comment.Content,
                    userId = comment.AccountID,
                    createdAt = comment.CreatedAt
                });

                return CreatedAtAction(nameof(GetCommentById), new { id = comment.CommentID }, comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, "Internal server error");
            }
        }


        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors });
            }

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
                return NotFound("Comment not found");

            if (comment.Quantity <= 0)
                return BadRequest("Comment cannot be updated because it has been deleted.");

            comment.Content = request.Content;

            try
            {
                var result = await _commentService.UpdateCommentAsync(comment);
                if (!result)
                    return BadRequest("Invalid VideoHistoryID or AccountID.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var result = await _commentService.DeleteCommentAsync(id);
                if (!result)
                    return NotFound("Comment not found");

                return Ok(new { message = "Comment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
