using System;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for SigningKey-specific data access operations.
    /// </summary>
    public interface ISigningKeyRepository : IGenericRepository<SigningKey>
    {
        /// <summary>
        /// Gets the currently active signing key.
        /// </summary>
        Task<SigningKey?> GetActiveKeyAsync();

        /// <summary>
        /// Gets a signing key by its version number.
        /// </summary>
        Task<SigningKey?> GetByVersionAsync(int keyVersion);
    }
}
