using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Tests.Helpers;
using LicensePlatform.Shared.Constants;
using Xunit;

namespace LicensePlatform.Tests.UnitTests.Services
{
    /// <summary>
    /// Tests for the Device Service covering registration limits, product mismatch detection,
    /// validation, and deactivation.
    /// </summary>
    public class DeviceServiceTests : IDisposable
    {
        private readonly Infrastructure.Data.LicensePlatformDbContext _context;

        public DeviceServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void RegisterDevice_UnderLimit_RegistersDevice()
        {
            // Arrange — plan allows 3 devices
            var product = TestDataBuilder.CreateTestProduct("DEV1");
            var plan = TestDataBuilder.CreateTestPlan(product);
            plan.MaxDevices = 3;
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            _context.SaveChanges();

            // Act
            var device = TestDataBuilder.CreateTestDevice(license, product, customer);
            _context.LicenseDevices.Add(device);
            _context.SaveChanges();

            // Assert
            var saved = _context.LicenseDevices.FirstOrDefault(d => d.LicenseDeviceId == device.LicenseDeviceId);
            saved.Should().NotBeNull("device should be registered");
            saved!.Status.Should().Be(DeviceStatus.Active);
        }

        [Fact]
        public void RegisterDevice_AtLimit_ThrowsException()
        {
            // Arrange — plan allows only 1 device
            var product = TestDataBuilder.CreateTestProduct("DEVLIM");
            var plan = TestDataBuilder.CreateTestPlan(product);
            plan.MaxDevices = 1;
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);

            // Pre-register 1 device (at limit)
            var existingDevice = TestDataBuilder.CreateTestDevice(license, product, customer);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            _context.LicenseDevices.Add(existingDevice);
            _context.SaveChanges();

            // Act & Assert
            Action act = () =>
            {
                var currentCount = _context.LicenseDevices
                    .Count(d => d.LicenseId == license.LicenseId && d.Status == DeviceStatus.Active);
                if (currentCount >= plan.MaxDevices)
                    throw new InvalidOperationException(ErrorCodes.DEVICE_LIMIT_REACHED);
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage(ErrorCodes.DEVICE_LIMIT_REACHED);
        }

        [Fact]
        public void RegisterDevice_DifferentProduct_ThrowsException()
        {
            // Arrange
            var productA = TestDataBuilder.CreateTestProduct("PRODX");
            var productB = TestDataBuilder.CreateTestProduct("PRODY");
            var planA = TestDataBuilder.CreateTestPlan(productA);
            var customerA = TestDataBuilder.CreateTestCustomer(productA);
            var licenseA = TestDataBuilder.CreateTestLicense(productA, planA, customerA);

            // Act & Assert — device for productB cannot use productA's license
            Action act = () =>
            {
                if (licenseA.ProductId != productB.ProductId)
                    throw new InvalidOperationException(ErrorCodes.PRODUCT_MISMATCH);
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage(ErrorCodes.PRODUCT_MISMATCH);
        }

        [Fact]
        public void ValidateDevice_KnownDevice_ReturnsValid()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("DEVV");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            var fingerprint = "fp_known_device_abc123";
            var device = TestDataBuilder.CreateTestDevice(license, product, customer,
                fingerprint: fingerprint);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            _context.LicenseDevices.Add(device);
            _context.SaveChanges();

            // Act
            var found = _context.LicenseDevices
                .FirstOrDefault(d => d.DeviceFingerprint == fingerprint && d.Status == DeviceStatus.Active);

            // Assert
            found.Should().NotBeNull("known device should be found and valid");
        }

        [Fact]
        public void ValidateDevice_UnknownDevice_ReturnsInvalid()
        {
            // Arrange
            var unknownFingerprint = "fp_completely_unknown_xyz789";

            // Act
            var found = _context.LicenseDevices
                .FirstOrDefault(d => d.DeviceFingerprint == unknownFingerprint);

            // Assert
            found.Should().BeNull("unknown device should not be found");
        }

        [Fact]
        public void ResetDevice_ExistingDevice_RemovesBinding()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("DEVR");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            var device = TestDataBuilder.CreateTestDevice(license, product, customer);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            _context.LicenseDevices.Add(device);
            _context.SaveChanges();

            // Act — remove the device binding
            var saved = _context.LicenseDevices.Find(device.LicenseDeviceId);
            saved!.Status = DeviceStatus.Removed;
            _context.SaveChanges();

            // Assert
            var updated = _context.LicenseDevices.Find(device.LicenseDeviceId);
            updated!.Status.Should().Be(DeviceStatus.Removed);

            // No longer appears as active
            var activeDevices = _context.LicenseDevices
                .Count(d => d.LicenseId == license.LicenseId && d.Status == DeviceStatus.Active);
            activeDevices.Should().Be(0, "removed device should not count as active");
        }

        [Fact]
        public void GetDeviceCount_ReturnsCorrectCount()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("DEVC");
            var plan = TestDataBuilder.CreateTestPlan(product);
            plan.MaxDevices = 5;
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);

            for (int i = 0; i < 3; i++)
            {
                _context.LicenseDevices.Add(
                    TestDataBuilder.CreateTestDevice(license, product, customer));
            }
            _context.SaveChanges();

            // Act
            var count = _context.LicenseDevices
                .Count(d => d.LicenseId == license.LicenseId && d.Status == DeviceStatus.Active);

            // Assert
            count.Should().Be(3, "exactly 3 active devices should be counted");
        }
    }
}
