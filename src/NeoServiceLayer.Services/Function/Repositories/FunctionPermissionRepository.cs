using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Repository for function permissions
    /// </summary>
    public class FunctionPermissionRepository : IFunctionPermissionRepository
    {
        private readonly ILogger<FunctionPermissionRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_permissions";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionPermissionRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionPermissionRepository(ILogger<FunctionPermissionRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionPermission> CreateAsync(FunctionPermission permission)
        {
            _logger.LogInformation("Creating function permission for principal {PrincipalId} of type {PrincipalType} for function {FunctionId}", permission.PrincipalId, permission.PrincipalType, permission.FunctionId);

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, permission);

            return permission;
        }

        /// <inheritdoc/>
        public async Task<FunctionPermission> UpdateAsync(Guid id, FunctionPermission permission)
        {
            _logger.LogInformation("Updating function permission: {Id}", id);

            // Ensure the ID matches
            permission.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionPermission, Guid>(_collectionName, id, permission);

            return permission;
        }

        /// <inheritdoc/>
        public async Task<FunctionPermission> UpdateAsync(FunctionPermission permission)
        {
            return await UpdateAsync(permission.Id, permission);
        }

        /// <inheritdoc/>
        public async Task<FunctionPermission> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function permission by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionPermission, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function permissions by function ID: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            // Get all permissions
            var permissions = await _storageProvider.GetAllAsync<FunctionPermission>(_collectionName);

            // Filter by function ID and apply pagination
            return permissions
                .Where(p => p.FunctionId == functionId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetByPrincipalAsync(string principalId, string principalType)
        {
            _logger.LogInformation("Getting function permissions by principal ID: {PrincipalId} of type {PrincipalType}", principalId, principalType);

            // Get all permissions
            var permissions = await _storageProvider.GetAllAsync<FunctionPermission>(_collectionName);

            // Filter by principal ID and type
            return permissions.Where(p => p.PrincipalId == principalId && p.PrincipalType == principalType);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetByFunctionIdAndPrincipalAsync(Guid functionId, string principalId, string principalType)
        {
            _logger.LogInformation("Getting function permissions by function ID: {FunctionId} and principal ID: {PrincipalId} of type {PrincipalType}", functionId, principalId, principalType);

            // Get all permissions
            var permissions = await _storageProvider.GetAllAsync<FunctionPermission>(_collectionName);

            // Filter by function ID, principal ID, and principal type
            return permissions.Where(p => p.FunctionId == functionId && p.PrincipalId == principalId && p.PrincipalType == principalType);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function permission: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionPermission, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Deleting function permissions by function ID: {FunctionId}", functionId);

            // Get permissions by function ID
            var permissions = await GetByFunctionIdAsync(functionId);

            // Delete each permission
            var success = true;
            foreach (var permission in permissions)
            {
                var result = await DeleteAsync(permission.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByPrincipalAsync(string principalId, string principalType)
        {
            _logger.LogInformation("Deleting function permissions by principal ID: {PrincipalId} of type {PrincipalType}", principalId, principalType);

            // Get permissions by principal ID and type
            var permissions = await GetByPrincipalAsync(principalId, principalType);

            // Delete each permission
            var success = true;
            foreach (var permission in permissions)
            {
                var result = await DeleteAsync(permission.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetByUserIdAsync(Guid userId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function permissions by user ID: {UserId}, limit: {Limit}, offset: {Offset}", userId, limit, offset);

            // Get all permissions
            var permissions = await _storageProvider.GetAllAsync<FunctionPermission>(_collectionName);

            // Filter by user ID and apply pagination
            return permissions
                .Where(p => p.PrincipalType == "User" && p.PrincipalId == userId.ToString())
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function permissions by account ID: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            // Get all permissions
            var permissions = await _storageProvider.GetAllAsync<FunctionPermission>(_collectionName);

            // Filter by account ID and apply pagination
            return permissions
                .Where(p => p.PrincipalType == "Account" && p.PrincipalId == accountId.ToString())
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetByPermissionTypeAsync(string permissionType, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function permissions by permission type: {PermissionType}, limit: {Limit}, offset: {Offset}", permissionType, limit, offset);

            // Get all permissions
            var permissions = await _storageProvider.GetAllAsync<FunctionPermission>(_collectionName);

            // Filter by permission type and apply pagination
            return permissions
                .Where(p => p.PermissionType == permissionType)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function permissions, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all permissions
            var permissions = await _storageProvider.GetAllAsync<FunctionPermission>(_collectionName);

            // Apply pagination
            return permissions
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetByFunctionIdAndUserIdAsync(Guid functionId, Guid userId)
        {
            _logger.LogInformation("Getting function permissions by function ID: {FunctionId} and user ID: {UserId}", functionId, userId);

            // Get all permissions
            var permissions = await _storageProvider.GetAllAsync<FunctionPermission>(_collectionName);

            // Filter by function ID and user ID
            return permissions.Where(p => p.FunctionId == functionId && p.PrincipalType == "User" && p.PrincipalId == userId.ToString());
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetByFunctionIdAndAccountIdAsync(Guid functionId, Guid accountId)
        {
            _logger.LogInformation("Getting function permissions by function ID: {FunctionId} and account ID: {AccountId}", functionId, accountId);

            // Get all permissions
            var permissions = await _storageProvider.GetAllAsync<FunctionPermission>(_collectionName);

            // Filter by function ID and account ID
            return permissions.Where(p => p.FunctionId == functionId && p.PrincipalType == "Account" && p.PrincipalId == accountId.ToString());
        }
    }
}
