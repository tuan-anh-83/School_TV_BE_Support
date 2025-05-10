using Azure.Core;
using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using School_TV_Show.DTO;
using School_TV_Show.Helpers;
using Services;
using Services.Hubs;
using System.Globalization;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdScheduleController : ControllerBase
    {
        private readonly IAdScheduleService _service;
        private readonly IHubContext<LiveStreamHub> _hubContext;
        private readonly IPackageService _packageService;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public AdScheduleController(
            IAdScheduleService service, 
            IHubContext<LiveStreamHub> hubContext,
            IPackageService packageService
        )
        {
            _service = service;
            _hubContext = hubContext;
            _packageService = packageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ads = await _service.GetAllAdSchedulesAsync();

            var response = ads.Select(ad => new AdScheduleResponseDTO
            {
                AdScheduleID = ad.AdScheduleID,
                Title = ad.Title,
                StartTime = ad.StartTime,
                EndTime = ad.EndTime,
                VideoUrl = ad.VideoUrl,
                CreatedAt = ad.CreatedAt
            });

            return Ok(new ApiResponse(true, "List of ad schedules", response));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("valid")]
        public async Task<IActionResult> GetValidAdSchedules([FromQuery] string start, [FromQuery] string end)
        {
            DateTime startDate = DateTime.ParseExact(start, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(end, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var ads = await _service.GetListAdsInRangeAsync(startDate, endDate);

            var response = ads.Select(ad => new AdScheduleResponseDTO
            {
                AdScheduleID = ad.AdScheduleID,
                Title = ad.Title,
                StartTime = ad.StartTime,
                EndTime = ad.EndTime,
                VideoUrl = ad.VideoUrl,
                CreatedAt = ad.CreatedAt
            });

            return Ok(new ApiResponse(true, "List of ad schedules", response));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetAdScheduleByIdAsync(id);
            if (result == null)
                return NotFound(new ApiResponse(false, "Ad schedule not found"));

            return Ok(new ApiResponse(true, "Ad schedule found", result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdScheduleRequestDTO request)
        {
            DateTime startDate = DateTime.ParseExact(request.StartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(request.EndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var ad = new AdSchedule
            {
                Title = request.Title,
                StartTime = startDate,
                EndTime = endDate,
                VideoUrl = request.VideoUrl,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
            };

            var success = await _service.CreateAdScheduleAsync(ad);
            if (!success)
                return StatusCode(500, new ApiResponse(false, "Failed to create ad schedule"));

/*            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var today = now.Date;
            var tomorrow = today.AddDays(1);

            var ads = await _service.GetAdsToday(today, tomorrow);
            await _hubContext.Clients.All.SendAsync("Ads", ads);*/

            return Ok(new ApiResponse(true, "Ad schedule created successfully"));
        }

        [Authorize(Roles = "Advertiser")]
        [HttpPost("ads")]
        public async Task<IActionResult> CreateAds([FromBody] CreateAdScheduleRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input", ModelState));

            DateTime startDate = DateTime.ParseExact(request.StartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(request.EndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { error = message });
            }

            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid account");

            var currentPackage = await _packageService.GetCurrentPackageAndDurationByAccountIdAsync(accountId);

            if (currentPackage == null)
                return NotFound(new { error = "No active package found." });

            var (package, remainingDuration, expiredAt) = currentPackage.Value;

            if (remainingDuration == null || remainingDuration <= 0 || expiredAt < TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone))
                return BadRequest(new { error = "Your package was expired." });

            var ad = new AdSchedule
            {
                Title = request.Title,
                StartTime = startDate,
                EndTime = endDate,
                VideoUrl = request.VideoUrl,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            var success = await _service.CreateAdScheduleAsync(ad);
            if (!success)
                return StatusCode(500, new ApiResponse(false, "Failed to create ad schedule"));

/*            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var today = now.Date;
            var tomorrow = today.AddDays(1);

            var ads = await _service.GetAdsToday(today, tomorrow);
            await _hubContext.Clients.All.SendAsync("Ads", ads);*/

            return Ok(new ApiResponse(true, "Ad schedule created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAdScheduleRequestDTO request)
        {
            var existing = await _service.GetAdScheduleByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse(false, "Ad schedule not found"));

            existing.Title = request.Title;
            existing.StartTime = request.StartTime;
            existing.EndTime = request.EndTime;
            existing.VideoUrl = request.VideoUrl;

            var success = await _service.UpdateAdScheduleAsync(existing);
            if (!success)
                return StatusCode(500, new ApiResponse(false, "Failed to update ad schedule"));

            return Ok(new ApiResponse(true, "Ad schedule updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAdScheduleAsync(id);
            if (!success)
                return NotFound(new ApiResponse(false, "Ad schedule not found"));

            return Ok(new ApiResponse(true, "Ad schedule deleted successfully"));
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            var result = await _service.FilterAdSchedulesAsync(startTime, endTime);
            return Ok(new ApiResponse(true, "Filtered ad schedules", result));
        }
    }
}
