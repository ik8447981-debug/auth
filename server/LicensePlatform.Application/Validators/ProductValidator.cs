using System.Text.RegularExpressions;

namespace LicensePlatform.Application.Validators
{
    public static class ProductValidator
    {
        private static readonly Regex ProductCodePattern = new(
            @"^[A-Z][A-Z0-9_]{1,49}$",
            RegexOptions.Compiled);

        public static ValidationResult ValidateProductCode(string productCode)
        {
            if (string.IsNullOrWhiteSpace(productCode))
            {
                return new ValidationResult(false, "Product code cannot be empty.");
            }

            string trimmed = productCode.Trim();

            if (trimmed.Length < 2 || trimmed.Length > 50)
            {
                return new ValidationResult(false, "Product code must be between 2 and 50 characters.");
            }

            if (!ProductCodePattern.IsMatch(trimmed))
            {
                return new ValidationResult(false, "Product code must start with a letter and contain only uppercase letters, digits, and underscores.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateProductName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                return new ValidationResult(false, "Product name cannot be empty.");
            }

            if (productName.Trim().Length < 2 || productName.Trim().Length > 100)
            {
                return new ValidationResult(false, "Product name must be between 2 and 100 characters.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return new ValidationResult(false, "Display name cannot be empty.");
            }

            if (displayName.Trim().Length < 2 || displayName.Trim().Length > 200)
            {
                return new ValidationResult(false, "Display name must be between 2 and 200 characters.");
            }

            return new ValidationResult(true);
        }
    }
}
