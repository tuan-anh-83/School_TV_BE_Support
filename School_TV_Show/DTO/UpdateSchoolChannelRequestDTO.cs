using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class UpdateSchoolChannelRequestDTO
    {
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        public string? Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [StringLength(1000, ErrorMessage = "Address cannot exceed 1000 characters.")]
        public string? Address { get; set; }

        [StringLength(255, ErrorMessage = "Website cannot exceed 255 characters.")]
        public string? Website { get; set; }
    }
}
