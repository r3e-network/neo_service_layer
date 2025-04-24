using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Storage.Providers;

namespace NeoServiceLayer.Services.Storage
{
    /// <summary>
    /// Implementation of the storage service
    /// </summary>
    public class StorageService : IStorageService
    {
        private readonly ILogger<StorageService> _logger;
        private readonly StorageOptions _options;
        private readonly S3StorageProvider _fileStorageProvider;
        private readonly InMemoryStorageProvider _keyValueStorageProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="options">Storage options</param>
        /// <param name="fileStorageProvider">File storage provider</param>
        /// <param name="keyValueStorageProvider">Key-value storage provider</param>
        public StorageService(
            ILogger<StorageService> logger,
            IOptions<StorageOptions> options,
            S3StorageProvider fileStorageProvider,
            InMemoryStorageProvider keyValueStorageProvider)
        {
            _logger = logger;
            _options = options.Value;
            _fileStorageProvider = fileStorageProvider;
            _keyValueStorageProvider = keyValueStorageProvider;
        }

        /// <inheritdoc/>
        public async Task<string> StoreFileAsync(Guid accountId, Guid? functionId, string fileName, string contentType, Stream content, bool isPublic = false)
        {
            _logger.LogInformation("Storing file {FileName} for account {AccountId}, function {FunctionId}", fileName, accountId, functionId);

            try
            {
                var path = GetFilePath(accountId, functionId, fileName);
                var metadata = new Dictionary<string, string>
                {
                    { "AccountId", accountId.ToString() },
                    { "ContentType", contentType },
                    { "IsPublic", isPublic.ToString() }
                };

                if (functionId.HasValue)
                {
                    metadata.Add("FunctionId", functionId.Value.ToString());
                }

                await _fileStorageProvider.StoreAsync(path, content, metadata);
                return await GetFileUrlAsync(accountId, functionId, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing file {FileName} for account {AccountId}, function {FunctionId}", fileName, accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Stream> RetrieveFileAsync(Guid accountId, Guid? functionId, string fileName)
        {
            _logger.LogInformation("Retrieving file {FileName} for account {AccountId}, function {FunctionId}", fileName, accountId, functionId);

            try
            {
                var path = GetFilePath(accountId, functionId, fileName);
                return await _fileStorageProvider.RetrieveAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file {FileName} for account {AccountId}, function {FunctionId}", fileName, accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteFileAsync(Guid accountId, Guid? functionId, string fileName)
        {
            _logger.LogInformation("Deleting file {FileName} for account {AccountId}, function {FunctionId}", fileName, accountId, functionId);

            try
            {
                var path = GetFilePath(accountId, functionId, fileName);
                return await _fileStorageProvider.DeleteAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName} for account {AccountId}, function {FunctionId}", fileName, accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListFilesAsync(Guid accountId)
        {
            _logger.LogInformation("Listing files for account {AccountId}", accountId);

            try
            {
                var prefix = $"{accountId}/";
                var files = await _fileStorageProvider.ListAsync(prefix);
                return files;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files for account {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListFilesAsync(Guid accountId, Guid functionId)
        {
            _logger.LogInformation("Listing files for account {AccountId}, function {FunctionId}", accountId, functionId);

            try
            {
                var prefix = $"{accountId}/{functionId}/";
                var files = await _fileStorageProvider.ListAsync(prefix);
                return files;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files for account {AccountId}, function {FunctionId}", accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetFileUrlAsync(Guid accountId, Guid? functionId, string fileName)
        {
            _logger.LogInformation("Getting URL for file {FileName} for account {AccountId}, function {FunctionId}", fileName, accountId, functionId);

            try
            {
                var path = GetFilePath(accountId, functionId, fileName);
                return await _fileStorageProvider.GetUrlAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting URL for file {FileName} for account {AccountId}, function {FunctionId}", fileName, accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> StoreKeyValueAsync(Guid accountId, Guid? functionId, string key, string value)
        {
            _logger.LogInformation("Storing key-value pair {Key} for account {AccountId}, function {FunctionId}", key, accountId, functionId);

            try
            {
                var path = GetKeyPath(accountId, functionId, key);
                var content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(value));
                var metadata = new Dictionary<string, string>
                {
                    { "AccountId", accountId.ToString() }
                };

                if (functionId.HasValue)
                {
                    metadata.Add("FunctionId", functionId.Value.ToString());
                }

                await _keyValueStorageProvider.StoreAsync(path, content, metadata);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing key-value pair {Key} for account {AccountId}, function {FunctionId}", key, accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> RetrieveKeyValueAsync(Guid accountId, Guid? functionId, string key)
        {
            _logger.LogInformation("Retrieving key-value pair {Key} for account {AccountId}, function {FunctionId}", key, accountId, functionId);

            try
            {
                var path = GetKeyPath(accountId, functionId, key);
                var stream = await _keyValueStorageProvider.RetrieveAsync(path);

                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving key-value pair {Key} for account {AccountId}, function {FunctionId}", key, accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteKeyValueAsync(Guid accountId, Guid? functionId, string key)
        {
            _logger.LogInformation("Deleting key-value pair {Key} for account {AccountId}, function {FunctionId}", key, accountId, functionId);

            try
            {
                var path = GetKeyPath(accountId, functionId, key);
                return await _keyValueStorageProvider.DeleteAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting key-value pair {Key} for account {AccountId}, function {FunctionId}", key, accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListKeysAsync(Guid accountId)
        {
            _logger.LogInformation("Listing keys for account {AccountId}", accountId);

            try
            {
                var prefix = $"{accountId}/";
                var keys = await _keyValueStorageProvider.ListAsync(prefix);
                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing keys for account {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListKeysAsync(Guid accountId, Guid functionId)
        {
            _logger.LogInformation("Listing keys for account {AccountId}, function {FunctionId}", accountId, functionId);

            try
            {
                var prefix = $"{accountId}/{functionId}/";
                var keys = await _keyValueStorageProvider.ListAsync(prefix);
                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing keys for account {AccountId}, function {FunctionId}", accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetStorageUsageAsync(Guid accountId)
        {
            _logger.LogInformation("Getting storage usage for account {AccountId}", accountId);

            try
            {
                var prefix = $"{accountId}/";
                var fileUsage = await _fileStorageProvider.GetStorageUsageAsync(prefix);
                var keyValueUsage = await _keyValueStorageProvider.GetStorageUsageAsync(prefix);
                return fileUsage + keyValueUsage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage for account {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetStorageUsageAsync(Guid accountId, Guid functionId)
        {
            _logger.LogInformation("Getting storage usage for account {AccountId}, function {FunctionId}", accountId, functionId);

            try
            {
                var prefix = $"{accountId}/{functionId}/";
                var fileUsage = await _fileStorageProvider.GetStorageUsageAsync(prefix);
                var keyValueUsage = await _keyValueStorageProvider.GetStorageUsageAsync(prefix);
                return fileUsage + keyValueUsage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage for account {AccountId}, function {FunctionId}", accountId, functionId);
                throw;
            }
        }

        /// <summary>
        /// Gets the file path for a file
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID</param>
        /// <param name="fileName">File name</param>
        /// <returns>File path</returns>
        private string GetFilePath(Guid accountId, Guid? functionId, string fileName)
        {
            if (functionId.HasValue)
            {
                return $"{accountId}/{functionId.Value}/files/{fileName}";
            }
            else
            {
                return $"{accountId}/files/{fileName}";
            }
        }

        /// <summary>
        /// Gets the key path for a key-value pair
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID</param>
        /// <param name="key">Key</param>
        /// <returns>Key path</returns>
        private string GetKeyPath(Guid accountId, Guid? functionId, string key)
        {
            if (functionId.HasValue)
            {
                return $"{accountId}/{functionId.Value}/keys/{key}";
            }
            else
            {
                return $"{accountId}/keys/{key}";
            }
        }
    }
}
