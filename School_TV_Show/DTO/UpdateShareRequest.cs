using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class UpdateShareRequest
    {
        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; }
    }
}
