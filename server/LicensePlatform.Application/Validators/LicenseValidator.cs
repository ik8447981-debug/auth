using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;

namespace LicensePlatform.Application.Validators
{
    public static class LicenseValidator
    {
        private static readonly Regex KeyPattern = new(
            @"^[A-Z0-9]{2,50}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$",
            RegexOptions.Compiled);

        public static ValidationResult ValidateKeyFormat(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                return new ValidationResult(false, "License key cannot be empty.");
            }

            if (!KeyPattern.IsMatch(licenseKey.Trim().ToUpperInvariant()))
            {
                return new ValidationResult(false, "License key format is invalid. Expected format: PRODUCTCODE-XXXX-XXXX-XXXX");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateExpiry(License license)
        {
            if (license.ExpiryDate.HasValue && license.ExpiryDate.Value < DateTime.UtcNow)
            {
                return new ValidationResult(false, "License has expired.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateStatus(License license, LicenseStatus requiredStatus)
        {
            if (license.Status != requiredStatus)
            {
                return new ValidationResult(false, $"License status is '{license.Status}' but expected '{requiredStatus}'.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateProductMatch(License license, Guid productId)
        {
            if (license.ProductId != productId)
            {
                return new ValidationResult(false, "License does not belong to the specified product.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateDeviceLimit(License license, int currentDeviceCount)
        {
            if (currentDeviceCount >= license.MaxDevices)
            {
                return new ValidationResult(false, $"Device limit reached. Maximum allowed devices: {license.MaxDevices}, currently registered: {currentDeviceCount}.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateActivation(License license)
        {
            if (license.Status == LicenseStatus.Revoked)
            {
                return new ValidationResult(false, "Cannot activate a revoked license.");
            }

            if (license.Status == LicenseStatus.Expired)
            {
                return new ValidationResult(false, "Cannot activate an expired license.");
            }

            if (license.ExpiryDate.HasValue && license.ExpiryDate.Value < DateTime.UtcNow)
            {
                return new ValidationResult(false, "Cannot activate a license that has already expired.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateSuspension(License license)
        {
            if (license.Status != LicenseStatus.Active)
            {
                return new ValidationResult(false, $"Only active licenses can be suspended. Current status: {license.Status}.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateRevocation(License license)
        {
            if (license.Status != LicenseStatus.Active && license.Status != LicenseStatus.Suspended)
            {
                return new ValidationResult(false, $"Only active or suspended licenses can be revoked. Current status: {license.Status}.");
            }

            return new ValidationResult(true);
        }

        public static ValidationResult ValidateExtension(License license, DateTime newExpiryDate)
        {
            if (license.Status == LicenseStatus.Revoked)
            {
                return new ValidationResult(false, "Cannot extend a revoked license.");
            }

            if (license.Status == LicenseStatus.Expired)
            {
                return new ValidationResult(false, "Cannot extend an expired license.");
            }

            if (license.ExpiryDate.HasValue && newExpiryDate <= license.ExpiryDate.Value)
            {
                return new ValidationResult(false, $"New expiry date ({newExpiryDate:O}) must be after current expiry date ({license.ExpiryDate.Value:O}).");
            }

            return new ValidationResult(true);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; }
        public string? ErrorMessage { get; }

        public ValidationResult(bool isValid, string? errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
    }
}
