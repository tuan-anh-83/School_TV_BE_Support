using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class VideoHistory
    {
        public int VideoHistoryID { get; set; }
        public int? ProgramID { get; set; }
        [Required]
        [MaxLength(500)]
        public string? URL { get; set; }
        public string? MP4Url { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? StreamAt { get; set; }
        public double? Duration { get; set; }

        public Program Program { get; set; }
        public ICollection<VideoLike> VideoLikes { get; set; }
        public ICollection<VideoView> VideoViews { get; set; }
        public ICollection<Share> Shares { get; set; }
        public ICollection<Comment> Comments { get; set; }

        [MaxLength(100)]
        public string? CloudflareStreamId { get; set; }

        [MaxLength(500)]
        public string? PlaybackUrl { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
}
