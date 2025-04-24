using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Storage.Providers
{
    /// <summary>
    /// In-memory storage provider for testing
    /// </summary>
    public class InMemoryStorageProvider : Core.Interfaces.IStorageProvider
    {
        private readonly ILogger<InMemoryStorageProvider> _logger;
        private readonly Dictionary<string, Dictionary<object, object>> _collections = new Dictionary<string, Dictionary<object, object>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryStorageProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public InMemoryStorageProvider(ILogger<InMemoryStorageProvider> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public string Name => "InMemory";

        /// <inheritdoc/>
        public string Type => "InMemory";

        /// <inheritdoc/>
        public Task<bool> InitializeAsync()
        {
            _logger.LogInformation("Initializing in-memory storage provider");
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> HealthCheckAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<T> CreateAsync<T>(string collection, T entity) where T : class
        {
            _logger.LogInformation("Creating entity in collection: {Collection}", collection);

            if (!_collections.TryGetValue(collection, out var entities))
            {
                entities = new Dictionary<object, object>();
                _collections[collection] = entities;
            }

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
                else if (idProperty.PropertyType == typeof(int))
                {
                    id = entities.Count > 0 ? (int)entities.Keys.Max() + 1 : 1;
                    idProperty.SetValue(entity, id);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported ID type: {idProperty.PropertyType.Name}");
                }
            }

            // Store entity
            entities[id] = entity;

            return Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public Task<T> GetByIdAsync<T, TKey>(string collection, TKey id) where T : class
        {
            _logger.LogInformation("Getting entity by ID from collection: {Collection}", collection);

            if (!_collections.TryGetValue(collection, out var entities))
            {
                return Task.FromResult<T>(null);
            }

            if (!entities.TryGetValue(id, out var entity))
            {
                return Task.FromResult<T>(null);
            }

            return Task.FromResult((T)entity);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T>> GetByFilterAsync<T>(string collection, Func<T, bool> filter) where T : class
        {
            _logger.LogInformation("Getting entities by filter from collection: {Collection}", collection);

            if (!_collections.TryGetValue(collection, out var entities))
            {
                return Task.FromResult<IEnumerable<T>>(new List<T>());
            }

            var result = entities.Values
                .Cast<T>()
                .Where(filter)
                .ToList();

            return Task.FromResult<IEnumerable<T>>(result);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class
        {
            _logger.LogInformation("Getting all entities from collection: {Collection}", collection);

            if (!_collections.TryGetValue(collection, out var entities))
            {
                return Task.FromResult<IEnumerable<T>>(new List<T>());
            }

            var result = entities.Values
                .Cast<T>()
                .ToList();

            return Task.FromResult<IEnumerable<T>>(result);
        }

        /// <inheritdoc/>
        public Task<T> UpdateAsync<T, TKey>(string collection, TKey id, T entity) where T : class
        {
            _logger.LogInformation("Updating entity in collection: {Collection}", collection);

            if (!_collections.TryGetValue(collection, out var entities))
            {
                return Task.FromResult<T>(null);
            }

            if (!entities.ContainsKey(id))
            {
                return Task.FromResult<T>(null);
            }

            // Get ID property
            var idProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name == "Id");
            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity type {typeof(T).Name} does not have an Id property");
            }

            // Ensure ID is set
            idProperty.SetValue(entity, id);

            // Update entity
            entities[id] = entity;

            return Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync<T, TKey>(string collection, TKey id) where T : class
        {
            _logger.LogInformation("Deleting entity from collection: {Collection}", collection);

            if (!_collections.TryGetValue(collection, out var entities))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(entities.Remove(id));
        }

        /// <inheritdoc/>
        public Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class
        {
            _logger.LogInformation("Counting entities in collection: {Collection}", collection);

            if (!_collections.TryGetValue(collection, out var entities))
            {
                return Task.FromResult(0);
            }

            if (filter == null)
            {
                return Task.FromResult(entities.Count);
            }

            var count = entities.Values
                .Cast<T>()
                .Count(filter);

            return Task.FromResult(count);
        }

        /// <inheritdoc/>
        public Task<bool> CollectionExistsAsync(string collection)
        {
            return Task.FromResult(_collections.ContainsKey(collection));
        }

        /// <inheritdoc/>
        public Task<bool> CreateCollectionAsync(string collection)
        {
            _logger.LogInformation("Creating collection: {Collection}", collection);

            if (_collections.ContainsKey(collection))
            {
                return Task.FromResult(false);
            }

            _collections[collection] = new Dictionary<object, object>();
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCollectionAsync(string collection)
        {
            _logger.LogInformation("Deleting collection: {Collection}", collection);
            return Task.FromResult(_collections.Remove(collection));
        }

        /// <summary>
        /// Stores a stream at the specified path
        /// </summary>
        /// <param name="path">Path to store the stream at</param>
        /// <param name="content">Content to store</param>
        /// <param name="metadata">Optional metadata</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public Task StoreAsync(string path, Stream content, Dictionary<string, string> metadata = null)
        {
            _logger.LogInformation("Storing content at path: {Path}", path);

            try
            {
                // Create a memory stream to store the content
                var memoryStream = new MemoryStream();
                content.CopyTo(memoryStream);
                memoryStream.Position = 0;

                // Store in memory
                var collection = "files";
                if (!_collections.TryGetValue(collection, out var files))
                {
                    files = new Dictionary<object, object>();
                    _collections[collection] = files;
                }

                files[path] = memoryStream;

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing content at path: {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a stream from the specified path
        /// </summary>
        /// <param name="path">Path to retrieve the stream from</param>
        /// <returns>Stream if found, null otherwise</returns>
        public Task<Stream> RetrieveAsync(string path)
        {
            _logger.LogInformation("Retrieving content from path: {Path}", path);

            try
            {
                var collection = "files";
                if (!_collections.TryGetValue(collection, out var files))
                {
                    return Task.FromResult<Stream>(null);
                }

                if (!files.TryGetValue(path, out var content))
                {
                    return Task.FromResult<Stream>(null);
                }

                var memoryStream = content as MemoryStream;
                if (memoryStream == null)
                {
                    return Task.FromResult<Stream>(null);
                }

                // Create a copy of the memory stream to avoid position issues
                var copy = new MemoryStream(memoryStream.ToArray());
                return Task.FromResult<Stream>(copy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content from path: {Path}", path);
                return Task.FromResult<Stream>(null);
            }
        }

        /// <summary>
        /// Deletes a file at the specified path
        /// </summary>
        /// <param name="path">Path to delete</param>
        /// <returns>True if deleted, false otherwise</returns>
        public Task<bool> DeleteAsync(string path)
        {
            _logger.LogInformation("Deleting content at path: {Path}", path);

            try
            {
                var collection = "files";
                if (!_collections.TryGetValue(collection, out var files))
                {
                    return Task.FromResult(false);
                }

                return Task.FromResult(files.Remove(path));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting content at path: {Path}", path);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Lists files with the specified prefix
        /// </summary>
        /// <param name="prefix">Prefix to filter by</param>
        /// <returns>List of file paths</returns>
        public Task<IEnumerable<string>> ListAsync(string prefix)
        {
            _logger.LogInformation("Listing files with prefix: {Prefix}", prefix);

            try
            {
                var collection = "files";
                if (!_collections.TryGetValue(collection, out var files))
                {
                    return Task.FromResult<IEnumerable<string>>(new List<string>());
                }

                var keys = files.Keys
                    .Cast<string>()
                    .Where(k => k.StartsWith(prefix))
                    .ToList();

                return Task.FromResult<IEnumerable<string>>(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files with prefix: {Prefix}", prefix);
                return Task.FromResult<IEnumerable<string>>(new List<string>());
            }
        }

        /// <summary>
        /// Gets the URL for a file
        /// </summary>
        /// <param name="path">Path to get URL for</param>
        /// <returns>URL for the file</returns>
        public Task<string> GetUrlAsync(string path)
        {
            // In-memory provider doesn't have URLs, so we just return the path
            return Task.FromResult(path);
        }

        /// <summary>
        /// Gets the storage usage for files with the specified prefix
        /// </summary>
        /// <param name="prefix">Prefix to filter by</param>
        /// <returns>Storage usage in bytes</returns>
        public Task<long> GetStorageUsageAsync(string prefix)
        {
            _logger.LogInformation("Getting storage usage for prefix: {Prefix}", prefix);

            try
            {
                var collection = "files";
                if (!_collections.TryGetValue(collection, out var files))
                {
                    return Task.FromResult(0L);
                }

                var usage = files
                    .Where(kv => ((string)kv.Key).StartsWith(prefix))
                    .Sum(kv => (kv.Value as MemoryStream)?.Length ?? 0);

                return Task.FromResult(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage for prefix: {Prefix}", prefix);
                return Task.FromResult(0L);
            }
        }
    }
}
