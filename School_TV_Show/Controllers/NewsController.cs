using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using School_TV_Show.DTO;
using Services;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly IFollowService _followService;

        public NewsController(INewsService newsService, IFollowService followService)
        {
            _newsService = newsService;
            _followService = followService;

        }

        [HttpPost("CreateNews")]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<IActionResult> CreateNews([FromForm] CreateNewsRequest request)
        {
            if (request == null || request.ImageFiles == null || request.ImageFiles.Count == 0)
            {
                return BadRequest("Request data or image files are missing.");
            }

            try
            {
                var accountClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (accountClaim == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                int accountId = int.Parse(accountClaim.Value);

                bool isOwner = await _newsService.ValidateSchoolChannelOwnershipAsync(request.SchoolChannelID, accountId);
                if (!isOwner)
                {
                    return Unauthorized("You do not have permission to create news for this school channel.");
                }

                var news = new News
                {
                    SchoolChannelID = request.SchoolChannelID,
                    CategoryNewsID = request.CategoryNewsID,
                    Title = request.Title,
                    Content = request.Content,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = true,
                    FollowerMode = request.FollowerMode,
                    NewsPictures = new List<NewsPicture>()
                };

                var newsId = await _newsService.CreateNewsAsync(news, request.ImageFiles);
                return Ok(new { Message = "News created successfully.", NewsId = newsId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error creating news.", Error = ex.Message });
            }
        }



        [HttpPut("UpdateNews/{id}")]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<IActionResult> UpdateNews(int id, [FromForm] UpdateNewsRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request data is missing.");
            }

            try
            {
                var accountClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (accountClaim == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                int accountId = int.Parse(accountClaim.Value);

                var existingNews = await _newsService.GetNewsByIdAsync(id);
                if (existingNews == null)
                {
                    return NotFound("News not found.");
                }

                if (!existingNews.Status)
                {
                    return BadRequest("Cannot update inactive news.");
                }

                bool isOwner = await _newsService.ValidateSchoolChannelOwnershipAsync(existingNews.SchoolChannelID, accountId);
                if (!isOwner)
                {
                    return Unauthorized("You do not have permission to update this news.");
                }

                var updatedNews = new News
                {
                    NewsID = id,
                    Title = request.Title ?? existingNews.Title,
                    Content = request.Content ?? existingNews.Content,
                    UpdatedAt = DateTime.UtcNow,
                    Status = existingNews.Status,
                    FollowerMode = request.FollowerMode ?? existingNews.FollowerMode,
                    CategoryNewsID = request.CategoryNewsID ?? existingNews.CategoryNewsID
                };

                int updatedNewsId = await _newsService.UpdateNewsAsync(updatedNews, request.ImageFiles);

                if (updatedNewsId == 0)
                {
                    return StatusCode(500, "Error updating news.");
                }

                return Ok(new { Message = "News updated successfully.", NewsId = updatedNewsId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating news.", Error = ex.Message });
            }
        }



        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<News>> GetNews(int id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null) return NotFound();
            return Ok(news);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SchoolOwner,Admin")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var result = await _newsService.DeleteNewsAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpGet("combined")]
        public async Task<ActionResult<IEnumerable<News>>> GetActiveNewsCombined()
        {
            var accountClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int? accountId = accountClaim != null ? int.Parse(accountClaim.Value) : (int?)null;

            var news = await _newsService.GetActiveNewsWithFollowCheckAndWithoutFollowingAsync(accountId);
            return Ok(news);
        }

        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpGet("school-channel/{schoolChannelId}")]
        public async Task<ActionResult<IEnumerable<News>>> GetSchoolChannelNews(int schoolChannelId)
        {
            var accountClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int? accountId = accountClaim != null ? int.Parse(accountClaim.Value) : (int?)null;

            // Check if the user follows this school channel
            bool isFollowing = false;
            if (accountId.HasValue)
            {
                isFollowing = await _followService.IsFollowingAsync(accountId.Value, schoolChannelId);
            }

            // Fetch the news based on follow status and follower mode
            var news = await _newsService.GetNewsBySchoolChannelAsync(schoolChannelId, accountId, isFollowing);

            return Ok(news);
        }


        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<News>>> GetAllNews()
        {
            var news = await _newsService.GetAllNewsNoFilterAsync();
            return Ok(news);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("statistics/daily-news")]
        public async Task<IActionResult> GetDailyNewsStatistics()
        {
            var stats = await _newsService.GetDailyNewsStatisticsAsync();
            return Ok(stats);
        }

    }
}
