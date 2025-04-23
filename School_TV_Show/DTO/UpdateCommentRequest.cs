using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class UpdateCommentRequest
    {
        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; }
    }
}
