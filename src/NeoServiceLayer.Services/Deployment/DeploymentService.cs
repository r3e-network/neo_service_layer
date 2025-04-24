using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Deployment;
using NeoServiceLayer.Services.Deployment.Strategies;

namespace NeoServiceLayer.Services.Deployment
{
    /// <summary>
    /// Service for deployments
    /// </summary>
    public class DeploymentService : IDeploymentService
    {
        private readonly ILogger<DeploymentService> _logger;
        private readonly IDeploymentRepository _deploymentRepository;
        private readonly IDeploymentEnvironmentRepository _environmentRepository;
        private readonly IDeploymentVersionRepository _versionRepository;
        private readonly IDeploymentValidator _validator;
        private readonly IFunctionService _functionService;
        private readonly Dictionary<DeploymentStrategy, IDeploymentStrategy> _strategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="deploymentRepository">Deployment repository</param>
        /// <param name="environmentRepository">Environment repository</param>
        /// <param name="versionRepository">Version repository</param>
        /// <param name="validator">Deployment validator</param>
        /// <param name="functionService">Function service</param>
        /// <param name="allAtOnceStrategy">All-at-once deployment strategy</param>
        /// <param name="blueGreenStrategy">Blue-Green deployment strategy</param>
        /// <param name="canaryStrategy">Canary deployment strategy</param>
        public DeploymentService(
            ILogger<DeploymentService> logger,
            IDeploymentRepository deploymentRepository,
            IDeploymentEnvironmentRepository environmentRepository,
            IDeploymentVersionRepository versionRepository,
            IDeploymentValidator validator,
            IFunctionService functionService,
            AllAtOnceDeploymentStrategy allAtOnceStrategy,
            BlueGreenDeploymentStrategy blueGreenStrategy,
            CanaryDeploymentStrategy canaryStrategy)
        {
            _logger = logger;
            _deploymentRepository = deploymentRepository;
            _environmentRepository = environmentRepository;
            _versionRepository = versionRepository;
            _validator = validator;
            _functionService = functionService;

            // Initialize strategies
            _strategies = new Dictionary<DeploymentStrategy, IDeploymentStrategy>
            {
                { DeploymentStrategy.AllAtOnce, allAtOnceStrategy },
                { DeploymentStrategy.BlueGreen, blueGreenStrategy },
                { DeploymentStrategy.Canary, canaryStrategy }
            };
        }

        #region Deployment Management

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> CreateDeploymentAsync(Core.Models.Deployment.Deployment deployment)
        {
            _logger.LogInformation("Creating deployment: {Name}", deployment.Name);

            try
            {
                // Validate function exists
                var function = await _functionService.GetFunctionAsync(deployment.FunctionId);
                if (function == null)
                {
                    throw new ArgumentException($"Function not found: {deployment.FunctionId}");
                }

                // Validate environment exists
                var environment = await _environmentRepository.GetAsync(deployment.EnvironmentId);
                if (environment == null)
                {
                    throw new ArgumentException($"Environment not found: {deployment.EnvironmentId}");
                }

                // Validate compatibility
                var compatibilityResults = await _validator.ValidateCompatibilityAsync(deployment.FunctionId, deployment.EnvironmentId);
                if (!compatibilityResults.Passed)
                {
                    var errors = string.Join(", ", compatibilityResults.Checks
                        .Where(c => !c.Passed && (c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error))
                        .Select(c => c.Message));
                    throw new ArgumentException($"Function and environment are not compatible: {errors}");
                }

                // Set initial status
                deployment.Status = DeploymentStatus.Pending;

                // Set creation timestamp
                deployment.CreatedAt = DateTime.UtcNow;
                deployment.UpdatedAt = DateTime.UtcNow;

                // Initialize metrics and health
                deployment.Metrics = new DeploymentMetrics
                {
                    LastUpdatedAt = DateTime.UtcNow
                };

                deployment.Health = new DeploymentHealth
                {
                    Status = HealthStatus.Unknown,
                    LastCheckedAt = DateTime.UtcNow
                };

                // Create deployment
                var createdDeployment = await _deploymentRepository.CreateAsync(deployment);

                // Add deployment to environment
                await _environmentRepository.AddDeploymentAsync(deployment.EnvironmentId, createdDeployment.Id);

                return createdDeployment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating deployment: {Name}", deployment.Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> GetDeploymentAsync(Guid id)
        {
            _logger.LogInformation("Getting deployment: {Id}", id);

            try
            {
                return await _deploymentRepository.GetAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Deployment.Deployment>> GetDeploymentsByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting deployments for account: {AccountId}", accountId);

            try
            {
                return await _deploymentRepository.GetByAccountAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployments for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Deployment.Deployment>> GetDeploymentsByFunctionAsync(Guid functionId)
        {
            _logger.LogInformation("Getting deployments for function: {FunctionId}", functionId);

            try
            {
                return await _deploymentRepository.GetByFunctionAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployments for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Deployment.Deployment>> GetDeploymentsByEnvironmentAsync(Guid environmentId)
        {
            _logger.LogInformation("Getting deployments for environment: {EnvironmentId}", environmentId);

            try
            {
                return await _deploymentRepository.GetByEnvironmentAsync(environmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deployments for environment: {EnvironmentId}", environmentId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> UpdateDeploymentAsync(Core.Models.Deployment.Deployment deployment)
        {
            _logger.LogInformation("Updating deployment: {Id}", deployment.Id);

            try
            {
                // Check if deployment exists
                var existingDeployment = await _deploymentRepository.GetAsync(deployment.Id);
                if (existingDeployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {deployment.Id}");
                }

                // Preserve certain fields that shouldn't be updated
                deployment.CreatedAt = existingDeployment.CreatedAt;
                deployment.Status = existingDeployment.Status;
                deployment.LastDeployedAt = existingDeployment.LastDeployedAt;
                deployment.CurrentVersionId = existingDeployment.CurrentVersionId;
                deployment.PreviousVersionId = existingDeployment.PreviousVersionId;

                // Update timestamp
                deployment.UpdatedAt = DateTime.UtcNow;

                // Update deployment
                return await _deploymentRepository.UpdateAsync(deployment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating deployment: {Id}", deployment.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteDeploymentAsync(Guid id)
        {
            _logger.LogInformation("Deleting deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var existingDeployment = await _deploymentRepository.GetAsync(id);
                if (existingDeployment == null)
                {
                    return false;
                }

                // Check if deployment is in a state that can be deleted
                if (existingDeployment.Status == DeploymentStatus.Deploying || existingDeployment.Status == DeploymentStatus.RollingBack)
                {
                    throw new InvalidOperationException($"Cannot delete deployment in state: {existingDeployment.Status}");
                }

                // Remove deployment from environment
                await _environmentRepository.RemoveDeploymentAsync(existingDeployment.EnvironmentId, id);

                // Delete deployment
                return await _deploymentRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> DeployFunctionAsync(Guid functionId, Guid environmentId, DeploymentConfiguration configuration)
        {
            _logger.LogInformation("Deploying function {FunctionId} to environment {EnvironmentId}", functionId, environmentId);

            try
            {
                // Validate function exists
                var function = await _functionService.GetFunctionAsync(functionId);
                if (function == null)
                {
                    throw new ArgumentException($"Function not found: {functionId}");
                }

                // Validate environment exists
                var environment = await _environmentRepository.GetAsync(environmentId);
                if (environment == null)
                {
                    throw new ArgumentException($"Environment not found: {environmentId}");
                }

                // Validate compatibility
                var compatibilityResults = await _validator.ValidateCompatibilityAsync(functionId, environmentId);
                if (!compatibilityResults.Passed)
                {
                    var errors = string.Join(", ", compatibilityResults.Checks
                        .Where(c => !c.Passed && (c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error))
                        .Select(c => c.Message));
                    throw new ArgumentException($"Function and environment are not compatible: {errors}");
                }

                // Validate configuration
                var configResults = await _validator.ValidateConfigurationAsync(configuration);
                if (!configResults.Passed)
                {
                    var errors = string.Join(", ", configResults.Checks
                        .Where(c => !c.Passed && (c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error))
                        .Select(c => c.Message));
                    throw new ArgumentException($"Invalid deployment configuration: {errors}");
                }

                // Check if there's an existing deployment for this function and environment
                var existingDeployments = await _deploymentRepository.GetByFunctionAsync(functionId);
                var existingDeployment = existingDeployments.FirstOrDefault(d => d.EnvironmentId == environmentId);

                if (existingDeployment != null)
                {
                    // Check if deployment is in a state that can be updated
                    if (existingDeployment.Status == DeploymentStatus.Deploying || existingDeployment.Status == DeploymentStatus.RollingBack)
                    {
                        throw new InvalidOperationException($"Cannot deploy function when deployment is in state: {existingDeployment.Status}");
                    }

                    // Create a new version
                    var version = new DeploymentVersion
                    {
                        DeploymentId = existingDeployment.Id,
                        FunctionId = functionId,
                        VersionNumber = GenerateVersionNumber(),
                        VersionLabel = $"v{GenerateVersionNumber()}",
                        Description = "Deployment from DeployFunctionAsync",
                        AccountId = existingDeployment.AccountId,
                        CreatedBy = existingDeployment.CreatedBy,
                        SourceCodePackageUrl = function.SourceCodeUrl,
                        SourceCodeHash = function.SourceCodeHash,
                        Configuration = new VersionConfiguration
                        {
                            Runtime = function.Runtime,
                            MemorySizeMb = function.MemoryRequirementMb,
                            CpuSize = function.CpuRequirement,
                            TimeoutSeconds = function.TimeoutSeconds,
                            EntryPoint = function.EntryPoint,
                            Handler = function.Handler
                        },
                        Status = VersionStatus.Created
                    };

                    var createdVersion = await _versionRepository.CreateAsync(version);

                    // Update deployment configuration
                    existingDeployment.Configuration = configuration;
                    existingDeployment.Strategy = configuration.Strategy.Type;
                    existingDeployment.UpdatedAt = DateTime.UtcNow;
                    await _deploymentRepository.UpdateAsync(existingDeployment);

                    // Get the appropriate strategy
                    if (!_strategies.TryGetValue(configuration.Strategy.Type, out var strategy))
                    {
                        throw new ArgumentException($"Unsupported deployment strategy: {configuration.Strategy.Type}");
                    }

                    // Deploy using the selected strategy
                    await strategy.DeployAsync(existingDeployment.Id, createdVersion.Id, configuration);

                    // Get the updated deployment
                    return await _deploymentRepository.GetAsync(existingDeployment.Id);
                }
                else
                {
                    // Create a new deployment
                    var deployment = new Core.Models.Deployment.Deployment
                    {
                        Name = $"{function.Name} - {environment.Name}",
                        Description = $"Deployment of {function.Name} to {environment.Name}",
                        AccountId = function.AccountId,
                        CreatedBy = Guid.Parse(function.CreatedBy),
                        FunctionId = functionId,
                        EnvironmentId = environmentId,
                        Strategy = configuration.Strategy.Type,
                        Configuration = configuration,
                        Status = DeploymentStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Metrics = new DeploymentMetrics
                        {
                            LastUpdatedAt = DateTime.UtcNow
                        },
                        Health = new DeploymentHealth
                        {
                            Status = HealthStatus.Unknown,
                            LastCheckedAt = DateTime.UtcNow
                        }
                    };

                    var createdDeployment = await _deploymentRepository.CreateAsync(deployment);

                    // Add deployment to environment
                    await _environmentRepository.AddDeploymentAsync(environmentId, createdDeployment.Id);

                    // Create a new version
                    var version = new DeploymentVersion
                    {
                        DeploymentId = createdDeployment.Id,
                        FunctionId = functionId,
                        VersionNumber = GenerateVersionNumber(),
                        VersionLabel = $"v{GenerateVersionNumber()}",
                        Description = "Initial deployment",
                        AccountId = function.AccountId,
                        CreatedBy = Guid.Parse(function.CreatedBy),
                        SourceCodePackageUrl = function.SourceCodeUrl,
                        SourceCodeHash = function.SourceCodeHash,
                        Configuration = new VersionConfiguration
                        {
                            Runtime = function.Runtime,
                            MemorySizeMb = function.MemoryRequirementMb,
                            CpuSize = function.CpuRequirement,
                            TimeoutSeconds = function.TimeoutSeconds,
                            EntryPoint = function.EntryPoint,
                            Handler = function.Handler
                        },
                        Status = VersionStatus.Created
                    };

                    var createdVersion = await _versionRepository.CreateAsync(version);

                    // Get the appropriate strategy
                    if (!_strategies.TryGetValue(configuration.Strategy.Type, out var strategy))
                    {
                        throw new ArgumentException($"Unsupported deployment strategy: {configuration.Strategy.Type}");
                    }

                    // Deploy using the selected strategy
                    await strategy.DeployAsync(createdDeployment.Id, createdVersion.Id, configuration);

                    // Get the updated deployment
                    return await _deploymentRepository.GetAsync(createdDeployment.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying function {FunctionId} to environment {EnvironmentId}", functionId, environmentId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> StopDeploymentAsync(Guid id)
        {
            _logger.LogInformation("Stopping deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(id);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {id}");
                }

                // Check if deployment is in a state that can be stopped
                if (deployment.Status != DeploymentStatus.Deploying && deployment.Status != DeploymentStatus.RollingBack)
                {
                    throw new InvalidOperationException($"Cannot stop deployment in state: {deployment.Status}");
                }

                // Update deployment status to stopped
                await _deploymentRepository.UpdateStatusAsync(id, DeploymentStatus.Stopped);

                // Get the updated deployment
                return await _deploymentRepository.GetAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> RollbackDeploymentAsync(Guid id)
        {
            _logger.LogInformation("Rolling back deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(id);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {id}");
                }

                // Check if deployment has a previous version to roll back to
                if (!deployment.PreviousVersionId.HasValue)
                {
                    throw new InvalidOperationException("No previous version to roll back to");
                }

                // Check if deployment is in a state that can be rolled back
                if (deployment.Status == DeploymentStatus.RollingBack)
                {
                    throw new InvalidOperationException("Deployment is already rolling back");
                }

                // Get the appropriate strategy
                if (!_strategies.TryGetValue(deployment.Strategy, out var strategy))
                {
                    throw new ArgumentException($"Unsupported deployment strategy: {deployment.Strategy}");
                }

                // Roll back using the selected strategy
                await strategy.RollbackAsync(id);

                // Get the updated deployment
                return await _deploymentRepository.GetAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentMetrics> GetDeploymentMetricsAsync(Guid id)
        {
            _logger.LogInformation("Getting metrics for deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(id);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {id}");
                }

                return deployment.Metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentHealth> GetDeploymentHealthAsync(Guid id)
        {
            _logger.LogInformation("Getting health for deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(id);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {id}");
                }

                return deployment.Health;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health for deployment: {Id}", id);
                throw;
            }
        }

        #endregion

        #region Environment Management

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> CreateEnvironmentAsync(DeploymentEnvironment environment)
        {
            _logger.LogInformation("Creating environment: {Name}", environment.Name);

            try
            {
                // Validate environment
                if (string.IsNullOrEmpty(environment.Name))
                {
                    throw new ArgumentException("Environment name is required");
                }

                if (environment.Configuration == null)
                {
                    throw new ArgumentException("Environment configuration is required");
                }

                // Set creation timestamp
                environment.CreatedAt = DateTime.UtcNow;
                environment.UpdatedAt = DateTime.UtcNow;

                // Set initial status
                environment.Status = EnvironmentStatus.Creating;

                // Create environment
                var createdEnvironment = await _environmentRepository.CreateAsync(environment);

                // In a real implementation, this would create the actual environment resources
                // For now, we'll just update the status to active
                createdEnvironment.Status = EnvironmentStatus.Active;
                await _environmentRepository.UpdateStatusAsync(createdEnvironment.Id, EnvironmentStatus.Active);

                return createdEnvironment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating environment: {Name}", environment.Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> GetEnvironmentAsync(Guid id)
        {
            _logger.LogInformation("Getting environment: {Id}", id);

            try
            {
                return await _environmentRepository.GetAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting environment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentEnvironment>> GetEnvironmentsByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting environments for account: {AccountId}", accountId);

            try
            {
                return await _environmentRepository.GetByAccountAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting environments for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> UpdateEnvironmentAsync(DeploymentEnvironment environment)
        {
            _logger.LogInformation("Updating environment: {Id}", environment.Id);

            try
            {
                // Check if environment exists
                var existingEnvironment = await _environmentRepository.GetAsync(environment.Id);
                if (existingEnvironment == null)
                {
                    throw new ArgumentException($"Environment not found: {environment.Id}");
                }

                // Validate environment
                var validationResults = await _validator.ValidateEnvironmentAsync(environment.Id);
                if (!validationResults.Passed)
                {
                    var errors = string.Join(", ", validationResults.Checks
                        .Where(c => !c.Passed && (c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error))
                        .Select(c => c.Message));
                    throw new ArgumentException($"Invalid environment: {errors}");
                }

                // Preserve certain fields that shouldn't be updated
                environment.CreatedAt = existingEnvironment.CreatedAt;
                environment.Status = existingEnvironment.Status;

                // Update timestamp
                environment.UpdatedAt = DateTime.UtcNow;

                // Update environment
                return await _environmentRepository.UpdateAsync(environment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating environment: {Id}", environment.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteEnvironmentAsync(Guid id)
        {
            _logger.LogInformation("Deleting environment: {Id}", id);

            try
            {
                // Check if environment exists
                var environment = await _environmentRepository.GetAsync(id);
                if (environment == null)
                {
                    return false;
                }

                // Check if environment has deployments
                var deployments = await _deploymentRepository.GetByEnvironmentAsync(id);
                if (deployments.Any())
                {
                    throw new InvalidOperationException("Cannot delete environment with active deployments");
                }

                // Delete environment
                return await _environmentRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting environment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> AddEnvironmentSecretAsync(Guid environmentId, EnvironmentSecret secret)
        {
            _logger.LogInformation("Adding secret to environment: {EnvironmentId}", environmentId);

            try
            {
                // Check if environment exists
                var environment = await _environmentRepository.GetAsync(environmentId);
                if (environment == null)
                {
                    throw new ArgumentException($"Environment not found: {environmentId}");
                }

                // Add secret to environment
                // In a real implementation, this would securely store the secret
                // For now, we'll just add it to the environment's secrets collection
                if (environment.Secrets == null)
                {
                    environment.Secrets = new List<EnvironmentSecret>();
                }

                // Check if secret already exists
                var existingSecret = environment.Secrets.FirstOrDefault(s => s.Name == secret.Name);
                if (existingSecret != null)
                {
                    // Update existing secret
                    existingSecret.SecretReference = secret.SecretReference;
                }
                else
                {
                    // Add new secret
                    environment.Secrets.Add(secret);
                }

                // Update environment
                environment.UpdatedAt = DateTime.UtcNow;
                return await _environmentRepository.UpdateAsync(environment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding secret to environment: {EnvironmentId}", environmentId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> RemoveEnvironmentSecretAsync(Guid environmentId, string secretName)
        {
            _logger.LogInformation("Removing secret from environment: {EnvironmentId}, {SecretName}", environmentId, secretName);

            try
            {
                // Check if environment exists
                var environment = await _environmentRepository.GetAsync(environmentId);
                if (environment == null)
                {
                    throw new ArgumentException($"Environment not found: {environmentId}");
                }

                // Remove secret from environment
                if (environment.Secrets != null)
                {
                    environment.Secrets.RemoveAll(s => s.Name == secretName);
                }

                // Update environment
                environment.UpdatedAt = DateTime.UtcNow;
                return await _environmentRepository.UpdateAsync(environment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing secret from environment: {EnvironmentId}, {SecretName}", environmentId, secretName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentEnvironment> UpdateEnvironmentVariablesAsync(Guid environmentId, Dictionary<string, string> environmentVariables)
        {
            _logger.LogInformation("Updating environment variables for environment: {EnvironmentId}", environmentId);

            try
            {
                // Check if environment exists
                var environment = await _environmentRepository.GetAsync(environmentId);
                if (environment == null)
                {
                    throw new ArgumentException($"Environment not found: {environmentId}");
                }

                // Update environment variables
                environment.EnvironmentVariables = environmentVariables;

                // Update environment
                environment.UpdatedAt = DateTime.UtcNow;
                return await _environmentRepository.UpdateAsync(environment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating environment variables for environment: {EnvironmentId}", environmentId);
                throw;
            }
        }

        #endregion

        #region Version Management

        /// <inheritdoc/>
        public async Task<DeploymentVersion> GetVersionAsync(Guid id)
        {
            _logger.LogInformation("Getting version: {Id}", id);

            try
            {
                return await _versionRepository.GetAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentVersion>> GetVersionsByDeploymentAsync(Guid deploymentId)
        {
            _logger.LogInformation("Getting versions for deployment: {DeploymentId}", deploymentId);

            try
            {
                return await _versionRepository.GetByDeploymentAsync(deploymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting versions for deployment: {DeploymentId}", deploymentId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> CreateVersionAsync(DeploymentVersion version)
        {
            _logger.LogInformation("Creating version for deployment: {DeploymentId}", version.DeploymentId);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(version.DeploymentId);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {version.DeploymentId}");
                }

                // Set version number if not provided
                if (string.IsNullOrEmpty(version.VersionNumber) || version.VersionNumber == "0")
                {
                    version.VersionNumber = GenerateVersionNumber();
                }

                // Set version label if not provided
                if (string.IsNullOrEmpty(version.VersionLabel))
                {
                    version.VersionLabel = $"v{version.VersionNumber}";
                }

                // Set creation timestamp
                version.CreatedAt = DateTime.UtcNow;
                version.UpdatedAt = DateTime.UtcNow;

                // Set initial status
                version.Status = VersionStatus.Created;

                // Create version
                return await _versionRepository.CreateAsync(version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating version for deployment: {DeploymentId}", version.DeploymentId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentVersion>> GetVersionsByFunctionAsync(Guid functionId)
        {
            _logger.LogInformation("Getting versions for function: {FunctionId}", functionId);

            try
            {
                return await _versionRepository.GetByFunctionAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting versions for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> UpdateVersionAsync(DeploymentVersion version)
        {
            _logger.LogInformation("Updating version: {Id}", version.Id);

            try
            {
                // Check if version exists
                var existingVersion = await _versionRepository.GetAsync(version.Id);
                if (existingVersion == null)
                {
                    throw new ArgumentException($"Version not found: {version.Id}");
                }

                // Preserve certain fields that shouldn't be updated
                version.CreatedAt = existingVersion.CreatedAt;
                version.DeploymentId = existingVersion.DeploymentId;
                version.FunctionId = existingVersion.FunctionId;

                // Update timestamp
                version.UpdatedAt = DateTime.UtcNow;

                // Update version
                return await _versionRepository.UpdateAsync(version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating version: {Id}", version.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteVersionAsync(Guid id)
        {
            _logger.LogInformation("Deleting version: {Id}", id);

            try
            {
                // Check if version exists
                var version = await _versionRepository.GetAsync(id);
                if (version == null)
                {
                    return false;
                }

                // Check if version is in use
                var deployment = await _deploymentRepository.GetAsync(version.DeploymentId);
                if (deployment != null && (deployment.CurrentVersionId == id || deployment.PreviousVersionId == id))
                {
                    throw new InvalidOperationException("Cannot delete version that is currently in use");
                }

                // Delete version
                return await _versionRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting version: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateVersionAsync(Guid id)
        {
            _logger.LogInformation("Validating version: {Id}", id);

            try
            {
                return await _validator.ValidateVersionAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating version: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> BuildVersionAsync(Guid id)
        {
            _logger.LogInformation("Building version: {Id}", id);

            try
            {
                // Check if version exists
                var version = await _versionRepository.GetAsync(id);
                if (version == null)
                {
                    throw new ArgumentException($"Version not found: {id}");
                }

                // Update version status
                await _versionRepository.UpdateStatusAsync(id, VersionStatus.Building);

                // In a real implementation, this would build the version
                // For now, we'll just update the status to built
                await _versionRepository.UpdateStatusAsync(id, VersionStatus.Built);

                // Get the updated version
                return await _versionRepository.GetAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building version: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentVersion> ArchiveVersionAsync(Guid id)
        {
            _logger.LogInformation("Archiving version: {Id}", id);

            try
            {
                // Check if version exists
                var version = await _versionRepository.GetAsync(id);
                if (version == null)
                {
                    throw new ArgumentException($"Version not found: {id}");
                }

                // Update version status
                await _versionRepository.UpdateStatusAsync(id, VersionStatus.Archived);

                // Get the updated version
                return await _versionRepository.GetAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving version: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentLog>> GetVersionLogsAsync(Guid id)
        {
            _logger.LogInformation("Getting logs for version: {Id}", id);

            try
            {
                // Check if version exists
                var version = await _versionRepository.GetAsync(id);
                if (version == null)
                {
                    throw new ArgumentException($"Version not found: {id}");
                }

                // In a real implementation, this would retrieve logs from a logging system
                // For now, we'll just return an empty list
                return new List<DeploymentLog>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting logs for version: {Id}", id);
                throw;
            }
        }

        #endregion

        #region Deployment Operations

        /// <inheritdoc/>
        public async Task<DeploymentStatus> GetDeploymentStatusAsync(Guid id)
        {
            _logger.LogInformation("Getting status for deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(id);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {id}");
                }

                return deployment.Status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeploymentLog>> GetDeploymentLogsAsync(Guid id)
        {
            _logger.LogInformation("Getting logs for deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(id);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {id}");
                }

                // In a real implementation, this would retrieve logs from a logging system
                // For now, we'll just return an empty list
                return new List<DeploymentLog>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting logs for deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> PromoteDeploymentAsync(Guid deploymentId, Guid targetEnvironmentId)
        {
            _logger.LogInformation("Promoting deployment {DeploymentId} to environment {TargetEnvironmentId}", deploymentId, targetEnvironmentId);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(deploymentId);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {deploymentId}");
                }

                // Check if target environment exists
                var targetEnvironment = await _environmentRepository.GetAsync(targetEnvironmentId);
                if (targetEnvironment == null)
                {
                    throw new ArgumentException($"Target environment not found: {targetEnvironmentId}");
                }

                // Check if deployment has a current version
                if (deployment.CurrentVersionId == Guid.Empty)
                {
                    throw new InvalidOperationException("Deployment has no current version to promote");
                }

                // Get the current version
                var currentVersion = await _versionRepository.GetAsync(deployment.CurrentVersionId);
                if (currentVersion == null)
                {
                    throw new InvalidOperationException("Current version not found");
                }

                // Create a new deployment in the target environment
                var newDeployment = new Core.Models.Deployment.Deployment
                {
                    Name = $"{deployment.Name} (Promoted)",
                    Description = $"Promoted from {deployment.Name}",
                    AccountId = deployment.AccountId,
                    CreatedBy = deployment.CreatedBy,
                    FunctionId = deployment.FunctionId,
                    EnvironmentId = targetEnvironmentId,
                    Strategy = deployment.Strategy,
                    Configuration = deployment.Configuration,
                    Status = DeploymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Metrics = new DeploymentMetrics
                    {
                        LastUpdatedAt = DateTime.UtcNow
                    },
                    Health = new DeploymentHealth
                    {
                        Status = HealthStatus.Unknown,
                        LastCheckedAt = DateTime.UtcNow
                    }
                };

                var createdDeployment = await _deploymentRepository.CreateAsync(newDeployment);

                // Add deployment to environment
                await _environmentRepository.AddDeploymentAsync(targetEnvironmentId, createdDeployment.Id);

                // Create a new version based on the current version
                var newVersion = new DeploymentVersion
                {
                    DeploymentId = createdDeployment.Id,
                    FunctionId = currentVersion.FunctionId,
                    VersionNumber = GenerateVersionNumber(),
                    VersionLabel = $"v{GenerateVersionNumber()}",
                    Description = $"Promoted from {currentVersion.VersionLabel}",
                    AccountId = currentVersion.AccountId,
                    CreatedBy = currentVersion.CreatedBy,
                    SourceCodePackageUrl = currentVersion.SourceCodePackageUrl,
                    SourceCodeHash = currentVersion.SourceCodeHash,
                    Configuration = currentVersion.Configuration,
                    Status = VersionStatus.Created
                };

                var createdVersion = await _versionRepository.CreateAsync(newVersion);

                // Get the appropriate strategy
                if (!_strategies.TryGetValue(newDeployment.Strategy, out var strategy))
                {
                    throw new ArgumentException($"Unsupported deployment strategy: {newDeployment.Strategy}");
                }

                // Deploy using the selected strategy
                await strategy.DeployAsync(createdDeployment.Id, createdVersion.Id, newDeployment.Configuration);

                // Get the updated deployment
                return await _deploymentRepository.GetAsync(createdDeployment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting deployment {DeploymentId} to environment {TargetEnvironmentId}", deploymentId, targetEnvironmentId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> ScaleDeploymentAsync(Guid id, int minInstances, int maxInstances, int desiredInstances)
        {
            _logger.LogInformation("Scaling deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(id);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {id}");
                }

                // Validate scaling parameters
                if (minInstances < 0)
                {
                    throw new ArgumentException("Minimum instances must be non-negative");
                }

                if (maxInstances <= 0)
                {
                    throw new ArgumentException("Maximum instances must be greater than 0");
                }

                if (maxInstances < minInstances)
                {
                    throw new ArgumentException("Maximum instances must be greater than or equal to minimum instances");
                }

                if (desiredInstances < minInstances || desiredInstances > maxInstances)
                {
                    throw new ArgumentException("Desired instances must be between minimum and maximum instances");
                }

                // Update scaling configuration
                if (deployment.Configuration == null)
                {
                    deployment.Configuration = new DeploymentConfiguration();
                }

                if (deployment.Configuration.Scaling == null)
                {
                    deployment.Configuration.Scaling = new ScalingConfiguration();
                }

                deployment.Configuration.Scaling.MinInstances = minInstances;
                deployment.Configuration.Scaling.MaxInstances = maxInstances;
                deployment.Configuration.Scaling.DesiredInstances = desiredInstances;

                // Update deployment
                deployment.UpdatedAt = DateTime.UtcNow;
                return await _deploymentRepository.UpdateAsync(deployment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scaling deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Deployment.Deployment> UpdateTrafficRoutingAsync(Guid id, TrafficRoutingConfiguration trafficRouting)
        {
            _logger.LogInformation("Updating traffic routing for deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(id);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {id}");
                }

                // Update traffic routing configuration
                if (deployment.Configuration == null)
                {
                    deployment.Configuration = new DeploymentConfiguration();
                }

                deployment.Configuration.TrafficRouting = trafficRouting;

                // Get the appropriate strategy
                if (!_strategies.TryGetValue(deployment.Strategy, out var strategy))
                {
                    throw new ArgumentException($"Unsupported deployment strategy: {deployment.Strategy}");
                }

                // Update traffic routing using the selected strategy
                await strategy.UpdateTrafficRoutingAsync(id, trafficRouting);

                // Update deployment
                deployment.UpdatedAt = DateTime.UtcNow;
                return await _deploymentRepository.UpdateAsync(deployment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating traffic routing for deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DeploymentHealth> RunHealthCheckAsync(Guid id)
        {
            _logger.LogInformation("Running health check for deployment: {Id}", id);

            try
            {
                // Check if deployment exists
                var deployment = await _deploymentRepository.GetAsync(id);
                if (deployment == null)
                {
                    throw new ArgumentException($"Deployment not found: {id}");
                }

                // Run health check
                var results = await _validator.ValidateHealthCheckAsync(id);

                // Update deployment health
                var health = new DeploymentHealth
                {
                    Status = results.Passed ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                    LastCheckedAt = DateTime.UtcNow,
                    Checks = results.Checks.Select(c => new HealthCheck
                    {
                        Name = c.Name,
                        Status = c.Passed ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                        Message = c.Message,
                        Timestamp = c.Timestamp
                    }).ToList()
                };

                deployment.Health = health;
                deployment.UpdatedAt = DateTime.UtcNow;
                await _deploymentRepository.UpdateAsync(deployment);

                return health;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running health check for deployment: {Id}", id);
                throw;
            }
        }

        #endregion

        #region Validation

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateDeploymentAsync(Guid id)
        {
            _logger.LogInformation("Validating deployment: {Id}", id);

            try
            {
                return await _validator.ValidateDeploymentAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating deployment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateEnvironmentAsync(Guid id)
        {
            _logger.LogInformation("Validating environment: {Id}", id);

            try
            {
                return await _validator.ValidateEnvironmentAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating environment: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateConfigurationAsync(DeploymentConfiguration configuration)
        {
            _logger.LogInformation("Validating deployment configuration");

            try
            {
                return await _validator.ValidateConfigurationAsync(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating deployment configuration");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateCompatibilityAsync(Guid functionId, Guid environmentId)
        {
            _logger.LogInformation("Validating compatibility between function {FunctionId} and environment {EnvironmentId}", functionId, environmentId);

            try
            {
                return await _validator.ValidateCompatibilityAsync(functionId, environmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating compatibility between function {FunctionId} and environment {EnvironmentId}", functionId, environmentId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateHealthCheckAsync(Guid deploymentId)
        {
            _logger.LogInformation("Validating health check for deployment: {DeploymentId}", deploymentId);

            try
            {
                return await _validator.ValidateHealthCheckAsync(deploymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating health check for deployment: {DeploymentId}", deploymentId);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generates a version number based on the current timestamp
        /// </summary>
        /// <returns>Version number</returns>
        private string GenerateVersionNumber()
        {
            // Use the current timestamp to generate a version number
            // This is a simple implementation for demonstration purposes
            // In a real implementation, this would be more sophisticated
            return ((int)(DateTime.UtcNow - new DateTime(2020, 1, 1)).TotalSeconds).ToString();
        }

        #endregion
    }
}
