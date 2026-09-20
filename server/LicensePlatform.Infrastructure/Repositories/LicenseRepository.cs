using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;
using LicensePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for License data access.
    /// </summary>
    public class LicenseRepository : GenericRepository<License>, ILicenseRepository
    {
        public LicenseRepository(LicensePlatformDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<License?> GetByKeyAsync(string licenseKey)
        {
            return await _dbSet
                .FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<License>> GetByCustomerAsync(Guid customerId)
        {
            return await _dbSet
                .Where(l => l.CustomerId == customerId)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<License>> GetByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(l => l.ProductId == productId)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<License>> GetActiveLicensesAsync()
        {
            return await _dbSet
                .Where(l => l.Status == LicenseStatus.Active)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<License>> GetExpiredLicensesAsync()
        {
            return await _dbSet
                .Where(l => l.Status == LicenseStatus.Active && l.ExpiryDate != null && l.ExpiryDate < DateTime.UtcNow)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> CountByProductAsync(Guid productId)
        {
            return await _dbSet
                .CountAsync(l => l.ProductId == productId);
        }

        /// <inheritdoc />
        public async Task<int> CountByStatusAsync(LicenseStatus status)
        {
            return await _dbSet
                .CountAsync(l => l.Status == status);
        }

        /// <inheritdoc />
        public async Task<License?> GetWithDetailsAsync(Guid licenseId)
        {
            return await _dbSet
                .Include(l => l.Product)
                .Include(l => l.Customer)
                .Include(l => l.Plan)
                .Include(l => l.Devices)
                .Include(l => l.LicenseFeatures)
                    .ThenInclude(lf => lf.Feature)
                .FirstOrDefaultAsync(l => l.LicenseId == licenseId);
        }
    }
}
