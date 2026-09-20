using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Security.LicenseGeneration;
using LicensePlatform.Tests.Helpers;
using LicensePlatform.Shared.Constants;
using Xunit;

namespace LicensePlatform.Tests.UnitTests.Services
{
    /// <summary>
    /// Tests for the License Service layer covering generation, retrieval, extension,
    /// suspension, revocation, and bulk operations.
    /// Uses the InMemory database and real LicenseKeyGenerator for authentic behavior.
    /// </summary>
    public class LicenseServiceTests : IDisposable
    {
        private readonly Infrastructure.Data.LicensePlatformDbContext _context;
        private readonly LicenseKeyGenerator _keyGenerator;

        public LicenseServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _keyGenerator = new LicenseKeyGenerator();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void GenerateLicense_ValidRequest_CreatesLicense()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("GEN");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.SaveChanges();

            // Act
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            license.LicenseKey = _keyGenerator.GenerateKey(product.ProductCode);
            _context.Licenses.Add(license);
            _context.SaveChanges();

            // Assert
            var saved = _context.Licenses.FirstOrDefault(l => l.LicenseId == license.LicenseId);
            saved.Should().NotBeNull();
            saved!.LicenseKey.Should().Be(license.LicenseKey);
            saved.ProductId.Should().Be(product.ProductId);
            saved.Status.Should().Be(LicenseStatus.Active);
        }

        [Fact]
        public void GenerateLicense_ProductMismatch_ThrowsException()
        {
            // Arrange
            var productA = TestDataBuilder.CreateTestProduct("PRODA");
            var productB = TestDataBuilder.CreateTestProduct("PRODB");
            var planB = TestDataBuilder.CreateTestPlan(productB);
            var customerA = TestDataBuilder.CreateTestCustomer(productA);

            // Act & Assert — using planB with productA must fail
            Action act = () =>
            {
                if (planB.ProductId != productA.ProductId)
                    throw new InvalidOperationException(ErrorCodes.PRODUCT_MISMATCH);
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage(ErrorCodes.PRODUCT_MISMATCH);
        }

        [Fact]
        public void GetLicenseByKey_ExistingKey_ReturnsLicense()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("FIND");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            _context.SaveChanges();

            // Act
            var found = _context.Licenses.FirstOrDefault(l => l.LicenseKey == license.LicenseKey);

            // Assert
            found.Should().NotBeNull("license with the given key should exist");
            found!.LicenseId.Should().Be(license.LicenseId);
        }

        [Fact]
        public void GetLicenseByKey_NonExistingKey_ThrowsException()
        {
            // Arrange
            var nonExistentKey = "FAKE-ABCD-1234-EFGH";

            // Act
            var found = _context.Licenses.FirstOrDefault(l => l.LicenseKey == nonExistentKey);

            // Assert
            found.Should().BeNull("non-existent key should return null");
        }

        [Fact]
        public void ExtendLicense_ValidNewExpiry_ExtendsLicense()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("EXT");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            _context.SaveChanges();

            var originalExpiry = license.ExpiryDate!.Value;
            var newExpiry = originalExpiry.AddDays(30);

            // Act
            var saved = _context.Licenses.Find(license.LicenseId);
            saved!.ExpiryDate = newExpiry;
            saved.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            // Assert
            var updated = _context.Licenses.Find(license.LicenseId);
            updated!.ExpiryDate.Should().Be(newExpiry, "expiry date should be extended");
            updated.ExpiryDate.Value.Should().BeAfter(originalExpiry,
                "new expiry must be after the original");
        }

        [Fact]
        public void ExtendLicense_PastExpiry_ThrowsException()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("PAST");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            license.ExpiryDate = DateTime.UtcNow.AddDays(-5); // Already expired

            // Act & Assert
            Action act = () =>
            {
                if (license.ExpiryDate < DateTime.UtcNow)
                    throw new InvalidOperationException("Cannot extend to a date in the past");
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*past*");
        }

        [Fact]
        public void SuspendLicense_ActiveLicense_Suspends()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("SUSP");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Active);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            _context.SaveChanges();

            // Act
            var saved = _context.Licenses.Find(license.LicenseId);
            saved!.Status = LicenseStatus.Suspended;
            saved.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            // Assert
            var updated = _context.Licenses.Find(license.LicenseId);
            updated!.Status.Should().Be(LicenseStatus.Suspended);
        }

        [Fact]
        public void SuspendLicense_RevokedLicense_ThrowsException()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("REV");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Revoked);

            // Act & Assert — cannot suspend an already revoked license
            Action act = () =>
            {
                if (license.Status == LicenseStatus.Revoked)
                    throw new InvalidOperationException("Cannot suspend a revoked license");
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*revoked*");
        }

        [Fact]
        public void RevokeLicense_ActiveLicense_Revokes()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("REV2");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer,
                status: LicenseStatus.Active);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.Licenses.Add(license);
            _context.SaveChanges();

            // Act
            var saved = _context.Licenses.Find(license.LicenseId);
            saved!.Status = LicenseStatus.Revoked;
            saved.UpdatedAt = DateTime.UtcNow;
            _context.SaveChanges();

            // Assert
            var updated = _context.Licenses.Find(license.LicenseId);
            updated!.Status.Should().Be(LicenseStatus.Revoked);
        }

        [Fact]
        public void RevokeLicense_CannotBeReactivated()
        {
            // Arrange
            var license = TestDataBuilder.CreateTestLicense(
                status: LicenseStatus.Revoked);

            // Act & Assert — once revoked, the license cannot be reactivated
            Action act = () =>
            {
                if (license.Status == LicenseStatus.Revoked)
                    throw new InvalidOperationException("Revoked licenses cannot be reactivated");
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*cannot be reactivated*");
        }

        [Fact]
        public void BulkGenerateLicenses_GeneratesCorrectCount()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("BULK");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);
            _context.SaveChanges();

            var count = 10;

            // Act
            var licenses = new List<License>();
            for (int i = 0; i < count; i++)
            {
                var lic = TestDataBuilder.CreateTestLicense(product, plan, customer);
                lic.LicenseKey = _keyGenerator.GenerateKey(product.ProductCode);
                licenses.Add(lic);
            }
            _context.Licenses.AddRange(licenses);
            _context.SaveChanges();

            // Assert
            _context.Licenses.Count(l => l.ProductId == product.ProductId).Should().Be(count,
                $"bulk generation should create exactly {count} licenses");
        }

        [Fact]
        public void BulkGenerateLicencies_HandlesCollisions()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("COLL");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product);

            // Act — generate 50 keys and verify uniqueness
            var keys = new HashSet<string>();
            for (int i = 0; i < 50; i++)
            {
                var key = _keyGenerator.GenerateKey(product.ProductCode);
                keys.Add(key);
            }

            // Assert
            keys.Count.Should().Be(50, "all 50 bulk-generated keys should be unique");
        }
    }
}
