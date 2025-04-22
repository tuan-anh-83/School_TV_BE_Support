using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SchoolOwner,Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IVideoViewService _viewService;
        private readonly IVideoLikeService _likeService;
        private readonly IShareService _shareService;
        private readonly ICommentService _commentService;
        private readonly IVideoHistoryService _videoHistoryService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IVideoViewService viewService,
            IVideoLikeService likeService,
            IShareService shareService,
            ICommentService commentService,
            IVideoHistoryService videoHistoryService,
            ILogger<DashboardController> logger)
        {
            _viewService = viewService;
            _likeService = likeService;
            _shareService = shareService;
            _commentService = commentService;
            _videoHistoryService = videoHistoryService;
            _logger = logger;
        }

        [HttpGet("totals")]
        public async Task<IActionResult> GetTotalCounts()
        {
            var totalViews = await _viewService.GetTotalViewsAsync();
            var totalLikes = await _likeService.GetTotalLikesAsync();
            var totalShares = await _shareService.GetTotalSharesAsync();
            var totalComments = await _commentService.GetAllCommentsAsync();
            var totalVideos = await _videoHistoryService.GetTotalVideosAsync();

            return Ok(new
            {
                Views = totalViews,
                Likes = totalLikes,
                Shares = totalShares,
                Comments = totalComments.Count,
                TotalVideos = totalVideos
            });
        }

        [HttpGet("per-video")]
        public async Task<IActionResult> GetStatsPerVideo()
        {
            var views = await _viewService.GetViewsPerVideoAsync();
            var likes = await _likeService.GetLikesPerVideoAsync();
            var shares = await _shareService.GetSharesPerVideoAsync();
            var allComments = await _commentService.GetAllCommentsAsync();

            var commentsGrouped = allComments
                .Where(c => c.Quantity > 0)
                .GroupBy(c => c.VideoHistoryID)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity));

            var allVideoIds = views.Keys
                .Union(likes.Keys)
                .Union(shares.Keys)
                .Union(commentsGrouped.Keys)
                .Distinct();

            var result = allVideoIds.Select(id => new
            {
                VideoHistoryID = id,
                Views = views.ContainsKey(id) ? views[id] : 0,
                Likes = likes.ContainsKey(id) ? likes[id] : 0,
                Shares = shares.ContainsKey(id) ? shares[id] : 0,
                Comments = commentsGrouped.ContainsKey(id) ? commentsGrouped[id] : 0
            });

            return Ok(result);
        }

        [HttpGet("summary-per-school")]
        public async Task<IActionResult> GetInteractionSummaryPerSchool()
        {
            try
            {
                var videoHistories = await _videoHistoryService.GetAllVideoHistoriesAsync();

                var summary = videoHistories
                    .Where(v => v.Status && v.Program != null && v.Program.SchoolChannel != null)
                    .GroupBy(v => v.Program.SchoolChannel.Name)
                    .Select(g => new
                    {
                        SchoolChannelName = g.Key,
                        TotalVideos = g.Count(),
                        TotalLikes = g.Sum(v => v.VideoLikes?.Sum(l => l.Quantity) ?? 0),
                        TotalViews = g.Sum(v => v.VideoViews?.Sum(vv => vv.Quantity) ?? 0),
                        TotalShares = g.Sum(v => v.Shares?.Sum(s => s.Quantity) ?? 0),
                        TotalComments = g.Sum(v => v.Comments?.Sum(c => c.Quantity) ?? 0)
                    });

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate school-wise video interaction summary.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
