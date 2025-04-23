using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class CreateCommentRequest
    {
        [Required(ErrorMessage = "Content is required.")]
        [StringLength(1000, ErrorMessage = "Content cannot exceed 1000 characters.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "VideoHistoryID is required.")]
        public int VideoHistoryID { get; set; }
    }
}
