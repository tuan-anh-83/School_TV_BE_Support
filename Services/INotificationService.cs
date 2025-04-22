using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsForAccountAsync(int accountId);
        Task CreateNotificationAsync(Notification notification);
        Task MarkAsReadAsync(int notificationId);
        Task BroadcastAsync(List<Notification> notifications);

    }
}
