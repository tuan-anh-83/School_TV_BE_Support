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
            var accountId = Context.GetHttpContext()?.Request.Query["accountId"];
            if (!string.IsNullOrEmpty(accountId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, accountId);
            }
            await base.OnConnectedAsync();
        }
    }
}
