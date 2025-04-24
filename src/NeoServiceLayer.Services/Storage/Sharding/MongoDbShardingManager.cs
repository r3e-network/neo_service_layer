using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using NeoServiceLayer.Services.Storage.Configuration;

namespace NeoServiceLayer.Services.Storage.Sharding
{
    /// <summary>
    /// Manager for MongoDB sharding
    /// </summary>
    public class MongoDbShardingManager
    {
        private readonly ILogger<MongoDbShardingManager> _logger;
        private readonly MongoDbShardingConfiguration _configuration;
        private IMongoClient _client;
        private IMongoDatabase _adminDb;
        private bool _isInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbShardingManager"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Sharding configuration</param>
        public MongoDbShardingManager(
            ILogger<MongoDbShardingManager> logger,
            IOptions<MongoDbShardingConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _isInitialized = false;
        }

        /// <summary>
        /// Initializes the sharding manager
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task<bool> InitializeAsync()
        {
            _logger.LogInformation("Initializing MongoDB sharding manager");

            try
            {
                // Get configuration options
                var connectionString = _configuration.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("MongoDB connection string is required");
                }

                // Create MongoDB client
                _client = new MongoClient(connectionString);
                _adminDb = _client.GetDatabase("admin");

                // Test connection
                await _adminDb.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing MongoDB sharding manager");
                return false;
            }
        }

        /// <summary>
        /// Enables sharding for a database
        /// </summary>
        /// <param name="databaseName">Database name</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task<bool> EnableShardingForDatabaseAsync(string databaseName)
        {
            EnsureInitialized();
            _logger.LogInformation("Enabling sharding for database: {DatabaseName}", databaseName);

            try
            {
                var command = new BsonDocument
                {
                    { "enableSharding", databaseName }
                };

                await _adminDb.RunCommandAsync<BsonDocument>(command);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling sharding for database: {DatabaseName}", databaseName);
                throw;
            }
        }

        /// <summary>
        /// Enables sharding for a collection
        /// </summary>
        /// <param name="databaseName">Database name</param>
        /// <param name="collectionName">Collection name</param>
        /// <param name="shardKey">Shard key</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task<bool> ShardCollectionAsync(string databaseName, string collectionName, string shardKey)
        {
            EnsureInitialized();
            _logger.LogInformation("Sharding collection: {DatabaseName}.{CollectionName} with key: {ShardKey}", databaseName, collectionName, shardKey);

            try
            {
                var command = new BsonDocument
                {
                    { "shardCollection", $"{databaseName}.{collectionName}" },
                    { "key", new BsonDocument { { shardKey, 1 } } }
                };

                await _adminDb.RunCommandAsync<BsonDocument>(command);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharding collection: {DatabaseName}.{CollectionName}", databaseName, collectionName);
                throw;
            }
        }

        /// <summary>
        /// Adds a shard to the cluster
        /// </summary>
        /// <param name="shardHost">Shard host</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task<bool> AddShardAsync(string shardHost)
        {
            EnsureInitialized();
            _logger.LogInformation("Adding shard: {ShardHost}", shardHost);

            try
            {
                var command = new BsonDocument
                {
                    { "addShard", shardHost }
                };

                await _adminDb.RunCommandAsync<BsonDocument>(command);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding shard: {ShardHost}", shardHost);
                throw;
            }
        }

        /// <summary>
        /// Lists all shards in the cluster
        /// </summary>
        /// <returns>List of shards</returns>
        public async Task<IEnumerable<BsonDocument>> ListShardsAsync()
        {
            EnsureInitialized();
            _logger.LogInformation("Listing shards");

            try
            {
                var command = new BsonDocument
                {
                    { "listShards", 1 }
                };

                var result = await _adminDb.RunCommandAsync<BsonDocument>(command);
                var shards = result["shards"].AsBsonArray;
                
                var shardList = new List<BsonDocument>();
                foreach (var shard in shards)
                {
                    shardList.Add(shard.AsBsonDocument);
                }
                
                return shardList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing shards");
                throw;
            }
        }

        /// <summary>
        /// Gets the status of the sharded cluster
        /// </summary>
        /// <returns>Sharding status</returns>
        public async Task<BsonDocument> GetShardingStatusAsync()
        {
            EnsureInitialized();
            _logger.LogInformation("Getting sharding status");

            try
            {
                var command = new BsonDocument
                {
                    { "printShardingStatus", 1 }
                };

                return await _adminDb.RunCommandAsync<BsonDocument>(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sharding status");
                throw;
            }
        }

        /// <summary>
        /// Ensures the manager is initialized
        /// </summary>
        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("MongoDB sharding manager is not initialized");
            }
        }
    }
}
