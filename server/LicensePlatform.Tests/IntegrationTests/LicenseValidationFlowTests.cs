using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Tests.Helpers;
using LicensePlatform.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LicensePlatform.Tests.IntegrationTests
{
    /// <summary>
    /// Integration tests for the license validation flow.
    /// Simulates the server-side validation process that occurs during periodic
    /// check-ins from client devices. Covers online success, expiry detection,
    /// and revocation detection.
    /// </summary>
    public class LicenseValidationFlowTests : IDisposable
    {
        private readonly LicensePlatformDbContext _context;

        public LicenseValidationFlowTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        /// <summary>
        /// Simulates a successful online validation: license is active, not expired,
        /// device is known, and the server returns a valid response.
        /// </summary>
        [Fact]
        public async Task ValidationFlow_OnlineSuccess()
        {
            // Arrange — set up full entity chain
            var seeded = TestDbContextFactory.SeedTestData(_context);
            var license = seeded.License;
            var device = seeded.Device;

            // Ensure license is active and not expired
            license.Status = LicenseStatus.Active;
            license.ExpiryDate = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            // Act — server validates
            var foundLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.LicenseId == license.LicenseId);

            var isRevoked = foundLicense!.Status == LicenseStatus.Revoked;
            var isExpired = foundLicense.ExpiryDate.HasValue && foundLicense.ExpiryDate.Value < DateTime.UtcNow;
            var isValid = !isRevoked && !isExpired && foundLicense.Status == LicenseStatus.Active;

            // Check device is registered
            var deviceExists = await _context.LicenseDevices
                .AnyAsync(d => d.LicenseId == license.LicenseId
                           && d.DeviceFingerprint == device.DeviceFingerprint
                           && d.Status == DeviceStatus.Active);

            // Record validation event
            var validationEvent = new ValidationEvent
            {
                ValidationEventId = Guid.NewGuid(),
                LicenseId = license.LicenseId,
                ProductId = seeded.Product.ProductId,
                DeviceFingerprint = device.DeviceFingerprint,
                EventType = ValidationEventType.Validation,
                Success = isValid && deviceExists,
                ErrorCode = isValid && deviceExists ? null : ErrorCodes.DEVICE_NOT_AUTHORIZED,
                ServerTimestamp = DateTime.UtcNow,
                ClientTimestamp = DateTime.UtcNow,
                IpAddress = "127.0.0.1",
                UserAgent = "TestClient/1.0"
            };
            _context.ValidationEvents.Add(validationEvent);
            await _context.SaveChangesAsync();

            // Assert
            isValid.Should().BeTrue("active, non-expired license should be valid");
            deviceExists.Should().BeTrue("known device should be found");
            validationEvent.Success.Should().BeTrue("validation event should record success");
            validationEvent.ErrorCode.Should().BeNull("no error code on success");
        }

        /// <summary>
        /// Validation must detect an expired license and return LICENSE_EXPIRED.
        /// </summary>
        [Fact]
        public async Task ValidationFlow_ExpiredLicense_DetectsExpiry()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("VAL_EXPIR");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Active);
            license.ExpiryDate = DateTime.UtcNow.AddDays(-1); // Expired yesterday

            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            // Act
            var foundLicense = await _context.Licenses.FindAsync(license.LicenseId);
            var isExpired = foundLicense!.ExpiryDate.HasValue && foundLicense.ExpiryDate.Value < DateTime.UtcNow;

            var validationEvent = new ValidationEvent
            {
                ValidationEventId = Guid.NewGuid(),
                LicenseId = license.LicenseId,
                ProductId = product.ProductId,
                DeviceFingerprint = "fp_test",
                EventType = ValidationEventType.Validation,
                Success = false,
                ErrorCode = isExpired ? ErrorCodes.LICENSE_EXPIRED : null,
                ServerTimestamp = DateTime.UtcNow,
                ClientTimestamp = DateTime.UtcNow,
                IpAddress = "127.0.0.1",
                UserAgent = "TestClient/1.0"
            };
            _context.ValidationEvents.Add(validationEvent);
            await _context.SaveChangesAsync();

            // Assert
            isExpired.Should().BeTrue("license expired yesterday must be detected");
            validationEvent.Success.Should().BeFalse();
            validationEvent.ErrorCode.Should().Be(ErrorCodes.LICENSE_EXPIRED);
        }

        /// <summary>
        /// Validation must detect a revoked license and return LICENSE_REVOKED.
        /// </summary>
        [Fact]
        public async Task ValidationFlow_RevokedLicense_DetectsRevocation()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("VAL_REVOK");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Revoked);

            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            // Act
            var foundLicense = await _context.Licenses.FindAsync(license.LicenseId);
            var isRevoked = foundLicense!.Status == LicenseStatus.Revoked;

            var validationEvent = new ValidationEvent
            {
                ValidationEventId = Guid.NewGuid(),
                LicenseId = license.LicenseId,
                ProductId = product.ProductId,
                DeviceFingerprint = "fp_test",
                EventType = ValidationEventType.Validation,
                Success = false,
                ErrorCode = isRevoked ? ErrorCodes.LICENSE_REVOKED : null,
                ServerTimestamp = DateTime.UtcNow,
                ClientTimestamp = DateTime.UtcNow,
                IpAddress = "127.0.0.1",
                UserAgent = "TestClient/1.0"
            };
            _context.ValidationEvents.Add(validationEvent);
            await _context.SaveChangesAsync();

            // Assert
            isRevoked.Should().BeTrue("revoked license must be detected");
            validationEvent.Success.Should().BeFalse();
            validationEvent.ErrorCode.Should().Be(ErrorCodes.LICENSE_REVOKED);
        }

        /// <summary>
        /// Validation records the event with correct metadata.
        /// </summary>
        [Fact]
        public async Task ValidationFlow_RecordAuditTrail()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("VAL_AUDIT");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Active);
            license.ExpiryDate = DateTime.UtcNow.AddDays(30);

            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            var now = DateTime.UtcNow;

            // Act — record validation event
            var validationEvent = new ValidationEvent
            {
                ValidationEventId = Guid.NewGuid(),
                LicenseId = license.LicenseId,
                ProductId = product.ProductId,
                DeviceFingerprint = "fp_audit_test",
                EventType = ValidationEventType.Heartbeat,
                Success = true,
                ServerTimestamp = now,
                ClientTimestamp = now,
                IpAddress = "192.168.1.100",
                UserAgent = "TestApp/2.0"
            };
            _context.ValidationEvents.Add(validationEvent);
            await _context.SaveChangesAsync();

            // Assert
            var savedEvent = await _context.ValidationEvents
                .FirstOrDefaultAsync(v => v.ValidationEventId == validationEvent.ValidationEventId);
            savedEvent.Should().NotBeNull();
            savedEvent!.LicenseId.Should().Be(license.LicenseId);
            savedEvent.IpAddress.Should().Be("192.168.1.100");
            savedEvent.UserAgent.Should().Be("TestApp/2.0");
        }
    }
}
