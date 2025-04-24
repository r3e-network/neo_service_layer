using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NeoServiceLayer.Services.Storage.Configuration;

namespace NeoServiceLayer.Services.Storage.ConnectionPool
{
    /// <summary>
    /// Connection pool for MongoDB connections
    /// </summary>
    public class MongoDbConnectionPool : IDisposable
    {
        private readonly ILogger<MongoDbConnectionPool> _logger;
        private readonly MongoDbConnectionPoolConfiguration _configuration;
        private readonly ConcurrentDictionary<string, PooledConnection> _connections = new ConcurrentDictionary<string, PooledConnection>();
        private readonly SemaphoreSlim _semaphore;
        private readonly Timer _cleanupTimer;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbConnectionPool"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Connection pool configuration</param>
        public MongoDbConnectionPool(
            ILogger<MongoDbConnectionPool> logger,
            IOptions<MongoDbConnectionPoolConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _semaphore = new SemaphoreSlim(_configuration.MaxConnections);
            _cleanupTimer = new Timer(CleanupConnections, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Gets a connection from the pool
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="databaseName">Database name</param>
        /// <returns>Pooled connection</returns>
        public async Task<PooledConnection> GetConnectionAsync(string connectionString, string databaseName)
        {
            var key = $"{connectionString}:{databaseName}";

            // Check if connection exists in pool
            if (_connections.TryGetValue(key, out var connection))
            {
                // Check if connection is still valid
                if (connection.IsValid())
                {
                    _logger.LogDebug("Using existing connection from pool: {Key}", key);
                    connection.LastUsed = DateTime.UtcNow;
                    return connection;
                }
                else
                {
                    _logger.LogInformation("Connection in pool is no longer valid, creating new connection: {Key}", key);
                    _connections.TryRemove(key, out _);
                    connection.Dispose();
                }
            }

            // Wait for semaphore
            await _semaphore.WaitAsync();

            try
            {
                // Check again after acquiring semaphore
                if (_connections.TryGetValue(key, out connection))
                {
                    if (connection.IsValid())
                    {
                        _logger.LogDebug("Using existing connection from pool after semaphore: {Key}", key);
                        connection.LastUsed = DateTime.UtcNow;
                        return connection;
                    }
                    else
                    {
                        _logger.LogInformation("Connection in pool is no longer valid after semaphore, creating new connection: {Key}", key);
                        _connections.TryRemove(key, out _);
                        connection.Dispose();
                    }
                }

                // Create new connection
                _logger.LogInformation("Creating new connection: {Key}", key);
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);

                // Test connection
                await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));

                // Create pooled connection
                connection = new PooledConnection(client, database);
                _connections[key] = connection;

                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating connection: {Key}", key);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Cleans up idle connections
        /// </summary>
        /// <param name="state">Timer state</param>
        private void CleanupConnections(object state)
        {
            try
            {
                _logger.LogInformation("Cleaning up idle connections");
                var idleThreshold = DateTime.UtcNow.AddMinutes(-_configuration.IdleTimeoutMinutes);
                var keysToRemove = new List<string>();

                foreach (var kvp in _connections)
                {
                    if (kvp.Value.LastUsed < idleThreshold)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    if (_connections.TryRemove(key, out var connection))
                    {
                        _logger.LogInformation("Removing idle connection: {Key}", key);
                        connection.Dispose();
                    }
                }

                _logger.LogInformation("Cleaned up {Count} idle connections, {RemainingCount} connections remaining", keysToRemove.Count, _connections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up idle connections");
            }
        }

        /// <summary>
        /// Disposes the connection pool
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the connection pool
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cleanupTimer?.Dispose();
                    _semaphore?.Dispose();

                    foreach (var connection in _connections.Values)
                    {
                        connection.Dispose();
                    }

                    _connections.Clear();
                }

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Pooled MongoDB connection
    /// </summary>
    public class PooledConnection : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledConnection"/> class
        /// </summary>
        /// <param name="client">MongoDB client</param>
        /// <param name="database">MongoDB database</param>
        public PooledConnection(IMongoClient client, IMongoDatabase database)
        {
            Client = client;
            Database = database;
            Created = DateTime.UtcNow;
            LastUsed = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the MongoDB client
        /// </summary>
        public IMongoClient Client { get; }

        /// <summary>
        /// Gets the MongoDB database
        /// </summary>
        public IMongoDatabase Database { get; }

        /// <summary>
        /// Gets when the connection was created
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        /// Gets or sets when the connection was last used
        /// </summary>
        public DateTime LastUsed { get; set; }

        /// <summary>
        /// Checks if the connection is valid
        /// </summary>
        /// <returns>True if the connection is valid, false otherwise</returns>
        public bool IsValid()
        {
            try
            {
                // Check if connection is still valid
                Database.RunCommandAsync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1)).GetAwaiter().GetResult();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes the connection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the connection
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // MongoDB client doesn't need to be disposed
                }

                _disposed = true;
            }
        }
    }
}
