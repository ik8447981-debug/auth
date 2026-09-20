using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for AdminUser data access.
    /// </summary>
    public class AdminUserRepository : GenericRepository<AdminUser>, IAdminUserRepository
    {
        public AdminUserRepository(LicensePlatformDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<AdminUser?> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.Username == username);
        }
    }
}
