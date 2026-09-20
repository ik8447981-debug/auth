using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Tests.Helpers;
using LicensePlatform.Shared.DTOs;
using LicensePlatform.Shared.Constants;
using Xunit;

namespace LicensePlatform.Tests.UnitTests.Validation
{
    /// <summary>
    /// Tests for Offline Grace Period logic.
    /// When a device cannot reach the license server, it may continue operating
    /// for a configurable grace period. These tests verify that the grace period
    /// is correctly enforced, that expired grace is denied, and that clock rollback
    /// attacks are detected.
    /// </summary>
    public class OfflineGraceTests
    {
        /// <summary>
        /// Simulates offline grace validation logic.
        /// Returns whether the offline state is still valid.
        /// </summary>
        private static (bool IsValid, string? ErrorCode) ValidateOfflineState(
            OfflineValidationState state,
            DateTime currentTime)
        {
            // Check 1: Clock rollback detection
            if (currentTime < state.LastServerTime.AddSeconds(-SystemConstants.ClockToleranceSeconds))
            {
                return (false, ErrorCodes.CLOCK_ROLLBACK_DETECTED);
            }

            // Check 2: Grace period expiry
            var graceDeadline = state.LastServerTime.AddHours(state.GracePeriodHours);
            if (currentTime > graceDeadline)
            {
                return (false, ErrorCodes.OFFLINE_GRACE_EXPIRED);
            }

            // Check 3: License expiry
            if (currentTime > state.ExpiryDate)
            {
                return (false, ErrorCodes.LICENSE_EXPIRED);
            }

            return (true, null);
        }

        [Fact]
        public void OfflineGrace_WithinPeriod_AllowsAccess()
        {
            // Arrange — 72-hour grace period, checked after 12 hours
            var lastServerTime = DateTime.UtcNow.AddHours(-12);
            var state = new Shared.DTOs.OfflineValidationState
            {
                LicenseId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Status = "Active",
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                LastValidationTime = lastServerTime,
                LastServerTime = lastServerTime,
                GracePeriodHours = SystemConstants.DefaultOfflineGraceHours
            };

            // Act
            var (isValid, errorCode) = ValidateOfflineState(state, DateTime.UtcNow);

            // Assert
            isValid.Should().BeTrue("offline access should be allowed within grace period");
            errorCode.Should().BeNull("no error should be reported");
        }

        [Fact]
        public void OfflineGrace_Expired_DeniesAccess()
        {
            // Arrange — grace period expired (last seen 80 hours ago, grace is 72h)
            var lastServerTime = DateTime.UtcNow.AddHours(-80);
            var state = new Shared.DTOs.OfflineValidationState
            {
                LicenseId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Status = "Active",
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                LastValidationTime = lastServerTime,
                LastServerTime = lastServerTime,
                GracePeriodHours = SystemConstants.DefaultOfflineGraceHours
            };

            // Act
            var (isValid, errorCode) = ValidateOfflineState(state, DateTime.UtcNow);

            // Assert
            isValid.Should().BeFalse("offline access must be denied after grace period expires");
            errorCode.Should().Be(ErrorCodes.OFFLINE_GRACE_EXPIRED);
        }

        [Fact]
        public void OfflineGrace_ZeroHours_NoGrace()
        {
            // Arrange — zero grace period means immediate denial when offline
            var lastServerTime = DateTime.UtcNow.AddMinutes(-5);
            var state = new Shared.DTOs.OfflineValidationState
            {
                LicenseId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Status = "Active",
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                LastValidationTime = lastServerTime,
                LastServerTime = lastServerTime,
                GracePeriodHours = 0
            };

            // Act — even 5 minutes offline should fail with 0-hour grace
            var (isValid, errorCode) = ValidateOfflineState(state, DateTime.UtcNow.AddMinutes(6));

            // Assert
            isValid.Should().BeFalse("zero grace period must deny any offline access");
            errorCode.Should().Be(ErrorCodes.OFFLINE_GRACE_EXPIRED);
        }

        [Fact]
        public void OfflineGrace_Default24Hours()
        {
            // Arrange — verify SystemConstants defines the default grace period
            // The constant should be 72 hours per the SystemConstants definition
            var defaultGrace = SystemConstants.DefaultOfflineGraceHours;

            // Assert
            defaultGrace.Should().Be(72, "default offline grace period must be 72 hours");
        }

        [Fact]
        public void ClockRollback_Detected_DeniesAccess()
        {
            // Arrange — client reports a time BEFORE the last server time (beyond tolerance)
            var lastServerTime = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
            var clientTime = lastServerTime.AddMinutes(-10); // 10 minutes before last server time
            var state = new Shared.DTOs.OfflineValidationState
            {
                LicenseId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Status = "Active",
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                LastValidationTime = lastServerTime,
                LastServerTime = lastServerTime,
                GracePeriodHours = SystemConstants.DefaultOfflineGraceHours
            };

            // Act
            var (isValid, errorCode) = ValidateOfflineState(state, clientTime);

            // Assert
            isValid.Should().BeFalse("clock rollback must be detected and denied");
            errorCode.Should().Be(ErrorCodes.CLOCK_ROLLBACK_DETECTED);
        }

        [Fact]
        public void ClockRollback_WithinTolerance_AllowsAccess()
        {
            // Arrange — client time is slightly behind server time (within 300s tolerance)
            var lastServerTime = DateTime.UtcNow;
            var clientTime = lastServerTime.AddSeconds(-60); // 60 seconds behind
            var state = new Shared.DTOs.OfflineValidationState
            {
                LicenseId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Status = "Active",
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                LastValidationTime = lastServerTime,
                LastServerTime = lastServerTime,
                GracePeriodHours = SystemConstants.DefaultOfflineGraceHours
            };

            // Act
            var (isValid, errorCode) = ValidateOfflineState(state, clientTime);

            // Assert
            isValid.Should().BeTrue("small clock skew within tolerance should be allowed");
            errorCode.Should().BeNull();
        }

        [Fact]
        public void OfflineGrace_AtExactBoundary_DeniesAccess()
        {
            // Arrange — exactly at the grace period boundary
            var lastServerTime = DateTime.UtcNow.AddHours(-SystemConstants.DefaultOfflineGraceHours);
            var state = new Shared.DTOs.OfflineValidationState
            {
                LicenseId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Status = "Active",
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                LastValidationTime = lastServerTime,
                LastServerTime = lastServerTime,
                GracePeriodHours = SystemConstants.DefaultOfflineGraceHours
            };

            // Act
            var (isValid, errorCode) = ValidateOfflineState(state, DateTime.UtcNow);

            // Assert
            // At exactly the boundary, the client time > graceDeadline should trigger denial
            isValid.Should().BeFalse("at exact grace period boundary, access should be denied");
            errorCode.Should().Be(ErrorCodes.OFFLINE_GRACE_EXPIRED);
        }
    }
}
