using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for deployment version repository
    /// </summary>
    public interface IDeploymentVersionRepository
    {
        /// <summary>
        /// Creates a new deployment version
        /// </summary>
        /// <param name="version">Version to create</param>
        /// <returns>The created version</returns>
        Task<DeploymentVersion> CreateAsync(DeploymentVersion version);

        /// <summary>
        /// Gets a deployment version by ID
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <returns>The version if found, null otherwise</returns>
        Task<DeploymentVersion> GetAsync(Guid id);

        /// <summary>
        /// Gets deployment versions by deployment ID
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>List of versions for the deployment</returns>
        Task<IEnumerable<DeploymentVersion>> GetByDeploymentAsync(Guid deploymentId);

        /// <summary>
        /// Gets deployment versions by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of versions for the function</returns>
        Task<IEnumerable<DeploymentVersion>> GetByFunctionAsync(Guid functionId);

        /// <summary>
        /// Gets deployment versions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of versions for the account</returns>
        Task<IEnumerable<DeploymentVersion>> GetByAccountAsync(Guid accountId);

        /// <summary>
        /// Updates a deployment version
        /// </summary>
        /// <param name="version">Version to update</param>
        /// <returns>The updated version</returns>
        Task<DeploymentVersion> UpdateAsync(DeploymentVersion version);

        /// <summary>
        /// Deletes a deployment version
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <returns>True if the version was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Updates the version status
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <param name="status">Version status</param>
        /// <returns>The updated version</returns>
        Task<DeploymentVersion> UpdateStatusAsync(Guid id, VersionStatus status);

        /// <summary>
        /// Updates the validation results for a version
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <param name="validationResults">Validation results</param>
        /// <returns>The updated version</returns>
        Task<DeploymentVersion> UpdateValidationResultsAsync(Guid id, ValidationResults validationResults);

        /// <summary>
        /// Adds a log entry to a version
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <param name="log">Log entry</param>
        /// <returns>The updated version</returns>
        Task<DeploymentVersion> AddLogAsync(Guid id, DeploymentLog log);

        /// <summary>
        /// Gets the logs for a version
        /// </summary>
        /// <param name="id">Version ID</param>
        /// <returns>The logs for the version</returns>
        Task<IEnumerable<DeploymentLog>> GetLogsAsync(Guid id);

        /// <summary>
        /// Gets versions by status
        /// </summary>
        /// <param name="status">Version status</param>
        /// <returns>List of versions with the specified status</returns>
        Task<IEnumerable<DeploymentVersion>> GetByStatusAsync(VersionStatus status);

        /// <summary>
        /// Gets versions by tag
        /// </summary>
        /// <param name="tagKey">Tag key</param>
        /// <param name="tagValue">Tag value</param>
        /// <returns>List of versions with the specified tag</returns>
        Task<IEnumerable<DeploymentVersion>> GetByTagAsync(string tagKey, string tagValue);
    }
}
