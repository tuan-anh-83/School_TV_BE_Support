using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAOs
{
    public class NotificationDAO
    {
        private static NotificationDAO instance = null;
        private readonly DataContext _context;

        private NotificationDAO()
        {
            _context = new DataContext();
        }

        public static NotificationDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NotificationDAO();
                }
                return instance;
            }
        }

        public async Task<List<Notification>> GetByAccountIdAsync(int accountId)
        {
            return await _context.Notifications
                .Where(n => n.AccountID == accountId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
        public async Task AddManyAsync(IEnumerable<Notification> notifications)
        {
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var noti = await _context.Notifications.FindAsync(notificationId);
            if (noti != null)
            {
                noti.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
