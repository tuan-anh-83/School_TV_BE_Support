using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepo _notificationRepo;

        public NotificationService(INotificationRepo notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task<List<Notification>> GetNotificationsForAccountAsync(int accountId)
        {
            return await _notificationRepo.GetByAccountIdAsync(accountId);
        }

        public async Task CreateNotificationAsync(Notification notification)
        {
            await _notificationRepo.AddAsync(notification);
        }
        public async Task BroadcastAsync(List<Notification> notifications)
        {
            await _notificationRepo.AddManyAsync(notifications);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            await _notificationRepo.MarkAsReadAsync(notificationId);
        }
    }
}
