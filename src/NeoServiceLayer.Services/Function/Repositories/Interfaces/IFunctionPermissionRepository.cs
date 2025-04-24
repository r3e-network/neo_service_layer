using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function permission repository
    /// </summary>
    public interface IFunctionPermissionRepository
    {
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
        /// <param name="limit">Maximum number of permissions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function permissions for the specified function</returns>
        Task<IEnumerable<FunctionPermission>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function permissions by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="limit">Maximum number of permissions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function permissions for the specified user</returns>
        Task<IEnumerable<FunctionPermission>> GetByUserIdAsync(Guid userId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function permissions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of permissions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function permissions for the specified account</returns>
        Task<IEnumerable<FunctionPermission>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function permissions by permission type
        /// </summary>
        /// <param name="permissionType">Permission type</param>
        /// <param name="limit">Maximum number of permissions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function permissions with the specified permission type</returns>
        Task<IEnumerable<FunctionPermission>> GetByPermissionTypeAsync(string permissionType, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function permission
        /// </summary>
        /// <param name="permission">Permission to create</param>
        /// <returns>The created function permission</returns>
        Task<FunctionPermission> CreateAsync(FunctionPermission permission);

        /// <summary>
        /// Updates a function permission
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <param name="permission">Updated permission</param>
        /// <returns>The updated function permission</returns>
        Task<FunctionPermission> UpdateAsync(Guid id, FunctionPermission permission);

        /// <summary>
        /// Deletes a function permission
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <returns>True if the permission was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function permissions
        /// </summary>
        /// <param name="limit">Maximum number of permissions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function permissions</returns>
        Task<IEnumerable<FunctionPermission>> GetAllAsync(int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function permissions by function ID and user ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>List of function permissions for the specified function and user</returns>
        Task<IEnumerable<FunctionPermission>> GetByFunctionIdAndUserIdAsync(Guid functionId, Guid userId);

        /// <summary>
        /// Gets function permissions by function ID and account ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of function permissions for the specified function and account</returns>
        Task<IEnumerable<FunctionPermission>> GetByFunctionIdAndAccountIdAsync(Guid functionId, Guid accountId);
    }
}
