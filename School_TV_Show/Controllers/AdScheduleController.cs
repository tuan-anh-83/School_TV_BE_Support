using Azure.Core;
using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using School_TV_Show.DTO;
using School_TV_Show.Helpers;
using Services;
using Services.Hubs;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdScheduleController : ControllerBase
    {
        private readonly IAdScheduleService _service;
        private readonly HttpClient _httpClient;
        private readonly IHubContext<LiveStreamHub> _hubContext;
        private readonly IPackageService _packageService;
        private readonly CloudflareSettings _cloudflareSettings;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public AdScheduleController(
            IAdScheduleService service, 
            IHubContext<LiveStreamHub> hubContext,
            IPackageService packageService,
            IOptions<CloudflareSettings> cloudflareSettings
        )
        {
            _service = service;
            _hubContext = hubContext;
            _cloudflareSettings = cloudflareSettings.Value;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareSettings.ApiToken);
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
                DurationSeconds = ad.DurationSeconds,
                VideoUrl = ad.VideoUrl,
                CreatedAt = ad.CreatedAt
            });

            return Ok(new ApiResponse(true, "List of ad schedules", response));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("valid")]
        public async Task<IActionResult> GetValidAdSchedules()
        {
            var ads = await _service.GetAllAdSchedulesAsync();

            var response = ads.Select(ad => new AdScheduleResponseDTO
            {
                AdScheduleID = ad.AdScheduleID,
                Title = ad.Title,
                DurationSeconds = ad.DurationSeconds,
                VideoUrl = ad.VideoUrl,
                CreatedAt = ad.CreatedAt
            });

            return Ok(new ApiResponse(true, "List of ad schedules", response));
        }

        [Authorize(Roles = "Advertiser")]
        [HttpGet("my")]
        public async Task<IActionResult> GetAllForAdvertiser()
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid account");

            var ads = await _service.GetAllForAdvertiserAsync(accountId);

            var response = ads.Select(ad => new AdScheduleResponseDTO
            {
                AdScheduleID = ad.AdScheduleID,
                Title = ad.Title,
                DurationSeconds = ad.DurationSeconds,
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
        public async Task<IActionResult> Create([FromForm] CreateAdScheduleRequestDTO request)
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid account");

            var ad = new AdSchedule
            {
                Title = request.Title,
                DurationSeconds= request.DurationSeconds,
                VideoUrl = "",
                AccountID = accountId,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
            };

            var success = await _service.CreateAdScheduleAsync(ad);
            if (!success)
                return StatusCode(500, new ApiResponse(false, "Failed to create ad schedule"));

            return Ok(new ApiResponse(true, "Ad schedule created successfully"));
        }

        [Authorize(Roles = "Advertiser")]
        [HttpPost("ads")]
        public async Task<IActionResult> CreateAds([FromForm] CreateAdScheduleRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input", ModelState));

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
                return NotFound(new { error = "Chưa có gói đăng kí, vui lòng đăng kí gói để tạo quảng cáo." });

            var (package, remainingDuration, expiredAt) = currentPackage.Value;

            if (remainingDuration == null || remainingDuration <= 0 || expiredAt < TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone))
                return BadRequest(new { error = "Your package was expired." });

            var url = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream";

            using var content = new MultipartFormDataContent();
            using var fileStream = request.VideoFile.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.VideoFile.ContentType);

            content.Add(fileContent, "file", request.VideoFile.FileName);
            content.Add(new StringContent(request.Title), "meta[name]");

            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Cloudflare upload failed: " + json);
                return BadRequest(new { error = "Cloudflare upload failed" });
            }

            var jsonDoc = JsonDocument.Parse(json);
            var uid = jsonDoc.RootElement.GetProperty("result").GetProperty("uid").GetString();

            var ad = new AdSchedule
            {
                Title = request.Title,
                DurationSeconds = request.DurationSeconds,
                VideoUrl = $"https://iframe.videodelivery.net/{uid}",
                AccountID = accountId,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            var success = await _service.CreateAdScheduleAsync(ad);
            if (!success)
                return StatusCode(500, new ApiResponse(false, "Failed to create ad schedule"));

            return Ok(new ApiResponse(true, "Ad schedule created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateAdScheduleRequestDTO request)
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid account");

            var existing = await _service.GetAdScheduleByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse(false, "Ad schedule not found"));

            existing.Title = request.Title;
            existing.DurationSeconds = request.DurationSeconds;
            
            if(request.VideoUrl != null)
            {

            }

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
