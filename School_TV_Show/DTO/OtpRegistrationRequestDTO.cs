using School_TV_Show.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class OtpRegistrationRequestDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Username contains invalid characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [CustomPasswordValidation]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fullname is required.")]
        [StringLength(50)]
        public string Fullname { get; set; } = string.Empty;

        [StringLength(50)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [DataType(DataType.PhoneNumber)]
        [CustomPhoneValidation]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
