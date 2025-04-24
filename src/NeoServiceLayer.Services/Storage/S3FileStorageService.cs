using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Storage
{
    /// <summary>
    /// Implementation of the file storage service using AWS S3
    /// </summary>
    public class S3FileStorageService : IFileStorageService
    {
        private readonly ILogger<S3FileStorageService> _logger;
        private readonly StorageConfiguration _configuration;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _cdnBaseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="S3FileStorageService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Storage configuration</param>
        public S3FileStorageService(ILogger<S3FileStorageService> logger, IOptions<StorageConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;

            // Get S3 provider configuration
            var s3Config = _configuration.Providers.FirstOrDefault(p => p.Type == "S3");
            if (s3Config == null)
            {
                throw new InvalidOperationException("S3 storage provider configuration not found");
            }

            // Get configuration options
            s3Config.Options.TryGetValue("BucketName", out var bucketName);
            s3Config.Options.TryGetValue("Region", out var region);
            s3Config.Options.TryGetValue("AccessKey", out var accessKey);
            s3Config.Options.TryGetValue("SecretKey", out var secretKey);
            s3Config.Options.TryGetValue("CdnBaseUrl", out var cdnBaseUrl);

            if (string.IsNullOrEmpty(bucketName))
            {
                throw new InvalidOperationException("S3 bucket name is required");
            }

            _bucketName = bucketName;
            _cdnBaseUrl = cdnBaseUrl;

            // Create S3 client
            var clientConfig = new AmazonS3Config();
            if (!string.IsNullOrEmpty(region))
            {
                clientConfig.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
            }

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            {
                _s3Client = new AmazonS3Client(accessKey, secretKey, clientConfig);
            }
            else
            {
                // Use IAM role or environment variables
                _s3Client = new AmazonS3Client(clientConfig);
            }

            // Check if bucket exists
            var bucketExists = BucketExistsAsync(_bucketName).GetAwaiter().GetResult();
            if (!bucketExists)
            {
                _logger.LogWarning("S3 bucket does not exist: {BucketName}", _bucketName);

                // Create bucket if specified in options
                if (s3Config.Options.TryGetValue("CreateBucketIfNotExists", out var createBucket) &&
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

                // Create key
                var key = GetFileKey(accountId, functionId, fileName);

                // Upload file to S3
                using (var transferUtility = new TransferUtility(_s3Client))
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = _bucketName,
                        Key = key,
                        InputStream = content,
                        ContentType = contentType
                    };

                    if (isPublic)
                    {
                        uploadRequest.CannedACL = S3CannedACL.PublicRead;
                    }

                    await transferUtility.UploadAsync(uploadRequest);
                }

                // Store metadata
                var metadataKey = $"{key}.meta";
                var metadata = new
                {
                    FileName = fileName,
                    ContentType = contentType,
                    Size = content.Length,
                    IsPublic = isPublic,
                    CreatedAt = DateTime.UtcNow
                };
                var metadataJson = JsonSerializer.Serialize(metadata);
                var metadataRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = metadataKey,
                    ContentBody = metadataJson,
                    ContentType = "application/json"
                };
                await _s3Client.PutObjectAsync(metadataRequest);

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
                var key = GetFileKey(accountId, functionId, fileName);
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
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
                var key = GetFileKey(accountId, functionId, fileName);
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);

                // Delete metadata if exists
                var metadataKey = $"{key}.meta";
                var metadataDeleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = metadataKey
                };

                try
                {
                    await _s3Client.DeleteObjectAsync(metadataDeleteRequest);
                }
                catch (AmazonS3Exception)
                {
                    // Ignore if metadata doesn't exist
                }

                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
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
                var prefix = $"files/{accountId}/";
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    Delimiter = "/"
                };

                var files = new List<string>();
                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(listRequest);
                    foreach (var s3Object in response.S3Objects)
                    {
                        var key = s3Object.Key;
                        if (!key.EndsWith(".meta"))
                        {
                            var fileName = key.Substring(key.LastIndexOf('/') + 1);
                            files.Add(fileName);
                        }
                    }
                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                return files;
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
                var prefix = $"files/{accountId}/{functionId}/";
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    Delimiter = "/"
                };

                var files = new List<string>();
                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(listRequest);
                    foreach (var s3Object in response.S3Objects)
                    {
                        var key = s3Object.Key;
                        if (!key.EndsWith(".meta"))
                        {
                            var fileName = key.Substring(key.LastIndexOf('/') + 1);
                            files.Add(fileName);
                        }
                    }
                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                return files;
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
                var key = GetFileKey(accountId, functionId, fileName);

                // Check if file exists
                var getRequest = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                try
                {
                    await _s3Client.GetObjectMetadataAsync(getRequest);
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                // Check if file is public
                var isPublic = false;
                var metadataKey = $"{key}.meta";
                try
                {
                    var metadataRequest = new GetObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = metadataKey
                    };
                    var metadataResponse = await _s3Client.GetObjectAsync(metadataRequest);
                    using var reader = new StreamReader(metadataResponse.ResponseStream);
                    var metadataJson = await reader.ReadToEndAsync();
                    var metadata = JsonSerializer.Deserialize<JsonElement>(metadataJson);
                    isPublic = metadata.GetProperty("IsPublic").GetBoolean();
                }
                catch
                {
                    // Ignore metadata errors
                }

                // Generate URL
                if (isPublic && !string.IsNullOrEmpty(_cdnBaseUrl))
                {
                    // Use CDN URL for public files
                    return $"{_cdnBaseUrl.TrimEnd('/')}/{key}";
                }
                else if (isPublic)
                {
                    // Generate pre-signed URL with long expiration for public files
                    var urlRequest = new GetPreSignedUrlRequest
                    {
                        BucketName = _bucketName,
                        Key = key,
                        Expires = DateTime.UtcNow.AddYears(1)
                    };
                    return _s3Client.GetPreSignedURL(urlRequest);
                }
                else
                {
                    // Generate pre-signed URL with short expiration for private files
                    var urlRequest = new GetPreSignedUrlRequest
                    {
                        BucketName = _bucketName,
                        Key = key,
                        Expires = DateTime.UtcNow.AddHours(1)
                    };
                    return _s3Client.GetPreSignedURL(urlRequest);
                }
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
                var s3Key = GetKeyValueKey(accountId, functionId, key);
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key,
                    ContentBody = value,
                    ContentType = "text/plain"
                };
                await _s3Client.PutObjectAsync(putRequest);
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
                var s3Key = GetKeyValueKey(accountId, functionId, key);
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key
                };

                try
                {
                    var response = await _s3Client.GetObjectAsync(getRequest);
                    using var reader = new StreamReader(response.ResponseStream);
                    return await reader.ReadToEndAsync();
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
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
                var s3Key = GetKeyValueKey(accountId, functionId, key);
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key
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
                var prefix = $"keyvalues/{accountId}/";
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    Delimiter = "/"
                };

                var keys = new List<string>();
                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(listRequest);
                    foreach (var s3Object in response.S3Objects)
                    {
                        var s3Key = s3Object.Key;
                        var keyName = s3Key.Substring(s3Key.LastIndexOf('/') + 1);
                        keys.Add(keyName);
                    }
                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

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
                var prefix = $"keyvalues/{accountId}/{functionId}/";
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    Delimiter = "/"
                };

                var keys = new List<string>();
                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(listRequest);
                    foreach (var s3Object in response.S3Objects)
                    {
                        var s3Key = s3Object.Key;
                        var keyName = s3Key.Substring(s3Key.LastIndexOf('/') + 1);
                        keys.Add(keyName);
                    }
                    listRequest.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

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
                var prefix = $"{accountId}/";
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
                _logger.LogError(ex, "Error getting storage usage for account: {AccountId}", accountId);
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetStorageUsageAsync(Guid accountId, Guid functionId)
        {
            _logger.LogInformation("Getting storage usage for account: {AccountId}, function: {FunctionId}", accountId, functionId);

            try
            {
                var prefix = $"{accountId}/{functionId}/";
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
                _logger.LogError(ex, "Error getting storage usage for account: {AccountId}, function: {FunctionId}", accountId, functionId);
                return 0;
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

        private string GetFileKey(Guid accountId, Guid? functionId, string fileName)
        {
            return functionId.HasValue
                ? $"files/{accountId}/{functionId}/{fileName}"
                : $"files/{accountId}/{fileName}";
        }

        private string GetKeyValueKey(Guid accountId, Guid? functionId, string key)
        {
            return functionId.HasValue
                ? $"keyvalues/{accountId}/{functionId}/{key}"
                : $"keyvalues/{accountId}/{key}";
        }
    }
}
