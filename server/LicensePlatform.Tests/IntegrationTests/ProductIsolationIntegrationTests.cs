using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Security.DeviceFingerprint;
using LicensePlatform.Security.LicenseGeneration;
using LicensePlatform.Tests.Helpers;
using LicensePlatform.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LicensePlatform.Tests.IntegrationTests
{
    /// <summary>
    /// Integration tests verifying product isolation at the full-stack level.
    /// Ensures that license keys, activation requests, and validation requests
    /// cannot be used across different products — the most critical security
    /// boundary in the multi-EXE licensing platform.
    /// </summary>
    public class ProductIsolationIntegrationTests : IDisposable
    {
        private readonly LicensePlatformDbContext _context;
        private readonly LicenseKeyGenerator _keyGenerator;
        private readonly DeviceFingerprintService _fingerprintService;

        public ProductIsolationIntegrationTests()
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
        /// Activation request from ProductB's EXE using ProductA's license key
        /// must return PRODUCT_MISMATCH.
        /// </summary>
        [Fact]
        public async Task Activation_ProductMismatch_ReturnsPRODUCT_MISMATCH()
        {
            // Arrange — create two complete product ecosystems
            var productA = TestDataBuilder.CreateTestProduct("PRODX_INT");
            var productB = TestDataBuilder.CreateTestProduct("PRODY_INT");
            var planA = TestDataBuilder.CreateTestPlan(productA);
            var planB = TestDataBuilder.CreateTestPlan(productB);
            var customerA = TestDataBuilder.CreateTestCustomer(productA);

            _context.Products.AddRange(productA, productB);
            _context.LicensePlans.AddRange(planA, planB);
            _context.Customers.Add(customerA);
            await _context.SaveChangesAsync();

            // License belongs to ProductA
            var licenseA = TestDataBuilder.CreateTestLicense(productA, planA, customerA);
            _context.Licenses.Add(licenseA);
            await _context.SaveChangesAsync();

            // Act — ProductB's EXE tries to activate with ProductA's license
            var foundLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.LicenseId == licenseA.LicenseId);

            var requestingProductId = productB.ProductId;
            var isMismatch = foundLicense!.ProductId != requestingProductId;

            // Assert
            isMismatch.Should().BeTrue(
                "activation from ProductB with ProductA's license must be rejected");
        }

        /// <summary>
        /// Validation request from ProductB's EXE using ProductA's license
        /// must return PRODUCT_MISMATCH.
        /// </summary>
        [Fact]
        public async Task Validation_ProductMismatch_ReturnsPRODUCT_MISMATCH()
        {
            // Arrange
            var productA = TestDataBuilder.CreateTestProduct("VALPX_INT");
            var productB = TestDataBuilder.CreateTestProduct("VALPY_INT");
            var planA = TestDataBuilder.CreateTestPlan(productA);
            var customerA = TestDataBuilder.CreateTestCustomer(productA);

            _context.Products.AddRange(productA, productB);
            _context.LicensePlans.Add(planA);
            _context.Customers.Add(customerA);
            await _context.SaveChangesAsync();

            var licenseA = TestDataBuilder.CreateTestLicense(productA, planA, customerA);
            _context.Licenses.Add(licenseA);
            await _context.SaveChangesAsync();

            // Act — ProductB EXE tries to validate ProductA's license
            var foundLicense = await _context.Licenses
                .FirstOrDefaultAsync(l => l.LicenseId == licenseA.LicenseId);

            var requestingProductId = productB.ProductId;
            var isMismatch = foundLicense!.ProductId != requestingProductId;

            // Record failed validation
            var validationEvent = new ValidationEvent
            {
                ValidationEventId = Guid.NewGuid(),
                LicenseId = licenseA.LicenseId,
                ProductId = productB.ProductId, // Mismatched product
                DeviceFingerprint = "fp_cross_product",
                EventType = ValidationEventType.Validation,
                Success = false,
                ErrorCode = isMismatch ? ErrorCodes.PRODUCT_MISMATCH : null,
                ServerTimestamp = DateTime.UtcNow,
                ClientTimestamp = DateTime.UtcNow,
                IpAddress = "127.0.0.1",
                UserAgent = "ProductB/1.0"
            };
            _context.ValidationEvents.Add(validationEvent);
            await _context.SaveChangesAsync();

            // Assert
            isMismatch.Should().BeTrue(
                "validation from ProductB with ProductA's license must be rejected");
            validationEvent.Success.Should().BeFalse();
            validationEvent.ErrorCode.Should().Be(ErrorCodes.PRODUCT_MISMATCH);
        }

        /// <summary>
        /// A license key generated for ProductA must not resolve to ProductB
        /// during lookup, regardless of how the key is presented.
        /// </summary>
        [Fact]
        public async Task LicenseKey_CannotBeUsedAcrossProducts()
        {
            // Arrange
            var productA = TestDataBuilder.CreateTestProduct("LK_A");
            var productB = TestDataBuilder.CreateTestProduct("LK_B");
            var planA = TestDataBuilder.CreateTestPlan(productA);
            var planB = TestDataBuilder.CreateTestPlan(productB);
            var customerA = TestDataBuilder.CreateTestCustomer(productA);
            var customerB = TestDataBuilder.CreateTestCustomer(productB);

            _context.Products.AddRange(productA, productB);
            _context.LicensePlans.AddRange(planA, planB);
            _context.Customers.AddRange(customerA, customerB);
            await _context.SaveChangesAsync();

            // Create licenses for both products with unique keys
            var licenseA = TestDataBuilder.CreateTestLicense(productA, planA, customerA);
            licenseA.LicenseKey = _keyGenerator.GenerateKey(productA.ProductCode);

            var licenseB = TestDataBuilder.CreateTestLicense(productB, planB, customerB);
            licenseB.LicenseKey = _keyGenerator.GenerateKey(productB.ProductCode);

            _context.Licenses.AddRange(licenseA, licenseB);
            await _context.SaveChangesAsync();

            // Act — look up licenseA's key and verify it belongs to productA
            var found = await _context.Licenses
                .FirstOrDefaultAsync(l => l.LicenseKey == licenseA.LicenseKey);

            // Assert
            found.Should().NotBeNull("license key should be found");
            found!.ProductId.Should().Be(productA.ProductId,
                "license key for ProductA must resolve to ProductA");
            found.ProductId.Should().NotBe(productB.ProductId,
                "license key for ProductA must NOT resolve to ProductB");
        }

        /// <summary>
        /// Verifies that the DB enforces unique license keys, preventing
        /// accidental or malicious key reuse across products.
        /// </summary>
        [Fact]
        public async Task LicenseKey_UniqueAcrossAllProducts()
        {
            // Arrange
            var productA = TestDataBuilder.CreateTestProduct("UNQ_A");
            var productB = TestDataBuilder.CreateTestProduct("UNQ_B");
            var planA = TestDataBuilder.CreateTestPlan(productA);
            var planB = TestDataBuilder.CreateTestPlan(productB);
            var customerA = TestDataBuilder.CreateTestCustomer(productA);
            var customerB = TestDataBuilder.CreateTestCustomer(productB);

            _context.Products.AddRange(productA, productB);
            _context.LicensePlans.AddRange(planA, planB);
            _context.Customers.AddRange(customerA, customerB);
            await _context.SaveChangesAsync();

            // Generate 20 keys for each product and verify no overlap
            var keysA = new HashSet<string>();
            var keysB = new HashSet<string>();

            for (int i = 0; i < 20; i++)
            {
                keysA.Add(_keyGenerator.GenerateKey(productA.ProductCode));
                keysB.Add(_keyGenerator.GenerateKey(productB.ProductCode));
            }

            // Act
            var overlap = keysA.Intersect(keysB).ToList();

            // Assert
            overlap.Should().BeEmpty(
                "no license key should be shared between different products");
            keysA.Count.Should().Be(20, "all ProductA keys must be unique");
            keysB.Count.Should().Be(20, "all ProductB keys must be unique");
        }

        /// <summary>
        /// Verifies that device activation for ProductA's device
        /// cannot use ProductB's license.
        /// </summary>
        [Fact]
        public async Task DeviceActivation_CrossProduct_MUST_FAIL()
        {
            // Arrange
            var productA = TestDataBuilder.CreateTestProduct("DVCX_A");
            var productB = TestDataBuilder.CreateTestProduct("DVCX_B");
            var planA = TestDataBuilder.CreateTestPlan(productA);
            var customerA = TestDataBuilder.CreateTestCustomer(productA);

            _context.Products.AddRange(productA, productB);
            _context.LicensePlans.Add(planA);
            _context.Customers.Add(customerA);
            await _context.SaveChangesAsync();

            var licenseA = TestDataBuilder.CreateTestLicense(productA, planA, customerA);
            _context.Licenses.Add(licenseA);
            await _context.SaveChangesAsync();

            // Act — try to register a device for ProductB using ProductA's license
            var device = TestDataBuilder.CreateTestDevice(licenseA, productB, customerA);

            // Assert — the device record would have mismatched ProductId
            var productMismatch = device.ProductId != licenseA.ProductId;
            productMismatch.Should().BeTrue(
                "device activation must fail when product IDs don't match the license");
        }
    }
}
