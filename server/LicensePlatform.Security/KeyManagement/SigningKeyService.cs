using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LicensePlatform.Security.KeyManagement
{
    /// <summary>
    /// Service for generating, signing, and verifying data using RSA-2048 key pairs.
    /// </summary>
    public interface ISigningKeyService
    {
        /// <summary>
        /// Generates a new RSA-2048 key pair and returns the PEM-encoded private and public keys.
        /// </summary>
        /// <returns>A JSON string containing PrivateKeyPem and PublicKeyPem fields.</returns>
        string GenerateSigningKeyPair();

        /// <summary>
        /// Signs the given data using the provided private key.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <param name="privateKeyPem">PEM-encoded RSA private key.</param>
        /// <returns>Base64-encoded signature.</returns>
        string SignData(string data, string privateKeyPem);

        /// <summary>
        /// Verifies a signature against data using the provided public key.
        /// </summary>
        /// <param name="data">The original data.</param>
        /// <param name="signature">Base64-encoded signature.</param>
        /// <param name="publicKeyPem">PEM-encoded RSA public key.</param>
        /// <returns>True if the signature is valid; otherwise false.</returns>
        bool VerifySignature(string data, string signature, string publicKeyPem);

        /// <summary>
        /// Gets the PEM-encoded public key for the currently active signing key.
        /// </summary>
        string GetActivePublicKey();

        /// <summary>
        /// Gets the version number of the currently active signing key.
        /// </summary>
        int GetActiveKeyVersion();
    }

    /// <summary>
    /// RSA-2048 implementation of the signing key service.
    /// </summary>
    public class RsaSigningKeyService : ISigningKeyService
    {
        private readonly int _keySize = 2048;

        private string? _cachedActivePublicKey;
        private int _cachedKeyVersion;

        /// <inheritdoc />
        public string GenerateSigningKeyPair()
        {
            using RSA rsa = RSA.Create(_keySize);

            string privateKeyPem = ConvertToPrivateKeyPem(rsa);
            string publicKeyPem = ConvertToPublicKeyPem(rsa);

            var result = new
            {
                PrivateKeyPem = privateKeyPem,
                PublicKeyPem = publicKeyPem
            };

            return JsonSerializer.Serialize(result);
        }

        /// <inheritdoc />
        public string SignData(string data, string privateKeyPem)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(privateKeyPem))
                throw new ArgumentNullException(nameof(privateKeyPem));

            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem.AsSpan());

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }

        /// <inheritdoc />
        public bool VerifySignature(string data, string signature, string publicKeyPem)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(signature))
                throw new ArgumentNullException(nameof(signature));
            if (string.IsNullOrEmpty(publicKeyPem))
                throw new ArgumentNullException(nameof(publicKeyPem));

            try
            {
                using RSA rsa = RSA.Create();
                rsa.ImportFromPem(publicKeyPem.AsSpan());

                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signatureBytes = Convert.FromBase64String(signature);

                return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public string GetActivePublicKey()
        {
            if (!string.IsNullOrEmpty(_cachedActivePublicKey))
                return _cachedActivePublicKey;

            // In production, this would load from configuration or database.
            // Generate a temporary key pair for bootstrapping.
            using RSA rsa = RSA.Create(_keySize);
            _cachedActivePublicKey = ConvertToPublicKeyPem(rsa);
            _cachedKeyVersion = 1;

            return _cachedActivePublicKey;
        }

        /// <inheritdoc />
        public int GetActiveKeyVersion()
        {
            return _cachedKeyVersion;
        }

        /// <summary>
        /// Sets the active public key and version from external source (e.g., database).
        /// </summary>
        public void SetActiveKey(string publicKeyPem, int keyVersion)
        {
            _cachedActivePublicKey = publicKeyPem;
            _cachedKeyVersion = keyVersion;
        }

        private static string ConvertToPrivateKeyPem(RSA rsa)
        {
            byte[] privateKeyBytes = rsa.ExportPkcs8PrivateKey();
            string base64 = Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks);
            return $"-----BEGIN PRIVATE KEY-----\n{base64}\n-----END PRIVATE KEY-----";
        }

        private static string ConvertToPublicKeyPem(RSA rsa)
        {
            byte[] publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
            string base64 = Convert.ToBase64String(publicKeyBytes, Base64FormattingOptions.InsertLineBreaks);
            return $"-----BEGIN PUBLIC KEY-----\n{base64}\n-----END PUBLIC KEY-----";
        }
    }
}
