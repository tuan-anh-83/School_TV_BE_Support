using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface INotificationRepo
    {
        Task<List<Notification>> GetByAccountIdAsync(int accountId);
        Task AddAsync(Notification notification);
        Task MarkAsReadAsync(int notificationId);
        Task AddManyAsync(IEnumerable<Notification> notifications);
    }
}
