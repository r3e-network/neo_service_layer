using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for storage providers
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// Gets the name of the storage provider
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the storage provider
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Initializes the storage provider
        /// </summary>
        /// <returns>True if initialization was successful, false otherwise</returns>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Checks if the storage provider is healthy
        /// </summary>
        /// <returns>True if the storage provider is healthy, false otherwise</returns>
        Task<bool> HealthCheckAsync();

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="entity">Entity to create</param>
        /// <returns>The created entity</returns>
        Task<T> CreateAsync<T>(string collection, T entity) where T : class;

        /// <summary>
        /// Gets an entity by ID
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TKey">Type of entity ID</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T> GetByIdAsync<T, TKey>(string collection, TKey id) where T : class;

        /// <summary>
        /// Gets entities by a filter
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="filter">Filter expression</param>
        /// <returns>List of entities matching the filter</returns>
        Task<IEnumerable<T>> GetByFilterAsync<T>(string collection, Func<T, bool> filter) where T : class;

        /// <summary>
        /// Gets all entities in a collection
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="collection">Collection name</param>
        /// <returns>List of all entities in the collection</returns>
        Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class;

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TKey">Type of entity ID</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <param name="entity">Updated entity</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync<T, TKey>(string collection, TKey id, T entity) where T : class;

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TKey">Type of entity ID</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <returns>True if the entity was deleted, false otherwise</returns>
        Task<bool> DeleteAsync<T, TKey>(string collection, TKey id) where T : class;

        /// <summary>
        /// Counts entities in a collection
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="filter">Optional filter expression</param>
        /// <returns>Number of entities matching the filter</returns>
        Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class;

        /// <summary>
        /// Checks if a collection exists
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <returns>True if the collection exists, false otherwise</returns>
        Task<bool> CollectionExistsAsync(string collection);

        /// <summary>
        /// Creates a collection
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <returns>True if the collection was created, false otherwise</returns>
        Task<bool> CreateCollectionAsync(string collection);

        /// <summary>
        /// Deletes a collection
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <returns>True if the collection was deleted, false otherwise</returns>
        Task<bool> DeleteCollectionAsync(string collection);
    }
}
