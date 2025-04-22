using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using School_TV_Show.DTO;
using Services;
using Services.Hubs;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize(Roles = "User,SchoolOwner,Admin")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(INotificationService notificationService, IHubContext<NotificationHub> hubContext)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            int accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var notis = await _notificationService.GetNotificationsForAccountAsync(accountId);
            var paged = notis.Skip((page - 1) * pageSize).Take(pageSize);
            return Ok(paged);
        }

        [HttpGet("my/unread")]
        public async Task<IActionResult> GetUnread()
        {
            int accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var all = await _notificationService.GetNotificationsForAccountAsync(accountId);
            return Ok(all.Where(n => !n.IsRead));
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Marked as read." });
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetByAccountId(int accountId)
        {
            var notis = await _notificationService.GetNotificationsForAccountAsync(accountId);
            return Ok(notis);
        }

        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastNotificationInput input)
        {
            if (input.AccountIds == null || !input.AccountIds.Any())
                return BadRequest("Account list cannot be empty.");

            var now = DateTime.UtcNow;

            var notifications = input.AccountIds.Distinct().Select(id => new Notification
            {
                AccountID = id,
                Title = input.Title,
                Message = input.Message,
                Content = input.Content,
                ProgramID = input.ProgramID,
                SchoolChannelID = input.SchoolChannelID,
                CreatedAt = now,
                IsRead = false
            }).ToList();

            await _notificationService.BroadcastAsync(notifications);

            foreach (var noti in notifications)
            {
                await _hubContext.Clients.User(noti.AccountID.ToString()).SendAsync("ReceiveNotification", new
                {
                    title = noti.Title,
                    content = noti.Content,
                    message = noti.Message,
                    createdAt = noti.CreatedAt
                });
            }

            return Ok(new { message = "Notifications sent successfully." });
        }
    }
}
