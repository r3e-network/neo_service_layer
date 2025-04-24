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
    /// Repository for function access requests
    /// </summary>
    public class FunctionAccessRequestRepository : IFunctionAccessRequestRepository
    {
        private readonly ILogger<FunctionAccessRequestRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_access_requests";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAccessRequestRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionAccessRequestRepository(ILogger<FunctionAccessRequestRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessRequest> CreateAsync(FunctionAccessRequest request)
        {
            _logger.LogInformation("Creating function access request for principal {PrincipalId} of type {PrincipalType} for function {FunctionId}", request.PrincipalId, request.PrincipalType, request.FunctionId);

            // Ensure ID is set
            if (request.Id == Guid.Empty)
            {
                request.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, request);

            return request;
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessRequest> UpdateAsync(Guid id, FunctionAccessRequest request)
        {
            _logger.LogInformation("Updating function access request: {Id}", id);

            // Ensure the ID matches
            request.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionAccessRequest, Guid>(_collectionName, id, request);

            return request;
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessRequest> UpdateAsync(FunctionAccessRequest request)
        {
            return await UpdateAsync(request.Id, request);
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessRequest> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function access request by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionAccessRequest, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function access requests by function ID: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            // Get all requests
            var requests = await _storageProvider.GetAllAsync<FunctionAccessRequest>(_collectionName);

            // Filter by function ID and apply pagination
            return requests
                .Where(r => r.FunctionId == functionId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetByPrincipalAsync(string principalId, string principalType)
        {
            _logger.LogInformation("Getting function access requests by principal ID: {PrincipalId} of type {PrincipalType}", principalId, principalType);

            // Get all requests
            var requests = await _storageProvider.GetAllAsync<FunctionAccessRequest>(_collectionName);

            // Filter by principal ID and type
            return requests.Where(r => r.PrincipalId == principalId && r.PrincipalType == principalType);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetByStatusAsync(string status, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function access requests by status: {Status}, limit: {Limit}, offset: {Offset}", status, limit, offset);

            // Get all requests
            var requests = await _storageProvider.GetAllAsync<FunctionAccessRequest>(_collectionName);

            // Filter by status and apply pagination
            return requests
                .Where(r => r.Status == status)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetByFunctionIdAndStatusAsync(Guid functionId, string status)
        {
            _logger.LogInformation("Getting function access requests by function ID: {FunctionId} and status: {Status}", functionId, status);

            // Get all requests
            var requests = await _storageProvider.GetAllAsync<FunctionAccessRequest>(_collectionName);

            // Filter by function ID and status
            return requests.Where(r => r.FunctionId == functionId && r.Status == status);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetByPrincipalAndStatusAsync(string principalId, string principalType, string status)
        {
            _logger.LogInformation("Getting function access requests by principal ID: {PrincipalId} of type {PrincipalType} and status: {Status}", principalId, principalType, status);

            // Get all requests
            var requests = await _storageProvider.GetAllAsync<FunctionAccessRequest>(_collectionName);

            // Filter by principal ID, principal type, and status
            return requests.Where(r => r.PrincipalId == principalId && r.PrincipalType == principalType && r.Status == status);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function access request: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionAccessRequest, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Deleting function access requests by function ID: {FunctionId}", functionId);

            // Get requests by function ID
            var requests = await GetByFunctionIdAsync(functionId);

            // Delete each request
            var success = true;
            foreach (var request in requests)
            {
                var result = await DeleteAsync(request.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetByRequesterIdAsync(Guid requesterId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function access requests by requester ID: {RequesterId}, limit: {Limit}, offset: {Offset}", requesterId, limit, offset);

            // Get all requests
            var requests = await _storageProvider.GetAllAsync<FunctionAccessRequest>(_collectionName);

            // Filter by requester ID and apply pagination
            return requests
                .Where(r => r.RequesterId == requesterId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetByApproverIdAsync(Guid approverId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function access requests by approver ID: {ApproverId}, limit: {Limit}, offset: {Offset}", approverId, limit, offset);

            // Get all requests
            var requests = await _storageProvider.GetAllAsync<FunctionAccessRequest>(_collectionName);

            // Filter by approver ID and apply pagination
            return requests
                .Where(r => r.ApproverId != null && Guid.Parse(r.ApproverId) == approverId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function access requests, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all requests
            var requests = await _storageProvider.GetAllAsync<FunctionAccessRequest>(_collectionName);

            // Apply pagination
            return requests
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetByFunctionIdAndRequesterIdAsync(Guid functionId, Guid requesterId)
        {
            _logger.LogInformation("Getting function access requests by function ID: {FunctionId} and requester ID: {RequesterId}", functionId, requesterId);

            // Get all requests
            var requests = await _storageProvider.GetAllAsync<FunctionAccessRequest>(_collectionName);

            // Filter by function ID and requester ID
            return requests.Where(r => r.FunctionId == functionId && r.RequesterId == requesterId);
        }
    }
}
