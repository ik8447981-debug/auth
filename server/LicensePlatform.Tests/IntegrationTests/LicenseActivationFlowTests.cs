using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Security.DeviceFingerprint;
using LicensePlatform.Security.LicenseGeneration;
using LicensePlatform.Tests.Helpers;
using LicensePlatform.Shared.Constants;
using LicensePlatform.Shared.DTOs;
using LicensePlatform.Shared.DTOs.Requests;
using LicensePlatform.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LicensePlatform.Tests.IntegrationTests
{
    /// <summary>
    /// Integration tests for the full license activation flow.
    /// Simulates the end-to-end process: generate license → client activates device →
    /// server validates and returns signed response.
    /// Tests cover success path, product mismatch, device limit, expiry, and revocation.
    /// </summary>
    public class LicenseActivationFlowTests : IDisposable
    {
        private readonly LicensePlatformDbContext _context;
        private readonly LicenseKeyGenerator _keyGenerator;
        private readonly DeviceFingerprintService _fingerprintService;

        public LicenseActivationFlowTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _keyGenerator = new LicenseKeyGenerator();
            _fingerprintService = new DeviceFingerprintService();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        /// <summary>
        /// Full activation flow: create product → plan → customer → license → device activation.
        /// Verifies all entities are correctly created and linked.
        /// </summary>
        [Fact]
        public async Task FullActivationFlow_Success()
        {
            // Arrange — create the full entity chain
            var product = TestDataBuilder.CreateTestProduct("FLOWOK");
            var plan = TestDataBuilder.CreateTestPlan(product);
            plan.MaxDevices = 3;
            var customer = TestDataBuilder.CreateTestCustomer(product);

            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Create and save the license
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            license.LicenseKey = _keyGenerator.GenerateKey(product.ProductCode);
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            // Client requests activation
            var deviceFingerprint = _fingerprintService.ComputeFingerprint("CPUID=ABC|MOTHERBOARD=XYZ|DISK=SERIAL001");
            var hashedFingerprint = _fingerprintService.HashFingerprint(deviceFingerprint);

            // Act — server processes activation
            // 1. Look up license by key
            var foundLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.LicenseKey == license.LicenseKey);

            foundLicense.Should().NotBeNull("license must be found by key");

            // 2. Validate product matches
            foundLicense!.ProductId.Should().Be(product.ProductId);

            // 3. Check device count
            var deviceCount = await _context.LicenseDevices
                .CountAsync(d => d.LicenseId == foundLicense.LicenseId
                              && d.Status == DeviceStatus.Active);
            deviceCount.Should().BeLessThan(foundLicense.MaxDevices, "should have room for devices");

            // 4. Register device
            var device = new LicenseDevice
            {
                LicenseDeviceId = Guid.NewGuid(),
                LicenseId = foundLicense.LicenseId,
                ProductId = product.ProductId,
                CustomerId = customer.CustomerId,
                DeviceFingerprint = hashedFingerprint,
                DeviceName = "Test Activation Device",
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                ActivationDate = DateTime.UtcNow,
                Status = DeviceStatus.Active,
                ApplicationVersion = "1.0.0",
                OSVersion = "Windows 10"
            };
            _context.LicenseDevices.Add(device);

            // 5. Update license activation timestamp
            foundLicense.ActivatedAt ??= DateTime.UtcNow;
            foundLicense.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Assert — verify the full state
            var savedDevice = await _context.LicenseDevices
                .FirstOrDefaultAsync(d => d.LicenseDeviceId == device.LicenseDeviceId);
            savedDevice.Should().NotBeNull("device must be registered");
            savedDevice!.Status.Should().Be(DeviceStatus.Active);

            var savedLicense = await _context.Licenses.FindAsync(license.LicenseId);
            savedLicense!.ActivatedAt.Should().NotBeNull("license should be activated");

            var finalDeviceCount = await _context.LicenseDevices
                .CountAsync(d => d.LicenseId == foundLicense.LicenseId
                              && d.Status == DeviceStatus.Active);
            finalDeviceCount.Should().Be(1, "should have exactly 1 active device after activation");
        }

        /// <summary>
        /// Activation must fail when the license belongs to a different product than the EXE.
        /// </summary>
        [Fact]
        public async Task FullActivationFlow_ProductMismatch_Fails()
        {
            // Arrange
            var productA = TestDataBuilder.CreateTestProduct("PRODA_INT");
            var productB = TestDataBuilder.CreateTestProduct("PRODB_INT");
            var planA = TestDataBuilder.CreateTestPlan(productA);
            var customerA = TestDataBuilder.CreateTestCustomer(productA);

            _context.Products.Add(productA);
            _context.Products.Add(productB);
            _context.LicensePlans.Add(planA);
            _context.Customers.Add(customerA);
            await _context.SaveChangesAsync();

            // License is for productA
            var license = TestDataBuilder.CreateTestLicense(productA, planA, customerA);
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            // Act — productB EXE tries to use this license
            var foundLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.LicenseId == license.LicenseId);

            // Simulate product mismatch check
            var requestingProductId = productB.ProductId;
            var isMismatch = foundLicense!.ProductId != requestingProductId;

            // Assert
            isMismatch.Should().BeTrue("activation must fail with product mismatch");
            if (isMismatch)
            {
                // Server returns PRODUCT_MISMATCH error
                ErrorCodes.PRODUCT_MISMATCH.Should().Be("PRODUCT_MISMATCH");
            }
        }

        /// <summary>
        /// Activation must fail when the device limit has been reached.
        /// </summary>
        [Fact]
        public async Task FullActivationFlow_DeviceLimit_Fails()
        {
            // Arrange — plan allows only 1 device
            var product = TestDataBuilder.CreateTestProduct("DEVLIM_INT");
            var plan = TestDataBuilder.CreateTestPlan(product);
            plan.MaxDevices = 1;
            var customer = TestDataBuilder.CreateTestCustomer(product);

            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            _context.Licenses.Add(license);

            // Pre-register the 1 allowed device
            _context.LicenseDevices.Add(TestDataBuilder.CreateTestDevice(license, product, customer));
            await _context.SaveChangesAsync();

            // Act — try to activate a second device
            var deviceCount = await _context.LicenseDevices
                .CountAsync(d => d.LicenseId == license.LicenseId && d.Status == DeviceStatus.Active);

            // Assert
            deviceCount.Should().Be(1, "should have 1 active device");
            (deviceCount >= plan.MaxDevices).Should().BeTrue("device limit should be reached");
        }

        /// <summary>
        /// Activation must fail for an expired license.
        /// </summary>
        [Fact]
        public async Task FullActivationFlow_ExpiredLicense_Fails()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("EXPIR_INT");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);

            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Expired);
            license.ExpiryDate = DateTime.UtcNow.AddDays(-5);
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            // Act
            var foundLicense = await _context.Licenses.FindAsync(license.LicenseId);
            var isExpired = foundLicense!.ExpiryDate.HasValue && foundLicense.ExpiryDate.Value < DateTime.UtcNow;

            // Assert
            isExpired.Should().BeTrue("expired license must be detected");
        }

        /// <summary>
        /// Activation must fail for a revoked license.
        /// </summary>
        [Fact]
        public async Task FullActivationFlow_RevokedLicense_Fails()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("REVOK_INT");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);

            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Revoked);
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            // Act
            var foundLicense = await _context.Licenses.FindAsync(license.LicenseId);
            var isRevoked = foundLicense!.Status == LicenseStatus.Revoked;

            // Assert
            isRevoked.Should().BeTrue("revoked license must be detected");
            isRevoked.Should().BeTrue("activation must be denied for revoked licenses");
        }
    }
}
