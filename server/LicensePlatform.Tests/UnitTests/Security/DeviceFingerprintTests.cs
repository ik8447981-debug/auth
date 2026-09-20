using FluentAssertions;
using LicensePlatform.Security.DeviceFingerprint;
using Xunit;

namespace LicensePlatform.Tests.UnitTests.Security
{
    /// <summary>
    /// Tests for the DeviceFingerprintService covering computation, hashing, and verification
    /// of device fingerprints used for machine binding.
    /// </summary>
    public class DeviceFingerprintTests
    {
        private readonly DeviceFingerprintService _service;

        public DeviceFingerprintTests()
        {
            _service = new DeviceFingerprintService();
        }

        [Fact]
        public void ComputeFingerprint_SameInput_SameOutput()
        {
            // Arrange
            var rawSignals = "CPUID=ABC123|MOTHERBOARD=XYZ789|DISK=SERIAL001";

            // Act
            var fp1 = _service.ComputeFingerprint(rawSignals);
            var fp2 = _service.ComputeFingerprint(rawSignals);

            // Assert
            fp1.Should().Be(fp2, "the same raw input must always produce the same fingerprint");
        }

        [Fact]
        public void ComputeFingerprint_DifferentInput_DifferentOutput()
        {
            // Arrange
            var rawSignals1 = "CPUID=ABC123|MOTHERBOARD=XYZ789|DISK=SERIAL001";
            var rawSignals2 = "CPUID=DEF456|MOTHERBOARD=UVW012|DISK=SERIAL002";

            // Act
            var fp1 = _service.ComputeFingerprint(rawSignals1);
            var fp2 = _service.ComputeFingerprint(rawSignals2);

            // Assert
            fp1.Should().NotBe(fp2, "different inputs must produce different fingerprints");
        }

        [Fact]
        public void HashFingerprint_ReturnsHash()
        {
            // Arrange
            var fingerprint = _service.ComputeFingerprint("CPUID=ABC|MOTHERBOARD=XYZ");

            // Act
            var hashed = _service.HashFingerprint(fingerprint);

            // Assert
            hashed.Should().NotBeNullOrWhiteSpace("hash must be non-empty");
            hashed.Should().NotBe(fingerprint, "hash should differ from the original fingerprint");
            // SHA256 hex output is 64 characters
            hashed.Length.Should().Be(64, "SHA256 hex hash must be 64 characters");
        }

        [Fact]
        public void VerifyFingerprint_Matching_ReturnsTrue()
        {
            // Arrange
            var raw = "CPUID=ABC|MOTHERBOARD=XYZ|DISK=SERIAL001";
            var fp = _service.ComputeFingerprint(raw);
            var hashed = _service.HashFingerprint(fp);

            // Act
            var result = _service.VerifyFingerprint(hashed, hashed);

            // Assert
            result.Should().BeTrue("identical hashes must verify as matching");
        }

        [Fact]
        public void VerifyFingerprint_NonMatching_ReturnsFalse()
        {
            // Arrange
            var raw1 = "CPUID=ABC|MOTHERBOARD=XYZ";
            var raw2 = "CPUID=DEF|MOTHERBOARD=UVW";
            var hashed1 = _service.HashFingerprint(_service.ComputeFingerprint(raw1));
            var hashed2 = _service.HashFingerprint(_service.ComputeFingerprint(raw2));

            // Act
            var result = _service.VerifyFingerprint(hashed1, hashed2);

            // Assert
            result.Should().BeFalse("different hashes must not verify as matching");
        }

        [Fact]
        public void ComputeFingerprint_CaseInsensitiveAfterNormalization()
        {
            // Arrange — normalization lowercases and strips separators
            var raw1 = "CPUID=ABC123|MOTHERBOARD=XYZ789";
            var raw2 = "cpuid=abc123|motherboard=xyz789";

            // Act
            var fp1 = _service.ComputeFingerprint(raw1);
            var fp2 = _service.ComputeFingerprint(raw2);

            // Assert
            fp1.Should().Be(fp2, "normalization should treat case differences as equal");
        }

        [Fact]
        public void ComputeFingerprint_NullInput_Throws()
        {
            // Act & Assert
            Action act = () => _service.ComputeFingerprint(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HashFingerprint_NullInput_Throws()
        {
            // Act & Assert
            Action act = () => _service.HashFingerprint(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void VerifyFingerprint_EmptyInputs_ReturnsFalse()
        {
            // Act
            var result = _service.VerifyFingerprint("", "");

            // Assert
            result.Should().BeFalse("empty strings must not verify");
        }
    }
}
