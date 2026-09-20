using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for LicenseDevice-specific data access operations.
    /// </summary>
    public interface IDeviceRepository : IGenericRepository<LicenseDevice>
    {
        /// <summary>
        /// Gets a device by its hardware fingerprint.
        /// </summary>
        Task<LicenseDevice?> GetByFingerprintAsync(string fingerprint);

        /// <summary>
        /// Gets all devices registered to a specific license.
        /// </summary>
        Task<IReadOnlyList<LicenseDevice>> GetByLicenseAsync(Guid licenseId);

        /// <summary>
        /// Counts the number of active devices for a specific license.
        /// </summary>
        Task<int> CountActiveDevicesAsync(Guid licenseId);

        /// <summary>
        /// Gets a device with its associated license loaded.
        /// </summary>
        Task<LicenseDevice?> GetDeviceWithLicenseAsync(Guid deviceId);
    }
}
