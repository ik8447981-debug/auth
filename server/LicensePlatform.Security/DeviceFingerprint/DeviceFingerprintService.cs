using System;
using System.Security.Cryptography;
using System.Text;

namespace LicensePlatform.Security.DeviceFingerprint
{
    /// <summary>
    /// Service for computing, hashing, and verifying device fingerprints.
    /// </summary>
    public interface IDeviceFingerprintService
    {
        /// <summary>
        /// Computes a fingerprint hash from normalized raw hardware signals.
        /// </summary>
        /// <param name="rawSignals">Raw hardware signal data to normalize and hash.</param>
        /// <returns>A hex-encoded SHA256 hash of the normalized input.</returns>
        string ComputeFingerprint(string rawSignals);

        /// <summary>
        /// Produces a storage-safe hash of a fingerprint using SHA256.
        /// </summary>
        /// <param name="fingerprint">The computed fingerprint.</param>
        /// <returns>A hex-encoded SHA256 hash suitable for database storage.</returns>
        string HashFingerprint(string fingerprint);

        /// <summary>
        /// Verifies that a provided fingerprint matches a stored fingerprint using constant-time comparison.
        /// </summary>
        /// <param name="stored">The fingerprint hash stored in the database.</param>
        /// <param name="provided">The fingerprint hash provided by the client.</param>
        /// <returns>True if the fingerprints match; otherwise false.</returns>
        bool VerifyFingerprint(string stored, string provided);
    }

    /// <summary>
    /// SHA256-based implementation of the device fingerprint service.
    /// </summary>
    public class DeviceFingerprintService : IDeviceFingerprintService
    {
        /// <inheritdoc />
        public string ComputeFingerprint(string rawSignals)
        {
            if (string.IsNullOrEmpty(rawSignals))
                throw new ArgumentNullException(nameof(rawSignals));

            string normalized = NormalizeInput(rawSignals);

            byte[] inputBytes = Encoding.UTF8.GetBytes(normalized);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <inheritdoc />
        public string HashFingerprint(string fingerprint)
        {
            if (string.IsNullOrEmpty(fingerprint))
                throw new ArgumentNullException(nameof(fingerprint));

            byte[] inputBytes = Encoding.UTF8.GetBytes(fingerprint);
            byte[] hashBytes = SHA256.HashData(inputBytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <inheritdoc />
        public bool VerifyFingerprint(string stored, string provided)
        {
            if (string.IsNullOrEmpty(stored) || string.IsNullOrEmpty(provided))
                return false;

            byte[] storedBytes = Convert.FromHexString(stored);
            byte[] providedBytes = Convert.FromHexString(provided);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(storedBytes, providedBytes);
        }

        /// <summary>
        /// Normalizes raw signal input by trimming whitespace, converting to lowercase,
        /// and removing extraneous separators.
        /// </summary>
        private static string NormalizeInput(string rawSignals)
        {
            return rawSignals
                .ToLowerInvariant()
                .Trim()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(":", "");
        }
    }
}
