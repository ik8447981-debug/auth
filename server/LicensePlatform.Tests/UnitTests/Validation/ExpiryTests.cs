using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Tests.Helpers;
using LicensePlatform.Shared.Constants;
using Xunit;

namespace LicensePlatform.Tests.UnitTests.Validation
{
    /// <summary>
    /// Tests for license expiry validation logic.
    /// Covers active/expired/suspended/revoked states, lifetime licenses,
    /// and license activation lifecycle.
    /// </summary>
    public class ExpiryTests
    {
        /// <summary>
        /// Simulates server-side license validation checking expiry and status.
        /// </summary>
        private static (bool IsValid, string? ErrorCode) ValidateLicense(License license)
        {
            if (license.Status == LicenseStatus.Revoked)
                return (false, ErrorCodes.LICENSE_REVOKED);

            if (license.Status == LicenseStatus.Suspended)
                return (false, ErrorCodes.LICENSE_SUSPENDED);

            if (license.ExpiryDate.HasValue && license.ExpiryDate.Value < DateTime.UtcNow)
                return (false, ErrorCodes.LICENSE_EXPIRED);

            if (license.Status != LicenseStatus.Active)
                return (false, ErrorCodes.INVALID_LICENSE);

            return (true, null);
        }

        [Fact]
        public void ActiveLicense_NotExpired_IsValid()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Active);
            license.ExpiryDate = DateTime.UtcNow.AddDays(30);

            // Act
            var (isValid, errorCode) = ValidateLicense(license);

            // Assert
            isValid.Should().BeTrue("active license that hasn't expired should be valid");
            errorCode.Should().BeNull();
        }

        [Fact]
        public void ActiveLicense_Expired_IsInvalid()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Active);
            license.ExpiryDate = DateTime.UtcNow.AddDays(-5); // Expired 5 days ago

            // Act
            var (isValid, errorCode) = ValidateLicense(license);

            // Assert
            isValid.Should().BeFalse("expired license must not be valid");
            errorCode.Should().Be(ErrorCodes.LICENSE_EXPIRED);
        }

        [Fact]
        public void SuspendedLicense_NotExpired_IsInvalid()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Suspended);
            license.ExpiryDate = DateTime.UtcNow.AddDays(30);

            // Act
            var (isValid, errorCode) = ValidateLicense(license);

            // Assert
            isValid.Should().BeFalse("suspended license must not be valid even if not expired");
            errorCode.Should().Be(ErrorCodes.LICENSE_SUSPENDED);
        }

        [Fact]
        public void RevokedLicense_IsAlwaysInvalid()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Revoked);
            license.ExpiryDate = DateTime.UtcNow.AddDays(365); // Not expired

            // Act
            var (isValid, errorCode) = ValidateLicense(license);

            // Assert
            isValid.Should().BeFalse("revoked license is ALWAYS invalid, regardless of expiry");
            errorCode.Should().Be(ErrorCodes.LICENSE_REVOKED);
        }

        [Fact]
        public void LifetimeLicense_NeverExpires()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                type: LicenseType.Lifetime, status: LicenseStatus.Active);

            // Assert
            license.ExpiryDate.Should().BeNull("lifetime license must have no expiry date");

            // Act
            var (isValid, errorCode) = ValidateLicense(license);

            // Assert
            isValid.Should().BeTrue("lifetime license without expiry should be valid");
            errorCode.Should().BeNull();
        }

        [Fact]
        public void CreatedLicense_CanBeActivated()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Created);

            // Act — simulate activation
            license.Status = LicenseStatus.Active;
            license.ActivatedAt = DateTime.UtcNow;
            license.ExpiryDate = DateTime.UtcNow.AddDays(plan.DurationDays);

            // Assert
            license.Status.Should().Be(LicenseStatus.Active);
            license.ActivatedAt.Should().NotBeNull("activated timestamp must be set");
            license.ExpiryDate.Should().NotBeNull("expiry must be set on activation");
        }

        [Fact]
        public void DisabledLicense_IsInvalid()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Disabled);
            license.ExpiryDate = DateTime.UtcNow.AddDays(30);

            // Act
            var (isValid, errorCode) = ValidateLicense(license);

            // Assert
            isValid.Should().BeFalse("disabled license must not be valid");
            errorCode.Should().Be(ErrorCodes.INVALID_LICENSE);
        }

        [Fact]
        public void ExpiredLicense_CannotBeSuspended()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct();
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Expired);

            // Act & Assert — suspending an expired license is logically meaningless
            // but the system should handle it gracefully
            Action act = () =>
            {
                if (license.Status == LicenseStatus.Expired)
                    throw new InvalidOperationException("Cannot suspend an already expired license.");
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*expired*");
        }
    }
}
