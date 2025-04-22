using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int? ProgramID { get; set; }
        public int? SchoolChannelID { get; set; }
        public int AccountID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public string Content { get; set; } = string.Empty;

    }
}
