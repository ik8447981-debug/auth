using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LicensePlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository interface providing standard CRUD operations for any entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its primary key identifier.
        /// </summary>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all entities of this type.
        /// </summary>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>
        /// Finds entities matching the specified predicate.
        /// </summary>
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds a new entity and returns it.
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity by its primary key identifier.
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Counts all entities matching the specified predicate.
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        /// <summary>
        /// Checks if any entity matches the specified predicate.
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}
