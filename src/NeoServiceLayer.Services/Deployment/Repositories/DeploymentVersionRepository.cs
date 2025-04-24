using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Services.Deployment.Repositories
{
    /// <summary>
    /// Repository for deployment versions
    /// </summary>
    public class DeploymentVersionRepository : IDeploymentVersionRepository
    {
        private readonly ILogger<DeploymentVersionRepository> _logger;
        private readonly IObjectStorageService _storageService;
        private readonly string _containerName = "deployment-versions";
        private readonly Dictionary<Guid, DeploymentVersion> _versions = new Dictionary<Guid, DeploymentVersion>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentVersionRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageService">Storage service</param>
        public DeploymentVersionRepository(ILogger<DeploymentVersionRepository> logger, IObjectStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> CreateAsync(DeploymentVersion version)
        {
            _logger.LogInformation("Creating version: {Id}", version.Id);

            if (version.Id == Guid.Empty)
            {
                version.Id = Guid.NewGuid();
            }

            version.CreatedAt = DateTime.UtcNow;
            version.UpdatedAt = DateTime.UtcNow;

            // Initialize validation results if not set
            if (version.ValidationResults == null)
            {
                version.ValidationResults = new ValidationResults
                {
                    Passed = false,
                    ValidatedAt = DateTime.MinValue
                };
            }

            // Store in memory
            _versions[version.Id] = version;

            // Store in persistent storage
            await StoreVersionAsync(version);

            return version;
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> GetAsync(Guid id)
        {
            _logger.LogInformation("Getting version: {Id}", id);

            // Try to get from memory first
            if (_versions.TryGetValue(id, out var version))
            {
                return version;
            }

            // If not in memory, try to get from storage
            try
            {
                var json = await _storageService.GetObjectAsync(_containerName, $"{id}.json");
                if (!string.IsNullOrEmpty(json))
                {
                    version = System.Text.Json.JsonSerializer.Deserialize<DeploymentVersion>(json);
                    if (version != null)
                    {
                        // Add to memory cache
                        _versions[version.Id] = version;
                        return version;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version from storage: {Id}", id);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentVersion>> GetByDeploymentAsync(Guid deploymentId)
        {
            _logger.LogInformation("Getting versions for deployment: {DeploymentId}", deploymentId);

            // Get all versions from memory and filter by deployment ID
            var versions = _versions.Values.Where(v => v.DeploymentId == deploymentId).ToList();

            // If no versions in memory, try to get from storage
            if (versions.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_versions.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the version from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var version = System.Text.Json.JsonSerializer.Deserialize<DeploymentVersion>(json);
                            if (version != null && version.DeploymentId == deploymentId)
                            {
                                // Add to memory cache
                                _versions[version.Id] = version;
                                versions.Add(version);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting versions from storage for deployment: {DeploymentId}", deploymentId);
                }
            }

            return versions;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentVersion>> GetByFunctionAsync(Guid functionId)
        {
            _logger.LogInformation("Getting versions for function: {FunctionId}", functionId);

            // Get all versions from memory and filter by function ID
            var versions = _versions.Values.Where(v => v.FunctionId == functionId).ToList();

            // If no versions in memory, try to get from storage
            if (versions.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_versions.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the version from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var version = System.Text.Json.JsonSerializer.Deserialize<DeploymentVersion>(json);
                            if (version != null && version.FunctionId == functionId)
                            {
                                // Add to memory cache
                                _versions[version.Id] = version;
                                versions.Add(version);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting versions from storage for function: {FunctionId}", functionId);
                }
            }

            return versions;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentVersion>> GetByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting versions for account: {AccountId}", accountId);

            // Get all versions from memory and filter by account ID
            var versions = _versions.Values.Where(v => v.AccountId == accountId).ToList();

            // If no versions in memory, try to get from storage
            if (versions.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_versions.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the version from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var version = System.Text.Json.JsonSerializer.Deserialize<DeploymentVersion>(json);
                            if (version != null && version.AccountId == accountId)
                            {
                                // Add to memory cache
                                _versions[version.Id] = version;
                                versions.Add(version);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting versions from storage for account: {AccountId}", accountId);
                }
            }

            return versions;
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> UpdateAsync(DeploymentVersion version)
        {
            _logger.LogInformation("Updating version: {Id}", version.Id);

            // Check if version exists
            var existingVersion = await GetAsync(version.Id);
            if (existingVersion == null)
            {
                throw new ArgumentException($"Version not found: {version.Id}");
            }

            // Update timestamp
            version.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _versions[version.Id] = version;

            // Update in persistent storage
            await StoreVersionAsync(version);

            return version;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting version: {Id}", id);

            // Check if version exists
            var existingVersion = await GetAsync(id);
            if (existingVersion == null)
            {
                return false;
            }

            // Remove from memory
            _versions.Remove(id);

            // Remove from persistent storage
            try
            {
                await _storageService.DeleteObjectAsync(_containerName, $"{id}.json");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting version from storage: {Id}", id);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> UpdateStatusAsync(Guid id, VersionStatus status)
        {
            _logger.LogInformation("Updating status for version: {Id} to {Status}", id, status);

            // Check if version exists
            var version = await GetAsync(id);
            if (version == null)
            {
                throw new ArgumentException($"Version not found: {id}");
            }

            // Update status
            version.Status = status;
            version.UpdatedAt = DateTime.UtcNow;

            // If deployed, update deployed timestamp
            if (status == VersionStatus.Deployed)
            {
                version.DeployedAt = DateTime.UtcNow;
            }

            // Update in memory
            _versions[id] = version;

            // Update in persistent storage
            await StoreVersionAsync(version);

            return version;
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> UpdateValidationResultsAsync(Guid id, ValidationResults validationResults)
        {
            _logger.LogInformation("Updating validation results for version: {Id}", id);

            // Check if version exists
            var version = await GetAsync(id);
            if (version == null)
            {
                throw new ArgumentException($"Version not found: {id}");
            }

            // Update validation results
            validationResults.ValidatedAt = DateTime.UtcNow;
            version.ValidationResults = validationResults;
            version.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _versions[id] = version;

            // Update in persistent storage
            await StoreVersionAsync(version);

            return version;
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> AddLogAsync(Guid id, DeploymentLog log)
        {
            _logger.LogInformation("Adding log entry to version: {Id}", id);

            // Check if version exists
            var version = await GetAsync(id);
            if (version == null)
            {
                throw new ArgumentException($"Version not found: {id}");
            }

            // Initialize logs list if null
            if (version.Logs == null)
            {
                version.Logs = new List<DeploymentLog>();
            }

            // Add log entry
            version.Logs.Add(log);
            version.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _versions[id] = version;

            // Update in persistent storage
            await StoreVersionAsync(version);

            return version;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentLog>> GetLogsAsync(Guid id)
        {
            _logger.LogInformation("Getting logs for version: {Id}", id);

            // Check if version exists
            var version = await GetAsync(id);
            if (version == null)
            {
                throw new ArgumentException($"Version not found: {id}");
            }

            // Return logs or empty list if null
            return version.Logs ?? new List<DeploymentLog>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentVersion>> GetByStatusAsync(VersionStatus status)
        {
            _logger.LogInformation("Getting versions with status: {Status}", status);

            // Get all versions from memory and filter by status
            var versions = _versions.Values.Where(v => v.Status == status).ToList();

            // If no versions in memory, try to get from storage
            if (versions.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_versions.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the version from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var version = System.Text.Json.JsonSerializer.Deserialize<DeploymentVersion>(json);
                            if (version != null && version.Status == status)
                            {
                                // Add to memory cache
                                _versions[version.Id] = version;
                                versions.Add(version);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting versions from storage with status: {Status}", status);
                }
            }

            return versions;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentVersion>> GetByTagAsync(string tagKey, string tagValue)
        {
            _logger.LogInformation("Getting versions with tag: {TagKey}={TagValue}", tagKey, tagValue);

            // Get all versions from memory and filter by tag
            var versions = _versions.Values.Where(v => v.Tags != null && v.Tags.TryGetValue(tagKey, out var value) && value == tagValue).ToList();

            // If no versions in memory, try to get from storage
            if (versions.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_versions.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the version from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var version = System.Text.Json.JsonSerializer.Deserialize<DeploymentVersion>(json);
                            if (version != null && version.Tags != null && version.Tags.TryGetValue(tagKey, out var value) && value == tagValue)
                            {
                                // Add to memory cache
                                _versions[version.Id] = version;
                                versions.Add(version);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting versions from storage with tag: {TagKey}={TagValue}", tagKey, tagValue);
                }
            }

            return versions;
        }

        /// <summary>
        /// Stores a version in persistent storage
        /// </summary>
        /// <param name="version">Version to store</param>
        private async Task StoreVersionAsync(DeploymentVersion version)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(version);
                await _storageService.PutObjectAsync(_containerName, $"{version.Id}.json", json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing version in storage: {Id}", version.Id);
                throw;
            }
        }
    }
}
