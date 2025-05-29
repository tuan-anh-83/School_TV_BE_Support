using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class UpdateScheduleRequest
    {
        [Required(ErrorMessage = "StartTime is required.")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "EndTime is required.")]
        public string EndTime { get; set; }
        public bool IsReplay { get; set; } = false;

        [Required(ErrorMessage = "Thumbnail is required.")]
        public IFormFile ThumbnailFile { get; set; }
    }
}
