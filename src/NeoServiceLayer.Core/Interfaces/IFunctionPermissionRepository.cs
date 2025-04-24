using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function permission repository
    /// </summary>
    public interface IFunctionPermissionRepository
    {
        /// <summary>
        /// Creates a new function permission
        /// </summary>
        /// <param name="permission">Function permission to create</param>
        /// <returns>The created function permission</returns>
        Task<FunctionPermission> CreateAsync(FunctionPermission permission);

        /// <summary>
        /// Updates a function permission
        /// </summary>
        /// <param name="permission">Function permission to update</param>
        /// <returns>The updated function permission</returns>
        Task<FunctionPermission> UpdateAsync(FunctionPermission permission);

        /// <summary>
        /// Gets a function permission by ID
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <returns>The function permission if found, null otherwise</returns>
        Task<FunctionPermission> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function permissions by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of function permissions</returns>
        Task<IEnumerable<FunctionPermission>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function permissions by principal ID and type
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>List of function permissions</returns>
        Task<IEnumerable<FunctionPermission>> GetByPrincipalAsync(string principalId, string principalType);

        /// <summary>
        /// Gets function permissions by function ID, principal ID, and principal type
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>List of function permissions</returns>
        Task<IEnumerable<FunctionPermission>> GetByFunctionIdAndPrincipalAsync(Guid functionId, string principalId, string principalType);

        /// <summary>
        /// Deletes a function permission
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <returns>True if the permission was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function permissions by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the permissions were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Deletes function permissions by principal ID and type
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>True if the permissions were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByPrincipalAsync(string principalId, string principalType);
    }
}
