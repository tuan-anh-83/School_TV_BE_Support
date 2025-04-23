using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class ResetPasswordRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm new password is required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
