using BOs.Models;
using Microsoft.AspNetCore.Mvc;
using School_TV_Show.DTO;
using Services;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdScheduleController : ControllerBase
    {
        private readonly IAdScheduleService _service;

        public AdScheduleController(IAdScheduleService service)
        {
            _service = service;
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
            var ad = new AdSchedule
            {
                Title = request.Title,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                VideoUrl = request.VideoUrl,

                CreatedAt = DateTime.UtcNow
            };

            var success = await _service.CreateAdScheduleAsync(ad);
            if (!success)
                return StatusCode(500, new ApiResponse(false, "Failed to create ad schedule"));

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
