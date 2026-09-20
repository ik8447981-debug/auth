using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using LicensePlatform.Shared.Security;

namespace LicensePlatform.Security.LicenseResponse
{
    /// <summary>
    /// Service for signing and verifying license response payloads.
    /// </summary>
    public interface ILicenseResponseSigner
    {
        /// <summary>
        /// Signs a license response by computing a canonical serialization and RSA signature.
        /// </summary>
        /// <param name="response">The license response to sign.</param>
        /// <param name="privateKeyPem">PEM-encoded RSA private key.</param>
        /// <param name="keyVersion">Version of the signing key used.</param>
        /// <returns>The signed license response with the Signature and KeyVersion fields populated.</returns>
        SignedLicenseResponse SignLicenseResponse(SignedLicenseResponse response, string privateKeyPem, int keyVersion);

        /// <summary>
        /// Verifies the signature of a license response against its canonical serialization.
        /// </summary>
        /// <param name="response">The signed license response to verify.</param>
        /// <param name="publicKeyPem">PEM-encoded RSA public key.</param>
        /// <returns>True if the signature is valid; otherwise false.</returns>
        bool VerifyLicenseResponse(SignedLicenseResponse response, string publicKeyPem);
    }

    /// <summary>
    /// Signs and verifies license responses using canonical JSON serialization and RSA signatures.
    /// </summary>
    public class LicenseResponseSigner : ILicenseResponseSigner
    {
        private readonly KeyManagement.ISigningKeyService _signingKeyService;

        public LicenseResponseSigner(KeyManagement.ISigningKeyService signingKeyService)
        {
            _signingKeyService = signingKeyService ?? throw new ArgumentNullException(nameof(signingKeyService));
        }

        /// <inheritdoc />
        public SignedLicenseResponse SignLicenseResponse(SignedLicenseResponse response, string privateKeyPem, int keyVersion)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            if (string.IsNullOrEmpty(privateKeyPem))
                throw new ArgumentNullException(nameof(privateKeyPem));

            string canonical = SerializeCanonical(response);
            string signature = _signingKeyService.SignData(canonical, privateKeyPem);

            response.Signature = signature;
            response.KeyVersion = keyVersion;

            return response;
        }

        /// <inheritdoc />
        public bool VerifyLicenseResponse(SignedLicenseResponse response, string publicKeyPem)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            if (string.IsNullOrEmpty(publicKeyPem))
                throw new ArgumentNullException(nameof(publicKeyPem));

            if (string.IsNullOrEmpty(response.Signature))
                return false;

            // Create a copy with an empty signature for canonical serialization
            var verificationCopy = new SignedLicenseResponse
            {
                LicenseId = response.LicenseId,
                ProductId = response.ProductId,
                CustomerId = response.CustomerId,
                Status = response.Status,
                Plan = response.Plan,
                IssuedAt = response.IssuedAt,
                ExpiresAt = response.ExpiresAt,
                DeviceId = response.DeviceId,
                Features = response.Features,
                MaxDevices = response.MaxDevices,
                ServerTime = response.ServerTime,
                LicenseVersion = response.LicenseVersion,
                KeyVersion = response.KeyVersion,
                Nonce = response.Nonce,
                Signature = string.Empty
            };

            string canonical = SerializeCanonical(verificationCopy);
            return _signingKeyService.VerifySignature(canonical, response.Signature, publicKeyPem);
        }

        /// <summary>
        /// Produces a deterministic canonical JSON string by sorting all properties alphabetically.
        /// </summary>
        private static string SerializeCanonical(SignedLicenseResponse response)
        {
            var properties = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["CustomerId"] = response.CustomerId,
                ["DeviceId"] = response.DeviceId,
                ["ExpiresAt"] = response.ExpiresAt.ToString("o"),
                ["Features"] = response.Features ?? new List<string>(),
                ["IssuedAt"] = response.IssuedAt.ToString("o"),
                ["KeyVersion"] = response.KeyVersion,
                ["LicenseId"] = response.LicenseId,
                ["LicenseVersion"] = response.LicenseVersion,
                ["MaxDevices"] = response.MaxDevices,
                ["Nonce"] = response.Nonce ?? string.Empty,
                ["Plan"] = response.Plan ?? string.Empty,
                ["ProductId"] = response.ProductId,
                ["ServerTime"] = response.ServerTime.ToString("o"),
                ["Status"] = response.Status ?? string.Empty
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = null
            };

            return JsonSerializer.Serialize(properties, options);
        }
    }
}
