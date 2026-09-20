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
    /// Repository implementation for LicenseDevice data access.
    /// </summary>
    public class DeviceRepository : GenericRepository<LicenseDevice>, IDeviceRepository
    {
        public DeviceRepository(LicensePlatformDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<LicenseDevice?> GetByFingerprintAsync(string fingerprint)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.DeviceFingerprint == fingerprint);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<LicenseDevice>> GetByLicenseAsync(Guid licenseId)
        {
            return await _dbSet
                .Where(d => d.LicenseId == licenseId)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> CountActiveDevicesAsync(Guid licenseId)
        {
            return await _dbSet
                .CountAsync(d => d.LicenseId == licenseId && d.Status == DeviceStatus.Active);
        }

        /// <inheritdoc />
        public async Task<LicenseDevice?> GetDeviceWithLicenseAsync(Guid deviceId)
        {
            return await _dbSet
                .Include(d => d.License)
                .FirstOrDefaultAsync(d => d.LicenseDeviceId == deviceId);
        }
    }
}
