using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using School_TV_Show.DTO;
using Services;

namespace School_TV_Show.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ILogger<AnalyticsController> _logger;
        private readonly IVideoViewService _videoViewService;
        private readonly IVideoHistoryService _videoHistoryService;
        private readonly ISchoolChannelFollowService _schoolChannelFollowService;
        private TimeZoneInfo vietnameseTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public AnalyticsController(
            ILogger<AnalyticsController> logger,
            IVideoViewService videoViewService,
            IVideoHistoryService videoHistoryService,
            ISchoolChannelFollowService schoolChannelFollowService
        )
        {
            _logger = logger;
            _videoViewService = videoViewService;
            _videoHistoryService = videoHistoryService;
            _schoolChannelFollowService = schoolChannelFollowService;
        }

        [Authorize(Roles = "SchoolOwner")]
        [HttpGet("analys-by-channel")]
        public async Task<IActionResult> AnalysByChannel([FromQuery] int channelId, [FromQuery] string dateRange, [FromQuery] string? customRange)
        {
            try
            {
                // Parse date range
                DateTimeOffset startDate, endDate;
                ParseDateRange(dateRange, customRange, out startDate, out endDate);

                // Get analytics data
                var totalViews = await _videoViewService.GetTotalViewsByChannelAsync(channelId, startDate, endDate);
                var viewsComparisonPercent = await _videoViewService.GetViewsComparisonPercentAsync(channelId, startDate, endDate);

                var watchTimeHours = await _videoHistoryService.GetTotalWatchTimeByChannelAsync(channelId, startDate, endDate);
                var watchTimeComparisonPercent = await _videoHistoryService.GetWatchTimeComparisonPercentAsync(channelId, startDate, endDate);

                var newFollowers = await _schoolChannelFollowService.GetNewFollowersByChannelAsync(channelId, startDate, endDate);
                var followersComparisonPercent = await _schoolChannelFollowService.GetFollowersComparisonPercentAsync(channelId, startDate, endDate);

                var result = new ChannelAnalyticsDTO
                {
                    TotalViews = totalViews,
                    ViewsComparisonPercent = viewsComparisonPercent,
                    WatchTimeHours = watchTimeHours,
                    WatchTimeComparisonPercent = watchTimeComparisonPercent,
                    NewFollowers = newFollowers,
                    FollowersComparisonPercent = followersComparisonPercent,
                    DateRange = dateRange,
                    StartDate = startDate,
                    EndDate = endDate
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting channel analytics for channelId {ChannelId}", channelId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        private void ParseDateRange(string dateRange, string? customRange, out DateTimeOffset startDate, out DateTimeOffset endDate)
        {
            endDate = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, vietnameseTimeZone);

            switch (dateRange.ToLower())
            {
                case "7":
                    startDate = endDate.AddDays(-7);
                    break;
                case "30":
                    startDate = endDate.AddDays(-30);
                    break;
                case "90":
                    startDate = endDate.AddDays(-90);
                    break;
                case "custom":
                    if (string.IsNullOrEmpty(customRange))
                    {
                        startDate = endDate.AddDays(-30); // Default to 30 days if no custom range
                        break;
                    }

                    var dateParts = customRange.Split(',');
                    if (dateParts.Length == 2 &&
                        DateTimeOffset.TryParse(dateParts[0], out var customStartDate) &&
                        DateTimeOffset.TryParse(dateParts[1], out var customEndDate))
                    {
                        startDate = customStartDate;
                        endDate = customEndDate;
                    }
                    else
                    {
                        startDate = endDate.AddDays(-30); // Default to 30 days if invalid custom range
                    }
                    break;
                default:
                    startDate = endDate.AddDays(-30); // Default to 30 days
                    break;
            }
        }
    }
}
