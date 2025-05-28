using Microsoft.AspNetCore.SignalR;

namespace Services.Hubs
{
    public class AccountStatusHub : Hub
    {
        public async Task JoinAccountGroup(string accountId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Account_{accountId}");
        }

        public async Task LeaveAccountGroup(string accountId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Account_{accountId}");
        }
    }
} 