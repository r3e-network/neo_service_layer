using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function access request repository
    /// </summary>
    public interface IFunctionAccessRequestRepository
    {
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
        /// <param name="limit">Maximum number of requests to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function access requests for the specified function</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function access requests by requester ID
        /// </summary>
        /// <param name="requesterId">Requester ID</param>
        /// <param name="limit">Maximum number of requests to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function access requests for the specified requester</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByRequesterIdAsync(Guid requesterId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function access requests by approver ID
        /// </summary>
        /// <param name="approverId">Approver ID</param>
        /// <param name="limit">Maximum number of requests to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function access requests for the specified approver</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByApproverIdAsync(Guid approverId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function access requests by status
        /// </summary>
        /// <param name="status">Request status</param>
        /// <param name="limit">Maximum number of requests to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function access requests with the specified status</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByStatusAsync(string status, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function access request
        /// </summary>
        /// <param name="request">Request to create</param>
        /// <returns>The created function access request</returns>
        Task<FunctionAccessRequest> CreateAsync(FunctionAccessRequest request);

        /// <summary>
        /// Updates a function access request
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <param name="request">Updated request</param>
        /// <returns>The updated function access request</returns>
        Task<FunctionAccessRequest> UpdateAsync(Guid id, FunctionAccessRequest request);

        /// <summary>
        /// Deletes a function access request
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <returns>True if the request was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function access requests
        /// </summary>
        /// <param name="limit">Maximum number of requests to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function access requests</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetAllAsync(int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function access requests by function ID and requester ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="requesterId">Requester ID</param>
        /// <returns>List of function access requests for the specified function and requester</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetByFunctionIdAndRequesterIdAsync(Guid functionId, Guid requesterId);
    }
}
