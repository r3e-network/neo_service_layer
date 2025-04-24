using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function access policy repository
    /// </summary>
    public interface IFunctionAccessPolicyRepository
    {
        /// <summary>
        /// Gets a function access policy by ID
        /// </summary>
        /// <param name="id">Policy ID</param>
        /// <returns>The function access policy if found, null otherwise</returns>
        Task<FunctionAccessPolicy> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function access policies by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of policies to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function access policies for the specified function</returns>
        Task<IEnumerable<FunctionAccessPolicy>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function access policies by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of policies to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function access policies for the specified account</returns>
        Task<IEnumerable<FunctionAccessPolicy>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function access policies by policy type
        /// </summary>
        /// <param name="policyType">Policy type</param>
        /// <param name="limit">Maximum number of policies to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function access policies with the specified policy type</returns>
        Task<IEnumerable<FunctionAccessPolicy>> GetByPolicyTypeAsync(string policyType, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function access policy
        /// </summary>
        /// <param name="policy">Policy to create</param>
        /// <returns>The created function access policy</returns>
        Task<FunctionAccessPolicy> CreateAsync(FunctionAccessPolicy policy);

        /// <summary>
        /// Updates a function access policy
        /// </summary>
        /// <param name="id">Policy ID</param>
        /// <param name="policy">Updated policy</param>
        /// <returns>The updated function access policy</returns>
        Task<FunctionAccessPolicy> UpdateAsync(Guid id, FunctionAccessPolicy policy);

        /// <summary>
        /// Deletes a function access policy
        /// </summary>
        /// <param name="id">Policy ID</param>
        /// <returns>True if the policy was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function access policies
        /// </summary>
        /// <param name="limit">Maximum number of policies to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function access policies</returns>
        Task<IEnumerable<FunctionAccessPolicy>> GetAllAsync(int limit = 100, int offset = 0);
    }
}
