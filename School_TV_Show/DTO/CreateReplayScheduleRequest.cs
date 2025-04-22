using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class CreateReplayScheduleRequest
    {
        [Required]
        public int VideoHistoryId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}
