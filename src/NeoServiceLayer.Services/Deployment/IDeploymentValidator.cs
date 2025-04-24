using System;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Services.Deployment
{
    /// <summary>
    /// Interface for deployment validation
    /// </summary>
    public interface IDeploymentValidator
    {
        /// <summary>
        /// Validates a deployment configuration
        /// </summary>
        /// <param name="configuration">Deployment configuration</param>
        /// <returns>Validation results</returns>
        Task<ValidationResults> ValidateConfigurationAsync(DeploymentConfiguration configuration);

        /// <summary>
        /// Validates an environment
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>Validation results</returns>
        Task<ValidationResults> ValidateEnvironmentAsync(Guid environmentId);

        /// <summary>
        /// Validates a deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Validation results</returns>
        Task<ValidationResults> ValidateDeploymentAsync(Guid deploymentId);

        /// <summary>
        /// Validates compatibility between a function and an environment
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>Validation results</returns>
        Task<ValidationResults> ValidateCompatibilityAsync(Guid functionId, Guid environmentId);

        /// <summary>
        /// Validates a health check for a deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Validation results</returns>
        Task<ValidationResults> ValidateHealthCheckAsync(Guid deploymentId);

        /// <summary>
        /// Validates a version
        /// </summary>
        /// <param name="versionId">Version ID</param>
        /// <returns>Validation results</returns>
        Task<ValidationResults> ValidateVersionAsync(Guid versionId);
    }
}
