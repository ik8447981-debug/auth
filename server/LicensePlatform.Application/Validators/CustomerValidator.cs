using System.Text.RegularExpressions;

namespace LicensePlatform.Application.Validators
{
    public static class CustomerValidator
    {
        private static readonly Regex EmailPattern = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled);

        public static ValidationResult ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ValidationResult(false, "Email address cannot be empty.");
            }

            string trimmed = email.Trim();

            if (trimmed.Length > 300)
            {
                return new ValidationResult(false, "Email address cannot exceed 300 characters.");
            }

            if (!EmailPattern.IsMatch(trimmed))
            {
                return new ValidationResult(false, "Email address format is invalid.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ValidationResult(false, "Customer name cannot be empty.");
            }

            if (name.Trim().Length < 2 || name.Trim().Length > 200)
            {
                return new ValidationResult(false, "Customer name must be between 2 and 200 characters.");
            }

            return new ValidationResult(true);
        }
    }
}
