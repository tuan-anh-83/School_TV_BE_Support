using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class Share
    {
        public int ShareID { get; set; }
        [Required]
        public int AccountID { get; set; }
        [Required]
        public int VideoHistoryID { get; set; }
        [Required]
        public int Quantity { get; set; } = 1;
        public VideoHistory VideoHistory { get; set; }
    }
}
