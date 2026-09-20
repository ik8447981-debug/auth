using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for SigningKey data access.
    /// </summary>
    public class SigningKeyRepository : GenericRepository<SigningKey>, ISigningKeyRepository
    {
        public SigningKeyRepository(LicensePlatformDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<SigningKey?> GetActiveKeyAsync()
        {
            return await _dbSet
                .FirstOrDefaultAsync(k => k.IsActive);
        }

        /// <inheritdoc />
        public async Task<SigningKey?> GetByVersionAsync(int keyVersion)
        {
            return await _dbSet
                .FirstOrDefaultAsync(k => k.KeyVersion == keyVersion);
        }
    }
}
