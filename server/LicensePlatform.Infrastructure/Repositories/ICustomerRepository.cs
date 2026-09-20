using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for Customer-specific data access operations.
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Gets a customer by their email address within a specific product scope.
        /// </summary>
        Task<Customer?> GetByEmailAsync(Guid productId, string email);

        /// <summary>
        /// Searches customers by name or email within a specific product.
        /// </summary>
        Task<IReadOnlyList<Customer>> SearchCustomersAsync(Guid productId, string searchTerm);
    }
}
