using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Utilities;
using NeoServiceLayer.Services.Storage.Configuration;
using StackExchange.Redis;

namespace NeoServiceLayer.Services.Storage.Providers
{
    /// <summary>
    /// Redis storage provider for caching and session storage
    /// </summary>
    public class RedisStorageProvider : Core.Interfaces.IStorageProvider
    {
        private readonly ILogger<RedisStorageProvider> _logger;
        private readonly StorageProviderConfiguration _configuration;
        private ConnectionMultiplexer _redis;
        private IDatabase _database;
        private bool _isInitialized;
        private string _keyPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisStorageProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Storage provider configuration</param>
        public RedisStorageProvider(ILogger<RedisStorageProvider> logger, StorageProviderConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _isInitialized = false;
        }

        /// <inheritdoc/>
        public string Name => _configuration.Name;

        /// <inheritdoc/>
        public string Type => "Redis";

        /// <inheritdoc/>
        public async Task<bool> InitializeAsync()
        {
            _logger.LogInformation("Initializing Redis storage provider: {Name}", Name);

            try
            {
                // Get configuration options
                _configuration.Options.TryGetValue("ConnectionString", out var connectionString);
                _configuration.Options.TryGetValue("KeyPrefix", out var keyPrefix);

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Redis connection string is required");
                }

                _keyPrefix = string.IsNullOrEmpty(keyPrefix) ? "nsl:" : keyPrefix;

                // Create Redis connection
                _redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
                _database = _redis.GetDatabase();

                // Test connection
                await _database.PingAsync();

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Redis storage provider: {Name}", Name);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HealthCheckAsync()
        {
            if (!_isInitialized)
            {
                return false;
            }

            try
            {
                // Test connection
                await _database.PingAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for Redis storage provider: {Name}", Name);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<T> CreateAsync<T>(string collection, T entity) where T : class
        {
            EnsureInitialized();
            _logger.LogInformation("Creating entity in collection: {Collection}", collection);

            try
            {
                // Get ID property
                var idProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name == "Id");
                if (idProperty == null)
                {
                    throw new InvalidOperationException($"Entity type {typeof(T).Name} does not have an Id property");
                }

                // Get ID value
                var id = idProperty.GetValue(entity);
                if (id == null)
                {
                    // Generate new ID if not set
                    if (idProperty.PropertyType == typeof(Guid))
                    {
                        id = Guid.NewGuid();
                        idProperty.SetValue(entity, id);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported ID type: {idProperty.PropertyType.Name}");
                    }
                }

                // Store entity
                var key = GetKey(collection, id.ToString());
                var json = JsonUtility.Serialize(entity);
                await _database.StringSetAsync(key, json);

                // Add to collection index
                await _database.SetAddAsync(GetCollectionKey(collection), id.ToString());

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity in collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync<T, TKey>(string collection, TKey id) where T : class
        {
            EnsureInitialized();
            _logger.LogInformation("Getting entity by ID from collection: {Collection}", collection);

            try
            {
                var key = GetKey(collection, id.ToString());
                var json = await _database.StringGetAsync(key);

                if (json.IsNullOrEmpty)
                {
                    return null;
                }

                return JsonUtility.Deserialize<T>(json.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by ID from collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class
        {
            EnsureInitialized();
            _logger.LogInformation("Getting all entities from collection: {Collection}", collection);

            try
            {
                var collectionKey = GetCollectionKey(collection);
                var ids = await _database.SetMembersAsync(collectionKey);

                if (ids.Length == 0)
                {
                    return Enumerable.Empty<T>();
                }

                var result = new List<T>();
                foreach (var id in ids)
                {
                    var key = GetKey(collection, id);
                    var json = await _database.StringGetAsync(key);

                    if (!json.IsNullOrEmpty)
                    {
                        result.Add(JsonUtility.Deserialize<T>(json.ToString()));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all entities from collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetByFilterAsync<T>(string collection, Func<T, bool> filter) where T : class
        {
            EnsureInitialized();
            _logger.LogInformation("Querying entities from collection: {Collection}", collection);

            try
            {
                var entities = await GetAllAsync<T>(collection);
                return filter != null ? entities.Where(filter) : entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying entities from collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T> UpdateAsync<T, TKey>(string collection, TKey id, T entity) where T : class
        {
            EnsureInitialized();
            _logger.LogInformation("Updating entity in collection: {Collection}", collection);

            try
            {
                var key = GetKey(collection, id.ToString());

                // Check if entity exists
                var exists = await _database.KeyExistsAsync(key);
                if (!exists)
                {
                    return null;
                }

                // Update entity
                var json = JsonUtility.Serialize(entity);
                await _database.StringSetAsync(key, json);

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity in collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync<T, TKey>(string collection, TKey id) where T : class
        {
            EnsureInitialized();
            _logger.LogInformation("Deleting entity from collection: {Collection}", collection);

            try
            {
                var key = GetKey(collection, id.ToString());
                var collectionKey = GetCollectionKey(collection);

                // Remove from collection index
                await _database.SetRemoveAsync(collectionKey, id.ToString());

                // Delete entity
                return await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity from collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class
        {
            EnsureInitialized();
            _logger.LogInformation("Counting entities in collection: {Collection}", collection);

            try
            {
                if (filter == null)
                {
                    var collectionKey = GetCollectionKey(collection);
                    return (int)await _database.SetLengthAsync(collectionKey);
                }
                else
                {
                    var entities = await GetAllAsync<T>(collection);
                    return entities.Count(filter);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities in collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CollectionExistsAsync(string collection)
        {
            EnsureInitialized();

            try
            {
                var collectionKey = GetCollectionKey(collection);
                return await _database.KeyExistsAsync(collectionKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if collection exists: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CreateCollectionAsync(string collection)
        {
            EnsureInitialized();
            _logger.LogInformation("Creating collection: {Collection}", collection);

            try
            {
                var collectionKey = GetCollectionKey(collection);

                // Create empty set if it doesn't exist
                if (!await _database.KeyExistsAsync(collectionKey))
                {
                    await _database.SetAddAsync(collectionKey, Array.Empty<RedisValue>());
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCollectionAsync(string collection)
        {
            EnsureInitialized();
            _logger.LogInformation("Deleting collection: {Collection}", collection);

            try
            {
                var collectionKey = GetCollectionKey(collection);

                // Get all entity IDs
                var ids = await _database.SetMembersAsync(collectionKey);

                // Delete all entities
                foreach (var id in ids)
                {
                    var key = GetKey(collection, id);
                    await _database.KeyDeleteAsync(key);
                }

                // Delete collection index
                return await _database.KeyDeleteAsync(collectionKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string collection, string key) where T : class
        {
            EnsureInitialized();
            _logger.LogInformation("Getting entity by key from collection: {Collection}", collection);

            try
            {
                var redisKey = GetKey(collection, key);
                var json = await _database.StringGetAsync(redisKey);

                if (json.IsNullOrEmpty)
                {
                    return null;
                }

                return JsonUtility.Deserialize<T>(json.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by key from collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> StoreAsync<T>(string collection, string key, T value) where T : class
        {
            EnsureInitialized();
            _logger.LogInformation("Storing entity by key in collection: {Collection}", collection);

            try
            {
                var redisKey = GetKey(collection, key);
                var collectionKey = GetCollectionKey(collection);
                var json = JsonUtility.Serialize(value);

                // Store entity
                await _database.StringSetAsync(redisKey, json);

                // Add to collection index
                await _database.SetAddAsync(collectionKey, key);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing entity by key in collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string collection, string key)
        {
            EnsureInitialized();
            _logger.LogInformation("Deleting entity by key from collection: {Collection}", collection);

            try
            {
                var redisKey = GetKey(collection, key);
                var collectionKey = GetCollectionKey(collection);

                // Remove from collection index
                await _database.SetRemoveAsync(collectionKey, key);

                // Delete entity
                return await _database.KeyDeleteAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity by key from collection: {Collection}", collection);
                throw;
            }
        }

        /// <summary>
        /// Gets a Redis key for an entity
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <param name="id">Entity ID</param>
        /// <returns>Redis key</returns>
        private string GetKey(string collection, string id)
        {
            return $"{_keyPrefix}{collection}:{id}";
        }

        /// <summary>
        /// Gets a Redis key for a collection
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <returns>Redis key</returns>
        private string GetCollectionKey(string collection)
        {
            return $"{_keyPrefix}{collection}:index";
        }

        /// <summary>
        /// Ensures the provider is initialized
        /// </summary>
        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Redis storage provider is not initialized");
            }
        }
    }
}
