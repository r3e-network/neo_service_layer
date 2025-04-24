using System;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for deployment validator
    /// </summary>
    public interface IDeploymentValidator
    {
        /// <summary>
        /// Validates a deployment version
        /// </summary>
        /// <param name="versionId">Version ID</param>
        /// <returns>The validation results</returns>
        Task<ValidationResults> ValidateVersionByIdAsync(Guid versionId);

        /// <summary>
        /// Validates a deployment configuration
        /// </summary>
        /// <param name="configuration">Deployment configuration</param>
        /// <returns>The validation results</returns>
        Task<ValidationResults> ValidateConfigurationAsync(DeploymentConfiguration configuration);

        /// <summary>
        /// Validates a deployment environment
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>The validation results</returns>
        Task<ValidationResults> ValidateEnvironmentByIdAsync(Guid environmentId);

        /// <summary>
        /// Validates a deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>The validation results</returns>
        Task<ValidationResults> ValidateDeploymentAsync(Guid deploymentId);

        /// <summary>
        /// Validates compatibility between a function and an environment
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>The validation results</returns>
        Task<ValidationResults> ValidateCompatibilityAsync(Guid functionId, Guid environmentId);

        /// <summary>
        /// Validates a deployment health check
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>The validation results</returns>
        Task<ValidationResults> ValidateHealthCheckAsync(Guid deploymentId);
    }
}
