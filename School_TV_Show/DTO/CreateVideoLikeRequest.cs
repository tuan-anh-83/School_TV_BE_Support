using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class CreateVideoLikeRequest
    {
        [Required(ErrorMessage = "VideoHistoryID is required.")]
        public int VideoHistoryID { get; set; }

        [Required(ErrorMessage = "AccountID is required.")]
        public int AccountID { get; set; }
    }
}
