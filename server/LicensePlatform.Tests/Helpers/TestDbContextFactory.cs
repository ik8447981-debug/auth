using System;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Tests.Helpers
{
    /// <summary>
    /// Factory for creating InMemory DbContext instances for testing.
    /// Provides isolated database instances per test to prevent cross-test pollution.
    /// </summary>
    public static class TestDbContextFactory
    {
        /// <summary>
        /// Creates a new InMemory DbContext with a unique database name.
        /// Each call produces an isolated database that will not interfere with other tests.
        /// </summary>
        public static LicensePlatformDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<LicensePlatformDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new LicensePlatformDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Creates a new InMemory DbContext with a deterministic name.
        /// Useful when you need multiple contexts to share the same in-memory store.
        /// </summary>
        public static LicensePlatformDbContext CreateInMemoryDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<LicensePlatformDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            var context = new LicensePlatformDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>
        /// Populates the database with a complete set of test data: product, plan, features,
        /// customer, license, and device. All entities are saved before returning.
        /// </summary>
        /// <returns>A tuple of all created entities for easy assertion in tests.</returns>
        public static (
            Product Product,
            LicensePlan Plan,
            ProductFeature Feature,
            PlanFeature PlanFeature,
            Customer Customer,
            License License,
            LicenseDevice Device,
            AdminUser AdminUser,
            SigningKey SigningKey
        ) SeedTestData(LicensePlatformDbContext context)
        {
            // Product
            var product = TestDataBuilder.CreateTestProduct("SEED", "Seeded Product");
            context.Products.Add(product);

            // Plan
            var plan = TestDataBuilder.CreateTestPlan(product, "Seeded Plan");
            context.LicensePlans.Add(plan);

            // Feature
            var feature = TestDataBuilder.CreateTestFeature(product, "BASIC_SCAN");
            context.ProductFeatures.Add(feature);

            // PlanFeature
            var planFeature = TestDataBuilder.CreateTestPlanFeature(plan, feature);
            context.PlanFeatures.Add(planFeature);

            // Customer
            var customer = TestDataBuilder.CreateTestCustomer(product, "seed@test.com");
            context.Customers.Add(customer);

            // License
            var license = TestDataBuilder.CreateTestLicense(product, plan, customer);
            context.Licenses.Add(license);

            // Device
            var device = TestDataBuilder.CreateTestDevice(license, product, customer);
            context.LicenseDevices.Add(device);

            // Admin
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.SuperAdmin, username: "seedadmin");
            context.AdminUsers.Add(admin);

            // Signing Key
            var signingKey = TestDataBuilder.CreateTestSigningKey(1, true);
            context.SigningKeys.Add(signingKey);

            context.SaveChanges();

            return (product, plan, feature, planFeature, customer, license, device, admin, signingKey);
        }
    }
}
