using School_TV_Show.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.DTO
{
    public class PartialAccountUpdateRequest
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Fullname must be up to 50 characters long.")]
        public string? Fullname { get; set; }

        [StringLength(50, ErrorMessage = "Address must be up to 50 characters long.")]
        public string? Address { get; set; }

        [DataType(DataType.PhoneNumber)]
        [CustomPhoneValidation]
        public string? PhoneNumber { get; set; }
    }
}
