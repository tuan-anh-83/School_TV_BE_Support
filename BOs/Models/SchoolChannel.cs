using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class SchoolChannel
    {
        public int SchoolChannelID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int AccountID { get; set; }
        public bool Status { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? TotalDuration { get; set; }
        public virtual Account Account { get; set; }
        public virtual ICollection<News> News { get; set; } = new List<News>();

        public ICollection<SchoolChannelFollow> Followers { get; set; }
    }
}
