using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using School_TV_Show.DTO;
using School_TV_Show.Helpers;
using Services;
using Services.Hubs;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly IHubContext<NotificationHub> _hubContext;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public ReportController(IReportService reportService, IHubContext<NotificationHub> hubContext)
        {
            _reportService = reportService;
            _hubContext = hubContext;
        }

        [HttpPost("CreateReport")]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request data is missing.");
            }

            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { message });
            }

            try
            {
                var accountClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (accountClaim == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                int accountId = int.Parse(accountClaim.Value);

                var report = new Report
                {
                    AccountID = accountId,
                    VideoHistoryID = request.VideoHistoryID,
                    Reason = request.Reason,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
                };

                var result = await _reportService.CreateReportAsync(report);
                await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification", result.ReportID);
                return Ok(new { Message = "Report created successfully.", result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error creating report.", Error = ex.Message });
            }
        }

        [HttpPost("UpdateReport/{id}")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] Report request)
        {
            if (request == null)
            {
                return BadRequest("Request data is missing.");
            }

            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { message });
            }

            try
            {
                var accountClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (accountClaim == null)
                {
                    return Unauthorized("User is not authenticated.");
                }

                int accountId = int.Parse(accountClaim.Value);

                var existingReport = await _reportService.GetReportByIdAsync(id);
                if (existingReport == null)
                {
                    return NotFound("Report not found.");
                }

                var updatedReport = new Report
                {
                    ReportID = id,
                    AccountID = accountId,
                    VideoHistoryID = request.VideoHistoryID,
                    Reason = request.Reason,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
                };

                bool result = await _reportService.UpdateReportAsync(updatedReport);

                if (!result)
                {
                    return StatusCode(500, "Error updating report.");
                }

                return Ok(new { Message = "Report updated successfully.", ReportID = updatedReport.ReportID });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating report.", Error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<News>> GetAllReports()
        {
            var reports = await _reportService.GetAllReportsAsync();
            return Ok(reports);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var result = await _reportService.DeleteReportAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
