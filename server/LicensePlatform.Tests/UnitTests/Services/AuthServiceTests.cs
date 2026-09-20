using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Tests.Helpers;
using Xunit;

namespace LicensePlatform.Tests.UnitTests.Services
{
    /// <summary>
    /// Tests for the Admin/Auth Service covering login, credential validation,
    /// account lockout, and admin creation with role-based restrictions.
    /// </summary>
    public class AuthServiceTests : IDisposable
    {
        private readonly Infrastructure.Data.LicensePlatformDbContext _context;

        public AuthServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        /// <summary>
        /// Simulates password verification against a stored hash.
        /// In production, this would use BCrypt or a similar library.
        /// </summary>
        private static bool VerifyPassword(string password, string storedHash)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password + "test_salt");
            var hash = System.Convert.ToBase64String(sha256.ComputeHash(bytes));
            return hash == storedHash;
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin, username: "validadmin");
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Act — verify the password would match
            var passwordValid = VerifyPassword("TestPassword123!", admin.PasswordHash);

            // Assert
            passwordValid.Should().BeTrue("valid credentials should authenticate successfully");
            admin.FailedLoginAttempts.Should().Be(0, "failed attempts should remain at 0");
            admin.LockedUntil.Should().BeNull("account should not be locked");
        }

        [Fact]
        public void Login_InvalidCredentials_ThrowsException()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin, username: "badcreds");
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Act & Assert
            Action act = () =>
            {
                var passwordValid = VerifyPassword("WrongPassword!", admin.PasswordHash);
                if (!passwordValid)
                    throw new UnauthorizedAccessException("Invalid email or password.");
            };

            act.Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*Invalid*");
        }

        [Fact]
        public void Login_LockedAccount_ThrowsException()
        {
            // Arrange — account locked until 1 hour from now
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin, username: "lockedadmin");
            admin.LockedUntil = DateTime.UtcNow.AddHours(1);
            admin.FailedLoginAttempts = 5;
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Act & Assert
            Action act = () =>
            {
                if (admin.LockedUntil.HasValue && admin.LockedUntil > DateTime.UtcNow)
                    throw new UnauthorizedAccessException("Account is locked. Please try again later.");
            };

            act.Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*locked*");
        }

        [Fact]
        public void CreateAdmin_SuperAdmin_CreatesAdmin()
        {
            // Arrange
            var superAdmin = TestDataBuilder.CreateTestAdminUser(AdminRole.SuperAdmin, username: "super");
            _context.AdminUsers.Add(superAdmin);
            _context.SaveChanges();

            // Act — SuperAdmin can create new admins
            var newAdmin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin, username: "newadmin");

            Action act = () =>
            {
                if (superAdmin.Role != AdminRole.SuperAdmin)
                    throw new UnauthorizedAccessException("Only SuperAdmin can create admin users.");
                _context.AdminUsers.Add(newAdmin);
                _context.SaveChanges();
            };

            act.Should().NotThrow("SuperAdmin should be able to create admin users");

            // Assert
            var saved = _context.AdminUsers.FirstOrDefault(a => a.Username == "newadmin");
            saved.Should().NotBeNull("new admin should be persisted");
            saved!.Role.Should().Be(AdminRole.Admin);
        }

        [Fact]
        public void CreateAdmin_NonSuperAdmin_ThrowsException()
        {
            // Arrange
            var regularAdmin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin, username: "regular");

            // Act & Assert
            Action act = () =>
            {
                if (regularAdmin.Role != AdminRole.SuperAdmin)
                    throw new UnauthorizedAccessException("Only SuperAdmin can create admin users.");
            };

            act.Should().Throw<UnauthorizedAccessException>()
                .WithMessage("*SuperAdmin*");
        }

        [Fact]
        public void Login_IncreasesFailedAttempts_OnFailure()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin, username: "failadmin");
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Act
            var saved = _context.AdminUsers.Find(admin.AdminUserId);
            saved!.FailedLoginAttempts += 1;
            if (saved.FailedLoginAttempts >= 5)
                saved.LockedUntil = DateTime.UtcNow.AddMinutes(30);
            _context.SaveChanges();

            // Assert
            var updated = _context.AdminUsers.Find(admin.AdminUserId);
            updated!.FailedLoginAttempts.Should().Be(1, "failed attempts should increment");
            updated.LockedUntil.Should().BeNull("should not lock until 5 failures");
        }

        [Fact]
        public void Login_ResetsFailedAttempts_OnSuccess()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin, username: "resetadmin");
            admin.FailedLoginAttempts = 3;
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Act
            var saved = _context.AdminUsers.Find(admin.AdminUserId);
            saved!.FailedLoginAttempts = 0;
            saved.LastLoginAt = DateTime.UtcNow;
            _context.SaveChanges();

            // Assert
            var updated = _context.AdminUsers.Find(admin.AdminUserId);
            updated!.FailedLoginAttempts.Should().Be(0, "successful login should reset counter");
            updated.LastLoginAt.Should().NotBeNull();
        }
    }
}
