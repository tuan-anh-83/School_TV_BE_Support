using BOs.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.Hubs
{
    public class LiveStreamHub : Hub
    {
        private static readonly ConcurrentDictionary<string, int> ViewerCounts = new();

        public async Task JoinStream(string streamId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, streamId);
            ViewerCounts.AddOrUpdate(streamId, 1, (_, count) => count + 1);
            await Clients.Group(streamId).SendAsync("ViewerCountUpdated", ViewerCounts[streamId]);
        }

        public async Task LeaveStream(string streamId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, streamId);
            if (ViewerCounts.ContainsKey(streamId))
            {
                ViewerCounts[streamId] = Math.Max(0, ViewerCounts[streamId] - 1);
                await Clients.Group(streamId).SendAsync("ViewerCountUpdated", ViewerCounts[streamId]);
            }
        }

        public async Task SendMessageToGroup(string streamId, string username, string message)
        {
            await Clients.Group(streamId).SendAsync("ReceiveMessage", username, message);
        }

        public async Task NotifyLike(int videoHistoryId)
        {
            await Clients.Group($"video_{videoHistoryId}")
                .SendAsync("LikeUpdated", new { videoId = videoHistoryId });
        }

        public async Task NotifyShare(int videoHistoryId)
        {
            await Clients.Group($"video_{videoHistoryId}")
                .SendAsync("ShareUpdated", new { videoId = videoHistoryId });
        }

        public async Task JoinVideoRoom(int videoHistoryId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"video_{videoHistoryId}");
        }

        public async Task LeaveVideoRoom(int videoHistoryId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"video_{videoHistoryId}");
        }

        public async Task NotifyStreamStarted(int videoHistoryId)
        {
            await Clients.Group($"video_{videoHistoryId}").SendAsync("StreamStarted", new
            {
                videoId = videoHistoryId,
                startedAt = DateTime.UtcNow
            });
        }

        public async Task NotifyStreamEnded(int videoHistoryId)
        {
            await Clients.Group($"video_{videoHistoryId}").SendAsync("StreamEnded", new
            {
                videoId = videoHistoryId,
                endedAt = DateTime.UtcNow
            });
        }
    }
}
