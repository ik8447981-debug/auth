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
    /// Repository implementation for Product data access.
    /// </summary>
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(LicensePlatformDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<Product?> GetByProductCodeAsync(string productCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.ProductCode == productCode);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == ProductStatus.Active)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Product?> GetProductWithDetailsAsync(Guid productId)
        {
            return await _dbSet
                .Include(p => p.Features)
                .Include(p => p.Plans)
                    .ThenInclude(pl => pl.PlanFeatures)
                        .ThenInclude(pf => pf.Feature)
                .Include(p => p.Licenses)
                .Include(p => p.Customers)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }
    }
}
