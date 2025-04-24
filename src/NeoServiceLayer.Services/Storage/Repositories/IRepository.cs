using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeoServiceLayer.Services.Storage.Repositories
{
    /// <summary>
    /// Generic repository interface
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    public interface IRepository<T, TKey> where T : class
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
        /// Creates a new entity
        /// </summary>
        /// <param name="entity">Entity to create</param>
        /// <returns>Created entity</returns>
        Task<T> CreateAsync(T entity);

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>Updated entity</returns>
        Task<T> UpdateAsync(T entity);

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
    }
}
