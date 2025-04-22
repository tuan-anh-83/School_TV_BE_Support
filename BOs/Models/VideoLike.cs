using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class VideoLike
    {
        public int LikeID { get; set; }
        [Required]
        public int AccountID { get; set; }
        [Required]
        public int VideoHistoryID { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public int Quantity { get; set; } = 1;
        public VideoHistory VideoHistory { get; set; }
    }
}
