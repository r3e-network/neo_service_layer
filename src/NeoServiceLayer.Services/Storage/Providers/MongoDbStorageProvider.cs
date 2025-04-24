using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage.Configuration;
using NeoServiceLayer.Services.Storage.ConnectionPool;

namespace NeoServiceLayer.Services.Storage.Providers
{
    /// <summary>
    /// MongoDB storage provider for storing data in MongoDB
    /// </summary>
    public class MongoDbStorageProvider : Core.Interfaces.IStorageProvider
    {
        private readonly ILogger<MongoDbStorageProvider> _logger;
        private readonly StorageProviderConfiguration _configuration;
        private readonly MongoDbConnectionPool _connectionPool;
        private readonly MongoDbConnectionPoolConfiguration _poolConfiguration;
        private PooledConnection _connection;
        private string _databaseName;
        private bool _isInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbStorageProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Storage provider configuration</param>
        /// <param name="connectionPool">MongoDB connection pool</param>
        /// <param name="poolConfiguration">Connection pool configuration</param>
        public MongoDbStorageProvider(
            ILogger<MongoDbStorageProvider> logger,
            StorageProviderConfiguration configuration,
            MongoDbConnectionPool connectionPool,
            IOptions<MongoDbConnectionPoolConfiguration> poolConfiguration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionPool = connectionPool;
            _poolConfiguration = poolConfiguration.Value;
            _isInitialized = false;
        }

        /// <inheritdoc/>
        public string Name => _configuration.Name;

        /// <inheritdoc/>
        public string Type => "MongoDB";

        /// <inheritdoc/>
        public async Task<bool> InitializeAsync()
        {
            _logger.LogInformation("Initializing MongoDB storage provider: {Name}", Name);

            try
            {
                // Get configuration options
                if (!_configuration.Options.TryGetValue("ConnectionString", out var connectionString))
                {
                    _logger.LogError("MongoDB connection string is missing in configuration for provider: {Name}", Name);
                    throw new InvalidOperationException("MongoDB connection string is required");
                }

                if (!_configuration.Options.TryGetValue("DatabaseName", out var databaseName))
                {
                    _logger.LogError("MongoDB database name is missing in configuration for provider: {Name}", Name);
                    throw new InvalidOperationException("MongoDB database name is required");
                }

                _logger.LogInformation("Connecting to MongoDB database: {DatabaseName} with provider: {Name}", databaseName, Name);
                _databaseName = databaseName;

                // Configure MongoDB serialization
                ConfigureMongoSerialization();

                // Get connection from pool if enabled
                if (_poolConfiguration.Enabled)
                {
                    _logger.LogInformation("Using connection pool for MongoDB provider: {Name}", Name);
                    _connection = await _connectionPool.GetConnectionAsync(connectionString, _databaseName);
                }
                else
                {
                    _logger.LogInformation("Connection pooling disabled for MongoDB provider: {Name}", Name);
                    // Create direct MongoDB client
                    var client = new MongoClient(connectionString);
                    var database = client.GetDatabase(_databaseName);
                    _connection = new PooledConnection(client, database);
                }

                // Test connection
                _logger.LogInformation("Testing MongoDB connection for provider: {Name}", Name);
                await _connection.Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
                _logger.LogInformation("MongoDB connection test successful for provider: {Name}", Name);

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing MongoDB storage provider: {Name}", Name);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HealthCheckAsync()
        {
            if (!_isInitialized || _connection == null)
            {
                return false;
            }

            try
            {
                // Test connection
                await _connection.Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for MongoDB storage provider: {Name}", Name);
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
                var coll = _connection.Database.GetCollection<T>(collection);
                await coll.InsertOneAsync(entity);
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
                var coll = _connection.Database.GetCollection<T>(collection);
                var filter = Builders<T>.Filter.Eq("_id", id);
                return await coll.Find(filter).FirstOrDefaultAsync();
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
                var coll = _connection.Database.GetCollection<T>(collection);
                return await coll.Find(Builders<T>.Filter.Empty).ToListAsync();
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
                var coll = _connection.Database.GetCollection<T>(collection);
                var entities = await coll.Find(Builders<T>.Filter.Empty).ToListAsync();
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
                var coll = _connection.Database.GetCollection<T>(collection);
                var filter = Builders<T>.Filter.Eq("_id", id);
                await coll.ReplaceOneAsync(filter, entity);
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
                var coll = _connection.Database.GetCollection<BsonDocument>(collection);
                var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                var result = await coll.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
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
                var coll = _connection.Database.GetCollection<T>(collection);
                var entities = await coll.Find(Builders<T>.Filter.Empty).ToListAsync();
                return filter != null ? entities.Count(filter) : entities.Count;
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
                var filter = new BsonDocument("name", collection);
                var collections = await _connection.Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
                return await collections.AnyAsync();
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
                await _connection.Database.CreateCollectionAsync(collection);
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
                await _connection.Database.DropCollectionAsync(collection);
                return true;
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
                var coll = _connection.Database.GetCollection<BsonDocument>(collection);
                var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
                var document = await coll.Find(filter).FirstOrDefaultAsync();

                if (document == null)
                {
                    return null;
                }

                return BsonSerializer.Deserialize<T>(document);
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
                var coll = _connection.Database.GetCollection<BsonDocument>(collection);
                var document = value.ToBsonDocument();
                document["_id"] = key;

                var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
                var options = new ReplaceOptions { IsUpsert = true };

                await coll.ReplaceOneAsync(filter, document, options);
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
                var coll = _connection.Database.GetCollection<BsonDocument>(collection);
                var filter = Builders<BsonDocument>.Filter.Eq("_id", key);
                var result = await coll.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity by key from collection: {Collection}", collection);
                throw;
            }
        }

        /// <summary>
        /// Configures MongoDB serialization
        /// </summary>
        private void ConfigureMongoSerialization()
        {
            // Register conventions
            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String)
            };

            ConventionRegistry.Register("NeoServiceLayerConventions", conventionPack, t => true);

            // Register class maps if needed
            // Example: BsonClassMap.RegisterClassMap<MyClass>(cm => { ... });
        }

        /// <summary>
        /// Ensures the provider is initialized
        /// </summary>
        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                _logger.LogWarning("MongoDB storage provider {Name} is not initialized. Attempting to initialize...", Name);
                try
                {
                    // Get configuration options
                    string connectionString;
                    string databaseName;

                    // Try to get connection string from environment variables first
                    var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MongoDB");
                    if (!string.IsNullOrEmpty(envConnectionString))
                    {
                        connectionString = envConnectionString;
                        _logger.LogInformation("Using MongoDB connection string from environment variable: {ConnectionString}", connectionString);
                    }
                    else if (!_configuration.Options.TryGetValue("ConnectionString", out connectionString))
                    {
                        _logger.LogError("MongoDB connection string is missing in configuration for provider: {Name}", Name);
                        throw new InvalidOperationException("MongoDB connection string is required");
                    }

                    // Try to get database name from environment variables first
                    var envDatabaseName = Environment.GetEnvironmentVariable("Database__Providers__2__Options__DatabaseName");
                    if (!string.IsNullOrEmpty(envDatabaseName))
                    {
                        databaseName = envDatabaseName;
                        _logger.LogInformation("Using MongoDB database name from environment variable: {DatabaseName}", databaseName);
                    }
                    else if (!_configuration.Options.TryGetValue("DatabaseName", out databaseName))
                    {
                        // Default to neo_service_layer if not specified
                        databaseName = "neo_service_layer";
                        _logger.LogWarning("MongoDB database name is missing in configuration for provider: {Name}, using default: {DatabaseName}", Name, databaseName);
                    }

                    _logger.LogInformation("Connecting to MongoDB database: {DatabaseName} with provider: {Name} using connection string: {ConnectionString}",
                        databaseName, Name, connectionString);
                    _databaseName = databaseName;

                    // Configure MongoDB serialization
                    ConfigureMongoSerialization();

                    // Create direct MongoDB client with retry logic
                    int maxRetries = 5;
                    int retryCount = 0;
                    bool connected = false;

                    while (!connected && retryCount < maxRetries)
                    {
                        try
                        {
                            var client = new MongoClient(connectionString);
                            var database = client.GetDatabase(_databaseName);
                            _connection = new PooledConnection(client, database);

                            // Test connection
                            _logger.LogInformation("Testing MongoDB connection for provider: {Name} (attempt {RetryCount}/{MaxRetries})", Name, retryCount + 1, maxRetries);
                            _connection.Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1)).GetAwaiter().GetResult();
                            _logger.LogInformation("MongoDB connection test successful for provider: {Name}", Name);

                            connected = true;
                            _isInitialized = true;
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            if (retryCount >= maxRetries)
                            {
                                _logger.LogError(ex, "Failed to initialize MongoDB storage provider {Name} after {MaxRetries} attempts", Name, maxRetries);
                                throw new InvalidOperationException($"MongoDB storage provider {Name} is not initialized after {maxRetries} attempts", ex);
                            }

                            _logger.LogWarning(ex, "Failed to connect to MongoDB (attempt {RetryCount}/{MaxRetries}). Retrying in 2 seconds...", retryCount, maxRetries);
                            Thread.Sleep(2000); // Wait 2 seconds before retrying
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize MongoDB storage provider {Name}", Name);
                    throw new InvalidOperationException($"MongoDB storage provider {Name} is not initialized", ex);
                }
            }
        }

        /// <summary>
        /// Creates an index on a collection
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <param name="field">Field name</param>
        /// <param name="unique">Whether the index should be unique</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task<bool> CreateIndexAsync(string collection, string field, bool unique = false)
        {
            EnsureInitialized();
            _logger.LogInformation("Creating index on collection: {Collection}, field: {Field}, unique: {Unique}", collection, field, unique);

            try
            {
                var coll = _connection.Database.GetCollection<BsonDocument>(collection);
                var indexKeysDefinition = Builders<BsonDocument>.IndexKeys.Ascending(field);
                var indexOptions = new CreateIndexOptions { Unique = unique };
                var indexModel = new CreateIndexModel<BsonDocument>(indexKeysDefinition, indexOptions);
                await coll.Indexes.CreateOneAsync(indexModel);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating index on collection: {Collection}, field: {Field}", collection, field);
                throw;
            }
        }

        /// <summary>
        /// Drops an index from a collection
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <param name="field">Field name</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task<bool> DropIndexAsync(string collection, string field)
        {
            EnsureInitialized();
            _logger.LogInformation("Dropping index on collection: {Collection}, field: {Field}", collection, field);

            try
            {
                var coll = _connection.Database.GetCollection<BsonDocument>(collection);
                var indexName = $"{field}_1"; // MongoDB index naming convention
                await coll.Indexes.DropOneAsync(indexName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping index on collection: {Collection}, field: {Field}", collection, field);
                throw;
            }
        }

        /// <summary>
        /// Lists all indexes on a collection
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <returns>List of index names</returns>
        public async Task<IEnumerable<string>> ListIndexesAsync(string collection)
        {
            EnsureInitialized();
            _logger.LogInformation("Listing indexes on collection: {Collection}", collection);

            try
            {
                var coll = _connection.Database.GetCollection<BsonDocument>(collection);
                var indexes = await coll.Indexes.ListAsync();
                var indexNames = new List<string>();

                await indexes.ForEachAsync(index =>
                {
                    if (index.TryGetValue("name", out var name))
                    {
                        indexNames.Add(name.AsString);
                    }
                });

                return indexNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing indexes on collection: {Collection}", collection);
                throw;
            }
        }
    }
}
