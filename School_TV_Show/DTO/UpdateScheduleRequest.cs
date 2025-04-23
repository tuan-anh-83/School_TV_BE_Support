using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class UpdateScheduleRequest
    {
        [Required(ErrorMessage = "StartTime is required.")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "EndTime is required.")]
        public DateTime EndTime { get; set; }
        public bool IsReplay { get; set; } = false;
    }
}
