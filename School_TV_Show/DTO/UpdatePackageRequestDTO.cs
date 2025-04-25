using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class UpdatePackageRequestDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "TimeDuration is required.")]
        public int TimeDuration { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public bool Status { get; set; }
    }
}
