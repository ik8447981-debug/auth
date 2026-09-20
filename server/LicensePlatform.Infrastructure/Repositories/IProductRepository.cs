using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository interface for Product-specific data access operations.
    /// </summary>
    public interface IProductRepository : IGenericRepository<Product>
    {
        /// <summary>
        /// Gets a product by its unique product code.
        /// </summary>
        Task<Product?> GetByProductCodeAsync(string productCode);

        /// <summary>
        /// Gets all products with active status.
        /// </summary>
        Task<IReadOnlyList<Product>> GetActiveProductsAsync();

        /// <summary>
        /// Gets a product with all related entities (Plans, Features, Versions, Licenses, Customers).
        /// </summary>
        Task<Product?> GetProductWithDetailsAsync(Guid productId);
    }
}
