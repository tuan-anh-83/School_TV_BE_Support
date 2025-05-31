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
            var result = paymentHistories.Select(ph => new {
                ph.PaymentHistoryID,
                ph.PaymentID,
                ph.Amount,
                ph.Status,
                ph.Timestamp,
                User = ph.Payment?.Order?.Account == null ? null : new {
                    ph.Payment.Order.Account.AccountID,
                    ph.Payment.Order.Account.Username,
                    ph.Payment.Order.Account.Fullname,
                    RoleName = ph.Payment.Order.Account.Role?.RoleName
                }
            });
            return Ok(result);
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
