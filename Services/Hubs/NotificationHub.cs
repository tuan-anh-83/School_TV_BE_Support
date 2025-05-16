using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            string? accountId = Context.GetHttpContext()?.Request.Query["accountId"].ToString();
            if (!string.IsNullOrEmpty(accountId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, accountId);
            }
            await base.OnConnectedAsync();
        }

        public async Task JoinNotiGroup(string scheduleId)
        {
            try
            {
                Console.WriteLine($"[JoinGroup] ConnectionId: {Context.ConnectionId}, scheduleId: {scheduleId}");

                if (string.IsNullOrEmpty(scheduleId))
                {
                    Console.WriteLine($"[ERROR] scheduleId is null or empty for ConnectionId: {Context.ConnectionId}");
                    throw new HubException("scheduleId không được null hoặc rỗng.");
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, scheduleId);
                Console.WriteLine($"[SUCCESS] Added to group: {scheduleId}, ConnectionId: {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Error joining group {scheduleId}: {ex.Message}");
                throw; // re-throw to send error to client
            }
        }

        public async Task LeavenNotiGroup(string scheduleId)
        {
            try
            {
                Console.WriteLine($"[LeaveGroup] ConnectionId: {Context.ConnectionId}, scheduleId: {scheduleId}");

                if (string.IsNullOrEmpty(scheduleId))
                {
                    Console.WriteLine($"[ERROR] scheduleId is null or empty for ConnectionId: {Context.ConnectionId}");
                    throw new HubException("scheduleId không được null hoặc rỗng.");
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, scheduleId);
                Console.WriteLine($"[SUCCESS] Removed from group: {scheduleId}, ConnectionId: {Context.ConnectionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Error leaving group {scheduleId}: {ex.Message}");
                throw; // re-throw to send error to client
            }
        }
    }
}
