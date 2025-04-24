using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for storage operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _logger;
        private readonly IStorageService _storageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageService">Storage service</param>
        public StorageController(ILogger<StorageController> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <param name="functionId">Optional function ID</param>
        /// <param name="isPublic">Indicates whether the file is publicly accessible</param>
        /// <returns>URL of the uploaded file</returns>
        [HttpPost("files")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] Guid? functionId = null, [FromQuery] bool isPublic = false)
        {
            _logger.LogInformation("Uploading file: {FileName}, Function: {FunctionId}, IsPublic: {IsPublic}", file.FileName, functionId, isPublic);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Check file size
                if (file.Length > 100 * 1024 * 1024) // 100 MB
                {
                    return BadRequest(new { Message = "File size exceeds maximum allowed size of 100 MB" });
                }

                // Check storage usage
                var usage = await _storageService.GetStorageUsageAsync(accountId);
                if (usage + file.Length > 1024 * 1024 * 1024) // 1 GB
                {
                    return BadRequest(new { Message = "Account storage usage would exceed maximum allowed size of 1 GB" });
                }

                // Store file
                using (var stream = file.OpenReadStream())
                {
                    var url = await _storageService.StoreFileAsync(accountId, functionId, file.FileName, file.ContentType, stream, isPublic);
                    return Ok(new { Url = url });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                return StatusCode(500, new { Message = "An error occurred while uploading the file" });
            }
        }

        /// <summary>
        /// Downloads a file
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="functionId">Optional function ID</param>
        /// <returns>File content</returns>
        [HttpGet("files/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName, [FromQuery] Guid? functionId = null)
        {
            _logger.LogInformation("Downloading file: {FileName}, Function: {FunctionId}", fileName, functionId);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Retrieve file
                var stream = await _storageService.RetrieveFileAsync(accountId, functionId, fileName);
                if (stream == null)
                {
                    return NotFound(new { Message = "File not found" });
                }

                // Determine content type
                var contentType = "application/octet-stream";
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        contentType = "image/jpeg";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".pdf":
                        contentType = "application/pdf";
                        break;
                    case ".txt":
                        contentType = "text/plain";
                        break;
                    case ".json":
                        contentType = "application/json";
                        break;
                    case ".csv":
                        contentType = "text/csv";
                        break;
                }

                return File(stream, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
                return StatusCode(500, new { Message = "An error occurred while downloading the file" });
            }
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="functionId">Optional function ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("files/{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName, [FromQuery] Guid? functionId = null)
        {
            _logger.LogInformation("Deleting file: {FileName}, Function: {FunctionId}", fileName, functionId);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Delete file
                var success = await _storageService.DeleteFileAsync(accountId, functionId, fileName);
                if (!success)
                {
                    return NotFound(new { Message = "File not found" });
                }

                return Ok(new { Message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
                return StatusCode(500, new { Message = "An error occurred while deleting the file" });
            }
        }

        /// <summary>
        /// Lists files
        /// </summary>
        /// <param name="functionId">Optional function ID</param>
        /// <returns>List of file names</returns>
        [HttpGet("files")]
        public async Task<IActionResult> ListFiles([FromQuery] Guid? functionId = null)
        {
            _logger.LogInformation("Listing files, Function: {FunctionId}", functionId);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // List files
                IEnumerable<string> files;
                if (functionId.HasValue)
                {
                    files = await _storageService.ListFilesAsync(accountId, functionId.Value);
                }
                else
                {
                    files = await _storageService.ListFilesAsync(accountId);
                }

                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files");
                return StatusCode(500, new { Message = "An error occurred while listing files" });
            }
        }

        /// <summary>
        /// Stores a key-value pair
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="functionId">Optional function ID</param>
        /// <returns>Success status</returns>
        [HttpPost("keyvalues/{key}")]
        public async Task<IActionResult> StoreKeyValue(string key, [FromBody] string value, [FromQuery] Guid? functionId = null)
        {
            _logger.LogInformation("Storing key-value: {Key}, Function: {FunctionId}", key, functionId);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Store key-value
                var success = await _storageService.StoreKeyValueAsync(accountId, functionId, key, value);
                if (!success)
                {
                    return StatusCode(500, new { Message = "Failed to store key-value" });
                }

                return Ok(new { Message = "Key-value stored successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing key-value: {Key}", key);
                return StatusCode(500, new { Message = "An error occurred while storing key-value" });
            }
        }

        /// <summary>
        /// Retrieves a key-value pair
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="functionId">Optional function ID</param>
        /// <returns>Value</returns>
        [HttpGet("keyvalues/{key}")]
        public async Task<IActionResult> RetrieveKeyValue(string key, [FromQuery] Guid? functionId = null)
        {
            _logger.LogInformation("Retrieving key-value: {Key}, Function: {FunctionId}", key, functionId);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Retrieve key-value
                var value = await _storageService.RetrieveKeyValueAsync(accountId, functionId, key);
                if (value == null)
                {
                    return NotFound(new { Message = "Key not found" });
                }

                return Ok(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving key-value: {Key}", key);
                return StatusCode(500, new { Message = "An error occurred while retrieving key-value" });
            }
        }

        /// <summary>
        /// Deletes a key-value pair
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="functionId">Optional function ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("keyvalues/{key}")]
        public async Task<IActionResult> DeleteKeyValue(string key, [FromQuery] Guid? functionId = null)
        {
            _logger.LogInformation("Deleting key-value: {Key}, Function: {FunctionId}", key, functionId);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Delete key-value
                var success = await _storageService.DeleteKeyValueAsync(accountId, functionId, key);
                if (!success)
                {
                    return NotFound(new { Message = "Key not found" });
                }

                return Ok(new { Message = "Key-value deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting key-value: {Key}", key);
                return StatusCode(500, new { Message = "An error occurred while deleting key-value" });
            }
        }

        /// <summary>
        /// Lists keys
        /// </summary>
        /// <param name="functionId">Optional function ID</param>
        /// <returns>List of keys</returns>
        [HttpGet("keyvalues")]
        public async Task<IActionResult> ListKeys([FromQuery] Guid? functionId = null)
        {
            _logger.LogInformation("Listing keys, Function: {FunctionId}", functionId);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // List keys
                IEnumerable<string> keys;
                if (functionId.HasValue)
                {
                    keys = await _storageService.ListKeysAsync(accountId, functionId.Value);
                }
                else
                {
                    keys = await _storageService.ListKeysAsync(accountId);
                }

                return Ok(keys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing keys");
                return StatusCode(500, new { Message = "An error occurred while listing keys" });
            }
        }

        /// <summary>
        /// Gets storage usage
        /// </summary>
        /// <param name="functionId">Optional function ID</param>
        /// <returns>Storage usage in bytes</returns>
        [HttpGet("usage")]
        public async Task<IActionResult> GetStorageUsage([FromQuery] Guid? functionId = null)
        {
            _logger.LogInformation("Getting storage usage, Function: {FunctionId}", functionId);

            try
            {
                var accountId = GetAccountId();
                if (accountId == Guid.Empty)
                {
                    return Unauthorized(new { Message = "Invalid account ID" });
                }

                // Get storage usage
                long usage;
                if (functionId.HasValue)
                {
                    usage = await _storageService.GetStorageUsageAsync(accountId, functionId.Value);
                }
                else
                {
                    usage = await _storageService.GetStorageUsageAsync(accountId);
                }

                return Ok(new
                {
                    UsageBytes = usage,
                    UsageKB = Math.Round(usage / 1024.0, 2),
                    UsageMB = Math.Round(usage / 1024.0 / 1024.0, 2),
                    UsageGB = Math.Round(usage / 1024.0 / 1024.0 / 1024.0, 2)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage");
                return StatusCode(500, new { Message = "An error occurred while getting storage usage" });
            }
        }

        private Guid GetAccountId()
        {
            var accountIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !Guid.TryParse(accountIdClaim, out var accountId))
            {
                return Guid.Empty;
            }

            return accountId;
        }
    }
}
