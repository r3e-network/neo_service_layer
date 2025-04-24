using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for deployment service
    /// </summary>
    public interface IDeploymentService
    {
        #region Deployment Management

        /// <summary>
        /// Creates a new deployment
        /// </summary>
        /// <param name="deployment">Deployment to create</param>
        /// <returns>The created deployment</returns>
        Task<Deployment> CreateDeploymentAsync(Deployment deployment);

        /// <summary>
        /// Gets a deployment by ID
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The deployment if found, null otherwise</returns>
        Task<Deployment> GetDeploymentAsync(Guid id);

        /// <summary>
        /// Gets deployments by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of deployments for the account</returns>
        Task<IEnumerable<Deployment>> GetDeploymentsByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets deployments by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of deployments for the function</returns>
        Task<IEnumerable<Deployment>> GetDeploymentsByFunctionAsync(Guid functionId);

        /// <summary>
        /// Gets deployments by environment ID
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>List of deployments for the environment</returns>
        Task<IEnumerable<Deployment>> GetDeploymentsByEnvironmentAsync(Guid environmentId);

        /// <summary>
        /// Updates a deployment
        /// </summary>
        /// <param name="deployment">Deployment to update</param>
        /// <returns>The updated deployment</returns>
        Task<Deployment> UpdateDeploymentAsync(Deployment deployment);

        /// <summary>
        /// Deletes a deployment
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>True if the deployment was deleted, false otherwise</returns>
        Task<bool> DeleteDeploymentAsync(Guid id);

        /// <summary>
        /// Deploys a function to an environment
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="environmentId">Environment ID</param>
        /// <param name="configuration">Deployment configuration</param>
        /// <returns>The deployment</returns>
        Task<Deployment> DeployFunctionAsync(Guid functionId, Guid environmentId, DeploymentConfiguration configuration);

        /// <summary>
        /// Stops a deployment
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The stopped deployment</returns>
        Task<Deployment> StopDeploymentAsync(Guid id);

        /// <summary>
        /// Rolls back a deployment
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The rolled back deployment</returns>
        Task<Deployment> RollbackDeploymentAsync(Guid id);

        /// <summary>
        /// Gets the deployment metrics
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The deployment metrics</returns>
        Task<DeploymentMetrics> GetDeploymentMetricsAsync(Guid id);

        /// <summary>
        /// Gets the deployment health
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The deployment health</returns>
        Task<DeploymentHealth> GetDeploymentHealthAsync(Guid id);

        #endregion

        #region Environment Management

        /// <summary>
        /// Creates a new deployment environment
        /// </summary>
        /// <param name="environment">Environment to create</param>
        /// <returns>The created environment</returns>
        Task<DeploymentEnvironment> CreateEnvironmentAsync(DeploymentEnvironment environment);

        /// <summary>
        /// Gets a deployment environment by ID
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <returns>The environment if found, null otherwise</returns>
        Task<DeploymentEnvironment> GetEnvironmentAsync(Guid id);

        /// <summary>
        /// Gets deployment environments by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of environments for the account</returns>
        Task<IEnumerable<DeploymentEnvironment>> GetEnvironmentsByAccountAsync(Guid accountId);

        /// <summary>
        /// Updates a deployment environment
        /// </summary>
        /// <param name="environment">Environment to update</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> UpdateEnvironmentAsync(DeploymentEnvironment environment);

        /// <summary>
        /// Deletes a deployment environment
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <returns>True if the environment was deleted, false otherwise</returns>
        Task<bool> DeleteEnvironmentAsync(Guid id);

        /// <summary>
        /// Adds a secret to a deployment environment
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <param name="secret">Secret to add</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> AddEnvironmentSecretAsync(Guid environmentId, EnvironmentSecret secret);

        /// <summary>
        /// Removes a secret from a deployment environment
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <param name="secretName">Secret name</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> RemoveEnvironmentSecretAsync(Guid environmentId, string secretName);

        /// <summary>
        /// Updates the environment variables for a deployment environment
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <param name="environmentVariables">Environment variables</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> UpdateEnvironmentVariablesAsync(Guid environmentId, Dictionary<string, string> environmentVariables);

        #endregion

        #region Version Management

        /// <summary>
        /// Creates a new deployment version
        /// </summary>
        /// <param name="version">Version to create</param>
        /// <returns>The created version</returns>
        Task<DeploymentVersion> CreateVersionAsync(DeploymentVersion version);

        /// <summary>
        /// Gets a deployment version by ID
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <returns>The version if found, null otherwise</returns>
        Task<DeploymentVersion> GetVersionAsync(Guid id);

        /// <summary>
        /// Gets deployment versions by deployment ID
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>List of versions for the deployment</returns>
        Task<IEnumerable<DeploymentVersion>> GetVersionsByDeploymentAsync(Guid deploymentId);

        /// <summary>
        /// Gets deployment versions by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of versions for the function</returns>
        Task<IEnumerable<DeploymentVersion>> GetVersionsByFunctionAsync(Guid functionId);

        /// <summary>
        /// Updates a deployment version
        /// </summary>
        /// <param name="version">Version to update</param>
        /// <returns>The updated version</returns>
        Task<DeploymentVersion> UpdateVersionAsync(DeploymentVersion version);

        /// <summary>
        /// Deletes a deployment version
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <returns>True if the version was deleted, false otherwise</returns>
        Task<bool> DeleteVersionAsync(Guid id);

        /// <summary>
        /// Validates a deployment version
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <returns>The validation results</returns>
        Task<ValidationResults> ValidateVersionAsync(Guid id);

        /// <summary>
        /// Builds a deployment version
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <returns>The built version</returns>
        Task<DeploymentVersion> BuildVersionAsync(Guid id);

        /// <summary>
        /// Archives a deployment version
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <returns>The archived version</returns>
        Task<DeploymentVersion> ArchiveVersionAsync(Guid id);

        /// <summary>
        /// Gets the logs for a deployment version
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <returns>The deployment logs</returns>
        Task<IEnumerable<DeploymentLog>> GetVersionLogsAsync(Guid id);

        #endregion

        #region Deployment Operations

        /// <summary>
        /// Gets the deployment status
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The deployment status</returns>
        Task<DeploymentStatus> GetDeploymentStatusAsync(Guid id);

        /// <summary>
        /// Gets the deployment logs
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The deployment logs</returns>
        Task<IEnumerable<DeploymentLog>> GetDeploymentLogsAsync(Guid id);

        /// <summary>
        /// Promotes a deployment from one environment to another
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <param name="targetEnvironmentId">Target environment ID</param>
        /// <returns>The new deployment in the target environment</returns>
        Task<Deployment> PromoteDeploymentAsync(Guid deploymentId, Guid targetEnvironmentId);

        /// <summary>
        /// Scales a deployment
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <param name="minInstances">Minimum number of instances</param>
        /// <param name="maxInstances">Maximum number of instances</param>
        /// <param name="desiredInstances">Desired number of instances</param>
        /// <returns>The scaled deployment</returns>
        Task<Deployment> ScaleDeploymentAsync(Guid id, int minInstances, int maxInstances, int desiredInstances);

        /// <summary>
        /// Updates the traffic routing for a deployment
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <param name="trafficRouting">Traffic routing configuration</param>
        /// <returns>The updated deployment</returns>
        Task<Deployment> UpdateTrafficRoutingAsync(Guid id, TrafficRoutingConfiguration trafficRouting);

        /// <summary>
        /// Runs a health check for a deployment
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The deployment health</returns>
        Task<DeploymentHealth> RunHealthCheckAsync(Guid id);

        #endregion
    }
}
