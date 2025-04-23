using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class UpdateVideoLikeRequest
    {
        [Required(ErrorMessage = "Quantity is required.")]
        public int Quantity { get; set; }
    }
}
