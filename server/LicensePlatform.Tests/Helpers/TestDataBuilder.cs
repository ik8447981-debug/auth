using System;
using System.Security.Cryptography;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Shared.Constants;

namespace LicensePlatform.Tests.Helpers
{
    /// <summary>
    /// Centralized factory for creating test entities with valid default values.
    /// Each method produces a fully-populated entity that passes validation,
    /// avoiding the need to construct objects from scratch in every test.
    /// </summary>
    public static class TestDataBuilder
    {
        private static Guid _productCounter = Guid.NewGuid();
        private static int _licenseCounter = 0;

        /// <summary>
        /// Creates a Product entity with valid defaults.
        /// Each call generates a unique ProductId and ProductCode.
        /// </summary>
        public static Product CreateTestProduct(string? productCode = null, string? productName = null)
        {
            var code = productCode ?? $"PRD{_productCounter.ToString()[..8].ToUpper()}";
            _productCounter = Guid.NewGuid();

            return new Product
            {
                ProductId = Guid.NewGuid(),
                ProductName = productName ?? $"Test Product {code}",
                DisplayName = $"Test Product {code} Display",
                Description = $"Description for test product {code}",
                ProductCode = code,
                Status = ProductStatus.Active,
                CurrentVersion = "1.0.0",
                MinimumSupportedVersion = "1.0.0",
                LatestVersion = "1.0.0",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
        }

        /// <summary>
        /// Creates a License entity with valid defaults linked to the given product and plan.
        /// </summary>
        public static License CreateTestLicense(
            Product? product = null,
            LicensePlan? plan = null,
            Customer? customer = null,
            LicenseStatus status = LicenseStatus.Active,
            LicenseType type = LicenseType.Monthly)
        {
            var prod = product ?? CreateTestProduct();
            var pl = plan ?? CreateTestPlan(prod);
            var cust = customer ?? CreateTestCustomer(prod);

            _licenseCounter++;
            var expiryDays = type switch
            {
                LicenseType.Trial => 7,
                LicenseType.Daily => 1,
                LicenseType.Weekly => 7,
                LicenseType.Monthly => 30,
                LicenseType.Quarterly => 90,
                LicenseType.Yearly => 365,
                LicenseType.Lifetime => 0,
                _ => 30
            };

            var startDate = DateTime.UtcNow.AddDays(-10);
            DateTime? expiryDate = type == LicenseType.Lifetime
                ? null
                : startDate.AddDays(expiryDays);

            return new License
            {
                LicenseId = Guid.NewGuid(),
                LicenseKey = GetTestLicenseKey(prod.ProductCode),
                ProductId = prod.ProductId,
                CustomerId = cust?.CustomerId,
                PlanId = pl.PlanId,
                LicenseType = type,
                Status = status,
                StartDate = startDate,
                ExpiryDate = expiryDate,
                MaxDevices = pl.MaxDevices,
                ActivatedAt = status == LicenseStatus.Active ? DateTime.UtcNow : null,
                LastValidatedAt = null,
                FailedValidationCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Product = prod,
                Customer = cust,
                Plan = pl
            };
        }

        /// <summary>
        /// Creates a Customer entity with valid defaults.
        /// </summary>
        public static Customer CreateTestCustomer(Product? product = null, string? email = null)
        {
            var prod = product ?? CreateTestProduct();
            return new Customer
            {
                CustomerId = Guid.NewGuid(),
                ProductId = prod.ProductId,
                Name = $"Test Customer {Guid.NewGuid().ToString()[..8]}",
                Email = email ?? $"customer_{Guid.NewGuid().ToString()[..8]}@test.com",
                Notes = "Test customer notes",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Product = prod
            };
        }

        /// <summary>
        /// Creates a LicensePlan entity with valid defaults linked to the given product.
        /// </summary>
        public static LicensePlan CreateTestPlan(Product? product = null, string? planName = null)
        {
            var prod = product ?? CreateTestProduct();
            return new LicensePlan
            {
                PlanId = Guid.NewGuid(),
                ProductId = prod.ProductId,
                PlanName = planName ?? $"Test Plan {Guid.NewGuid().ToString()[..6]}",
                DurationDays = 30,
                MaxDevices = 3,
                OfflineGraceHours = SystemConstants.DefaultOfflineGraceHours,
                ValidationIntervalMinutes = SystemConstants.DefaultValidationIntervalMinutes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Product = prod
            };
        }

        /// <summary>
        /// Creates a LicenseDevice entity with valid defaults.
        /// </summary>
        public static LicenseDevice CreateTestDevice(
            License? license = null,
            Product? product = null,
            Customer? customer = null,
            DeviceStatus status = DeviceStatus.Active,
            string? fingerprint = null)
        {
            var lic = license ?? CreateTestLicense();
            var prod = product ?? lic.Product;
            var cust = customer ?? lic.Customer!;

            return new LicenseDevice
            {
                LicenseDeviceId = Guid.NewGuid(),
                LicenseId = lic.LicenseId,
                ProductId = prod.ProductId,
                CustomerId = cust.CustomerId,
                DeviceFingerprint = fingerprint ?? $"fp_{Guid.NewGuid().ToString()[..16]}",
                DeviceName = $"Test Device {Guid.NewGuid().ToString()[..8]}",
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                ActivationDate = DateTime.UtcNow,
                Status = status,
                ApplicationVersion = "1.0.0",
                OSVersion = "Windows 10",
                License = lic
            };
        }

        /// <summary>
        /// Creates an AdminUser entity with valid defaults.
        /// </summary>
        public static AdminUser CreateTestAdminUser(
            AdminRole role = AdminRole.Admin,
            bool isActive = true,
            string? username = null)
        {
            return new AdminUser
            {
                AdminUserId = Guid.NewGuid(),
                Username = username ?? $"admin_{Guid.NewGuid().ToString()[..8]}",
                Email = $"admin_{Guid.NewGuid().ToString()[..8]}@test.com",
                PasswordHash = BCryptHashPassword("TestPassword123!"),
                Role = role,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = null,
                FailedLoginAttempts = 0,
                LockedUntil = null
            };
        }

        /// <summary>
        /// Creates a SigningKey entity with valid defaults.
        /// </summary>
        public static SigningKey CreateTestSigningKey(
            int keyVersion = 1,
            bool isActive = true)
        {
            return new SigningKey
            {
                SigningKeyId = Guid.NewGuid(),
                KeyVersion = keyVersion,
                PublicKeyPem = GenerateTestPublicKeyPem(),
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                ActivatedAt = isActive ? DateTime.UtcNow : null,
                DeactivatedAt = null
            };
        }

        /// <summary>
        /// Creates a ValidationEvent entity with valid defaults.
        /// </summary>
        public static ValidationEvent CreateTestValidationEvent(
            License? license = null,
            Product? product = null,
            ValidationEventType eventType = ValidationEventType.Validation,
            bool success = true)
        {
            var lic = license ?? CreateTestLicense();
            var prod = product ?? lic.Product;

            return new ValidationEvent
            {
                ValidationEventId = Guid.NewGuid(),
                LicenseId = lic.LicenseId,
                ProductId = prod.ProductId,
                DeviceFingerprint = $"fp_{Guid.NewGuid().ToString()[..16]}",
                EventType = eventType,
                Success = success,
                ErrorCode = success ? null : ErrorCodes.INVALID_LICENSE,
                ServerTimestamp = DateTime.UtcNow,
                ClientTimestamp = DateTime.UtcNow,
                IpAddress = "127.0.0.1",
                UserAgent = "TestAgent/1.0",
                License = lic
            };
        }

        /// <summary>
        /// Returns a fixed product code for deterministic testing.
        /// </summary>
        public static string GetTestProductCode() => "TEST";

        /// <summary>
        /// Returns a license key in the valid format: PRODUCTCODE-XXXX-XXXX-XXXX
        /// </summary>
        public static string GetTestLicenseKey(string? productCode = null)
        {
            var code = productCode ?? "TEST";
            return $"{code.ToUpper()}-{RandomSegment()}-{RandomSegment()}-{RandomSegment()}";
        }

        /// <summary>
        /// Creates a ProductFeature entity.
        /// </summary>
        public static ProductFeature CreateTestFeature(Product? product = null, string? featureKey = null)
        {
            var prod = product ?? CreateTestProduct();
            return new ProductFeature
            {
                FeatureId = Guid.NewGuid(),
                ProductId = prod.ProductId,
                FeatureKey = featureKey ?? $"FEATURE_{Guid.NewGuid().ToString()[..6].ToUpper()}",
                FeatureName = $"Feature {featureKey ?? "Test"}",
                Description = "Test feature description",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                Product = prod
            };
        }

        /// <summary>
        /// Creates a PlanFeature join entity.
        /// </summary>
        public static PlanFeature CreateTestPlanFeature(LicensePlan? plan = null, ProductFeature? feature = null)
        {
            var pl = plan ?? CreateTestPlan();
            var feat = feature ?? CreateTestFeature();
            return new PlanFeature
            {
                Id = Guid.NewGuid(),
                PlanId = pl.PlanId,
                FeatureId = feat.FeatureId,
                Plan = pl,
                Feature = feat
            };
        }

        /// <summary>
        /// Creates a LicenseFeature join entity.
        /// </summary>
        public static LicenseFeature CreateTestLicenseFeature(License? license = null, ProductFeature? feature = null)
        {
            var lic = license ?? CreateTestLicense();
            var feat = feature ?? CreateTestFeature();
            return new LicenseFeature
            {
                LicenseFeatureId = Guid.NewGuid(),
                LicenseId = lic.LicenseId,
                FeatureId = feat.FeatureId,
                License = lic,
                Feature = feat
            };
        }

        /// <summary>
        /// Generates a random 4-character alphanumeric segment for license keys.
        /// </summary>
        private static string RandomSegment()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var result = new char[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
            return new string(result);
        }

        /// <summary>
        /// Simple BCrypt-style hash for test passwords.
        /// </summary>
        private static string BCryptHashPassword(string password)
        {
            // Use a test-compatible hash. In real tests, verify via a helper
            // that matches the application's hashing strategy.
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password + "test_salt");
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Generates a test RSA public key PEM for signing key tests.
        /// </summary>
        private static string GenerateTestPublicKeyPem()
        {
            using var rsa = RSA.Create(2048);
            var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
            var base64 = Convert.ToBase64String(publicKeyBytes, System.Base64FormattingOptions.InsertLineBreaks);
            return $"-----BEGIN PUBLIC KEY-----\n{base64}\n-----END PUBLIC KEY-----";
        }
    }
}
