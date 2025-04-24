using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for deployment environment repository
    /// </summary>
    public interface IDeploymentEnvironmentRepository
    {
        /// <summary>
        /// Creates a new deployment environment
        /// </summary>
        /// <param name="environment">Environment to create</param>
        /// <returns>The created environment</returns>
        Task<DeploymentEnvironment> CreateAsync(DeploymentEnvironment environment);

        /// <summary>
        /// Gets a deployment environment by ID
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <returns>The environment if found, null otherwise</returns>
        Task<DeploymentEnvironment> GetAsync(Guid id);

        /// <summary>
        /// Gets deployment environments by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of environments for the account</returns>
        Task<IEnumerable<DeploymentEnvironment>> GetByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets deployment environments by type
        /// </summary>
        /// <param name="type">Environment type</param>
        /// <returns>List of environments of the specified type</returns>
        Task<IEnumerable<DeploymentEnvironment>> GetByTypeAsync(EnvironmentType type);

        /// <summary>
        /// Updates a deployment environment
        /// </summary>
        /// <param name="environment">Environment to update</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> UpdateAsync(DeploymentEnvironment environment);

        /// <summary>
        /// Deletes a deployment environment
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <returns>True if the environment was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Updates the environment status
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <param name="status">Environment status</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> UpdateStatusAsync(Guid id, EnvironmentStatus status);

        /// <summary>
        /// Adds a deployment to an environment
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> AddDeploymentAsync(Guid environmentId, Guid deploymentId);

        /// <summary>
        /// Removes a deployment from an environment
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> RemoveDeploymentAsync(Guid environmentId, Guid deploymentId);

        /// <summary>
        /// Updates the environment variables for an environment
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <param name="environmentVariables">Environment variables</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> UpdateEnvironmentVariablesAsync(Guid id, Dictionary<string, string> environmentVariables);

        /// <summary>
        /// Adds a secret to an environment
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <param name="secret">Secret to add</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> AddSecretAsync(Guid environmentId, EnvironmentSecret secret);

        /// <summary>
        /// Removes a secret from an environment
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <param name="secretName">Secret name</param>
        /// <returns>The updated environment</returns>
        Task<DeploymentEnvironment> RemoveSecretAsync(Guid environmentId, string secretName);

        /// <summary>
        /// Gets environments by tag
        /// </summary>
        /// <param name="tagKey">Tag key</param>
        /// <param name="tagValue">Tag value</param>
        /// <returns>List of environments with the specified tag</returns>
        Task<IEnumerable<DeploymentEnvironment>> GetByTagAsync(string tagKey, string tagValue);
    }
}
