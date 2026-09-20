using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace LicensePlatform.Security.LicenseGeneration
{
    /// <summary>
    /// Service for generating and validating license keys.
    /// </summary>
    public interface ILicenseKeyGenerator
    {
        /// <summary>
        /// Generates a unique license key for the specified product.
        /// </summary>
        /// <param name="productCode">The product code to include in the key.</param>
        /// <returns>A formatted license key string.</returns>
        string GenerateKey(string productCode);

        /// <summary>
        /// Validates whether a string matches the expected license key format.
        /// </summary>
        /// <param name="licenseKey">The license key to validate.</param>
        /// <returns>True if the format is valid; otherwise false.</returns>
        bool IsValidFormat(string licenseKey);
    }

    /// <summary>
    /// Generates license keys in the format: PRODUCTCODE-XXXX-XXXX-XXXX
    /// where each X is an uppercase alphanumeric character.
    /// </summary>
    public class LicenseKeyGenerator : ILicenseKeyGenerator
    {
        private const int SegmentLength = 4;
        private const int SegmentCount = 3;
        private const string AlphanumericChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        private static readonly Regex KeyPattern = new(
            @"^[A-Z0-9]{2,50}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$",
            RegexOptions.Compiled);

        /// <inheritdoc />
        public string GenerateKey(string productCode)
        {
            if (string.IsNullOrWhiteSpace(productCode))
                throw new ArgumentException("Product code cannot be null or empty.", nameof(productCode));

            string normalizedCode = productCode.ToUpperInvariant().Trim();

            string[] segments = new string[SegmentCount];
            for (int i = 0; i < SegmentCount; i++)
            {
                segments[i] = GenerateRandomSegment();
            }

            return $"{normalizedCode}-{string.Join("-", segments)}";
        }

        /// <inheritdoc />
        public bool IsValidFormat(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
                return false;

            return KeyPattern.IsMatch(licenseKey.Trim().ToUpperInvariant());
        }

        /// <summary>
        /// Generates a single random segment of the specified length using cryptographic randomness.
        /// </summary>
        private static string GenerateRandomSegment()
        {
            char[] result = new char[SegmentLength];
            Span<byte> randomBytes = stackalloc byte[SegmentLength];

            RandomNumberGenerator.Fill(randomBytes);

            for (int i = 0; i < SegmentLength; i++)
            {
                int index = randomBytes[i] % AlphanumericChars.Length;
                result[i] = AlphanumericChars[index];
            }

            return new string(result);
        }
    }
}
