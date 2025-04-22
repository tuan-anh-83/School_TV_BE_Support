using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class CreateVideoHistoryRequestDTO
    {
        [Required(ErrorMessage = "URL is required.")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters.")]
        public string URL { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required.")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters.")]
        public string Type { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "ProgramID is required.")]
        public int ProgramID { get; set; }
    }
}
