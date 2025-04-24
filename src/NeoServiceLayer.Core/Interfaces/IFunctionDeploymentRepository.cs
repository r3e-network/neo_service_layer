using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function deployment repository
    /// </summary>
    public interface IFunctionDeploymentRepository
    {
        /// <summary>
        /// Creates a new function deployment
        /// </summary>
        /// <param name="deployment">Function deployment to create</param>
        /// <returns>The created function deployment</returns>
        Task<FunctionDeployment> CreateAsync(FunctionDeployment deployment);

        /// <summary>
        /// Updates a function deployment
        /// </summary>
        /// <param name="deployment">Function deployment to update</param>
        /// <returns>The updated function deployment</returns>
        Task<FunctionDeployment> UpdateAsync(FunctionDeployment deployment);

        /// <summary>
        /// Gets a function deployment by ID
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>The function deployment if found, null otherwise</returns>
        Task<FunctionDeployment> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function deployments by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of function deployments</returns>
        Task<IEnumerable<FunctionDeployment>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function deployments by environment ID
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>List of function deployments</returns>
        Task<IEnumerable<FunctionDeployment>> GetByEnvironmentIdAsync(Guid environmentId);

        /// <summary>
        /// Gets function deployments by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of function deployments</returns>
        Task<IEnumerable<FunctionDeployment>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets function deployments by status
        /// </summary>
        /// <param name="status">Deployment status</param>
        /// <returns>List of function deployments</returns>
        Task<IEnumerable<FunctionDeployment>> GetByStatusAsync(string status);

        /// <summary>
        /// Gets function deployments by function ID and environment ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>List of function deployments</returns>
        Task<IEnumerable<FunctionDeployment>> GetByFunctionIdAndEnvironmentIdAsync(Guid functionId, Guid environmentId);

        /// <summary>
        /// Gets the latest function deployment by function ID and environment ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>The latest function deployment if found, null otherwise</returns>
        Task<FunctionDeployment> GetLatestByFunctionIdAndEnvironmentIdAsync(Guid functionId, Guid environmentId);

        /// <summary>
        /// Deletes a function deployment
        /// </summary>
        /// <param name="id">Deployment ID</param>
        /// <returns>True if the deployment was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function deployments by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the deployments were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Deletes function deployments by environment ID
        /// </summary>
        /// <param name="environmentId">Environment ID</param>
        /// <returns>True if the deployments were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByEnvironmentIdAsync(Guid environmentId);
    }
}
