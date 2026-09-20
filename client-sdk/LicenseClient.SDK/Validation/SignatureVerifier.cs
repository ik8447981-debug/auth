using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LicenseClient.SDK.Models;

namespace LicenseClient.SDK.Validation;

/// <summary>
/// Interface for cryptographic signature verification.
/// </summary>
public interface ISignatureVerifier
{
    /// <summary>
    /// Verifies the RSA signature on a signed license response.
    /// </summary>
    bool VerifySignature(SignedLicenseResponse response, string publicKeyPem);
}

/// <summary>
/// Verifies cryptographic signatures on license responses using RSA.
/// Uses the server's public key embedded in configuration.
/// NEVER contains private key material.
/// </summary>
public class SignatureVerifier : ISignatureVerifier
{
    /// <summary>
    /// Verifies the RSA signature on a signed license response.
    /// </summary>
    /// <param name="response">The signed license response to verify.</param>
    /// <param name="publicKeyPem">RSA public key in PEM format.</param>
    /// <returns>True if the signature is valid; false otherwise.</returns>
    public bool VerifySignature(SignedLicenseResponse response, string publicKeyPem)
    {
        if (response == null)
            throw new ArgumentNullException(nameof(response));

        if (string.IsNullOrWhiteSpace(publicKeyPem))
            throw new ArgumentException("Public key PEM is required.", nameof(publicKeyPem));

        if (string.IsNullOrWhiteSpace(response.Signature))
            return false;

        try
        {
            // Build canonical serialization for verification
            var canonicalPayload = BuildCanonicalPayload(response);

            // Compute SHA256 hash of the canonical payload
            var payloadBytes = Encoding.UTF8.GetBytes(canonicalPayload);
            var hash = SHA256.HashData(payloadBytes);

            // Decode the signature from Base64
            var signatureBytes = Convert.FromBase64String(response.Signature);

            // Import the RSA public key
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);

            // Verify the signature
            return rsa.VerifyHash(
                hash,
                signatureBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
        catch (FormatException)
        {
            // Invalid Base64 in signature
            return false;
        }
        catch (CryptographicException)
        {
            // Signature verification failed or invalid key
            return false;
        }
    }

    /// <summary>
    /// Builds a canonical string representation of the license response
    /// for signature verification. All signed fields are included in
    /// a deterministic order.
    /// </summary>
    internal static string BuildCanonicalPayload(SignedLicenseResponse response)
    {
        // Sort fields alphabetically for deterministic serialization
        var fields = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["customerId"] = response.CustomerId ?? string.Empty,
            ["deviceId"] = response.DeviceId ?? string.Empty,
            ["expiresAt"] = response.ExpiresAt?.ToString("O") ?? string.Empty,
            ["features"] = response.Features != null
                ? string.Join(",", response.Features.OrderBy(f => f, StringComparer.Ordinal))
                : string.Empty,
            ["issuedAt"] = response.IssuedAt.ToString("O"),
            ["keyVersion"] = response.KeyVersion.ToString(),
            ["licenseId"] = response.LicenseId ?? string.Empty,
            ["licenseVersion"] = response.LicenseVersion.ToString(),
            ["maxDevices"] = response.MaxDevices.ToString(),
            ["nonce"] = response.Nonce ?? string.Empty,
            ["plan"] = response.Plan ?? string.Empty,
            ["productId"] = response.ProductId ?? string.Empty,
            ["serverTime"] = response.ServerTime.ToString("O"),
            ["status"] = response.Status ?? string.Empty
        };

        var sb = new StringBuilder();
        foreach (var kvp in fields)
        {
            if (sb.Length > 0)
                sb.Append('&');
            sb.Append(kvp.Key);
            sb.Append('=');
            sb.Append(kvp.Value);
        }

        return sb.ToString();
    }
}
