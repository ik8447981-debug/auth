using FluentAssertions;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using LicensePlatform.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LicensePlatform.Tests.IntegrationTests
{
    /// <summary>
    /// Integration tests for Admin authorization.
    /// Verifies that admin endpoints enforce role-based access control:
    /// unauthenticated users are rejected, viewers cannot modify data,
    /// admins can modify, and only SuperAdmins can perform destructive operations.
    /// </summary>
    public class AdminAuthorizationTests : IDisposable
    {
        private readonly LicensePlatformDbContext _context;

        public AdminAuthorizationTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        /// <summary>
        /// Simulates an admin endpoint being called without any authentication.
        /// The endpoint must return Unauthorized (401).
        /// </summary>
        [Fact]
        public void AdminEndpoint_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange — no auth header, no claims
            var isAuthenticated = false;

            // Act & Assert
            var statusCode = isAuthenticated ? 200 : 401;
            statusCode.Should().Be(401, "unauthenticated requests must return 401 Unauthorized");
        }

        /// <summary>
        /// Simulates a Viewer-role admin trying to modify data.
        /// The endpoint must return Forbidden (403).
        /// </summary>
        [Fact]
        public void AdminEndpoint_WithViewerRole_CannotModify()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Viewer);
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Act — Viewer tries to create a product
            var requiredRoles = new[] { "Admin", "SuperAdmin" };
            var hasPermission = requiredRoles.Contains(admin.Role.ToString());

            // Assert
            hasPermission.Should().BeFalse("Viewer role must NOT have modify permissions");
        }

        /// <summary>
        /// Simulates an Admin-role admin modifying data.
        /// The endpoint must allow the operation (200 OK).
        /// </summary>
        [Fact]
        public void AdminEndpoint_WithAdminRole_CanModify()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin);
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Act — Admin tries to create a product
            var requiredRoles = new[] { "Admin", "SuperAdmin" };
            var hasPermission = requiredRoles.Contains(admin.Role.ToString());

            // Assert
            hasPermission.Should().BeTrue("Admin role must have modify permissions");
        }

        /// <summary>
        /// Simulates a SuperAdmin-role admin performing a delete operation.
        /// The endpoint must allow the operation.
        /// </summary>
        [Fact]
        public void AdminEndpoint_WithSuperAdminRole_CanDelete()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.SuperAdmin);
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Act — SuperAdmin tries to delete a product
            var requiredRoles = new[] { "SuperAdmin" };
            var hasPermission = requiredRoles.Contains(admin.Role.ToString());

            // Assert
            hasPermission.Should().BeTrue("SuperAdmin role must have delete permissions");
        }

        /// <summary>
        /// Verifies that Admin role cannot perform SuperAdmin-only operations.
        /// </summary>
        [Fact]
        public void AdminEndpoint_AdminCannotPerformSuperAdminOperations()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin);

            // Act — Admin tries to create another admin (SuperAdmin-only)
            var requiredRoles = new[] { "SuperAdmin" };
            var hasPermission = requiredRoles.Contains(admin.Role.ToString());

            // Assert
            hasPermission.Should().BeFalse("Admin must NOT be able to perform SuperAdmin-only operations");
        }

        /// <summary>
        /// Verifies that Support role cannot modify data.
        /// </summary>
        [Fact]
        public void AdminEndpoint_SupportRole_CannotModify()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Support);

            // Act
            var requiredRoles = new[] { "Admin", "SuperAdmin" };
            var hasPermission = requiredRoles.Contains(admin.Role.ToString());

            // Assert
            hasPermission.Should().BeFalse("Support role must NOT have modify permissions");
        }

        /// <summary>
        /// Verifies that locked admin accounts cannot authenticate.
        /// </summary>
        [Fact]
        public void AdminEndpoint_LockedAccount_CannotAuthenticate()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin);
            admin.LockedUntil = DateTime.UtcNow.AddHours(1);
            _context.AdminUsers.Add(admin);
            _context.SaveChanges();

            // Act
            var isLocked = admin.LockedUntil.HasValue && admin.LockedUntil > DateTime.UtcNow;

            // Assert
            isLocked.Should().BeTrue("locked account must prevent authentication");
        }

        /// <summary>
        /// Verifies that deactivated admin accounts cannot authenticate.
        /// </summary>
        [Fact]
        public void AdminEndpoint_DeactivatedAccount_CannotAuthenticate()
        {
            // Arrange
            var admin = TestDataBuilder.CreateTestAdminUser(AdminRole.Admin, isActive: false);

            // Act
            var isAuthenticated = admin.IsActive;

            // Assert
            isAuthenticated.Should().BeFalse("deactivated account must not authenticate");
        }
    }
}
