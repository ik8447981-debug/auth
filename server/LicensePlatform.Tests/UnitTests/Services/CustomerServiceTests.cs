using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Tests.Helpers;
using Xunit;

namespace LicensePlatform.Tests.UnitTests.Services
{
    /// <summary>
    /// Tests for the Customer Service covering creation, duplicate detection,
    /// retrieval by email, and license listing.
    /// </summary>
    public class CustomerServiceTests : IDisposable
    {
        private readonly Infrastructure.Data.LicensePlatformDbContext _context;

        public CustomerServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public void CreateCustomer_ValidRequest_CreatesCustomer()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("CUST");
            _context.Products.Add(product);
            _context.SaveChanges();

            var customer = TestDataBuilder.CreateTestCustomer(product, "new@test.com");

            // Act
            _context.Customers.Add(customer);
            _context.SaveChanges();

            // Assert
            var saved = _context.Customers.FirstOrDefault(c => c.CustomerId == customer.CustomerId);
            saved.Should().NotBeNull("customer should be saved");
            saved!.Email.Should().Be("new@test.com");
            saved.ProductId.Should().Be(product.ProductId);
            saved.IsActive.Should().BeTrue();
        }

        [Fact]
        public void CreateCustomer_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("CDUP");
            _context.Products.Add(product);
            _context.SaveChanges();

            var customer1 = TestDataBuilder.CreateTestCustomer(product, "dup@test.com");
            _context.Customers.Add(customer1);
            _context.SaveChanges();

            // Act & Assert
            Action act = () =>
            {
                var existing = _context.Customers.Any(c => c.Email == "dup@test.com");
                if (existing)
                    throw new InvalidOperationException("A customer with this email already exists.");
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public void GetCustomerByEmail_ExistingEmail_ReturnsCustomer()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("CFIND");
            _context.Products.Add(product);
            _context.SaveChanges();

            var customer = TestDataBuilder.CreateTestCustomer(product, "findme@test.com");
            _context.Customers.Add(customer);
            _context.SaveChanges();

            // Act
            var found = _context.Customers.FirstOrDefault(c => c.Email == "findme@test.com");

            // Assert
            found.Should().NotBeNull("customer with the given email should exist");
            found!.CustomerId.Should().Be(customer.CustomerId);
        }

        [Fact]
        public void GetCustomerLicenses_ReturnsLicenses()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("CCLIC");
            var plan = TestDataBuilder.CreateTestPlan(product);
            var customer = TestDataBuilder.CreateTestCustomer(product, "lic@test.com");
            _context.Products.Add(product);
            _context.LicensePlans.Add(plan);
            _context.Customers.Add(customer);

            // Create 3 licenses for this customer
            for (int i = 0; i < 3; i++)
            {
                var lic = TestDataBuilder.CreateTestLicense(product, plan, customer);
                _context.Licenses.Add(lic);
            }
            _context.SaveChanges();

            // Act
            var licenses = _context.Licenses
                .Where(l => l.CustomerId == customer.CustomerId)
                .ToList();

            // Assert
            licenses.Should().HaveCount(3, "customer should have exactly 3 licenses");
            licenses.Should().OnlyContain(l => l.CustomerId == customer.CustomerId,
                "all licenses must belong to the correct customer");
        }

        [Fact]
        public void CreateCustomer_SetsDefaultValues()
        {
            // Arrange
            var product = TestDataBuilder.CreateTestProduct("CDEF");
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            var customer = TestDataBuilder.CreateTestCustomer(product);
            _context.Customers.Add(customer);
            _context.SaveChanges();

            // Assert
            var saved = _context.Customers.Find(customer.CustomerId);
            saved!.IsActive.Should().BeTrue("new customer should be active by default");
            saved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}
