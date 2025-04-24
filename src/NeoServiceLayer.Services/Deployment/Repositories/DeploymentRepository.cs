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
    /// Repository for deployments
    /// </summary>
    public class DeploymentRepository : IDeploymentRepository
    {
        private readonly ILogger<DeploymentRepository> _logger;
        private readonly IObjectStorageService _storageService;
        private readonly string _containerName = "deployments";
        private readonly Dictionary<Guid, Core.Models.Deployment.Deployment> _deployments = new Dictionary<Guid, Core.Models.Deployment.Deployment>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageService">Storage service</param>
        public DeploymentRepository(ILogger<DeploymentRepository> logger, IObjectStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> CreateAsync(Core.Models.Deployment.Deployment deployment)
        {
            _logger.LogInformation("Creating deployment: {Id}", deployment.Id);

            if (deployment.Id == Guid.Empty)
            {
                deployment.Id = Guid.NewGuid();
            }

            deployment.CreatedAt = DateTime.UtcNow;
            deployment.UpdatedAt = DateTime.UtcNow;

            // Initialize metrics and health if not set
            if (deployment.Metrics == null)
            {
                deployment.Metrics = new DeploymentMetrics
                {
                    LastUpdatedAt = DateTime.UtcNow
                };
            }

            if (deployment.Health == null)
            {
                deployment.Health = new DeploymentHealth
                {
                    Status = HealthStatus.Unknown,
                    LastCheckedAt = DateTime.UtcNow
                };
            }

            // Store in memory
            _deployments[deployment.Id] = deployment;

            // Store in persistent storage
            await StoreDeploymentAsync(deployment);

            return deployment;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> GetAsync(Guid id)
        {
            _logger.LogInformation("Getting deployment: {Id}", id);

            // Try to get from memory first
            if (_deployments.TryGetValue(id, out var deployment))
            {
                return deployment;
            }

            // If not in memory, try to get from storage
            try
            {
                var json = await _storageService.GetObjectAsync(_containerName, $"{id}.json");
                if (!string.IsNullOrEmpty(json))
                {
                    deployment = System.Text.Json.JsonSerializer.Deserialize<Core.Models.Deployment.Deployment>(json);
                    if (deployment != null)
                    {
                        // Add to memory cache
                        _deployments[deployment.Id] = deployment;
                        return deployment;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployment from storage: {Id}", id);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Deployment.Deployment>> GetByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting deployments for account: {AccountId}", accountId);

            // Get all deployments from memory and filter by account ID
            var deployments = _deployments.Values.Where(d => d.AccountId == accountId).ToList();

            // If no deployments in memory, try to get from storage
            if (deployments.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_deployments.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the deployment from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var deployment = System.Text.Json.JsonSerializer.Deserialize<Core.Models.Deployment.Deployment>(json);
                            if (deployment != null && deployment.AccountId == accountId)
                            {
                                // Add to memory cache
                                _deployments[deployment.Id] = deployment;
                                deployments.Add(deployment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting deployments from storage for account: {AccountId}", accountId);
                }
            }

            return deployments;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Deployment.Deployment>> GetByFunctionAsync(Guid functionId)
        {
            _logger.LogInformation("Getting deployments for function: {FunctionId}", functionId);

            // Get all deployments from memory and filter by function ID
            var deployments = _deployments.Values.Where(d => d.FunctionId == functionId).ToList();

            // If no deployments in memory, try to get from storage
            if (deployments.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_deployments.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the deployment from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var deployment = System.Text.Json.JsonSerializer.Deserialize<Core.Models.Deployment.Deployment>(json);
                            if (deployment != null && deployment.FunctionId == functionId)
                            {
                                // Add to memory cache
                                _deployments[deployment.Id] = deployment;
                                deployments.Add(deployment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting deployments from storage for function: {FunctionId}", functionId);
                }
            }

            return deployments;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Deployment.Deployment>> GetByEnvironmentAsync(Guid environmentId)
        {
            _logger.LogInformation("Getting deployments for environment: {EnvironmentId}", environmentId);

            // Get all deployments from memory and filter by environment ID
            var deployments = _deployments.Values.Where(d => d.EnvironmentId == environmentId).ToList();

            // If no deployments in memory, try to get from storage
            if (deployments.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_deployments.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the deployment from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var deployment = System.Text.Json.JsonSerializer.Deserialize<Core.Models.Deployment.Deployment>(json);
                            if (deployment != null && deployment.EnvironmentId == environmentId)
                            {
                                // Add to memory cache
                                _deployments[deployment.Id] = deployment;
                                deployments.Add(deployment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting deployments from storage for environment: {EnvironmentId}", environmentId);
                }
            }

            return deployments;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> UpdateAsync(Core.Models.Deployment.Deployment deployment)
        {
            _logger.LogInformation("Updating deployment: {Id}", deployment.Id);

            // Check if deployment exists
            var existingDeployment = await GetAsync(deployment.Id);
            if (existingDeployment == null)
            {
                throw new ArgumentException($"Deployment not found: {deployment.Id}");
            }

            // Update timestamp
            deployment.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _deployments[deployment.Id] = deployment;

            // Update in persistent storage
            await StoreDeploymentAsync(deployment);

            return deployment;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting deployment: {Id}", id);

            // Check if deployment exists
            var existingDeployment = await GetAsync(id);
            if (existingDeployment == null)
            {
                return false;
            }

            // Remove from memory
            _deployments.Remove(id);

            // Remove from persistent storage
            try
            {
                await _storageService.DeleteObjectAsync(_containerName, $"{id}.json");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting deployment from storage: {Id}", id);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> UpdateMetricsAsync(Guid id, DeploymentMetrics metrics)
        {
            _logger.LogInformation("Updating metrics for deployment: {Id}", id);

            // Check if deployment exists
            var deployment = await GetAsync(id);
            if (deployment == null)
            {
                throw new ArgumentException($"Deployment not found: {id}");
            }

            // Update metrics
            metrics.LastUpdatedAt = DateTime.UtcNow;
            deployment.Metrics = metrics;
            deployment.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _deployments[id] = deployment;

            // Update in persistent storage
            await StoreDeploymentAsync(deployment);

            return deployment;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> UpdateHealthAsync(Guid id, DeploymentHealth health)
        {
            _logger.LogInformation("Updating health for deployment: {Id}", id);

            // Check if deployment exists
            var deployment = await GetAsync(id);
            if (deployment == null)
            {
                throw new ArgumentException($"Deployment not found: {id}");
            }

            // Update health
            health.LastCheckedAt = DateTime.UtcNow;
            deployment.Health = health;
            deployment.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _deployments[id] = deployment;

            // Update in persistent storage
            await StoreDeploymentAsync(deployment);

            return deployment;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> UpdateStatusAsync(Guid id, DeploymentStatus status)
        {
            _logger.LogInformation("Updating status for deployment: {Id} to {Status}", id, status);

            // Check if deployment exists
            var deployment = await GetAsync(id);
            if (deployment == null)
            {
                throw new ArgumentException($"Deployment not found: {id}");
            }

            // Update status
            deployment.Status = status;
            deployment.UpdatedAt = DateTime.UtcNow;

            // If deployed, update last deployed timestamp
            if (status == DeploymentStatus.Deployed)
            {
                deployment.LastDeployedAt = DateTime.UtcNow;
            }

            // Update in memory
            _deployments[id] = deployment;

            // Update in persistent storage
            await StoreDeploymentAsync(deployment);

            return deployment;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Deployment.Deployment>> GetByStatusAsync(DeploymentStatus status)
        {
            _logger.LogInformation("Getting deployments with status: {Status}", status);

            // Get all deployments from memory and filter by status
            var deployments = _deployments.Values.Where(d => d.Status == status).ToList();

            // If no deployments in memory, try to get from storage
            if (deployments.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_deployments.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the deployment from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var deployment = System.Text.Json.JsonSerializer.Deserialize<Core.Models.Deployment.Deployment>(json);
                            if (deployment != null && deployment.Status == status)
                            {
                                // Add to memory cache
                                _deployments[deployment.Id] = deployment;
                                deployments.Add(deployment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting deployments from storage with status: {Status}", status);
                }
            }

            return deployments;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Deployment.Deployment>> GetByTagAsync(string tagKey, string tagValue)
        {
            _logger.LogInformation("Getting deployments with tag: {TagKey}={TagValue}", tagKey, tagValue);

            // Get all deployments from memory and filter by tag
            var deployments = _deployments.Values.Where(d => d.Tags != null && d.Tags.TryGetValue(tagKey, out var value) && value == tagValue).ToList();

            // If no deployments in memory, try to get from storage
            if (deployments.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_deployments.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the deployment from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var deployment = System.Text.Json.JsonSerializer.Deserialize<Core.Models.Deployment.Deployment>(json);
                            if (deployment != null && deployment.Tags != null && deployment.Tags.TryGetValue(tagKey, out var value) && value == tagValue)
                            {
                                // Add to memory cache
                                _deployments[deployment.Id] = deployment;
                                deployments.Add(deployment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting deployments from storage with tag: {TagKey}={TagValue}", tagKey, tagValue);
                }
            }

            return deployments;
        }

        /// <summary>
        /// Stores a deployment in persistent storage
        /// </summary>
        /// <param name="deployment">Deployment to store</param>
        private async Task StoreDeploymentAsync(Core.Models.Deployment.Deployment deployment)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(deployment);
                await _storageService.PutObjectAsync(_containerName, $"{deployment.Id}.json", json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing deployment in storage: {Id}", deployment.Id);
                throw;
            }
        }
    }
}
