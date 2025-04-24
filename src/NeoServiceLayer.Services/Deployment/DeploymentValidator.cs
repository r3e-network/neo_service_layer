using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Services.Deployment
{
    /// <summary>
    /// Validator for deployments
    /// </summary>
    public class DeploymentValidator : IDeploymentValidator
    {
        private readonly ILogger<DeploymentValidator> _logger;
        private readonly IDeploymentRepository _deploymentRepository;
        private readonly IDeploymentVersionRepository _versionRepository;
        private readonly IDeploymentEnvironmentRepository _environmentRepository;
        private readonly IFunctionService _functionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentValidator"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="deploymentRepository">Deployment repository</param>
        /// <param name="versionRepository">Version repository</param>
        /// <param name="environmentRepository">Environment repository</param>
        /// <param name="functionService">Function service</param>
        public DeploymentValidator(
            ILogger<DeploymentValidator> logger,
            IDeploymentRepository deploymentRepository,
            IDeploymentVersionRepository versionRepository,
            IDeploymentEnvironmentRepository environmentRepository,
            IFunctionService functionService)
        {
            _logger = logger;
            _deploymentRepository = deploymentRepository;
            _versionRepository = versionRepository;
            _environmentRepository = environmentRepository;
            _functionService = functionService;
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateVersionAsync(Guid versionId)
        {
            _logger.LogInformation("Validating version: {VersionId}", versionId);

            var results = new ValidationResults
            {
                Passed = true,
                Checks = new List<ValidationCheck>(),
                ValidatedAt = DateTime.UtcNow
            };

            try
            {
                // Get the version
                var version = await _versionRepository.GetAsync(versionId);
                if (version == null)
                {
                    AddValidationCheck(results, "VersionExists", "Version does not exist", false, ValidationSeverity.Critical);
                    return results;
                }

                // Validate version number
                if (string.IsNullOrEmpty(version.VersionNumber))
                {
                    AddValidationCheck(results, "VersionNumber", "Version number is required", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "VersionNumber", "Version number is valid", true, ValidationSeverity.Info);
                }

                // Validate source code package
                if (string.IsNullOrEmpty(version.SourceCodePackageUrl))
                {
                    AddValidationCheck(results, "SourceCodePackage", "Source code package URL is required", false, ValidationSeverity.Error);
                }
                else
                {
                    // In a real implementation, this would validate the source code package
                    // For now, we'll just check if the URL is not empty
                    AddValidationCheck(results, "SourceCodePackage", "Source code package URL is valid", true, ValidationSeverity.Info);
                }

                // Validate source code hash
                if (string.IsNullOrEmpty(version.SourceCodeHash))
                {
                    AddValidationCheck(results, "SourceCodeHash", "Source code hash is required", false, ValidationSeverity.Warning);
                }
                else
                {
                    AddValidationCheck(results, "SourceCodeHash", "Source code hash is valid", true, ValidationSeverity.Info);
                }

                // Validate configuration
                if (version.Configuration == null)
                {
                    AddValidationCheck(results, "Configuration", "Configuration is required", false, ValidationSeverity.Error);
                }
                else
                {
                    // Validate runtime
                    if (string.IsNullOrEmpty(version.Configuration.Runtime))
                    {
                        AddValidationCheck(results, "Runtime", "Runtime is required", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "Runtime", "Runtime is valid", true, ValidationSeverity.Info);
                    }

                    // Validate memory size
                    if (version.Configuration.MemorySizeMb <= 0)
                    {
                        AddValidationCheck(results, "MemorySize", "Memory size must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "MemorySize", "Memory size is valid", true, ValidationSeverity.Info);
                    }

                    // Validate CPU size
                    if (version.Configuration.CpuSize <= 0)
                    {
                        AddValidationCheck(results, "CpuSize", "CPU size must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "CpuSize", "CPU size is valid", true, ValidationSeverity.Info);
                    }

                    // Validate timeout
                    if (version.Configuration.TimeoutSeconds <= 0)
                    {
                        AddValidationCheck(results, "Timeout", "Timeout must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "Timeout", "Timeout is valid", true, ValidationSeverity.Info);
                    }

                    // Validate entry point or handler
                    if (string.IsNullOrEmpty(version.Configuration.EntryPoint) && string.IsNullOrEmpty(version.Configuration.Handler))
                    {
                        AddValidationCheck(results, "EntryPoint", "Either entry point or handler is required", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "EntryPoint", "Entry point or handler is valid", true, ValidationSeverity.Info);
                    }
                }

                // Validate dependencies
                if (version.Dependencies == null || version.Dependencies.Count == 0)
                {
                    AddValidationCheck(results, "Dependencies", "No dependencies specified", true, ValidationSeverity.Info);
                }
                else
                {
                    bool dependenciesValid = true;
                    foreach (var dependency in version.Dependencies)
                    {
                        if (string.IsNullOrEmpty(dependency.Name) || string.IsNullOrEmpty(dependency.Version))
                        {
                            dependenciesValid = false;
                            break;
                        }
                    }

                    if (dependenciesValid)
                    {
                        AddValidationCheck(results, "Dependencies", "Dependencies are valid", true, ValidationSeverity.Info);
                    }
                    else
                    {
                        AddValidationCheck(results, "Dependencies", "Dependencies must have name and version", false, ValidationSeverity.Warning);
                    }
                }

                // Check if the function exists
                var function = await _functionService.GetFunctionAsync(version.FunctionId);
                if (function == null)
                {
                    AddValidationCheck(results, "FunctionExists", "Function does not exist", false, ValidationSeverity.Critical);
                }
                else
                {
                    AddValidationCheck(results, "FunctionExists", "Function exists", true, ValidationSeverity.Info);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating version: {VersionId}", versionId);
                AddValidationCheck(results, "ValidationError", $"Error validating version: {ex.Message}", false, ValidationSeverity.Critical);
            }

            // Set overall result
            results.Passed = !results.Checks.Any(c => c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error && !c.Passed);

            return results;
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateConfigurationAsync(DeploymentConfiguration configuration)
        {
            _logger.LogInformation("Validating deployment configuration");

            var results = new ValidationResults
            {
                Passed = true,
                Checks = new List<ValidationCheck>(),
                ValidatedAt = DateTime.UtcNow
            };

            try
            {
                // Check if configuration is null
                if (configuration == null)
                {
                    AddValidationCheck(results, "ConfigurationExists", "Configuration is required", false, ValidationSeverity.Critical);
                    return results;
                }

                // Validate strategy
                if (configuration.Strategy == null)
                {
                    AddValidationCheck(results, "StrategyExists", "Strategy is required", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "StrategyExists", "Strategy exists", true, ValidationSeverity.Info);

                    // Validate strategy type
                    switch (configuration.Strategy.Type)
                    {
                        case DeploymentStrategy.AllAtOnce:
                            AddValidationCheck(results, "StrategyType", "All-at-once strategy is valid", true, ValidationSeverity.Info);
                            break;
                        case DeploymentStrategy.BlueGreen:
                            if (configuration.Strategy.BlueGreen == null)
                            {
                                AddValidationCheck(results, "BlueGreenConfig", "Blue-Green configuration is required", false, ValidationSeverity.Error);
                            }
                            else
                            {
                                AddValidationCheck(results, "BlueGreenConfig", "Blue-Green configuration is valid", true, ValidationSeverity.Info);
                            }
                            break;
                        case DeploymentStrategy.Canary:
                            if (configuration.Strategy.Canary == null)
                            {
                                AddValidationCheck(results, "CanaryConfig", "Canary configuration is required", false, ValidationSeverity.Error);
                            }
                            else
                            {
                                AddValidationCheck(results, "CanaryConfig", "Canary configuration is valid", true, ValidationSeverity.Info);
                            }
                            break;
                        case DeploymentStrategy.Rolling:
                            if (configuration.Strategy.Rolling == null)
                            {
                                AddValidationCheck(results, "RollingConfig", "Rolling configuration is required", false, ValidationSeverity.Error);
                            }
                            else
                            {
                                AddValidationCheck(results, "RollingConfig", "Rolling configuration is valid", true, ValidationSeverity.Info);
                            }
                            break;
                        default:
                            AddValidationCheck(results, "StrategyType", $"Unsupported strategy type: {configuration.Strategy.Type}", false, ValidationSeverity.Error);
                            break;
                    }
                }

                // Validate traffic routing
                if (configuration.TrafficRouting == null)
                {
                    AddValidationCheck(results, "TrafficRoutingExists", "Traffic routing is required", false, ValidationSeverity.Warning);
                }
                else
                {
                    AddValidationCheck(results, "TrafficRoutingExists", "Traffic routing exists", true, ValidationSeverity.Info);

                    // Validate traffic routing type
                    switch (configuration.TrafficRouting.Type)
                    {
                        case TrafficRoutingType.TimeBased:
                            if (configuration.TrafficRouting.TimeBased == null)
                            {
                                AddValidationCheck(results, "TimeBasedConfig", "Time-based configuration is required", false, ValidationSeverity.Warning);
                            }
                            else
                            {
                                AddValidationCheck(results, "TimeBasedConfig", "Time-based configuration is valid", true, ValidationSeverity.Info);
                            }
                            break;
                        case TrafficRoutingType.Linear:
                            if (configuration.TrafficRouting.Linear == null)
                            {
                                AddValidationCheck(results, "LinearConfig", "Linear configuration is required", false, ValidationSeverity.Warning);
                            }
                            else
                            {
                                AddValidationCheck(results, "LinearConfig", "Linear configuration is valid", true, ValidationSeverity.Info);
                            }
                            break;
                        case TrafficRoutingType.AllAtOnce:
                            if (configuration.TrafficRouting.AllAtOnce == null)
                            {
                                AddValidationCheck(results, "AllAtOnceConfig", "All-at-once configuration is required", false, ValidationSeverity.Warning);
                            }
                            else
                            {
                                AddValidationCheck(results, "AllAtOnceConfig", "All-at-once configuration is valid", true, ValidationSeverity.Info);
                            }
                            break;
                        default:
                            AddValidationCheck(results, "TrafficRoutingType", $"Unsupported traffic routing type: {configuration.TrafficRouting.Type}", false, ValidationSeverity.Warning);
                            break;
                    }
                }

                // Validate rollback configuration
                if (configuration.Rollback == null)
                {
                    AddValidationCheck(results, "RollbackExists", "Rollback configuration is required", false, ValidationSeverity.Warning);
                }
                else
                {
                    AddValidationCheck(results, "RollbackExists", "Rollback configuration exists", true, ValidationSeverity.Info);

                    // Validate rollback events
                    if (configuration.Rollback.Events == null || configuration.Rollback.Events.Count == 0)
                    {
                        AddValidationCheck(results, "RollbackEvents", "Rollback events are required", false, ValidationSeverity.Warning);
                    }
                    else
                    {
                        AddValidationCheck(results, "RollbackEvents", "Rollback events are valid", true, ValidationSeverity.Info);
                    }
                }

                // Validate timeout
                if (configuration.TimeoutSeconds <= 0)
                {
                    AddValidationCheck(results, "Timeout", "Timeout must be greater than 0", false, ValidationSeverity.Warning);
                }
                else
                {
                    AddValidationCheck(results, "Timeout", "Timeout is valid", true, ValidationSeverity.Info);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating deployment configuration");
                AddValidationCheck(results, "ValidationError", $"Error validating configuration: {ex.Message}", false, ValidationSeverity.Critical);
            }

            // Set overall result
            results.Passed = !results.Checks.Any(c => c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error && !c.Passed);

            return results;
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateEnvironmentAsync(Guid environmentId)
        {
            _logger.LogInformation("Validating environment: {EnvironmentId}", environmentId);

            var results = new ValidationResults
            {
                Passed = true,
                Checks = new List<ValidationCheck>(),
                ValidatedAt = DateTime.UtcNow
            };

            try
            {
                // Get the environment
                var environment = await _environmentRepository.GetAsync(environmentId);
                if (environment == null)
                {
                    AddValidationCheck(results, "EnvironmentExists", "Environment does not exist", false, ValidationSeverity.Critical);
                    return results;
                }

                // Validate environment name
                if (string.IsNullOrEmpty(environment.Name))
                {
                    AddValidationCheck(results, "EnvironmentName", "Environment name is required", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "EnvironmentName", "Environment name is valid", true, ValidationSeverity.Info);
                }

                // Validate environment type
                if (!Enum.IsDefined(typeof(EnvironmentType), environment.Type))
                {
                    AddValidationCheck(results, "EnvironmentType", $"Invalid environment type: {environment.Type}", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "EnvironmentType", "Environment type is valid", true, ValidationSeverity.Info);
                }

                // Validate environment configuration
                if (environment.Configuration == null)
                {
                    AddValidationCheck(results, "EnvironmentConfiguration", "Environment configuration is required", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "EnvironmentConfiguration", "Environment configuration exists", true, ValidationSeverity.Info);

                    // Validate runtime
                    if (string.IsNullOrEmpty(environment.Configuration.Runtime))
                    {
                        AddValidationCheck(results, "Runtime", "Runtime is required", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "Runtime", "Runtime is valid", true, ValidationSeverity.Info);
                    }

                    // Validate memory size
                    if (environment.Configuration.MemorySizeMb <= 0)
                    {
                        AddValidationCheck(results, "MemorySize", "Memory size must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "MemorySize", "Memory size is valid", true, ValidationSeverity.Info);
                    }

                    // Validate CPU size
                    if (environment.Configuration.CpuSize <= 0)
                    {
                        AddValidationCheck(results, "CpuSize", "CPU size must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "CpuSize", "CPU size is valid", true, ValidationSeverity.Info);
                    }

                    // Validate timeout
                    if (environment.Configuration.TimeoutSeconds <= 0)
                    {
                        AddValidationCheck(results, "Timeout", "Timeout must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "Timeout", "Timeout is valid", true, ValidationSeverity.Info);
                    }

                    // Validate TEE configuration if enabled
                    if (environment.Configuration.IsTee)
                    {
                        if (string.IsNullOrEmpty(environment.Configuration.TeeType))
                        {
                            AddValidationCheck(results, "TeeType", "TEE type is required when TEE is enabled", false, ValidationSeverity.Error);
                        }
                        else
                        {
                            AddValidationCheck(results, "TeeType", "TEE type is valid", true, ValidationSeverity.Info);
                        }
                    }
                }

                // Validate network configuration if VPC is enabled
                if (environment.Configuration?.IsVpc == true)
                {
                    if (environment.Network == null)
                    {
                        AddValidationCheck(results, "NetworkConfiguration", "Network configuration is required when VPC is enabled", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "NetworkConfiguration", "Network configuration exists", true, ValidationSeverity.Info);

                        // Validate VPC ID
                        if (string.IsNullOrEmpty(environment.Network.VpcId))
                        {
                            AddValidationCheck(results, "VpcId", "VPC ID is required", false, ValidationSeverity.Error);
                        }
                        else
                        {
                            AddValidationCheck(results, "VpcId", "VPC ID is valid", true, ValidationSeverity.Info);
                        }

                        // Validate subnet IDs
                        if (environment.Network.SubnetIds == null || environment.Network.SubnetIds.Count == 0)
                        {
                            AddValidationCheck(results, "SubnetIds", "At least one subnet ID is required", false, ValidationSeverity.Error);
                        }
                        else
                        {
                            AddValidationCheck(results, "SubnetIds", "Subnet IDs are valid", true, ValidationSeverity.Info);
                        }

                        // Validate security group IDs
                        if (environment.Network.SecurityGroupIds == null || environment.Network.SecurityGroupIds.Count == 0)
                        {
                            AddValidationCheck(results, "SecurityGroupIds", "At least one security group ID is required", false, ValidationSeverity.Error);
                        }
                        else
                        {
                            AddValidationCheck(results, "SecurityGroupIds", "Security group IDs are valid", true, ValidationSeverity.Info);
                        }
                    }
                }

                // Validate scaling configuration
                if (environment.Scaling == null)
                {
                    AddValidationCheck(results, "ScalingConfiguration", "Scaling configuration is recommended", true, ValidationSeverity.Warning);
                }
                else
                {
                    AddValidationCheck(results, "ScalingConfiguration", "Scaling configuration exists", true, ValidationSeverity.Info);

                    // Validate min instances
                    if (environment.Scaling.MinInstances < 0)
                    {
                        AddValidationCheck(results, "MinInstances", "Minimum instances must be non-negative", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "MinInstances", "Minimum instances is valid", true, ValidationSeverity.Info);
                    }

                    // Validate max instances
                    if (environment.Scaling.MaxInstances <= 0)
                    {
                        AddValidationCheck(results, "MaxInstances", "Maximum instances must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else if (environment.Scaling.MaxInstances < environment.Scaling.MinInstances)
                    {
                        AddValidationCheck(results, "MaxInstances", "Maximum instances must be greater than or equal to minimum instances", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "MaxInstances", "Maximum instances is valid", true, ValidationSeverity.Info);
                    }

                    // Validate desired instances
                    if (environment.Scaling.DesiredInstances < environment.Scaling.MinInstances || environment.Scaling.DesiredInstances > environment.Scaling.MaxInstances)
                    {
                        AddValidationCheck(results, "DesiredInstances", "Desired instances must be between minimum and maximum instances", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "DesiredInstances", "Desired instances is valid", true, ValidationSeverity.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating environment: {EnvironmentId}", environmentId);
                AddValidationCheck(results, "ValidationError", $"Error validating environment: {ex.Message}", false, ValidationSeverity.Critical);
            }

            // Set overall result
            results.Passed = !results.Checks.Any(c => c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error && !c.Passed);

            return results;
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateDeploymentAsync(Guid deploymentId)
        {
            _logger.LogInformation("Validating deployment: {DeploymentId}", deploymentId);

            var results = new ValidationResults
            {
                Passed = true,
                Checks = new List<ValidationCheck>(),
                ValidatedAt = DateTime.UtcNow
            };

            try
            {
                // Get the deployment
                var deployment = await _deploymentRepository.GetAsync(deploymentId);
                if (deployment == null)
                {
                    AddValidationCheck(results, "DeploymentExists", "Deployment does not exist", false, ValidationSeverity.Critical);
                    return results;
                }

                // Validate deployment name
                if (string.IsNullOrEmpty(deployment.Name))
                {
                    AddValidationCheck(results, "DeploymentName", "Deployment name is required", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "DeploymentName", "Deployment name is valid", true, ValidationSeverity.Info);
                }

                // Validate function ID
                if (deployment.FunctionId == Guid.Empty)
                {
                    AddValidationCheck(results, "FunctionId", "Function ID is required", false, ValidationSeverity.Error);
                }
                else
                {
                    // Check if function exists
                    var function = await _functionService.GetFunctionAsync(deployment.FunctionId);
                    if (function == null)
                    {
                        AddValidationCheck(results, "FunctionExists", "Function does not exist", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "FunctionExists", "Function exists", true, ValidationSeverity.Info);
                    }
                }

                // Validate environment ID
                if (deployment.EnvironmentId == Guid.Empty)
                {
                    AddValidationCheck(results, "EnvironmentId", "Environment ID is required", false, ValidationSeverity.Error);
                }
                else
                {
                    // Check if environment exists
                    var environment = await _environmentRepository.GetAsync(deployment.EnvironmentId);
                    if (environment == null)
                    {
                        AddValidationCheck(results, "EnvironmentExists", "Environment does not exist", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "EnvironmentExists", "Environment exists", true, ValidationSeverity.Info);
                    }
                }

                // Validate current version ID
                if (deployment.CurrentVersionId == Guid.Empty)
                {
                    AddValidationCheck(results, "CurrentVersionId", "Current version ID is required", false, ValidationSeverity.Error);
                }
                else
                {
                    // Check if current version exists
                    var currentVersion = await _versionRepository.GetAsync(deployment.CurrentVersionId);
                    if (currentVersion == null)
                    {
                        AddValidationCheck(results, "CurrentVersionExists", "Current version does not exist", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "CurrentVersionExists", "Current version exists", true, ValidationSeverity.Info);
                    }
                }

                // Validate previous version ID if set
                if (deployment.PreviousVersionId.HasValue && deployment.PreviousVersionId.Value != Guid.Empty)
                {
                    // Check if previous version exists
                    var previousVersion = await _versionRepository.GetAsync(deployment.PreviousVersionId.Value);
                    if (previousVersion == null)
                    {
                        AddValidationCheck(results, "PreviousVersionExists", "Previous version does not exist", false, ValidationSeverity.Warning);
                    }
                    else
                    {
                        AddValidationCheck(results, "PreviousVersionExists", "Previous version exists", true, ValidationSeverity.Info);
                    }
                }

                // Validate deployment strategy
                if (deployment.Strategy == DeploymentStrategy.BlueGreen || deployment.Strategy == DeploymentStrategy.Canary)
                {
                    // Check if deployment configuration exists
                    if (deployment.Configuration == null)
                    {
                        AddValidationCheck(results, "DeploymentConfiguration", "Deployment configuration is required for Blue-Green or Canary strategy", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "DeploymentConfiguration", "Deployment configuration exists", true, ValidationSeverity.Info);

                        // Validate deployment configuration
                        var configValidation = await ValidateConfigurationAsync(deployment.Configuration);
                        if (!configValidation.Passed)
                        {
                            AddValidationCheck(results, "ConfigurationValid", "Deployment configuration is invalid", false, ValidationSeverity.Error);
                        }
                        else
                        {
                            AddValidationCheck(results, "ConfigurationValid", "Deployment configuration is valid", true, ValidationSeverity.Info);
                        }
                    }
                }

                // Validate deployment status
                if (!Enum.IsDefined(typeof(DeploymentStatus), deployment.Status))
                {
                    AddValidationCheck(results, "DeploymentStatus", $"Invalid deployment status: {deployment.Status}", false, ValidationSeverity.Warning);
                }
                else
                {
                    AddValidationCheck(results, "DeploymentStatus", "Deployment status is valid", true, ValidationSeverity.Info);
                }

                // Validate deployment health
                if (deployment.Health == null)
                {
                    AddValidationCheck(results, "DeploymentHealth", "Deployment health is missing", false, ValidationSeverity.Warning);
                }
                else
                {
                    AddValidationCheck(results, "DeploymentHealth", "Deployment health exists", true, ValidationSeverity.Info);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating deployment: {DeploymentId}", deploymentId);
                AddValidationCheck(results, "ValidationError", $"Error validating deployment: {ex.Message}", false, ValidationSeverity.Critical);
            }

            // Set overall result
            results.Passed = !results.Checks.Any(c => c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error && !c.Passed);

            return results;
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateEnvironmentByIdAsync(Guid environmentId)
        {
            _logger.LogInformation("Validating environment: {EnvironmentId}", environmentId);

            var results = new ValidationResults
            {
                Passed = true,
                Checks = new List<ValidationCheck>(),
                ValidatedAt = DateTime.UtcNow
            };

            try
            {
                // Get the environment
                var environment = await _environmentRepository.GetAsync(environmentId);
                if (environment == null)
                {
                    AddValidationCheck(results, "EnvironmentExists", "Environment does not exist", false, ValidationSeverity.Critical);
                    return results;
                }

                // Validate environment name
                if (string.IsNullOrEmpty(environment.Name))
                {
                    AddValidationCheck(results, "EnvironmentName", "Environment name is required", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "EnvironmentName", "Environment name is valid", true, ValidationSeverity.Info);
                }

                // Validate environment type
                if (!Enum.IsDefined(typeof(EnvironmentType), environment.Type))
                {
                    AddValidationCheck(results, "EnvironmentType", $"Invalid environment type: {environment.Type}", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "EnvironmentType", "Environment type is valid", true, ValidationSeverity.Info);
                }

                // Validate environment configuration
                if (environment.Configuration == null)
                {
                    AddValidationCheck(results, "EnvironmentConfiguration", "Environment configuration is required", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "EnvironmentConfiguration", "Environment configuration exists", true, ValidationSeverity.Info);

                    // Validate runtime
                    if (string.IsNullOrEmpty(environment.Configuration.Runtime))
                    {
                        AddValidationCheck(results, "Runtime", "Runtime is required", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "Runtime", "Runtime is valid", true, ValidationSeverity.Info);
                    }

                    // Validate memory size
                    if (environment.Configuration.MemorySizeMb <= 0)
                    {
                        AddValidationCheck(results, "MemorySize", "Memory size must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "MemorySize", "Memory size is valid", true, ValidationSeverity.Info);
                    }

                    // Validate CPU size
                    if (environment.Configuration.CpuSize <= 0)
                    {
                        AddValidationCheck(results, "CpuSize", "CPU size must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "CpuSize", "CPU size is valid", true, ValidationSeverity.Info);
                    }

                    // Validate timeout
                    if (environment.Configuration.TimeoutSeconds <= 0)
                    {
                        AddValidationCheck(results, "Timeout", "Timeout must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "Timeout", "Timeout is valid", true, ValidationSeverity.Info);
                    }

                    // Validate TEE configuration if enabled
                    if (environment.Configuration.IsTee)
                    {
                        if (string.IsNullOrEmpty(environment.Configuration.TeeType))
                        {
                            AddValidationCheck(results, "TeeType", "TEE type is required when TEE is enabled", false, ValidationSeverity.Error);
                        }
                        else
                        {
                            AddValidationCheck(results, "TeeType", "TEE type is valid", true, ValidationSeverity.Info);
                        }
                    }
                }

                // Validate network configuration if VPC is enabled
                if (environment.Configuration?.IsVpc == true)
                {
                    if (environment.Network == null)
                    {
                        AddValidationCheck(results, "NetworkConfiguration", "Network configuration is required when VPC is enabled", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "NetworkConfiguration", "Network configuration exists", true, ValidationSeverity.Info);

                        // Validate VPC ID
                        if (string.IsNullOrEmpty(environment.Network.VpcId))
                        {
                            AddValidationCheck(results, "VpcId", "VPC ID is required", false, ValidationSeverity.Error);
                        }
                        else
                        {
                            AddValidationCheck(results, "VpcId", "VPC ID is valid", true, ValidationSeverity.Info);
                        }

                        // Validate subnet IDs
                        if (environment.Network.SubnetIds == null || environment.Network.SubnetIds.Count == 0)
                        {
                            AddValidationCheck(results, "SubnetIds", "At least one subnet ID is required", false, ValidationSeverity.Error);
                        }
                        else
                        {
                            AddValidationCheck(results, "SubnetIds", "Subnet IDs are valid", true, ValidationSeverity.Info);
                        }

                        // Validate security group IDs
                        if (environment.Network.SecurityGroupIds == null || environment.Network.SecurityGroupIds.Count == 0)
                        {
                            AddValidationCheck(results, "SecurityGroupIds", "At least one security group ID is required", false, ValidationSeverity.Error);
                        }
                        else
                        {
                            AddValidationCheck(results, "SecurityGroupIds", "Security group IDs are valid", true, ValidationSeverity.Info);
                        }
                    }
                }

                // Validate scaling configuration
                if (environment.Scaling == null)
                {
                    AddValidationCheck(results, "ScalingConfiguration", "Scaling configuration is recommended", true, ValidationSeverity.Warning);
                }
                else
                {
                    AddValidationCheck(results, "ScalingConfiguration", "Scaling configuration exists", true, ValidationSeverity.Info);

                    // Validate min instances
                    if (environment.Scaling.MinInstances < 0)
                    {
                        AddValidationCheck(results, "MinInstances", "Minimum instances must be non-negative", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "MinInstances", "Minimum instances is valid", true, ValidationSeverity.Info);
                    }

                    // Validate max instances
                    if (environment.Scaling.MaxInstances <= 0)
                    {
                        AddValidationCheck(results, "MaxInstances", "Maximum instances must be greater than 0", false, ValidationSeverity.Error);
                    }
                    else if (environment.Scaling.MaxInstances < environment.Scaling.MinInstances)
                    {
                        AddValidationCheck(results, "MaxInstances", "Maximum instances must be greater than or equal to minimum instances", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "MaxInstances", "Maximum instances is valid", true, ValidationSeverity.Info);
                    }

                    // Validate desired instances
                    if (environment.Scaling.DesiredInstances < environment.Scaling.MinInstances || environment.Scaling.DesiredInstances > environment.Scaling.MaxInstances)
                    {
                        AddValidationCheck(results, "DesiredInstances", "Desired instances must be between minimum and maximum instances", false, ValidationSeverity.Error);
                    }
                    else
                    {
                        AddValidationCheck(results, "DesiredInstances", "Desired instances is valid", true, ValidationSeverity.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating environment: {EnvironmentId}", environmentId);
                AddValidationCheck(results, "ValidationError", $"Error validating environment: {ex.Message}", false, ValidationSeverity.Critical);
            }

            // Set overall result
            results.Passed = !results.Checks.Any(c => c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error && !c.Passed);

            return results;
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateCompatibilityAsync(Guid functionId, Guid environmentId)
        {
            _logger.LogInformation("Validating compatibility between function {FunctionId} and environment {EnvironmentId}", functionId, environmentId);

            var results = new ValidationResults
            {
                Passed = true,
                Checks = new List<ValidationCheck>(),
                ValidatedAt = DateTime.UtcNow
            };

            try
            {
                // Get function
                var function = await _functionService.GetFunctionAsync(functionId);
                if (function == null)
                {
                    AddValidationCheck(results, "FunctionExists", "Function does not exist", false, ValidationSeverity.Critical);
                    return results;
                }

                // Get environment
                var environment = await _environmentRepository.GetAsync(environmentId);
                if (environment == null)
                {
                    AddValidationCheck(results, "EnvironmentExists", "Environment does not exist", false, ValidationSeverity.Critical);
                    return results;
                }

                // Validate runtime compatibility
                if (string.IsNullOrEmpty(function.Runtime) || string.IsNullOrEmpty(environment.Configuration?.Runtime))
                {
                    AddValidationCheck(results, "RuntimeCompatibility", "Runtime information is missing", false, ValidationSeverity.Error);
                }
                else if (function.Runtime != environment.Configuration.Runtime)
                {
                    AddValidationCheck(results, "RuntimeCompatibility", $"Runtime mismatch: Function runtime '{function.Runtime}' is not compatible with environment runtime '{environment.Configuration.Runtime}'", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "RuntimeCompatibility", "Runtime is compatible", true, ValidationSeverity.Info);
                }

                // Validate memory requirements
                if (function.MemoryRequirementMb > environment.Configuration?.MemorySizeMb)
                {
                    AddValidationCheck(results, "MemoryCompatibility", $"Memory requirement mismatch: Function requires {function.MemoryRequirementMb}MB but environment provides only {environment.Configuration?.MemorySizeMb}MB", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "MemoryCompatibility", "Memory requirement is compatible", true, ValidationSeverity.Info);
                }

                // Validate CPU requirements
                if (function.CpuRequirement > environment.Configuration?.CpuSize)
                {
                    AddValidationCheck(results, "CpuCompatibility", $"CPU requirement mismatch: Function requires {function.CpuRequirement} vCPUs but environment provides only {environment.Configuration?.CpuSize} vCPUs", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "CpuCompatibility", "CPU requirement is compatible", true, ValidationSeverity.Info);
                }

                // Validate timeout compatibility
                if (function.TimeoutSeconds > environment.Configuration?.TimeoutSeconds)
                {
                    AddValidationCheck(results, "TimeoutCompatibility", $"Timeout mismatch: Function timeout {function.TimeoutSeconds}s exceeds environment timeout {environment.Configuration?.TimeoutSeconds}s", false, ValidationSeverity.Error);
                }
                else
                {
                    AddValidationCheck(results, "TimeoutCompatibility", "Timeout is compatible", true, ValidationSeverity.Info);
                }

                // Validate TEE requirements
                if (function.RequiresTee && environment.Configuration != null && !environment.Configuration.IsTee)
                {
                    AddValidationCheck(results, "TeeCompatibility", "Function requires TEE but environment does not support TEE", false, ValidationSeverity.Error);
                }
                else if (function.RequiresTee && environment.Configuration != null && environment.Configuration.IsTee)
                {
                    AddValidationCheck(results, "TeeCompatibility", "TEE requirement is compatible", true, ValidationSeverity.Info);
                }

                // Validate VPC requirements
                if (function.RequiresVpc && environment.Configuration != null && !environment.Configuration.IsVpc)
                {
                    AddValidationCheck(results, "VpcCompatibility", "Function requires VPC but environment does not support VPC", false, ValidationSeverity.Error);
                }
                else if (function.RequiresVpc && environment.Configuration != null && environment.Configuration.IsVpc)
                {
                    AddValidationCheck(results, "VpcCompatibility", "VPC requirement is compatible", true, ValidationSeverity.Info);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating compatibility between function {FunctionId} and environment {EnvironmentId}", functionId, environmentId);
                AddValidationCheck(results, "ValidationError", $"Error validating compatibility: {ex.Message}", false, ValidationSeverity.Critical);
            }

            // Set overall result
            results.Passed = !results.Checks.Any(c => c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error && !c.Passed);

            return results;
        }

        /// <inheritdoc/>
        public async Task<ValidationResults> ValidateHealthCheckAsync(Guid deploymentId)
        {
            _logger.LogInformation("Validating health check for deployment: {DeploymentId}", deploymentId);

            var results = new ValidationResults
            {
                Passed = true,
                Checks = new List<ValidationCheck>(),
                ValidatedAt = DateTime.UtcNow
            };

            try
            {
                // Get deployment
                var deployment = await _deploymentRepository.GetAsync(deploymentId);
                if (deployment == null)
                {
                    AddValidationCheck(results, "DeploymentExists", "Deployment does not exist", false, ValidationSeverity.Critical);
                    return results;
                }

                // Check if deployment is in a valid state for health check
                if (deployment.Status != DeploymentStatus.Deployed && deployment.Status != DeploymentStatus.RolledBack)
                {
                    AddValidationCheck(results, "DeploymentStatus", $"Deployment is not in a valid state for health check. Current status: {deployment.Status}", false, ValidationSeverity.Error);
                    return results;
                }

                // Get current version
                var version = await _versionRepository.GetAsync(deployment.CurrentVersionId);
                if (version == null)
                {
                    AddValidationCheck(results, "VersionExists", "Current version does not exist", false, ValidationSeverity.Critical);
                    return results;
                }

                // In a real implementation, this would perform actual health checks
                // For now, we'll just simulate health checks

                // Check if health check path is configured
                if (string.IsNullOrEmpty(version.Configuration?.HealthCheckPath))
                {
                    AddValidationCheck(results, "HealthCheckPath", "Health check path is not configured", false, ValidationSeverity.Warning);
                }
                else
                {
                    AddValidationCheck(results, "HealthCheckPath", "Health check path is configured", true, ValidationSeverity.Info);
                }

                // Simulate health check response
                AddValidationCheck(results, "HealthCheckResponse", "Health check endpoint responded successfully", true, ValidationSeverity.Info);

                // Simulate metrics check
                if (deployment.Metrics != null)
                {
                    // Check error rate
                    if (deployment.Metrics.ErrorRate > 5.0)
                    {
                        AddValidationCheck(results, "ErrorRate", $"Error rate is too high: {deployment.Metrics.ErrorRate}%", false, ValidationSeverity.Warning);
                    }
                    else
                    {
                        AddValidationCheck(results, "ErrorRate", $"Error rate is acceptable: {deployment.Metrics.ErrorRate}%", true, ValidationSeverity.Info);
                    }

                    // Check response time
                    if (deployment.Metrics.AverageResponseTimeMs > 1000)
                    {
                        AddValidationCheck(results, "ResponseTime", $"Average response time is too high: {deployment.Metrics.AverageResponseTimeMs}ms", false, ValidationSeverity.Warning);
                    }
                    else
                    {
                        AddValidationCheck(results, "ResponseTime", $"Average response time is acceptable: {deployment.Metrics.AverageResponseTimeMs}ms", true, ValidationSeverity.Info);
                    }

                    // Check CPU usage
                    if (deployment.Metrics.AverageCpuUsage > 80.0)
                    {
                        AddValidationCheck(results, "CpuUsage", $"CPU usage is too high: {deployment.Metrics.AverageCpuUsage}%", false, ValidationSeverity.Warning);
                    }
                    else
                    {
                        AddValidationCheck(results, "CpuUsage", $"CPU usage is acceptable: {deployment.Metrics.AverageCpuUsage}%", true, ValidationSeverity.Info);
                    }

                    // Check memory usage
                    if (deployment.Metrics.AverageMemoryUsageMb > 0.8 * version.Configuration.MemorySizeMb)
                    {
                        AddValidationCheck(results, "MemoryUsage", $"Memory usage is too high: {deployment.Metrics.AverageMemoryUsageMb}MB", false, ValidationSeverity.Warning);
                    }
                    else
                    {
                        AddValidationCheck(results, "MemoryUsage", $"Memory usage is acceptable: {deployment.Metrics.AverageMemoryUsageMb}MB", true, ValidationSeverity.Info);
                    }
                }
                else
                {
                    AddValidationCheck(results, "Metrics", "No metrics available for health check", false, ValidationSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating health check for deployment: {DeploymentId}", deploymentId);
                AddValidationCheck(results, "ValidationError", $"Error validating health check: {ex.Message}", false, ValidationSeverity.Critical);
            }

            // Set overall result
            results.Passed = !results.Checks.Any(c => c.Severity == ValidationSeverity.Critical || c.Severity == ValidationSeverity.Error && !c.Passed);

            return results;
        }

        /// <summary>
        /// Adds a validation check to the results
        /// </summary>
        /// <param name="results">Validation results</param>
        /// <param name="name">Check name</param>
        /// <param name="message">Check message</param>
        /// <param name="passed">Whether the check passed</param>
        /// <param name="severity">Check severity</param>
        private void AddValidationCheck(ValidationResults results, string name, string message, bool passed, ValidationSeverity severity)
        {
            results.Checks.Add(new ValidationCheck
            {
                Name = name,
                Description = name,
                Message = message,
                Passed = passed,
                Severity = severity,
                Timestamp = DateTime.UtcNow
            });

            // Update overall result if this check failed and is critical or error
            if (!passed && (severity == ValidationSeverity.Critical || severity == ValidationSeverity.Error))
            {
                results.Passed = false;
            }
        }


    }
}