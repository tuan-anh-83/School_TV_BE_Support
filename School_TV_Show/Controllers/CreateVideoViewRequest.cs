using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.Controllers
{
    public class CreateVideoViewRequest
    {
        [Required(ErrorMessage = "VideoHistoryID is required.")]
        public int VideoHistoryID { get; set; }
    }
}
