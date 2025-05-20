using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class UploadVideoHistoryRequest
    {
        [Required]
        public IFormFile VideoFile { get; set; }

        public int? ProgramID { get; set; }

        [Required]
        public string Type { get; set; }

        public string Description { get; set; }

        public string StreamAt { get; set; }

        public string ProgramName { get; set; }

        public string ProgramTitle { get; set; }

        public int SchoolChannelId { get; set; }
    }
}
