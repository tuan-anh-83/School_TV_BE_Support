using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentHistoryController : ControllerBase
    {
        private readonly IPaymentHistoryService _paymentHistoryService;

        public PaymentHistoryController(IPaymentHistoryService paymentHistoryService)
        {
            _paymentHistoryService = paymentHistoryService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPaymentHistoriesForAdmin()
        {
            var paymentHistories = await _paymentHistoryService.GetAllPaymentHistoriesAsync();
            return Ok(paymentHistories);
        }

        [HttpGet("school-owner")]
        [Authorize(Roles = "SchoolOwner")]
        public async Task<IActionResult> GetAllPaymentHistoriesForSchoolOwner()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest(new { message = "Invalid User ID." });
            }

            var paymentHistories = await _paymentHistoryService.GetPaymentHistoriesByUserIdAsync(userId);
            return Ok(paymentHistories);
        }
    }
}
