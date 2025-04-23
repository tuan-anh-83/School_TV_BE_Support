using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class Comment
    {
        public int CommentID { get; set; }

        [Required]
        public int AccountID { get; set; }

        [Required]
        public int VideoHistoryID { get; set; }

        [Required]
        public string Content { get; set; }

        public int Quantity { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Active";

        public VideoHistory VideoHistory { get; set; }
    }
}
