using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for database service
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Gets a database provider by name
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <returns>The database provider if found, null otherwise</returns>
        IStorageProvider GetProvider(string providerName);

        /// <summary>
        /// Gets a database provider by name asynchronously
        /// </summary>
        /// <param name="name">Provider name</param>
        /// <returns>The database provider if found, null otherwise</returns>
        Task<IStorageProvider> GetProviderByNameAsync(string name);

        /// <summary>
        /// Gets a database provider by name (synchronous version)
        /// </summary>
        /// <param name="name">Provider name</param>
        /// <returns>The database provider if found, null otherwise</returns>
        Task<IStorageProvider> GetProviderByName(string name);

        /// <summary>
        /// Gets all database providers
        /// </summary>
        /// <returns>List of database providers</returns>
        Task<IEnumerable<IStorageProvider>> GetProvidersAsync();

        /// <summary>
        /// Gets the default database provider
        /// </summary>
        /// <returns>The default database provider</returns>
        IStorageProvider GetDefaultProvider();

        /// <summary>
        /// Gets the default database provider asynchronously
        /// </summary>
        /// <returns>The default database provider</returns>
        Task<IStorageProvider> GetDefaultProviderAsync();

        /// <summary>
        /// Registers a database provider
        /// </summary>
        /// <param name="provider">Database provider to register</param>
        /// <returns>True if the provider was registered, false otherwise</returns>
        bool RegisterProvider(IStorageProvider provider);

        /// <summary>
        /// Initializes all registered database providers
        /// </summary>
        /// <returns>True if all providers were initialized successfully, false otherwise</returns>
        Task<bool> InitializeProvidersAsync();

        /// <summary>
        /// Checks if all registered database providers are healthy
        /// </summary>
        /// <returns>True if all providers are healthy, false otherwise</returns>
        Task<bool> HealthCheckAsync();

        /// <summary>
        /// Creates a new entity using the default provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="entity">Entity to create</param>
        /// <returns>The created entity</returns>
        Task<T> CreateAsync<T>(string collection, T entity) where T : class;

        /// <summary>
        /// Creates a new entity using a specific provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <param name="entity">Entity to create</param>
        /// <returns>The created entity</returns>
        Task<T> CreateAsync<T>(string providerName, string collection, T entity) where T : class;

        /// <summary>
        /// Gets an entity by ID using the default provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TKey">Type of entity ID</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T> GetByIdAsync<T, TKey>(string collection, TKey id) where T : class;

        /// <summary>
        /// Gets an entity by ID using a specific provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TKey">Type of entity ID</typeparam>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T> GetByIdAsync<T, TKey>(string providerName, string collection, TKey id) where T : class;

        /// <summary>
        /// Gets entities by a filter using the default provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="filter">Filter expression</param>
        /// <returns>List of entities matching the filter</returns>
        Task<IEnumerable<T>> GetByFilterAsync<T>(string collection, Func<T, bool> filter) where T : class;

        /// <summary>
        /// Gets entities by a filter using a specific provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <param name="filter">Filter expression</param>
        /// <returns>List of entities matching the filter</returns>
        Task<IEnumerable<T>> GetByFilterAsync<T>(string providerName, string collection, Func<T, bool> filter) where T : class;

        /// <summary>
        /// Gets all entities in a collection using the default provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="collection">Collection name</param>
        /// <returns>List of all entities in the collection</returns>
        Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class;

        /// <summary>
        /// Gets all entities in a collection using a specific provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <returns>List of all entities in the collection</returns>
        Task<IEnumerable<T>> GetAllAsync<T>(string providerName, string collection) where T : class;

        /// <summary>
        /// Updates an entity using the default provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TKey">Type of entity ID</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <param name="entity">Updated entity</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync<T, TKey>(string collection, TKey id, T entity) where T : class;

        /// <summary>
        /// Updates an entity using a specific provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TKey">Type of entity ID</typeparam>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <param name="entity">Updated entity</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync<T, TKey>(string providerName, string collection, TKey id, T entity) where T : class;

        /// <summary>
        /// Deletes an entity using the default provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TKey">Type of entity ID</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <returns>True if the entity was deleted, false otherwise</returns>
        Task<bool> DeleteAsync<T, TKey>(string collection, TKey id) where T : class;

        /// <summary>
        /// Deletes an entity using a specific provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <typeparam name="TKey">Type of entity ID</typeparam>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <returns>True if the entity was deleted, false otherwise</returns>
        Task<bool> DeleteAsync<T, TKey>(string providerName, string collection, TKey id) where T : class;

        /// <summary>
        /// Counts entities in a collection using the default provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="filter">Optional filter expression</param>
        /// <returns>Number of entities matching the filter</returns>
        Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class;

        /// <summary>
        /// Counts entities in a collection using a specific provider
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <param name="filter">Optional filter expression</param>
        /// <returns>Number of entities matching the filter</returns>
        Task<int> CountAsync<T>(string providerName, string collection, Func<T, bool> filter = null) where T : class;

        /// <summary>
        /// Checks if a collection exists using the default provider
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <returns>True if the collection exists, false otherwise</returns>
        Task<bool> CollectionExistsAsync(string collection);

        /// <summary>
        /// Checks if a collection exists using a specific provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <returns>True if the collection exists, false otherwise</returns>
        Task<bool> CollectionExistsAsync(string providerName, string collection);

        /// <summary>
        /// Creates a collection using the default provider
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <returns>True if the collection was created, false otherwise</returns>
        Task<bool> CreateCollectionAsync(string collection);

        /// <summary>
        /// Creates a collection using a specific provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <returns>True if the collection was created, false otherwise</returns>
        Task<bool> CreateCollectionAsync(string providerName, string collection);

        /// <summary>
        /// Deletes a collection using the default provider
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <returns>True if the collection was deleted, false otherwise</returns>
        Task<bool> DeleteCollectionAsync(string collection);

        /// <summary>
        /// Deletes a collection using a specific provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <returns>True if the collection was deleted, false otherwise</returns>
        Task<bool> DeleteCollectionAsync(string providerName, string collection);

        /// <summary>
        /// Gets database statistics
        /// </summary>
        /// <returns>Database statistics</returns>
        Task<DatabaseStatistics> GetStatisticsAsync();

        /// <summary>
        /// Gets database statistics for a specific provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <returns>Database statistics</returns>
        Task<DatabaseStatistics> GetStatisticsAsync(string providerName);

        /// <summary>
        /// Gets the size of a collection in bytes
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <returns>Size in bytes</returns>
        Task<long> GetCollectionSizeAsync(string collection);

        /// <summary>
        /// Gets the size of a collection in bytes using a specific provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <param name="collection">Collection name</param>
        /// <returns>Size in bytes</returns>
        Task<long> GetCollectionSizeAsync(string providerName, string collection);
    }
}
