using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Storage.Providers
{
    /// <summary>
    /// File storage provider for storing data in files
    /// </summary>
    public class FileStorageProvider : Core.Interfaces.IStorageProvider, IFileStorageProvider
    {
        private readonly ILogger<FileStorageProvider> _logger;
        private readonly StorageProviderConfiguration _configuration;
        private string _basePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStorageProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Storage provider configuration</param>
        public FileStorageProvider(ILogger<FileStorageProvider> logger, StorageProviderConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public string Name => _configuration.Name;

        /// <inheritdoc/>
        public string Type => "File";

        /// <inheritdoc/>
        public Task<bool> InitializeAsync()
        {
            _logger.LogInformation("Initializing file storage provider: {Name}", Name);

            try
            {
                _basePath = _configuration.BasePath;
                if (string.IsNullOrEmpty(_basePath))
                {
                    _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", Name);
                }

                // Create base directory if it doesn't exist
                if (!Directory.Exists(_basePath))
                {
                    Directory.CreateDirectory(_basePath);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing file storage provider: {Name}", Name);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public Task<bool> HealthCheckAsync()
        {
            try
            {
                // Check if base directory exists and is writable
                if (!Directory.Exists(_basePath))
                {
                    return Task.FromResult(false);
                }

                // Try to create a temporary file
                var tempFile = Path.Combine(_basePath, $"health_check_{Guid.NewGuid()}.tmp");
                File.WriteAllText(tempFile, "Health check");
                File.Delete(tempFile);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for file storage provider: {Name}", Name);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public async Task<T> CreateAsync<T>(string collection, T entity) where T : class
        {
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
                    else if (idProperty.PropertyType == typeof(int))
                    {
                        // Get all existing IDs
                        var collectionPath = GetCollectionPath(collection);
                        if (!Directory.Exists(collectionPath))
                        {
                            Directory.CreateDirectory(collectionPath);
                        }

                        var files = Directory.GetFiles(collectionPath, "*.json");
                        var ids = new List<int>();
                        foreach (var file in files)
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file);
                            if (int.TryParse(fileName, out var existingId))
                            {
                                ids.Add(existingId);
                            }
                        }

                        id = ids.Count > 0 ? ids.Max() + 1 : 1;
                        idProperty.SetValue(entity, id);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported ID type: {idProperty.PropertyType.Name}");
                    }
                }

                // Create collection directory if it doesn't exist
                var collectionDirectory = GetCollectionPath(collection);
                if (!Directory.Exists(collectionDirectory))
                {
                    Directory.CreateDirectory(collectionDirectory);
                }

                // Save entity to file
                var filePath = GetEntityFilePath(collection, id.ToString());
                var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);

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
            _logger.LogInformation("Getting entity by ID from collection: {Collection}", collection);

            try
            {
                var filePath = GetEntityFilePath(collection, id.ToString());
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by ID from collection: {Collection}", collection);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetByFilterAsync<T>(string collection, Func<T, bool> filter) where T : class
        {
            _logger.LogInformation("Getting entities by filter from collection: {Collection}", collection);

            try
            {
                var collectionPath = GetCollectionPath(collection);
                if (!Directory.Exists(collectionPath))
                {
                    return new List<T>();
                }

                var files = Directory.GetFiles(collectionPath, "*.json");
                var entities = new List<T>();

                foreach (var file in files)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var entity = JsonSerializer.Deserialize<T>(json);
                        if (filter(entity))
                        {
                            entities.Add(entity);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deserializing entity from file: {File}", file);
                    }
                }

                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entities by filter from collection: {Collection}", collection);
                return new List<T>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class
        {
            _logger.LogInformation("Getting all entities from collection: {Collection}", collection);

            try
            {
                var collectionPath = GetCollectionPath(collection);
                if (!Directory.Exists(collectionPath))
                {
                    return new List<T>();
                }

                var files = Directory.GetFiles(collectionPath, "*.json");
                var entities = new List<T>();

                foreach (var file in files)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var entity = JsonSerializer.Deserialize<T>(json);
                        entities.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deserializing entity from file: {File}", file);
                    }
                }

                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all entities from collection: {Collection}", collection);
                return new List<T>();
            }
        }

        /// <inheritdoc/>
        public async Task<T> UpdateAsync<T, TKey>(string collection, TKey id, T entity) where T : class
        {
            _logger.LogInformation("Updating entity in collection: {Collection}", collection);

            try
            {
                var filePath = GetEntityFilePath(collection, id.ToString());
                if (!File.Exists(filePath))
                {
                    return null;
                }

                // Get ID property
                var idProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name == "Id");
                if (idProperty == null)
                {
                    throw new InvalidOperationException($"Entity type {typeof(T).Name} does not have an Id property");
                }

                // Ensure ID is set
                idProperty.SetValue(entity, id);

                // Save entity to file
                var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity in collection: {Collection}", collection);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync<T, TKey>(string collection, TKey id) where T : class
        {
            _logger.LogInformation("Deleting entity from collection: {Collection}", collection);

            try
            {
                var filePath = GetEntityFilePath(collection, id.ToString());
                if (!File.Exists(filePath))
                {
                    return Task.FromResult(false);
                }

                File.Delete(filePath);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity from collection: {Collection}", collection);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class
        {
            _logger.LogInformation("Counting entities in collection: {Collection}", collection);

            try
            {
                var collectionPath = GetCollectionPath(collection);
                if (!Directory.Exists(collectionPath))
                {
                    return Task.FromResult(0);
                }

                var files = Directory.GetFiles(collectionPath, "*.json");

                if (filter == null)
                {
                    return Task.FromResult(files.Length);
                }

                var count = 0;
                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var entity = JsonSerializer.Deserialize<T>(json);
                        if (filter(entity))
                        {
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deserializing entity from file: {File}", file);
                    }
                }

                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities in collection: {Collection}", collection);
                return Task.FromResult(0);
            }
        }

        /// <inheritdoc/>
        public Task<bool> CollectionExistsAsync(string collection)
        {
            var collectionPath = GetCollectionPath(collection);
            return Task.FromResult(Directory.Exists(collectionPath));
        }

        /// <inheritdoc/>
        public Task<bool> CreateCollectionAsync(string collection)
        {
            _logger.LogInformation("Creating collection: {Collection}", collection);

            try
            {
                var collectionPath = GetCollectionPath(collection);
                if (Directory.Exists(collectionPath))
                {
                    return Task.FromResult(false);
                }

                Directory.CreateDirectory(collectionPath);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating collection: {Collection}", collection);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCollectionAsync(string collection)
        {
            _logger.LogInformation("Deleting collection: {Collection}", collection);

            try
            {
                var collectionPath = GetCollectionPath(collection);
                if (!Directory.Exists(collectionPath))
                {
                    return Task.FromResult(false);
                }

                Directory.Delete(collectionPath, true);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting collection: {Collection}", collection);
                return Task.FromResult(false);
            }
        }

        private string GetCollectionPath(string collection)
        {
            return Path.Combine(_basePath, collection);
        }

        private string GetEntityFilePath(string collection, string id)
        {
            return Path.Combine(GetCollectionPath(collection), $"{id}.json");
        }

        /// <inheritdoc/>
        public async Task StoreAsync(string path, Stream content, Dictionary<string, string> metadata = null)
        {
            _logger.LogInformation("Storing file: {Path}", path);

            try
            {
                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(Path.Combine(_basePath, path));
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save file
                var filePath = Path.Combine(_basePath, path);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await content.CopyToAsync(fileStream);
                }

                // Save metadata if provided
                if (metadata != null && metadata.Count > 0)
                {
                    var metadataPath = $"{filePath}.metadata";
                    var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(metadataPath, metadataJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing file: {Path}", path);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Stream> RetrieveAsync(string path)
        {
            _logger.LogInformation("Retrieving file: {Path}", path);

            try
            {
                var filePath = Path.Combine(_basePath, path);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                // Create memory stream and copy file content
                var memoryStream = new MemoryStream();
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }

                // Reset position to beginning
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file: {Path}", path);
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(string path)
        {
            _logger.LogInformation("Deleting file: {Path}", path);

            try
            {
                var filePath = Path.Combine(_basePath, path);
                if (!File.Exists(filePath))
                {
                    return Task.FromResult(false);
                }

                File.Delete(filePath);

                // Delete metadata if exists
                var metadataPath = $"{filePath}.metadata";
                if (File.Exists(metadataPath))
                {
                    File.Delete(metadataPath);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {Path}", path);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> ListAsync(string prefix)
        {
            _logger.LogInformation("Listing files with prefix: {Prefix}", prefix);

            try
            {
                var prefixPath = Path.Combine(_basePath, prefix);
                var directory = Path.GetDirectoryName(prefixPath);
                var searchPattern = $"{Path.GetFileName(prefixPath)}*";

                if (!Directory.Exists(directory))
                {
                    return Task.FromResult<IEnumerable<string>>(new List<string>());
                }

                var files = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
                var result = files
                    .Where(f => !f.EndsWith(".metadata"))
                    .Select(f => f.Substring(_basePath.Length).TrimStart('/', '\\'))
                    .ToList();

                return Task.FromResult<IEnumerable<string>>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files with prefix: {Prefix}", prefix);
                return Task.FromResult<IEnumerable<string>>(new List<string>());
            }
        }

        /// <inheritdoc/>
        public Task<string> GetUrlAsync(string path)
        {
            // For file storage, we just return the path
            return Task.FromResult(Path.Combine(_basePath, path));
        }

        /// <inheritdoc/>
        public Task<long> GetStorageUsageAsync(string prefix)
        {
            _logger.LogInformation("Getting storage usage for prefix: {Prefix}", prefix);

            try
            {
                var prefixPath = Path.Combine(_basePath, prefix);
                var directory = Path.GetDirectoryName(prefixPath);
                var searchPattern = $"{Path.GetFileName(prefixPath)}*";

                if (!Directory.Exists(directory))
                {
                    return Task.FromResult(0L);
                }

                var files = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
                var totalSize = files
                    .Where(f => !f.EndsWith(".metadata"))
                    .Sum(f => new FileInfo(f).Length);

                return Task.FromResult(totalSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage for prefix: {Prefix}", prefix);
                return Task.FromResult(0L);
            }
        }
    }
}
