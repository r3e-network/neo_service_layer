using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Storage.Providers
{
    /// <summary>
    /// AWS S3 storage provider for storing data in S3 buckets
    /// </summary>
    public class S3StorageProvider : Core.Interfaces.IStorageProvider, Core.Interfaces.IFileStorageProvider
    {
        private readonly ILogger<S3StorageProvider> _logger;
        private readonly StorageProviderConfiguration _configuration;
        private IAmazonS3 _s3Client;
        private string _bucketName;

        /// <summary>
        /// Initializes a new instance of the <see cref="S3StorageProvider"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Storage provider configuration</param>
        public S3StorageProvider(ILogger<S3StorageProvider> logger, StorageProviderConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public string Name => _configuration.Name;

        /// <inheritdoc/>
        public string Type => "S3";

        /// <inheritdoc/>
        public Task<bool> InitializeAsync()
        {
            _logger.LogInformation("Initializing S3 storage provider: {Name}", Name);

            try
            {
                // Get configuration options
                _configuration.Options.TryGetValue("BucketName", out var bucketName);
                _configuration.Options.TryGetValue("Region", out var region);
                _configuration.Options.TryGetValue("AccessKey", out var accessKey);
                _configuration.Options.TryGetValue("SecretKey", out var secretKey);

                if (string.IsNullOrEmpty(bucketName))
                {
                    throw new InvalidOperationException("S3 bucket name is required");
                }

                _bucketName = bucketName;

                // Create S3 client
                var config = new AmazonS3Config();
                if (!string.IsNullOrEmpty(region))
                {
                    config.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
                }

                if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
                {
                    _s3Client = new AmazonS3Client(accessKey, secretKey, config);
                }
                else
                {
                    // Use IAM role or environment variables
                    _s3Client = new AmazonS3Client(config);
                }

                // Check if bucket exists
                var bucketExists = BucketExistsAsync(_bucketName).GetAwaiter().GetResult();
                if (!bucketExists)
                {
                    _logger.LogWarning("S3 bucket does not exist: {BucketName}", _bucketName);

                    // Create bucket if specified in options
                    if (_configuration.Options.TryGetValue("CreateBucketIfNotExists", out var createBucket) &&
                        createBucket.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Creating S3 bucket: {BucketName}", _bucketName);
                        var putBucketRequest = new PutBucketRequest
                        {
                            BucketName = _bucketName
                        };
                        _s3Client.PutBucketAsync(putBucketRequest).GetAwaiter().GetResult();
                    }
                    else
                    {
                        throw new InvalidOperationException($"S3 bucket does not exist: {_bucketName}");
                    }
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing S3 storage provider: {Name}", Name);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                // Check if bucket exists
                var bucketExists = await BucketExistsAsync(_bucketName);
                if (!bucketExists)
                {
                    _logger.LogError("S3 bucket does not exist: {BucketName}", _bucketName);
                    return false;
                }

                // Try to list objects (with max 1 result) to verify access
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    MaxKeys = 1
                };
                await _s3Client.ListObjectsV2Async(listRequest);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for S3 storage provider: {Name}", Name);
                return false;
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
                        // For integer IDs, we need to get the max ID from existing entities
                        var maxId = await GetMaxIdAsync<T>(collection);
                        id = maxId + 1;
                        idProperty.SetValue(entity, id);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported ID type: {idProperty.PropertyType.Name}");
                    }
                }

                // Serialize entity to JSON
                var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });

                // Upload to S3
                var key = GetObjectKey(collection, id.ToString());
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    ContentBody = json,
                    ContentType = "application/json"
                };
                await _s3Client.PutObjectAsync(putRequest);

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
                // Get object from S3
                var key = GetObjectKey(collection, id.ToString());
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                try
                {
                    var response = await _s3Client.GetObjectAsync(getRequest);
                    using var reader = new StreamReader(response.ResponseStream);
                    var json = await reader.ReadToEndAsync();
                    return JsonSerializer.Deserialize<T>(json);
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
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
                // List all objects in the collection
                var prefix = $"{collection}/";
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix
                };

                var entities = new List<T>();
                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(listRequest);
                    foreach (var s3Object in response.S3Objects)
                    {
                        try
                        {
                            var getRequest = new GetObjectRequest
                            {
                                BucketName = _bucketName,
                                Key = s3Object.Key
                            };
                            var getResponse = await _s3Client.GetObjectAsync(getRequest);
                            using var reader = new StreamReader(getResponse.ResponseStream);
                            var json = await reader.ReadToEndAsync();
                            var entity = JsonSerializer.Deserialize<T>(json);
                            if (filter(entity))
                            {
                                entities.Add(entity);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deserializing entity from S3 object: {Key}", s3Object.Key);
                        }
                    }

                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

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
                // List all objects in the collection
                var prefix = $"{collection}/";
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix
                };

                var entities = new List<T>();
                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(listRequest);
                    foreach (var s3Object in response.S3Objects)
                    {
                        try
                        {
                            var getRequest = new GetObjectRequest
                            {
                                BucketName = _bucketName,
                                Key = s3Object.Key
                            };
                            var getResponse = await _s3Client.GetObjectAsync(getRequest);
                            using var reader = new StreamReader(getResponse.ResponseStream);
                            var json = await reader.ReadToEndAsync();
                            var entity = JsonSerializer.Deserialize<T>(json);
                            entities.Add(entity);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deserializing entity from S3 object: {Key}", s3Object.Key);
                        }
                    }

                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

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
                // Get ID property
                var idProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name == "Id");
                if (idProperty == null)
                {
                    throw new InvalidOperationException($"Entity type {typeof(T).Name} does not have an Id property");
                }

                // Ensure ID is set
                idProperty.SetValue(entity, id);

                // Serialize entity to JSON
                var json = JsonSerializer.Serialize(entity, new JsonSerializerOptions { WriteIndented = true });

                // Upload to S3
                var key = GetObjectKey(collection, id.ToString());
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    ContentBody = json,
                    ContentType = "application/json"
                };
                await _s3Client.PutObjectAsync(putRequest);

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
            _logger.LogInformation("Deleting entity from collection: {Collection}", collection);

            try
            {
                // Delete object from S3
                var key = GetObjectKey(collection, id.ToString());
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity from collection: {Collection}", collection);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class
        {
            _logger.LogInformation("Counting entities in collection: {Collection}", collection);

            try
            {
                if (filter == null)
                {
                    // Count all objects in the collection
                    var prefix = $"{collection}/";
                    var listRequest = new ListObjectsV2Request
                    {
                        BucketName = _bucketName,
                        Prefix = prefix
                    };

                    var count = 0;
                    ListObjectsV2Response response;
                    do
                    {
                        response = await _s3Client.ListObjectsV2Async(listRequest);
                        count += response.S3Objects.Count;
                        listRequest.ContinuationToken = response.NextContinuationToken;
                    } while (response.IsTruncated);

                    return count;
                }
                else
                {
                    // Get all entities and apply filter
                    var entities = await GetAllAsync<T>(collection);
                    return entities.Count(filter);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities in collection: {Collection}", collection);
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CollectionExistsAsync(string collection)
        {
            try
            {
                // Check if collection prefix exists
                var prefix = $"{collection}/";
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    MaxKeys = 1
                };

                var response = await _s3Client.ListObjectsV2Async(listRequest);
                return response.S3Objects.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if collection exists: {Collection}", collection);
                return false;
            }
        }

        /// <inheritdoc/>
        public Task<bool> CreateCollectionAsync(string collection)
        {
            // S3 doesn't have the concept of collections or folders, so we just return true
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCollectionAsync(string collection)
        {
            _logger.LogInformation("Deleting collection: {Collection}", collection);

            try
            {
                // Delete all objects with the collection prefix
                var prefix = $"{collection}/";
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix
                };

                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(listRequest);
                    if (response.S3Objects.Count > 0)
                    {
                        var deleteRequest = new DeleteObjectsRequest
                        {
                            BucketName = _bucketName,
                            Objects = response.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList()
                        };
                        await _s3Client.DeleteObjectsAsync(deleteRequest);
                    }
                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting collection: {Collection}", collection);
                return false;
            }
        }

        private async Task<bool> BucketExistsAsync(string bucketName)
        {
            try
            {
                var response = await _s3Client.ListBucketsAsync();
                return response.Buckets.Any(b => b.BucketName == bucketName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if bucket exists: {BucketName}", bucketName);
                return false;
            }
        }

        private string GetObjectKey(string collection, string id)
        {
            return $"{collection}/{id}.json";
        }

        private async Task<int> GetMaxIdAsync<T>(string collection) where T : class
        {
            try
            {
                var entities = await GetAllAsync<T>(collection);
                if (!entities.Any())
                {
                    return 0;
                }

                var idProperty = typeof(T).GetProperties().FirstOrDefault(p => p.Name == "Id");
                if (idProperty == null)
                {
                    throw new InvalidOperationException($"Entity type {typeof(T).Name} does not have an Id property");
                }

                var maxId = 0;
                foreach (var entity in entities)
                {
                    var id = idProperty.GetValue(entity);
                    if (id is int intId && intId > maxId)
                    {
                        maxId = intId;
                    }
                }

                return maxId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting max ID for collection: {Collection}", collection);
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> StoreAsync(string path, Stream content, Dictionary<string, string> metadata = null)
        {
            _logger.LogInformation("Storing file in S3: {Path}", path);

            try
            {
                // Upload to S3
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = path,
                    InputStream = content
                };

                // Add metadata if provided
                if (metadata != null && metadata.Count > 0)
                {
                    foreach (var kvp in metadata)
                    {
                        putRequest.Metadata.Add(kvp.Key, kvp.Value);
                    }
                }

                await _s3Client.PutObjectAsync(putRequest);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing file in S3: {Path}", path);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<Stream> RetrieveAsync(string path)
        {
            _logger.LogInformation("Retrieving file from S3: {Path}", path);

            try
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = path
                };

                try
                {
                    var response = await _s3Client.GetObjectAsync(getRequest);
                    var memoryStream = new MemoryStream();
                    await response.ResponseStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    return memoryStream;
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file from S3: {Path}", path);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string path)
        {
            _logger.LogInformation("Deleting file from S3: {Path}", path);

            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = path
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from S3: {Path}", path);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> ListAsync(string prefix)
        {
            _logger.LogInformation("Listing files from S3 with prefix: {Prefix}", prefix);

            try
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix
                };

                var keys = new List<string>();
                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(listRequest);
                    keys.AddRange(response.S3Objects.Select(o => o.Key));
                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files from S3 with prefix: {Prefix}", prefix);
                return new List<string>();
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetUrlAsync(string path, int expirationMinutes = 60)
        {
            try
            {
                // Generate a pre-signed URL that expires after the specified minutes
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = path,
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
                };

                return await Task.FromResult(_s3Client.GetPreSignedURL(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pre-signed URL for S3 object: {Path}", path);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetStorageUsageAsync(string prefix)
        {
            _logger.LogInformation("Getting storage usage from S3 with prefix: {Prefix}", prefix);

            try
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix
                };

                long totalSize = 0;
                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(listRequest);
                    totalSize += response.S3Objects.Sum(o => o.Size);
                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                return totalSize;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage from S3 with prefix: {Prefix}", prefix);
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, string>> GetMetadataAsync(string path)
        {
            _logger.LogInformation("Getting metadata for file: {Path}", path);

            try
            {
                var getRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = path
                };

                var response = await _s3Client.GetObjectMetadataAsync(getRequest);
                var metadata = new Dictionary<string, string>();

                foreach (var key in response.Metadata.Keys)
                {
                    metadata[key] = response.Metadata[key];
                }

                return metadata;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for file: {Path}", path);
                return new Dictionary<string, string>();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateMetadataAsync(string path, Dictionary<string, string> metadata)
        {
            _logger.LogInformation("Updating metadata for file: {Path}", path);

            try
            {
                // Get the existing object's metadata
                var getRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = path
                };

                var existingMetadata = await _s3Client.GetObjectMetadataAsync(getRequest);

                // Copy the object to itself with new metadata
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = path,
                    DestinationBucket = _bucketName,
                    DestinationKey = path,
                    CannedACL = S3CannedACL.Private,
                    MetadataDirective = S3MetadataDirective.REPLACE
                };

                // Add metadata
                foreach (var kvp in metadata)
                {
                    copyRequest.Metadata[kvp.Key] = kvp.Value;
                }

                await _s3Client.CopyObjectAsync(copyRequest);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metadata for file: {Path}", path);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string path)
        {
            _logger.LogInformation("Checking if file exists: {Path}", path);

            try
            {
                var getRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = path
                };

                await _s3Client.GetObjectMetadataAsync(getRequest);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if file exists: {Path}", path);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetSizeAsync(string path)
        {
            _logger.LogInformation("Getting size of file: {Path}", path);

            try
            {
                var getRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = path
                };

                var response = await _s3Client.GetObjectMetadataAsync(getRequest);
                return response.ContentLength;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting size of file: {Path}", path);
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CopyAsync(string sourcePath, string destinationPath)
        {
            _logger.LogInformation("Copying file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);

            try
            {
                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = sourcePath,
                    DestinationBucket = _bucketName,
                    DestinationKey = destinationPath,
                    CannedACL = S3CannedACL.Private
                };

                await _s3Client.CopyObjectAsync(copyRequest);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> MoveAsync(string sourcePath, string destinationPath)
        {
            _logger.LogInformation("Moving file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);

            try
            {
                // Copy the file
                var copyResult = await CopyAsync(sourcePath, destinationPath);
                if (!copyResult)
                {
                    return false;
                }

                // Delete the source file
                return await DeleteAsync(sourcePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
                return false;
            }
        }
    }
}
