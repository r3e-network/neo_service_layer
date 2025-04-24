using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Repositories
{
    /// <summary>
    /// Generic repository interface for CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    public interface IGenericRepository<T, TKey> where T : class
    {
        /// <summary>
        /// Gets an entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Entity if found, null otherwise</returns>
        Task<T> GetByIdAsync(TKey id);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>List of entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Gets entities by filter
        /// </summary>
        /// <param name="filter">Filter expression</param>
        /// <returns>List of entities matching the filter</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <param name="entity">Entity to create</param>
        /// <returns>Created entity</returns>
        Task<T> CreateAsync(T entity);

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="entity">Entity to update</param>
        /// <returns>Updated entity</returns>
        Task<T> UpdateAsync(TKey id, T entity);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>True if the entity was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(TKey id);

        /// <summary>
        /// Checks if an entity exists
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>True if the entity exists, false otherwise</returns>
        Task<bool> ExistsAsync(TKey id);

        /// <summary>
        /// Counts entities
        /// </summary>
        /// <param name="filter">Optional filter expression</param>
        /// <returns>Number of entities matching the filter</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null);

        /// <summary>
        /// Begins a transaction
        /// </summary>
        /// <returns>Transaction object</returns>
        Task<IRepositoryTransaction> BeginTransactionAsync();
    }
}
