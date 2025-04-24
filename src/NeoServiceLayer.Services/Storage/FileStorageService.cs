using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Storage.Providers;

namespace NeoServiceLayer.Services.Storage
{
    /// <summary>
    /// Implementation of the file storage service
    /// </summary>
    public class FileStorageService : Core.Interfaces.IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;
        private readonly StorageConfiguration _configuration;
        private readonly string _basePath;
        private readonly Core.Interfaces.IFileStorageProvider _fileStorageProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStorageService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Storage configuration</param>
        /// <param name="fileStorageProvider">File storage provider</param>
        public FileStorageService(ILogger<FileStorageService> logger, IOptions<StorageConfiguration> configuration, Core.Interfaces.IFileStorageProvider fileStorageProvider)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _fileStorageProvider = fileStorageProvider;

            // Set base path
            _basePath = _configuration.Providers.FirstOrDefault()?.BasePath;
            if (string.IsNullOrEmpty(_basePath))
            {
                _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
            }

            // Create base directory if it doesn't exist
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> StoreFileAsync(Guid accountId, Guid? functionId, string fileName, Stream content, string contentType, bool isPublic)
        {
            _logger.LogInformation("Storing file: {FileName} for account: {AccountId}, function: {FunctionId}", fileName, accountId, functionId);

            try
            {
                // Validate file size
                var maxFileSize = _configuration.Providers.FirstOrDefault()?.MaxFileSize ?? 104857600; // 100 MB
                if (content.Length > maxFileSize)
                {
                    throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024} MB");
                }

                // Check account storage usage
                var accountUsage = await GetStorageUsageAsync(accountId);
                var maxStorageSize = _configuration.Providers.FirstOrDefault()?.MaxStorageSize ?? 1073741824; // 1 GB
                if (accountUsage + content.Length > maxStorageSize)
                {
                    throw new InvalidOperationException($"Account storage usage would exceed maximum allowed size of {maxStorageSize / 1024 / 1024} MB");
                }

                // Create path for the file
                var path = GetFilePath(accountId, functionId, fileName);

                // Save file using the file storage provider
                content.Position = 0;
                var metadata = new Dictionary<string, string>
                {
                    { "FileName", fileName },
                    { "ContentType", contentType },
                    { "Size", content.Length.ToString() },
                    { "IsPublic", isPublic.ToString() },
                    { "CreatedAt", DateTime.UtcNow.ToString("o") }
                };

                await _fileStorageProvider.StoreAsync(path, content, metadata);

                // Return success
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing file: {FileName} for account: {AccountId}, function: {FunctionId}", fileName, accountId, functionId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<Stream> RetrieveFileAsync(Guid accountId, Guid? functionId, string fileName)
        {
            _logger.LogInformation("Retrieving file: {FileName} for account: {AccountId}, function: {FunctionId}", fileName, accountId, functionId);

            try
            {
                var path = GetFilePath(accountId, functionId, fileName);
                var fileStream = await _fileStorageProvider.RetrieveAsync(path);
                if (fileStream == null)
                {
                    return null;
                }
                return fileStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file: {FileName} for account: {AccountId}, function: {FunctionId}", fileName, accountId, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteFileAsync(Guid accountId, Guid? functionId, string fileName)
        {
            _logger.LogInformation("Deleting file: {FileName} for account: {AccountId}, function: {FunctionId}", fileName, accountId, functionId);

            try
            {
                var path = GetFilePath(accountId, functionId, fileName);
                return await _fileStorageProvider.DeleteAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName} for account: {AccountId}, function: {FunctionId}", fileName, accountId, functionId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListFilesAsync(Guid accountId)
        {
            _logger.LogInformation("Listing files for account: {AccountId}", accountId);

            try
            {
                var prefix = $"accounts/{accountId}/files/";
                var files = await _fileStorageProvider.ListAsync(prefix);

                // Extract just the filenames from the full paths
                var fileNames = files
                    .Select(f => Path.GetFileName(f))
                    .ToList();

                return fileNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files for account: {AccountId}", accountId);
                return new List<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListFunctionFilesAsync(Guid accountId, Guid functionId)
        {
            _logger.LogInformation("Listing files for account: {AccountId}, function: {FunctionId}", accountId, functionId);

            try
            {
                var prefix = $"accounts/{accountId}/functions/{functionId}/files/";
                var files = await _fileStorageProvider.ListAsync(prefix);

                // Extract just the filenames from the full paths
                var fileNames = files
                    .Select(f => Path.GetFileName(f))
                    .ToList();

                return fileNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files for account: {AccountId}, function: {FunctionId}", accountId, functionId);
                return new List<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetFileUrlAsync(Guid accountId, Guid? functionId, string fileName)
        {
            _logger.LogInformation("Getting URL for file: {FileName} for account: {AccountId}, function: {FunctionId}", fileName, accountId, functionId);

            try
            {
                var filePath = Path.Combine(GetDirectoryPath(accountId, functionId), fileName);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                // Check if file is public
                var metadataPath = Path.Combine(GetDirectoryPath(accountId, functionId), $"{fileName}.meta");
                var isPublic = false;
                if (File.Exists(metadataPath))
                {
                    try
                    {
                        var metadata = System.Text.Json.JsonSerializer.Deserialize<dynamic>(File.ReadAllText(metadataPath));
                        isPublic = metadata.GetProperty("IsPublic").GetBoolean();
                    }
                    catch
                    {
                        // Ignore metadata parsing errors
                    }
                }

                var path = GetFilePath(accountId, functionId, fileName);
                return await _fileStorageProvider.GetUrlAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting URL for file: {FileName} for account: {AccountId}, function: {FunctionId}", fileName, accountId, functionId);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> StoreKeyValueAsync(Guid accountId, Guid? functionId, string key, string value)
        {
            _logger.LogInformation("Storing key-value: {Key} for account: {AccountId}, function: {FunctionId}", key, accountId, functionId);

            try
            {
                // Create directory structure
                var directoryPath = GetKeyValueDirectoryPath(accountId, functionId);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Save value
                var filePath = Path.Combine(directoryPath, key);
                await File.WriteAllTextAsync(filePath, value);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing key-value: {Key} for account: {AccountId}, function: {FunctionId}", key, accountId, functionId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> RetrieveKeyValueAsync(Guid accountId, Guid? functionId, string key)
        {
            _logger.LogInformation("Retrieving key-value: {Key} for account: {AccountId}, function: {FunctionId}", key, accountId, functionId);

            try
            {
                var filePath = Path.Combine(GetKeyValueDirectoryPath(accountId, functionId), key);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving key-value: {Key} for account: {AccountId}, function: {FunctionId}", key, accountId, functionId);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteKeyValueAsync(Guid accountId, Guid? functionId, string key)
        {
            _logger.LogInformation("Deleting key-value: {Key} for account: {AccountId}, function: {FunctionId}", key, accountId, functionId);

            try
            {
                var filePath = Path.Combine(GetKeyValueDirectoryPath(accountId, functionId), key);
                if (!File.Exists(filePath))
                {
                    return false;
                }

                File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting key-value: {Key} for account: {AccountId}, function: {FunctionId}", key, accountId, functionId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListKeysAsync(Guid accountId)
        {
            _logger.LogInformation("Listing keys for account: {AccountId}", accountId);

            try
            {
                var directoryPath = GetKeyValueDirectoryPath(accountId, null);
                if (!Directory.Exists(directoryPath))
                {
                    return new List<string>();
                }

                var keys = Directory.GetFiles(directoryPath)
                    .Select(f => Path.GetFileName(f))
                    .ToList();

                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing keys for account: {AccountId}", accountId);
                return new List<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListKeysAsync(Guid accountId, Guid functionId)
        {
            _logger.LogInformation("Listing keys for account: {AccountId}, function: {FunctionId}", accountId, functionId);

            try
            {
                var directoryPath = GetKeyValueDirectoryPath(accountId, functionId);
                if (!Directory.Exists(directoryPath))
                {
                    return new List<string>();
                }

                var keys = Directory.GetFiles(directoryPath)
                    .Select(f => Path.GetFileName(f))
                    .ToList();

                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing keys for account: {AccountId}, function: {FunctionId}", accountId, functionId);
                return new List<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetStorageUsageAsync(Guid accountId)
        {
            _logger.LogInformation("Getting storage usage for account: {AccountId}", accountId);

            try
            {
                var prefix = $"accounts/{accountId}/";
                var directoryPath = Path.Combine(_basePath, accountId.ToString());
                return GetDirectorySize(directoryPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage for account: {AccountId}", accountId);
                return 0L;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetStorageUsageAsync(Guid accountId, Guid functionId)
        {
            _logger.LogInformation("Getting storage usage for account: {AccountId}, function: {FunctionId}", accountId, functionId);

            try
            {
                var prefix = $"accounts/{accountId}/functions/{functionId}/";
                var directoryPath = Path.Combine(_basePath, accountId.ToString(), functionId.ToString());
                return GetDirectorySize(directoryPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage for account: {AccountId}, function: {FunctionId}", accountId, functionId);
                return 0L;
            }
        }

        private string GetDirectoryPath(Guid accountId, Guid? functionId)
        {
            return functionId.HasValue
                ? Path.Combine(_basePath, accountId.ToString(), functionId.ToString(), "files")
                : Path.Combine(_basePath, accountId.ToString(), "files");
        }

        private string GetFilePath(Guid accountId, Guid? functionId, string fileName)
        {
            return functionId.HasValue
                ? $"accounts/{accountId}/functions/{functionId}/files/{fileName}"
                : $"accounts/{accountId}/files/{fileName}";
        }

        private string GetKeyValueDirectoryPath(Guid accountId, Guid? functionId)
        {
            return functionId.HasValue
                ? Path.Combine(_basePath, accountId.ToString(), functionId.ToString(), "keyvalues")
                : Path.Combine(_basePath, accountId.ToString(), "keyvalues");
        }

        private long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
            {
                return 0;
            }

            try
            {
                var directoryInfo = new DirectoryInfo(path);
                return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating directory size for {Path}", path);
                return 0;
            }
        }
    }
}
