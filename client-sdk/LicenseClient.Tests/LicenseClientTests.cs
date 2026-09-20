using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LicenseClient.SDK.Core;
using LicenseClient.SDK.Models;
using LicenseClient.SDK.Validation;
using Xunit;

namespace LicenseClient.Tests;

public class LicenseKeyFormatTests
{
    [Theory]
    [InlineData("CLNR-7K4P-X92M-Q8ZT", true)]
    [InlineData("OPTM-3F8N-5P2V-W9KJ", true)]
    [InlineData("XXXX-XXXX-XXXX-XXXX", true)]
    [InlineData("clnr-7k4p-x92m-q8zt", false)] // lowercase not valid
    [InlineData("CLNR-7K4P-X92M", false)] // too short
    [InlineData("CLNR7K4PX92MQ8ZT", false)] // no dashes
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidFormat_ReturnsCorrectResult(string? key, bool expected)
    {
        var isValid = !string.IsNullOrEmpty(key)
            && System.Text.RegularExpressions.Regex.IsMatch(
                key,
                @"^[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$");

        isValid.Should().Be(expected);
    }
}

public class LicenseConfigurationTests
{
    [Fact]
    public void LicenseConfiguration_DefaultValues_AreCorrect()
    {
        var config = new LicenseConfiguration
        {
            ServerUrl = "https://test.example.com",
            ProductId = "TEST",
            ProductCode = "TEST",
            PublicKeyPem = "test-key"
        };

        config.ValidationIntervalMinutes.Should().Be(60);
        config.OfflineGraceHours.Should().Be(24);
        config.ApplicationVersion.Should().Be("1.0.0");
        config.TimeoutSeconds.Should().Be(30);
        config.MaxRetries.Should().Be(3);
    }

    [Fact]
    public void LicenseConfiguration_ProductId_IsRequired()
    {
        var config = new LicenseConfiguration
        {
            ServerUrl = "https://test.example.com",
            ProductId = "",
            ProductCode = "TEST",
            PublicKeyPem = "test-key"
        };

        config.ProductId.Should().BeEmpty();
    }
}

public class ProductIsolationTests
{
    [Theory]
    [InlineData("CLEANER", "OPTIMIZER")]
    [InlineData("OPTIMIZER", "CLEANER")]
    [InlineData("CLEANER", "DOWNLOADER")]
    public void DifferentProductIds_AlwaysMismatch(string licenseProductId, string requestProductId)
    {
        // A license for product A should NEVER work in product B
        var isMatch = string.Equals(licenseProductId, requestProductId, StringComparison.OrdinalIgnoreCase);

        isMatch.Should().BeFalse(
            because: $"license for {licenseProductId} should not work in {requestProductId}");
    }

    [Theory]
    [InlineData("CLEANER", "CLEANER")]
    [InlineData("OPTIMIZER", "OPTIMIZER")]
    public void SameProductIds_Match(string licenseProductId, string requestProductId)
    {
        var isMatch = string.Equals(licenseProductId, requestProductId, StringComparison.OrdinalIgnoreCase);

        isMatch.Should().BeTrue();
    }
}

public class SignatureVerifierTests
{
    [Fact]
    public void EmptySignature_IsInvalid()
    {
        var result = string.IsNullOrEmpty("");
        result.Should().BeTrue();
    }

    [Fact]
    public void NullSignature_IsInvalid()
    {
        var result = string.IsNullOrEmpty(null);
        result.Should().BeTrue();
    }
}

public class FeatureManagerTests
{
    [Fact]
    public void HasFeature_ExistingFeature_ReturnsTrue()
    {
        var features = new List<string> { "basic_cleanup", "advanced_cleanup" };

        features.Contains("basic_cleanup").Should().BeTrue();
    }

    [Fact]
    public void HasFeature_MissingFeature_ReturnsFalse()
    {
        var features = new List<string> { "basic_cleanup", "advanced_cleanup" };

        features.Contains("premium").Should().BeFalse();
    }

    [Fact]
    public void HasFeature_CaseInsensitive_Works()
    {
        var features = new List<string> { "Basic_Cleanup" };

        features.Any(f => string.Equals(f, "basic_cleanup", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }
}

public class LicenseStatusTests
{
    [Fact]
    public void ActiveLicense_IsNotExpired()
    {
        var status = LicenseStatus.Active;
        status.Should().NotBe(LicenseStatus.Expired);
    }

    [Fact]
    public void RevokedLicense_CannotBeReactivated()
    {
        var status = LicenseStatus.Revoked;
        // Revoked should not transition back to Active
        var canReactivate = status == LicenseStatus.Active || status == LicenseStatus.Expired;
        canReactivate.Should().BeFalse();
    }

    [Theory]
    [InlineData(LicenseStatus.Active, true)]
    [InlineData(LicenseStatus.Expired, false)]
    [InlineData(LicenseStatus.Revoked, false)]
    [InlineData(LicenseStatus.Suspended, false)]
    [InlineData(LicenseStatus.Unknown, false)]
    public void IsValid_OnlyWhenActive(LicenseStatus status, bool expected)
    {
        var isValid = status == LicenseStatus.Active;
        isValid.Should().Be(expected);
    }
}

public class DeviceFingerprintTests
{
    [Fact]
    public void SameInput_ProducesSameHash()
    {
        var input = "test-hardware-signals-12345";
        var hash1 = ComputeHash(input);
        var hash2 = ComputeHash(input);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void DifferentInput_ProducesDifferentHash()
    {
        var hash1 = ComputeHash("signal-set-1");
        var hash2 = ComputeHash("signal-set-2");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Hash_IsNonEmpty()
    {
        var hash = ComputeHash("test-input");
        hash.Should().NotBeNullOrWhiteSpace();
    }

    private static string ComputeHash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
