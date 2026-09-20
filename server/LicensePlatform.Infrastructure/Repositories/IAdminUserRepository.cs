using System;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for AdminUser-specific data access operations.
    /// </summary>
    public interface IAdminUserRepository : IGenericRepository<AdminUser>
    {
        /// <summary>
        /// Gets an admin user by their unique username.
        /// </summary>
        Task<AdminUser?> GetByUsernameAsync(string username);
    }
}
