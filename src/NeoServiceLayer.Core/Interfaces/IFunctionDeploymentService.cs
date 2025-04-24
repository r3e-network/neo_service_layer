using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function deployment service
    /// </summary>
    public interface IFunctionDeploymentService
    {
        /// <summary>
        /// Creates a new environment
        /// </summary>
        /// <param name="environment">Environment to create</param>
        /// <returns>The created environment</returns>
        Task<FunctionEnvironment> CreateEnvironmentAsync(FunctionEnvironment environment);

        /// <summary>
        /// Updates an environment
        /// </summary>
        /// <param name="environment">Environment to update</param>
        /// <returns>The updated environment</returns>
        Task<FunctionEnvironment> UpdateEnvironmentAsync(FunctionEnvironment environment);

        /// <summary>
        /// Gets an environment by ID
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <returns>The environment if found, null otherwise</returns>
        Task<FunctionEnvironment> GetEnvironmentByIdAsync(Guid id);

        /// <summary>
        /// Gets environments by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of environments</returns>
        Task<IEnumerable<FunctionEnvironment>> GetEnvironmentsByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Deletes an environment
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <returns>True if the environment was deleted successfully, false otherwise</returns>
        Task<bool> DeleteEnvironmentAsync(Guid id);

        /// <summary>
        /// Deploys a function to an environment
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="environmentId">Environment ID</param>
        /// <param name="version">Function version</param>
        /// <returns>The deployment</returns>
        Task<FunctionDeployment> DeployFunctionAsync(Guid functionId, Guid environmentId, string version = null);

        /// <summary>
        /// Gets a deployment by ID
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The deployment if found, null otherwise</returns>
        Task<FunctionDeployment> GetDeploymentByIdAsync(Guid id);

        /// <summary>
        /// Gets deployments by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of deployments</returns>
        Task<IEnumerable<FunctionDeployment>> GetDeploymentsByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets deployments by environment ID
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>List of deployments</returns>
        Task<IEnumerable<FunctionDeployment>> GetDeploymentsByEnvironmentIdAsync(Guid environmentId);

        /// <summary>
        /// Gets deployments by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of deployments</returns>
        Task<IEnumerable<FunctionDeployment>> GetDeploymentsByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Undeploys a function from an environment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>True if the function was undeployed successfully, false otherwise</returns>
        Task<bool> UndeployFunctionAsync(Guid deploymentId);

        /// <summary>
        /// Promotes a deployment to another environment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <param name="targetEnvironmentId">Target environment ID</param>
        /// <returns>The new deployment</returns>
        Task<FunctionDeployment> PromoteDeploymentAsync(Guid deploymentId, Guid targetEnvironmentId);

        /// <summary>
        /// Rolls back a deployment to a previous version
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>The new deployment</returns>
        Task<FunctionDeployment> RollbackDeploymentAsync(Guid deploymentId);

        /// <summary>
        /// Gets deployment logs
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Deployment logs</returns>
        Task<IEnumerable<string>> GetDeploymentLogsAsync(Guid deploymentId);

        /// <summary>
        /// Gets deployment metrics
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Deployment metrics</returns>
        Task<FunctionDeploymentMetrics> GetDeploymentMetricsAsync(Guid deploymentId);

        /// <summary>
        /// Updates deployment settings
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <param name="settings">Deployment settings</param>
        /// <returns>The updated deployment</returns>
        Task<FunctionDeployment> UpdateDeploymentSettingsAsync(Guid deploymentId, FunctionDeploymentSettings settings);

        /// <summary>
        /// Updates deployment resources
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <param name="resources">Deployment resources</param>
        /// <returns>The updated deployment</returns>
        Task<FunctionDeployment> UpdateDeploymentResourcesAsync(Guid deploymentId, FunctionDeploymentResources resources);

        /// <summary>
        /// Restarts a deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>The restarted deployment</returns>
        Task<FunctionDeployment> RestartDeploymentAsync(Guid deploymentId);

        /// <summary>
        /// Scales a deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <param name="instances">Number of instances</param>
        /// <returns>The scaled deployment</returns>
        Task<FunctionDeployment> ScaleDeploymentAsync(Guid deploymentId, int instances);

        /// <summary>
        /// Gets deployment status
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Deployment status</returns>
        Task<string> GetDeploymentStatusAsync(Guid deploymentId);

        /// <summary>
        /// Gets deployment URL
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Deployment URL</returns>
        Task<string> GetDeploymentUrlAsync(Guid deploymentId);

        /// <summary>
        /// Tests a deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>Test results</returns>
        Task<object> TestDeploymentAsync(Guid deploymentId);
    }
}
