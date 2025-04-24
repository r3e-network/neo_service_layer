using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function access request repository
    /// </summary>
    public interface IFunctionAccessRequestRepository
    {
        /// <summary>
        /// Creates a new function access request
        /// </summary>
        /// <param name="request">Function access request to create</param>
        /// <returns>The created function access request</returns>
        Task<FunctionAccessRequest> CreateAsync(FunctionAccessRequest request);

        /// <summary>
        /// Updates a function access request
        /// </summary>
        /// <param name="request">Function access request to update</param>
        /// <returns>The updated function access request</returns>
        Task<FunctionAccessRequest> UpdateAsync(FunctionAccessRequest request);

        /// <summary>
        /// Gets a function access request by ID
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <returns>The function access request if found, null otherwise</returns>
        Task<FunctionAccessRequest> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function access requests by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of function access requests</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function access requests by principal ID and type
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>List of function access requests</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByPrincipalAsync(string principalId, string principalType);

        /// <summary>
        /// Gets function access requests by status
        /// </summary>
        /// <param name="status">Request status</param>
        /// <returns>List of function access requests</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByStatusAsync(string status);

        /// <summary>
        /// Gets function access requests by function ID and status
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="status">Request status</param>
        /// <returns>List of function access requests</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByFunctionIdAndStatusAsync(Guid functionId, string status);

        /// <summary>
        /// Gets function access requests by principal ID, principal type, and status
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <param name="status">Request status</param>
        /// <returns>List of function access requests</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByPrincipalAndStatusAsync(string principalId, string principalType, string status);

        /// <summary>
        /// Deletes a function access request
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <returns>True if the request was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function access requests by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the requests were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);
    }
}
