using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class Schedule
    {
        public int ScheduleID { get; set; }
        public int ProgramID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "Pending";
        public bool LiveStreamStarted { get; set; }
        public bool LiveStreamEnded { get; set; }
        public bool IsReplay { get; set; }
        public int? VideoHistoryID { get; set; }
        public Program Program { get; set; }
    }
}
