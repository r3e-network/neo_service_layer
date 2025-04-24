using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using CoreStorageProviderConfig = NeoServiceLayer.Core.Models.StorageProviderConfiguration;
using StorageProviderConfig = NeoServiceLayer.Services.Storage.Configuration.StorageProviderConfiguration;
using NeoServiceLayer.Services.Storage.Configuration;
using NeoServiceLayer.Services.Storage.ConnectionPool;
using NeoServiceLayer.Services.Storage.Providers;

namespace NeoServiceLayer.Services.Storage
{
    /// <summary>
    /// Implementation of the database service
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly DatabaseConfiguration _configuration;
        private readonly Dictionary<string, Core.Interfaces.IStorageProvider> _providers = new Dictionary<string, Core.Interfaces.IStorageProvider>();
        private string _defaultProviderName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Database configuration</param>
        public DatabaseService(ILogger<DatabaseService> logger, IOptions<DatabaseConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _defaultProviderName = _configuration.DefaultProvider;
        }

        /// <inheritdoc/>
        public Core.Interfaces.IStorageProvider GetProvider(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                return GetDefaultProvider();
            }

            if (_providers.TryGetValue(providerName, out var provider))
            {
                return provider;
            }

            return null;
        }

        /// <inheritdoc/>
        public Core.Interfaces.IStorageProvider GetDefaultProvider()
        {
            if (string.IsNullOrEmpty(_defaultProviderName) || !_providers.TryGetValue(_defaultProviderName, out var provider))
            {
                // Return the first provider if default is not set or not found
                return _providers.Values.FirstOrDefault();
            }

            return provider;
        }

        /// <inheritdoc/>
        public bool RegisterProvider(Core.Interfaces.IStorageProvider provider)
        {
            _logger.LogInformation("Registering database provider: {Name}", provider.Name);

            if (_providers.ContainsKey(provider.Name))
            {
                _logger.LogWarning("Database provider already registered: {Name}", provider.Name);
                return false;
            }

            _providers[provider.Name] = provider;
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> InitializeProvidersAsync()
        {
            _logger.LogInformation("Initializing database providers");

            if (_configuration == null)
            {
                _logger.LogError("Database configuration is null");
                return false;
            }

            if (_configuration.Providers == null || _configuration.Providers.Count == 0)
            {
                _logger.LogError("No database providers configured");
                return false;
            }

            _logger.LogInformation("Found {Count} database providers in configuration", _configuration.Providers.Count);

            // Create providers from configuration
            foreach (var providerConfig in _configuration.Providers)
            {
                try
                {
                    _logger.LogInformation("Creating database provider: {Name}, Type: {Type}", providerConfig.Name, providerConfig.Type);

                    Core.Interfaces.IStorageProvider provider;
                    var providerType = providerConfig.Type.ToLowerInvariant();
                    var coreConfig = providerConfig.ToStorageProviderConfig();
                    var servicesConfig = coreConfig.ToServicesConfig();

                    _logger.LogInformation("Provider {Name} configuration: {Config}", providerConfig.Name, servicesConfig);

                    switch (providerType)
                    {
                        case "inmemory":
                            _logger.LogInformation("Creating InMemory provider: {Name}", providerConfig.Name);
                            provider = new InMemoryStorageProvider(_logger as ILogger<InMemoryStorageProvider>);
                            break;
                        case "file":
                            _logger.LogInformation("Creating File provider: {Name}", providerConfig.Name);
                            provider = new FileStorageProvider(_logger as ILogger<FileStorageProvider>, servicesConfig.ToCoreConfig());
                            break;
                        case "s3":
                            _logger.LogInformation("Creating S3 provider: {Name}", providerConfig.Name);
                            provider = new S3StorageProvider(_logger as ILogger<S3StorageProvider>, servicesConfig.ToCoreConfig());
                            break;
                        case "mongodb":
                            _logger.LogInformation("Creating MongoDB provider: {Name}", providerConfig.Name);
                            provider = CreateMongoDbProvider(providerConfig);
                            break;
                        case "redis":
                            _logger.LogInformation("Creating Redis provider: {Name}", providerConfig.Name);
                            provider = new RedisStorageProvider(_logger as ILogger<RedisStorageProvider>, servicesConfig);
                            break;
                        default:
                            _logger.LogError("Unsupported database provider type: {Type}", providerConfig.Type);
                            throw new NotSupportedException($"Unsupported database provider type: {providerConfig.Type}");
                    }

                    _logger.LogInformation("Registering provider: {Name}", provider.Name);
                    RegisterProvider(provider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating database provider: {Name}, {Type}", providerConfig.Name, providerConfig.Type);
                }
            }

            if (_providers.Count == 0)
            {
                _logger.LogError("No database providers were successfully created");
                return false;
            }

            _logger.LogInformation("Successfully created {Count} database providers", _providers.Count);

            // Initialize all providers
            var success = true;
            foreach (var provider in _providers.Values)
            {
                try
                {
                    _logger.LogInformation("Initializing database provider: {Name}", provider.Name);
                    var initialized = await provider.InitializeAsync();
                    if (initialized)
                    {
                        _logger.LogInformation("Successfully initialized database provider: {Name}", provider.Name);

                        // Verify health check
                        var healthy = await provider.HealthCheckAsync();
                        _logger.LogInformation("Health check for provider {Name}: {Healthy}", provider.Name, healthy);
                    }
                    else
                    {
                        _logger.LogError("Failed to initialize database provider: {Name}", provider.Name);
                        success = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error initializing database provider: {Name}", provider.Name);
                    success = false;
                }
            }

            if (success)
            {
                _logger.LogInformation("All database providers initialized successfully");
            }
            else
            {
                _logger.LogError("One or more database providers failed to initialize");
            }

            return success;
        }

        /// <inheritdoc/>
        public virtual async Task<Core.Interfaces.IStorageProvider> GetDefaultProviderAsync()
        {
            if (_providers.Count == 0)
            {
                await InitializeProvidersAsync();
            }

            return GetDefaultProvider();
        }

        /// <inheritdoc/>
        public async Task<Core.Interfaces.IStorageProvider> GetProviderByNameAsync(string name)
        {
            if (_providers.Count == 0)
            {
                await InitializeProvidersAsync();
            }

            if (_providers.TryGetValue(name, out var provider))
            {
                return provider;
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<Core.Interfaces.IStorageProvider> GetProviderByName(string name)
        {
            return await GetProviderByNameAsync(name);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Interfaces.IStorageProvider>> GetProvidersAsync()
        {
            if (_providers.Count == 0)
            {
                await InitializeProvidersAsync();
            }

            return _providers.Values;
        }

        /// <inheritdoc/>
        public async Task<bool> HealthCheckAsync()
        {
            _logger.LogInformation("Checking health of database providers");

            var success = true;
            foreach (var provider in _providers.Values)
            {
                try
                {
                    var healthy = await provider.HealthCheckAsync();
                    if (!healthy)
                    {
                        _logger.LogError("Database provider is not healthy: {Name}", provider.Name);
                        success = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking health of database provider: {Name}", provider.Name);
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public Task<T> CreateAsync<T>(string collection, T entity) where T : class
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.CreateAsync(collection, entity);
        }

        /// <inheritdoc/>
        public Task<T> CreateAsync<T>(string providerName, string collection, T entity) where T : class
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.CreateAsync(collection, entity);
        }

        /// <inheritdoc/>
        public Task<T> GetByIdAsync<T, TKey>(string collection, TKey id) where T : class
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.GetByIdAsync<T, TKey>(collection, id);
        }

        /// <inheritdoc/>
        public Task<T> GetByIdAsync<T, TKey>(string providerName, string collection, TKey id) where T : class
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.GetByIdAsync<T, TKey>(collection, id);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T>> GetByFilterAsync<T>(string collection, Func<T, bool> filter) where T : class
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.GetByFilterAsync(collection, filter);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T>> GetByFilterAsync<T>(string providerName, string collection, Func<T, bool> filter) where T : class
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.GetByFilterAsync(collection, filter);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.GetAllAsync<T>(collection);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T>> GetAllAsync<T>(string providerName, string collection) where T : class
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.GetAllAsync<T>(collection);
        }

        /// <inheritdoc/>
        public Task<T> UpdateAsync<T, TKey>(string collection, TKey id, T entity) where T : class
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.UpdateAsync<T, TKey>(collection, id, entity);
        }

        /// <inheritdoc/>
        public Task<T> UpdateAsync<T, TKey>(string providerName, string collection, TKey id, T entity) where T : class
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.UpdateAsync<T, TKey>(collection, id, entity);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync<T, TKey>(string collection, TKey id) where T : class
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.DeleteAsync<T, TKey>(collection, id);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync<T, TKey>(string providerName, string collection, TKey id) where T : class
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.DeleteAsync<T, TKey>(collection, id);
        }

        /// <inheritdoc/>
        public Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.CountAsync(collection, filter);
        }

        /// <inheritdoc/>
        public Task<int> CountAsync<T>(string providerName, string collection, Func<T, bool> filter = null) where T : class
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.CountAsync(collection, filter);
        }

        /// <inheritdoc/>
        public Task<bool> CollectionExistsAsync(string collection)
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.CollectionExistsAsync(collection);
        }

        /// <inheritdoc/>
        public Task<bool> CollectionExistsAsync(string providerName, string collection)
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.CollectionExistsAsync(collection);
        }

        /// <inheritdoc/>
        public Task<bool> CreateCollectionAsync(string collection)
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.CreateCollectionAsync(collection);
        }

        /// <inheritdoc/>
        public Task<bool> CreateCollectionAsync(string providerName, string collection)
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.CreateCollectionAsync(collection);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCollectionAsync(string collection)
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return provider.DeleteCollectionAsync(collection);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCollectionAsync(string providerName, string collection)
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            return provider.DeleteCollectionAsync(collection);
        }

        /// <inheritdoc/>
        public async Task<DatabaseStatistics> GetStatisticsAsync()
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return await GetStatisticsAsync(provider.Name);
        }

        private Core.Interfaces.IStorageProvider CreateMongoDbProvider(DatabaseProviderConfiguration providerConfig)
        {
            _logger.LogInformation("Creating MongoDB provider: {Name}", providerConfig.Name);

            // Log MongoDB configuration
            if (providerConfig.Options.TryGetValue("ConnectionString", out var connString))
            {
                _logger.LogInformation("MongoDB connection string: {ConnectionString}", connString);
            }
            else
            {
                _logger.LogWarning("MongoDB connection string not found in configuration");
            }

            if (providerConfig.Options.TryGetValue("DatabaseName", out var dbName))
            {
                _logger.LogInformation("MongoDB database name: {DatabaseName}", dbName);
            }
            else
            {
                _logger.LogWarning("MongoDB database name not found in configuration");
            }

            // Create connection pool if not already created
            var poolConfig = new MongoDbConnectionPoolConfiguration
            {
                MaxConnections = 100,
                IdleTimeoutMinutes = 30,
                Enabled = true
            };

            _logger.LogInformation("Creating MongoDB connection pool with MaxConnections: {MaxConnections}, IdleTimeoutMinutes: {IdleTimeoutMinutes}, Enabled: {Enabled}",
                poolConfig.MaxConnections, poolConfig.IdleTimeoutMinutes, poolConfig.Enabled);

            var connectionPool = new MongoDbConnectionPool(
                _logger as ILogger<MongoDbConnectionPool>,
                Options.Create(poolConfig));

            // Create the provider
            var servicesConfig = providerConfig.ToServicesConfig();
            _logger.LogInformation("Creating MongoDB storage provider with configuration: {Config}", servicesConfig);

            return new MongoDbStorageProvider(
                _logger as ILogger<MongoDbStorageProvider>,
                servicesConfig,
                connectionPool,
                Options.Create(poolConfig));
        }

        /// <inheritdoc/>
        public async Task<DatabaseStatistics> GetStatisticsAsync(string providerName)
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            var statistics = new DatabaseStatistics
            {
                LastUpdated = DateTime.UtcNow
            };

            try
            {
                // Get all collections
                var collections = new List<string>();

                // This is a simplified approach - in a real implementation, we would need a way to list all collections
                // For now, we'll just use some common collections
                collections.AddRange(new[] { "accounts", "wallets", "functions", "secrets", "prices", "priceSources" });

                statistics.TotalCollections = collections.Count;

                // Get statistics for each collection
                foreach (var collection in collections)
                {
                    if (await provider.CollectionExistsAsync(collection))
                    {
                        // Count entities in the collection
                        var count = await provider.CountAsync<object>(collection);
                        statistics.EntitiesByCollection[collection] = count;
                        statistics.TotalEntities += count;

                        // Estimate size (this is a very rough estimate)
                        var sizeEstimate = count * 1024; // Assume 1KB per entity
                        statistics.SizeByCollection[collection] = sizeEstimate;
                        statistics.TotalSize += sizeEstimate;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database statistics for provider: {Name}", providerName);
            }

            return statistics;
        }

        /// <inheritdoc/>
        public async Task<long> GetCollectionSizeAsync(string collection)
        {
            var provider = GetDefaultProvider();
            if (provider == null)
            {
                throw new InvalidOperationException("No database provider available");
            }

            return await GetCollectionSizeAsync(provider.Name, collection);
        }

        /// <inheritdoc/>
        public async Task<long> GetCollectionSizeAsync(string providerName, string collection)
        {
            var provider = GetProvider(providerName);
            if (provider == null)
            {
                throw new InvalidOperationException($"Database provider not found: {providerName}");
            }

            try
            {
                // Check if collection exists
                if (!await provider.CollectionExistsAsync(collection))
                {
                    return 0;
                }

                // Count entities in the collection
                var count = await provider.CountAsync<object>(collection);

                // Estimate size (this is a very rough estimate)
                var sizeEstimate = count * 1024; // Assume 1KB per entity

                return sizeEstimate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collection size for collection: {Collection}, provider: {Name}", collection, providerName);
                return 0;
            }
        }
    }
}
