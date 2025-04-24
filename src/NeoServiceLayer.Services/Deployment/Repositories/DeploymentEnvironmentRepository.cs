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
    /// Repository for deployment environments
    /// </summary>
    public class DeploymentEnvironmentRepository : IDeploymentEnvironmentRepository
    {
        private readonly ILogger<DeploymentEnvironmentRepository> _logger;
        private readonly IObjectStorageService _storageService;
        private readonly string _containerName = "deployment-environments";
        private readonly Dictionary<Guid, DeploymentEnvironment> _environments = new Dictionary<Guid, DeploymentEnvironment>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentEnvironmentRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageService">Storage service</param>
        public DeploymentEnvironmentRepository(ILogger<DeploymentEnvironmentRepository> logger, IObjectStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> CreateAsync(DeploymentEnvironment environment)
        {
            _logger.LogInformation("Creating environment: {Id}", environment.Id);

            if (environment.Id == Guid.Empty)
            {
                environment.Id = Guid.NewGuid();
            }

            environment.CreatedAt = DateTime.UtcNow;
            environment.UpdatedAt = DateTime.UtcNow;

            // Store in memory
            _environments[environment.Id] = environment;

            // Store in persistent storage
            await StoreEnvironmentAsync(environment);

            return environment;
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> GetAsync(Guid id)
        {
            _logger.LogInformation("Getting environment: {Id}", id);

            // Try to get from memory first
            if (_environments.TryGetValue(id, out var environment))
            {
                return environment;
            }

            // If not in memory, try to get from storage
            try
            {
                var json = await _storageService.GetObjectAsync(_containerName, $"{id}.json");
                if (!string.IsNullOrEmpty(json))
                {
                    environment = System.Text.Json.JsonSerializer.Deserialize<DeploymentEnvironment>(json);
                    if (environment != null)
                    {
                        // Add to memory cache
                        _environments[environment.Id] = environment;
                        return environment;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting environment from storage: {Id}", id);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentEnvironment>> GetByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting environments for account: {AccountId}", accountId);

            // Get all environments from memory and filter by account ID
            var environments = _environments.Values.Where(e => e.AccountId == accountId).ToList();

            // If no environments in memory, try to get from storage
            if (environments.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_environments.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the environment from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var environment = System.Text.Json.JsonSerializer.Deserialize<DeploymentEnvironment>(json);
                            if (environment != null && environment.AccountId == accountId)
                            {
                                // Add to memory cache
                                _environments[environment.Id] = environment;
                                environments.Add(environment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting environments from storage for account: {AccountId}", accountId);
                }
            }

            return environments;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentEnvironment>> GetByTypeAsync(EnvironmentType type)
        {
            _logger.LogInformation("Getting environments of type: {Type}", type);

            // Get all environments from memory and filter by type
            var environments = _environments.Values.Where(e => e.Type == type).ToList();

            // If no environments in memory, try to get from storage
            if (environments.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_environments.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the environment from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var environment = System.Text.Json.JsonSerializer.Deserialize<DeploymentEnvironment>(json);
                            if (environment != null && environment.Type == type)
                            {
                                // Add to memory cache
                                _environments[environment.Id] = environment;
                                environments.Add(environment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting environments from storage of type: {Type}", type);
                }
            }

            return environments;
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> UpdateAsync(DeploymentEnvironment environment)
        {
            _logger.LogInformation("Updating environment: {Id}", environment.Id);

            // Check if environment exists
            var existingEnvironment = await GetAsync(environment.Id);
            if (existingEnvironment == null)
            {
                throw new ArgumentException($"Environment not found: {environment.Id}");
            }

            // Update timestamp
            environment.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _environments[environment.Id] = environment;

            // Update in persistent storage
            await StoreEnvironmentAsync(environment);

            return environment;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting environment: {Id}", id);

            // Check if environment exists
            var existingEnvironment = await GetAsync(id);
            if (existingEnvironment == null)
            {
                return false;
            }

            // Remove from memory
            _environments.Remove(id);

            // Remove from persistent storage
            try
            {
                await _storageService.DeleteObjectAsync(_containerName, $"{id}.json");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting environment from storage: {Id}", id);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> UpdateStatusAsync(Guid id, EnvironmentStatus status)
        {
            _logger.LogInformation("Updating status for environment: {Id} to {Status}", id, status);

            // Check if environment exists
            var environment = await GetAsync(id);
            if (environment == null)
            {
                throw new ArgumentException($"Environment not found: {id}");
            }

            // Update status
            environment.Status = status;
            environment.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _environments[id] = environment;

            // Update in persistent storage
            await StoreEnvironmentAsync(environment);

            return environment;
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> AddDeploymentAsync(Guid environmentId, Guid deploymentId)
        {
            _logger.LogInformation("Adding deployment {DeploymentId} to environment: {EnvironmentId}", deploymentId, environmentId);

            // Check if environment exists
            var environment = await GetAsync(environmentId);
            if (environment == null)
            {
                throw new ArgumentException($"Environment not found: {environmentId}");
            }

            // Add deployment ID if not already in the list
            if (!environment.DeploymentIds.Contains(deploymentId))
            {
                environment.DeploymentIds.Add(deploymentId);
                environment.UpdatedAt = DateTime.UtcNow;

                // Update in memory
                _environments[environmentId] = environment;

                // Update in persistent storage
                await StoreEnvironmentAsync(environment);
            }

            return environment;
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> RemoveDeploymentAsync(Guid environmentId, Guid deploymentId)
        {
            _logger.LogInformation("Removing deployment {DeploymentId} from environment: {EnvironmentId}", deploymentId, environmentId);

            // Check if environment exists
            var environment = await GetAsync(environmentId);
            if (environment == null)
            {
                throw new ArgumentException($"Environment not found: {environmentId}");
            }

            // Remove deployment ID if in the list
            if (environment.DeploymentIds.Contains(deploymentId))
            {
                environment.DeploymentIds.Remove(deploymentId);
                environment.UpdatedAt = DateTime.UtcNow;

                // Update in memory
                _environments[environmentId] = environment;

                // Update in persistent storage
                await StoreEnvironmentAsync(environment);
            }

            return environment;
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> UpdateEnvironmentVariablesAsync(Guid id, Dictionary<string, string> environmentVariables)
        {
            _logger.LogInformation("Updating environment variables for environment: {Id}", id);

            // Check if environment exists
            var environment = await GetAsync(id);
            if (environment == null)
            {
                throw new ArgumentException($"Environment not found: {id}");
            }

            // Update environment variables
            environment.EnvironmentVariables = environmentVariables ?? new Dictionary<string, string>();
            environment.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _environments[id] = environment;

            // Update in persistent storage
            await StoreEnvironmentAsync(environment);

            return environment;
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> AddSecretAsync(Guid environmentId, EnvironmentSecret secret)
        {
            _logger.LogInformation("Adding secret {SecretName} to environment: {EnvironmentId}", secret.Name, environmentId);

            // Check if environment exists
            var environment = await GetAsync(environmentId);
            if (environment == null)
            {
                throw new ArgumentException($"Environment not found: {environmentId}");
            }

            // Remove existing secret with the same name if it exists
            environment.Secrets.RemoveAll(s => s.Name == secret.Name);

            // Add the new secret
            secret.CreatedAt = DateTime.UtcNow;
            secret.UpdatedAt = DateTime.UtcNow;
            environment.Secrets.Add(secret);
            environment.UpdatedAt = DateTime.UtcNow;

            // Update in memory
            _environments[environmentId] = environment;

            // Update in persistent storage
            await StoreEnvironmentAsync(environment);

            return environment;
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> RemoveSecretAsync(Guid environmentId, string secretName)
        {
            _logger.LogInformation("Removing secret {SecretName} from environment: {EnvironmentId}", secretName, environmentId);

            // Check if environment exists
            var environment = await GetAsync(environmentId);
            if (environment == null)
            {
                throw new ArgumentException($"Environment not found: {environmentId}");
            }

            // Remove secret if it exists
            var removed = environment.Secrets.RemoveAll(s => s.Name == secretName);
            if (removed > 0)
            {
                environment.UpdatedAt = DateTime.UtcNow;

                // Update in memory
                _environments[environmentId] = environment;

                // Update in persistent storage
                await StoreEnvironmentAsync(environment);
            }

            return environment;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentEnvironment>> GetByTagAsync(string tagKey, string tagValue)
        {
            _logger.LogInformation("Getting environments with tag: {TagKey}={TagValue}", tagKey, tagValue);

            // Get all environments from memory and filter by tag
            var environments = _environments.Values.Where(e => e.Tags != null && e.Tags.TryGetValue(tagKey, out var value) && value == tagValue).ToList();

            // If no environments in memory, try to get from storage
            if (environments.Count == 0)
            {
                try
                {
                    // List all objects in the container
                    var objects = await _storageService.ListObjectsAsync(_containerName);
                    foreach (var obj in objects)
                    {
                        // Skip if already in memory
                        var id = Guid.Parse(obj.Replace(".json", ""));
                        if (_environments.ContainsKey(id))
                        {
                            continue;
                        }

                        // Get the environment from storage
                        var json = await _storageService.GetObjectAsync(_containerName, obj);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var environment = System.Text.Json.JsonSerializer.Deserialize<DeploymentEnvironment>(json);
                            if (environment != null && environment.Tags != null && environment.Tags.TryGetValue(tagKey, out var value) && value == tagValue)
                            {
                                // Add to memory cache
                                _environments[environment.Id] = environment;
                                environments.Add(environment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting environments from storage with tag: {TagKey}={TagValue}", tagKey, tagValue);
                }
            }

            return environments;
        }

        /// <summary>
        /// Stores an environment in persistent storage
        /// </summary>
        /// <param name="environment">Environment to store</param>
        private async Task StoreEnvironmentAsync(DeploymentEnvironment environment)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(environment);
                await _storageService.PutObjectAsync(_containerName, $"{environment.Id}.json", json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing environment in storage: {Id}", environment.Id);
                throw;
            }
        }
    }
}
