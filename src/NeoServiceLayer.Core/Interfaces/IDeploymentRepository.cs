using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for deployment repository
    /// </summary>
    public interface IDeploymentRepository
    {
        /// <summary>
        /// Creates a new deployment
        /// </summary>
        /// <param name="deployment">Deployment to create</param>
        /// <returns>The created deployment</returns>
        Task<Deployment> CreateAsync(Deployment deployment);

        /// <summary>
        /// Gets a deployment by ID
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The deployment if found, null otherwise</returns>
        Task<Deployment> GetAsync(Guid id);

        /// <summary>
        /// Gets deployments by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of deployments for the account</returns>
        Task<IEnumerable<Deployment>> GetByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets deployments by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of deployments for the function</returns>
        Task<IEnumerable<Deployment>> GetByFunctionAsync(Guid functionId);

        /// <summary>
        /// Gets deployments by environment ID
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>List of deployments for the environment</returns>
        Task<IEnumerable<Deployment>> GetByEnvironmentAsync(Guid environmentId);

        /// <summary>
        /// Updates a deployment
        /// </summary>
        /// <param name="deployment">Deployment to update</param>
        /// <returns>The updated deployment</returns>
        Task<Deployment> UpdateAsync(Deployment deployment);

        /// <summary>
        /// Deletes a deployment
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>True if the deployment was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Updates the deployment metrics
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <param name="metrics">Deployment metrics</param>
        /// <returns>The updated deployment</returns>
        Task<Deployment> UpdateMetricsAsync(Guid id, DeploymentMetrics metrics);

        /// <summary>
        /// Updates the deployment health
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <param name="health">Deployment health</param>
        /// <returns>The updated deployment</returns>
        Task<Deployment> UpdateHealthAsync(Guid id, DeploymentHealth health);

        /// <summary>
        /// Updates the deployment status
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <param name="status">Deployment status</param>
        /// <returns>The updated deployment</returns>
        Task<Deployment> UpdateStatusAsync(Guid id, DeploymentStatus status);

        /// <summary>
        /// Gets deployments by status
        /// </summary>
        /// <param name="status">Deployment status</param>
        /// <returns>List of deployments with the specified status</returns>
        Task<IEnumerable<Deployment>> GetByStatusAsync(DeploymentStatus status);

        /// <summary>
        /// Gets deployments by tag
        /// </summary>
        /// <param name="tagKey">Tag key</param>
        /// <param name="tagValue">Tag value</param>
        /// <returns>List of deployments with the specified tag</returns>
        Task<IEnumerable<Deployment>> GetByTagAsync(string tagKey, string tagValue);
    }
}
