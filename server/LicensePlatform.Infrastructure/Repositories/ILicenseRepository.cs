using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Domain.Enums;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for License-specific data access operations.
    /// </summary>
    public interface ILicenseRepository : IGenericRepository<License>
    {
        /// <summary>
        /// Gets a license by its unique license key.
        /// </summary>
        Task<License?> GetByKeyAsync(string licenseKey);

        /// <summary>
        /// Gets all licenses belonging to a specific customer.
        /// </summary>
        Task<IReadOnlyList<License>> GetByCustomerAsync(Guid customerId);

        /// <summary>
        /// Gets all licenses for a specific product.
        /// </summary>
        Task<IReadOnlyList<License>> GetByProductAsync(Guid productId);

        /// <summary>
        /// Gets all licenses with Active status.
        /// </summary>
        Task<IReadOnlyList<License>> GetActiveLicensesAsync();

        /// <summary>
        /// Gets all licenses that have expired.
        /// </summary>
        Task<IReadOnlyList<License>> GetExpiredLicensesAsync();

        /// <summary>
        /// Counts the total number of licenses for a specific product.
        /// </summary>
        Task<int> CountByProductAsync(Guid productId);

        /// <summary>
        /// Counts licenses by their status.
        /// </summary>
        Task<int> CountByStatusAsync(LicenseStatus status);

        /// <summary>
        /// Gets a license with all related entities (Product, Customer, Plan, Devices, Features).
        /// </summary>
        Task<License?> GetWithDetailsAsync(Guid licenseId);
    }
}
