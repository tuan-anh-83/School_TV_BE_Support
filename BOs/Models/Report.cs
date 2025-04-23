using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class Report
    {
        public int ReportID { get; set; }
        public int AccountID { get; set; }
        public int VideoHistoryID { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public Account Account { get; set; }
        public VideoHistory VideoHistory { get; set; }
    }
}
