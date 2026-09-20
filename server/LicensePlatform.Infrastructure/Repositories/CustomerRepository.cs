using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicensePlatform.Domain.Entities;
using LicensePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Customer data access.
    /// </summary>
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(LicensePlatformDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<Customer?> GetByEmailAsync(Guid productId, string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.Email == email);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Customer>> SearchCustomersAsync(Guid productId, string searchTerm)
        {
            string lowerTerm = searchTerm.ToLowerInvariant();

            return await _dbSet
                .Where(c => c.ProductId == productId &&
                    (c.Name.ToLower().Contains(lowerTerm) || c.Email.ToLower().Contains(lowerTerm)))
                .ToListAsync();
        }
    }
}
