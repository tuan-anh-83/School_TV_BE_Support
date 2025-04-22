using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class CreateShareRequest
    {
        [Required(ErrorMessage = "VideoHistoryID is required.")]
        public int VideoHistoryID { get; set; }
    }
}
