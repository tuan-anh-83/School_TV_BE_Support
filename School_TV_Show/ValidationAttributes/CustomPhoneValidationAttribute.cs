using System.ComponentModel.DataAnnotations;

namespace School_TV_Show.ValidationAttributes
{
    public class CustomPhoneValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var phoneNumber = value as string;

            if (string.IsNullOrWhiteSpace(phoneNumber))
                return ValidationResult.Success;

            if (!phoneNumber.All(char.IsDigit) || phoneNumber.Length < 10 || phoneNumber.Length > 15)
                return new ValidationResult("Phone number must be between 10 and 15 digits and contain only numbers.");

            if (phoneNumber.Distinct().Count() == 1)
                return new ValidationResult("Phone number cannot have all identical digits.");

            return ValidationResult.Success;
        }
    }
}
