using FluentAssertions;
using LicensePlatform.Security.KeyManagement;
using Xunit;

namespace LicensePlatform.Tests.UnitTests.Security
{
    /// <summary>
    /// Tests for RSA signing and verification operations.
    /// Ensures cryptographic operations produce correct results and fail safely
    /// when data is tampered with or the wrong key is used.
    /// </summary>
    public class SignatureTests
    {
        private readonly RsaSigningKeyService _signingKeyService;

        public SignatureTests()
        {
            _signingKeyService = new RsaSigningKeyService();
        }

        [Fact]
        public void SignAndVerify_ValidData_ReturnsTrue()
        {
            // Arrange
            var keyPairJson = _signingKeyService.GenerateSigningKeyPair();
            var privateKeyPem = ExtractPrivateKey(keyPairJson);
            var publicKeyPem = ExtractPublicKey(keyPairJson);
            var data = "test-data-to-sign-12345";

            // Act
            var signature = _signingKeyService.SignData(data, privateKeyPem);
            var result = _signingKeyService.VerifySignature(data, signature, publicKeyPem);

            // Assert
            result.Should().BeTrue("a valid signature over unmodified data should verify successfully");
        }

        [Fact]
        public void SignAndVerify_TamperedData_ReturnsFalse()
        {
            // Arrange
            var keyPairJson = _signingKeyService.GenerateSigningKeyPair();
            var privateKeyPem = ExtractPrivateKey(keyPairJson);
            var publicKeyPem = ExtractPublicKey(keyPairJson);
            var originalData = "original-data-to-sign";
            var tamperedData = "tampered-data-to-sign";

            // Act
            var signature = _signingKeyService.SignData(originalData, privateKeyPem);
            var result = _signingKeyService.VerifySignature(tamperedData, signature, publicKeyPem);

            // Assert
            result.Should().BeFalse("verifying a signature against tampered data must fail");
        }

        [Fact]
        public void SignAndVerify_WrongKey_ReturnsFalse()
        {
            // Arrange
            var keyPair1 = _signingKeyService.GenerateSigningKeyPair();
            var keyPair2 = _signingKeyService.GenerateSigningKeyPair();

            var privateKey1 = ExtractPrivateKey(keyPair1);
            var publicKey2 = ExtractPublicKey(keyPair2); // Use the OTHER key pair's public key

            var data = "data-signed-with-key1";

            // Act
            var signature = _signingKeyService.SignData(data, privateKey1);
            var result = _signingKeyService.VerifySignature(data, signature, publicKey2);

            // Assert
            result.Should().BeFalse("verifying with the wrong public key must fail");
        }

        [Fact]
        public void GenerateKeyPair_ReturnsValidKeys()
        {
            // Act
            var keyPairJson = _signingKeyService.GenerateSigningKeyPair();

            // Assert
            keyPairJson.Should().NotBeNullOrWhiteSpace("key pair JSON must be generated");
            keyPairJson.Should().Contain("PrivateKeyPem", "key pair must contain a private key");
            keyPairJson.Should().Contain("PublicKeyPem", "key pair must contain a public key");

            var privateKey = ExtractPrivateKey(keyPairJson);
            var publicKey = ExtractPublicKey(keyPairJson);

            privateKey.Should().StartWith("-----BEGIN PRIVATE KEY-----", "private key must be PEM-formatted");
            publicKey.Should().StartWith("-----BEGIN PUBLIC KEY-----", "public key must be PEM-formatted");
        }

        [Fact]
        public void SignData_ReturnsNonEmptySignature()
        {
            // Arrange
            var keyPairJson = _signingKeyService.GenerateSigningKeyPair();
            var privateKeyPem = ExtractPrivateKey(keyPairJson);
            var data = "data-to-sign";

            // Act
            var signature = _signingKeyService.SignData(data, privateKeyPem);

            // Assert
            signature.Should().NotBeNullOrWhiteSpace("signature must be non-empty");
            // Verify it is valid Base64
            Action act = () => Convert.FromBase64String(signature);
            act.Should().NotThrow("signature must be valid Base64");
        }

        [Fact]
        public void VerifySignature_EmptySignature_ReturnsFalse()
        {
            // Arrange
            var keyPairJson = _signingKeyService.GenerateSigningKeyPair();
            var publicKeyPem = ExtractPublicKey(keyPairJson);
            var data = "data-to-verify";

            // Act & Assert
            Action act = () => _signingKeyService.VerifySignature(data, "", publicKeyPem);
            act.Should().Throw<ArgumentNullException>("empty signature should throw");
        }

        /// <summary>
        /// Extracts the PrivateKeyPem value from the JSON returned by GenerateSigningKeyPair.
        /// </summary>
        private static string ExtractPrivateKey(string keyPairJson)
        {
            // Simple extraction: "PrivateKeyPem":"-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----"
            var startMarker = "\"PrivateKeyPem\":\"";
            var startIndex = keyPairJson.IndexOf(startMarker, StringComparison.Ordinal) + startMarker.Length;
            var endIndex = keyPairJson.IndexOf("\"", startIndex);
            var raw = keyPairJson.Substring(startIndex, endIndex - startIndex);
            return raw.Replace("\\n", "\n");
        }

        /// <summary>
        /// Extracts the PublicKeyPem value from the JSON returned by GenerateSigningKeyPair.
        /// </summary>
        private static string ExtractPublicKey(string keyPairJson)
        {
            var startMarker = "\"PublicKeyPem\":\"";
            var startIndex = keyPairJson.IndexOf(startMarker, StringComparison.Ordinal) + startMarker.Length;
            var endIndex = keyPairJson.IndexOf("\"", startIndex);
            var raw = keyPairJson.Substring(startIndex, endIndex - startIndex);
            return raw.Replace("\\n", "\n");
        }
    }
}
