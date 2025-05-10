using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class AdLiveStream
    {
        public int AdLiveStreamID { get; set; }
        public int AdScheduleID { get; set; }
        public int ScheduleID { get; set; }
        public int AccountID { get; set; }
        public DateTime PlayAt { get; set; }
        public bool IsPlayed { get; set; } = false;
        public int Duration { get; set; }

        [ForeignKey("AdScheduleID")]
        [InverseProperty("AdLiveStreams")]
        public virtual AdSchedule? AdSchedule { get; set; } = null!;

        [ForeignKey("ScheduleID")]
        [InverseProperty("AdLiveStreams")]
        public virtual Schedule? Schedule { get; set; } = null!;

        [ForeignKey("AccountID")]
        [InverseProperty("AdLiveStreams")]
        public virtual Account? Account { get; set; } = null!;
    }
}
