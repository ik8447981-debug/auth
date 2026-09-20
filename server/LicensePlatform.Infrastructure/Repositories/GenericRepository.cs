using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LicensePlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation using Entity Framework Core.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly LicensePlatformDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(LicensePlatformDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        /// <inheritdoc />
        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <inheritdoc />
        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <inheritdoc />
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }

        /// <inheritdoc />
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}
