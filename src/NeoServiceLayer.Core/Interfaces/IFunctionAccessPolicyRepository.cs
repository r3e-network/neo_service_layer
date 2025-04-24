using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function access policy repository
    /// </summary>
    public interface IFunctionAccessPolicyRepository
    {
        /// <summary>
        /// Creates a new function access policy
        /// </summary>
        /// <param name="policy">Function access policy to create</param>
        /// <returns>The created function access policy</returns>
        Task<FunctionAccessPolicy> CreateAsync(FunctionAccessPolicy policy);

        /// <summary>
        /// Updates a function access policy
        /// </summary>
        /// <param name="policy">Function access policy to update</param>
        /// <returns>The updated function access policy</returns>
        Task<FunctionAccessPolicy> UpdateAsync(FunctionAccessPolicy policy);

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
        /// <returns>List of function access policies</returns>
        Task<IEnumerable<FunctionAccessPolicy>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function access policies by policy type
        /// </summary>
        /// <param name="policyType">Policy type</param>
        /// <returns>List of function access policies</returns>
        Task<IEnumerable<FunctionAccessPolicy>> GetByPolicyTypeAsync(string policyType);

        /// <summary>
        /// Gets function access policies by function ID and policy type
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="policyType">Policy type</param>
        /// <returns>List of function access policies</returns>
        Task<IEnumerable<FunctionAccessPolicy>> GetByFunctionIdAndPolicyTypeAsync(Guid functionId, string policyType);

        /// <summary>
        /// Deletes a function access policy
        /// </summary>
        /// <param name="id">Policy ID</param>
        /// <returns>True if the policy was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function access policies by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the policies were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);
    }
}
