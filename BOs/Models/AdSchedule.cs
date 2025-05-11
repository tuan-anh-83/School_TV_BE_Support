using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class AdSchedule
    {
        public int AdScheduleID { get; set; }
        public string Title { get; set; }
        public int DurationSeconds { get; set; }
        public string VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        [InverseProperty("AdSchedule")]
        public virtual ICollection<AdLiveStream> AdLiveStreams { get; set; } = new List<AdLiveStream>();
    }
}
