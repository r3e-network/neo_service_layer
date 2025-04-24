using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Storage
{
    /// <summary>
    /// Implementation of the object storage service
    /// </summary>
    public class ObjectStorageService : IObjectStorageService
    {
        private readonly ILogger<ObjectStorageService> _logger;
        private readonly IFileStorageProvider _fileStorageProvider;
        private readonly string _basePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectStorageService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="fileStorageProvider">File storage provider</param>
        public ObjectStorageService(ILogger<ObjectStorageService> logger, IFileStorageProvider fileStorageProvider)
        {
            _logger = logger;
            _fileStorageProvider = fileStorageProvider;
            _basePath = "objects";
        }

        /// <inheritdoc/>
        public async Task<string> GetObjectAsync(string containerName, string objectName)
        {
            _logger.LogInformation("Getting object {ObjectName} from container {ContainerName}", objectName, containerName);

            try
            {
                var path = GetObjectPath(containerName, objectName);
                var stream = await _fileStorageProvider.RetrieveAsync(path);
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
                _logger.LogError(ex, "Error getting object {ObjectName} from container {ContainerName}", objectName, containerName);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PutObjectAsync(string containerName, string objectName, string content)
        {
            _logger.LogInformation("Putting object {ObjectName} in container {ContainerName}", objectName, containerName);

            try
            {
                var path = GetObjectPath(containerName, objectName);
                var contentBytes = Encoding.UTF8.GetBytes(content);
                var stream = new MemoryStream(contentBytes);
                var metadata = new Dictionary<string, string>
                {
                    { "ContentType", "application/json" },
                    { "Size", contentBytes.Length.ToString() },
                    { "CreatedAt", DateTime.UtcNow.ToString("o") }
                };

                return await _fileStorageProvider.StoreAsync(path, stream, metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error putting object {ObjectName} in container {ContainerName}", objectName, containerName);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteObjectAsync(string containerName, string objectName)
        {
            _logger.LogInformation("Deleting object {ObjectName} from container {ContainerName}", objectName, containerName);

            try
            {
                var path = GetObjectPath(containerName, objectName);
                return await _fileStorageProvider.DeleteAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting object {ObjectName} from container {ContainerName}", objectName, containerName);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListObjectsAsync(string containerName)
        {
            _logger.LogInformation("Listing objects in container {ContainerName}", containerName);

            try
            {
                var prefix = $"{_basePath}/{containerName}/";
                var objects = await _fileStorageProvider.ListAsync(prefix);
                return objects.Select(o => o.Substring(prefix.Length));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing objects in container {ContainerName}", containerName);
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets the path for an object
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="objectName">Object name</param>
        /// <returns>Object path</returns>
        private string GetObjectPath(string containerName, string objectName)
        {
            return $"{_basePath}/{containerName}/{objectName}";
        }
    }
}
