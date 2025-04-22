using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class NotificationRepo : INotificationRepo
    {
        public Task AddAsync(Notification notification)
        {
            return NotificationDAO.Instance.AddAsync(notification); 
        }

        public Task AddManyAsync(IEnumerable<Notification> notifications)
        {
            return NotificationDAO.Instance.AddManyAsync(notifications);    
        }

        public async Task<List<Notification>> GetByAccountIdAsync(int accountId)
        {
            return await NotificationDAO.Instance.GetByAccountIdAsync(accountId);   
        }

        public Task MarkAsReadAsync(int notificationId)
        {
            return NotificationDAO.Instance.MarkAsReadAsync(notificationId);    
        }
    }
}
