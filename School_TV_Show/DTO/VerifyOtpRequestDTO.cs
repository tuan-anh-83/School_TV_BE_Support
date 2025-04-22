using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class VerifyOtpRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OtpCode { get; set; } = string.Empty;
    }
}
