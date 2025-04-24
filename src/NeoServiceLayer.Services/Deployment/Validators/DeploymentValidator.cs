using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Services.Deployment.Validators
{
    /// <summary>
    /// Validator for deployment operations
    /// </summary>
    public class DeploymentValidator : IDeploymentValidator
    {
        private readonly ILogger<DeploymentValidator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentValidator"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public DeploymentValidator(ILogger<DeploymentValidator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates a deployment environment
        /// </summary>
        /// <param name="environment">Environment to validate</param>
        /// <returns>Validation result</returns>
        public async Task<ValidationResult> ValidateEnvironmentAsync(DeploymentEnvironment environment)
        {
            _logger.LogInformation("Validating environment: {EnvironmentName}", environment.Name);

            var result = new ValidationResult();

            try
            {
                // Validate environment name
                if (string.IsNullOrWhiteSpace(environment.Name))
                {
                    result.AddError("Environment name is required");
                }

                // Validate environment description
                if (string.IsNullOrWhiteSpace(environment.Description))
                {
                    result.AddError("Environment description is required");
                }

                // Validate network configuration
                if (environment.Network == null)
                {
                    result.AddError("Network configuration is required");
                }
                else
                {
                    // Validate VPC ID
                    if (string.IsNullOrWhiteSpace(environment.Network.VpcId))
                    {
                        result.AddError("VPC ID is required");
                    }

                    // Validate subnets
                    if (environment.Network.SubnetIds == null || !environment.Network.SubnetIds.Any())
                    {
                        result.AddError("At least one subnet is required");
                    }

                    // Validate load balancer
                    if (environment.Network.LoadBalancer == null)
                    {
                        result.AddError("Load balancer configuration is required");
                    }
                    else
                    {
                        // Validate load balancer type
                        if (string.IsNullOrWhiteSpace(environment.Network.LoadBalancer.Type))
                        {
                            result.AddError("Load balancer type is required");
                        }

                        // Validate target group ARN
                        if (string.IsNullOrWhiteSpace(environment.Network.LoadBalancer.TargetGroupArn))
                        {
                            result.AddError("Target group ARN is required");
                        }

                        // Validate listener ARN
                        if (string.IsNullOrWhiteSpace(environment.Network.LoadBalancer.ListenerArn))
                        {
                            result.AddError("Listener ARN is required");
                        }

                        // Validate health check
                        if (environment.Network.LoadBalancer.HealthCheck == null)
                        {
                            result.AddError("Health check configuration is required");
                        }
                        else
                        {
                            // Validate health check path
                            if (string.IsNullOrWhiteSpace(environment.Network.LoadBalancer.HealthCheck.Path))
                            {
                                result.AddError("Health check path is required");
                            }

                            // Validate health check protocol
                            if (string.IsNullOrWhiteSpace(environment.Network.LoadBalancer.HealthCheck.Protocol))
                            {
                                result.AddError("Health check protocol is required");
                            }

                            // Validate health check interval
                            if (environment.Network.LoadBalancer.HealthCheck.IntervalSeconds <= 0)
                            {
                                result.AddError("Health check interval must be greater than 0");
                            }

                            // Validate health check timeout
                            if (environment.Network.LoadBalancer.HealthCheck.TimeoutSeconds <= 0)
                            {
                                result.AddError("Health check timeout must be greater than 0");
                            }

                            // Validate health check healthy threshold
                            if (environment.Network.LoadBalancer.HealthCheck.HealthyThreshold <= 0)
                            {
                                result.AddError("Health check healthy threshold must be greater than 0");
                            }

                            // Validate health check unhealthy threshold
                            if (environment.Network.LoadBalancer.HealthCheck.UnhealthyThreshold <= 0)
                            {
                                result.AddError("Health check unhealthy threshold must be greater than 0");
                            }
                        }
                    }
                }

                // Validate scaling configuration
                if (environment.Scaling == null)
                {
                    result.AddError("Scaling configuration is required");
                }
                else
                {
                    // Validate min capacity
                    if (environment.Scaling.MinInstances < 1)
                    {
                        result.AddError("Minimum capacity must be at least 1");
                    }

                    // Validate max capacity
                    if (environment.Scaling.MaxInstances < environment.Scaling.MinInstances)
                    {
                        result.AddError("Maximum capacity must be greater than or equal to minimum capacity");
                    }

                    // Validate desired capacity
                    if (environment.Scaling.DesiredInstances < environment.Scaling.MinInstances ||
                        environment.Scaling.DesiredInstances > environment.Scaling.MaxInstances)
                    {
                        result.AddError("Desired capacity must be between minimum and maximum capacity");
                    }

                    // Validate CPU utilization threshold
                    if (environment.Scaling.CpuUtilizationTargetPercentage <= 0 ||
                        environment.Scaling.CpuUtilizationTargetPercentage > 100)
                    {
                        result.AddError("CPU utilization threshold must be between 1 and 100");
                    }

                    // Validate memory utilization threshold
                    if (environment.Scaling.MemoryUtilizationTargetPercentage <= 0 ||
                        environment.Scaling.MemoryUtilizationTargetPercentage > 100)
                    {
                        result.AddError("Memory utilization threshold must be between 1 and 100");
                    }
                }

                // Validate security configuration
                if (environment.Security == null)
                {
                    result.AddError("Security configuration is required");
                }
                else
                {
                    // Validate IAM role ARN
                    if (string.IsNullOrWhiteSpace(environment.Security.IamRoleArn))
                    {
                        result.AddError("IAM role ARN is required");
                    }

                    // Validate security groups
                    if (environment.Network.SecurityGroupIds == null ||
                        !environment.Network.SecurityGroupIds.Any())
                    {
                        result.AddError("At least one security group is required");
                    }

                    // Validate KMS key ARN
                    if (string.IsNullOrWhiteSpace(environment.Security.KmsKeyArn))
                    {
                        result.AddError("KMS key ARN is required");
                    }
                }

                // Validate environment variables
                if (environment.EnvironmentVariables != null)
                {
                    foreach (var variable in environment.EnvironmentVariables)
                    {
                        if (string.IsNullOrWhiteSpace(variable.Key))
                        {
                            result.AddError("Environment variable name is required");
                        }
                    }
                }

                // Validate secrets
                if (environment.Secrets != null)
                {
                    foreach (var secret in environment.Secrets)
                    {
                        if (string.IsNullOrWhiteSpace(secret.Name))
                        {
                            result.AddError("Secret name is required");
                        }

                        if (string.IsNullOrWhiteSpace(secret.SecretReference))
                        {
                            result.AddError("Secret reference is required");
                        }
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating environment: {EnvironmentName}", environment.Name);
                result.AddError($"Validation error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates a deployment version
        /// </summary>
        /// <param name="version">Version to validate</param>
        /// <returns>Validation result</returns>
        public async Task<ValidationResult> ValidateVersionAsync(DeploymentVersion version)
        {
            _logger.LogInformation("Validating version: {VersionId}", version.Id);

            var result = new ValidationResult();

            try
            {
                // Validate version ID
                if (version.Id == Guid.Empty)
                {
                    result.AddError("Version ID is required");
                }

                // Validate function ID
                if (version.FunctionId == Guid.Empty)
                {
                    result.AddError("Function ID is required");
                }

                // Validate version number
                if (string.IsNullOrWhiteSpace(version.VersionNumber))
                {
                    result.AddError("Version number is required");
                }

                // Validate source code
                if (string.IsNullOrWhiteSpace(version.SourceCodePackageUrl))
                {
                    result.AddError("Source code package URL is required");
                }

                // Validate runtime
                if (version.Configuration == null || string.IsNullOrWhiteSpace(version.Configuration.Runtime))
                {
                    result.AddError("Runtime is required");
                }

                // Validate entry point
                if (version.Configuration == null || string.IsNullOrWhiteSpace(version.Configuration.EntryPoint))
                {
                    result.AddError("Entry point is required");
                }

                // Validate dependencies
                if (version.Dependencies != null)
                {
                    foreach (var dependency in version.Dependencies)
                    {
                        if (string.IsNullOrWhiteSpace(dependency.Name))
                        {
                            result.AddError("Dependency name is required");
                        }

                        if (string.IsNullOrWhiteSpace(dependency.Version))
                        {
                            result.AddError("Dependency version is required");
                        }
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating version: {VersionId}", version.Id);
                result.AddError($"Validation error: {ex.Message}");
            }

            return result;
        }
    }

    /// <summary>
    /// Validation result
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets the validation errors
        /// </summary>
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Gets a value indicating whether the validation passed
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Adds an error to the validation result
        /// </summary>
        /// <param name="error">Error message</param>
        public void AddError(string error)
        {
            Errors.Add(error);
        }

        /// <summary>
        /// Adds errors to the validation result
        /// </summary>
        /// <param name="errors">Error messages</param>
        public void AddErrors(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }
    }
}
