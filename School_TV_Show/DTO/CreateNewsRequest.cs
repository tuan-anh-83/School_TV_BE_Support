using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class CreateNewsRequest
    {
        [Required]
        public int SchoolChannelID { get; set; }

        [Required]
        public int CategoryNewsID { get; set; }

        [Required]
        [StringLength(250, ErrorMessage = "The title cannot exceed 250 characters.")]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public bool FollowerMode { get; set; }

        [Required(ErrorMessage = "At least one image file is required.")]
        public List<IFormFile> ImageFiles { get; set; }
    }
}
