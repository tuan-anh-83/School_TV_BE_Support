using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class CreateSchoolChannelRequestDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [StringLength(255, ErrorMessage = "Website URL cannot exceed 255 characters.")]
        public string? Website { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }
    }
}
