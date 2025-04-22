using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class Program
    {
        public int ProgramID { get; set; }

        public int SchoolChannelID { get; set; }
        public string? CloudflareStreamId { get; set; }
        public string ProgramName { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }

        public virtual SchoolChannel SchoolChannel { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<VideoHistory> VideoHistories { get; set; }
        public ICollection<ProgramFollow> ProgramFollows { get; set; }
    }
}
