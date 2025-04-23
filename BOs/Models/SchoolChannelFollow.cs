using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class SchoolChannelFollow
    {
        public int AccountID { get; set; }
        public int SchoolChannelID { get; set; }
        public string Status { get; set; } = "Followed";
        public DateTime FollowedAt { get; set; }

        public Account Account { get; set; }
        public SchoolChannel SchoolChannel { get; set; }
    }
}
