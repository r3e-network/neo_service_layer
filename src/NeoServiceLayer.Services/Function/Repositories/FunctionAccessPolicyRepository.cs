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
    /// Repository for function access policies
    /// </summary>
    public class FunctionAccessPolicyRepository : IFunctionAccessPolicyRepository
    {
        private readonly ILogger<FunctionAccessPolicyRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_access_policies";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAccessPolicyRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionAccessPolicyRepository(ILogger<FunctionAccessPolicyRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessPolicy> CreateAsync(FunctionAccessPolicy policy)
        {
            _logger.LogInformation("Creating function access policy: {Name} for function {FunctionId}", policy.Name, policy.FunctionId);

            // Ensure ID is set
            if (policy.Id == Guid.Empty)
            {
                policy.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, policy);

            return policy;
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessPolicy> UpdateAsync(Guid id, FunctionAccessPolicy policy)
        {
            _logger.LogInformation("Updating function access policy: {Id}", id);

            // Ensure the ID matches
            policy.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionAccessPolicy, Guid>(_collectionName, id, policy);

            return policy;
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessPolicy> UpdateAsync(FunctionAccessPolicy policy)
        {
            return await UpdateAsync(policy.Id, policy);
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessPolicy> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function access policy by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionAccessPolicy, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessPolicy>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function access policies by function ID: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            // Get all policies
            var policies = await _storageProvider.GetAllAsync<FunctionAccessPolicy>(_collectionName);

            // Filter by function ID and apply pagination
            return policies
                .Where(p => p.FunctionId == functionId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessPolicy>> GetByPolicyTypeAsync(string policyType, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function access policies by policy type: {PolicyType}, limit: {Limit}, offset: {Offset}", policyType, limit, offset);

            // Get all policies
            var policies = await _storageProvider.GetAllAsync<FunctionAccessPolicy>(_collectionName);

            // Filter by policy type and apply pagination
            return policies
                .Where(p => p.PolicyType == policyType)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessPolicy>> GetByFunctionIdAndPolicyTypeAsync(Guid functionId, string policyType)
        {
            _logger.LogInformation("Getting function access policies by function ID: {FunctionId} and policy type: {PolicyType}", functionId, policyType);

            // Get all policies
            var policies = await _storageProvider.GetAllAsync<FunctionAccessPolicy>(_collectionName);

            // Filter by function ID and policy type
            return policies.Where(p => p.FunctionId == functionId && p.PolicyType == policyType);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function access policy: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionAccessPolicy, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Deleting function access policies by function ID: {FunctionId}", functionId);

            // Get policies by function ID
            var policies = await GetByFunctionIdAsync(functionId);

            // Delete each policy
            var success = true;
            foreach (var policy in policies)
            {
                var result = await DeleteAsync(policy.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessPolicy>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function access policies by account ID: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            // Get all policies
            var policies = await _storageProvider.GetAllAsync<FunctionAccessPolicy>(_collectionName);

            // Filter by account ID and apply pagination
            return policies
                .Where(p => p.AccountId == accountId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessPolicy>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function access policies, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all policies
            var policies = await _storageProvider.GetAllAsync<FunctionAccessPolicy>(_collectionName);

            // Apply pagination
            return policies
                .Skip(offset)
                .Take(limit);
        }
    }
}
