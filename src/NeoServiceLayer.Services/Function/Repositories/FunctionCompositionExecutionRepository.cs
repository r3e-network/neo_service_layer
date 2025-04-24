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
    /// Repository for function composition executions
    /// </summary>
    public class FunctionCompositionExecutionRepository : IFunctionCompositionExecutionRepository
    {
        private readonly ILogger<FunctionCompositionExecutionRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_composition_executions";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCompositionExecutionRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionCompositionExecutionRepository(ILogger<FunctionCompositionExecutionRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionCompositionExecution> CreateAsync(FunctionCompositionExecution execution)
        {
            _logger.LogInformation("Creating function composition execution for composition {CompositionId}", execution.CompositionId);

            // Ensure ID is set
            if (execution.Id == Guid.Empty)
            {
                execution.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, execution);

            return execution;
        }

        /// <inheritdoc/>
        public async Task<FunctionCompositionExecution> UpdateAsync(Guid id, FunctionCompositionExecution execution)
        {
            _logger.LogInformation("Updating function composition execution: {Id}", id);

            // Ensure the ID matches
            execution.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionCompositionExecution, Guid>(_collectionName, id, execution);

            return execution;
        }

        /// <inheritdoc/>
        public async Task<FunctionCompositionExecution> UpdateAsync(FunctionCompositionExecution execution)
        {
            return await UpdateAsync(execution.Id, execution);
        }

        /// <inheritdoc/>
        public async Task<FunctionCompositionExecution> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function composition execution by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionCompositionExecution, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetByCompositionIdAsync(Guid compositionId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by composition ID: {CompositionId}, limit: {Limit}, offset: {Offset}", compositionId, limit, offset);

            // Get all executions
            var executions = await _storageProvider.GetAllAsync<FunctionCompositionExecution>(_collectionName);

            // Filter by composition ID and sort by start time
            return executions
                .Where(e => e.CompositionId == compositionId)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetByAccountIdAsync(Guid accountId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by account ID: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            // Get all executions
            var executions = await _storageProvider.GetAllAsync<FunctionCompositionExecution>(_collectionName);

            // Filter by account ID and sort by start time
            return executions
                .Where(e => e.AccountId == accountId)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetByStatusAsync(string status, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by status: {Status}, limit: {Limit}, offset: {Offset}", status, limit, offset);

            // Get all executions
            var executions = await _storageProvider.GetAllAsync<FunctionCompositionExecution>(_collectionName);

            // Filter by status and sort by start time
            return executions
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetByCompositionIdAndStatusAsync(Guid compositionId, string status, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by composition ID: {CompositionId} and status: {Status}, limit: {Limit}, offset: {Offset}", compositionId, status, limit, offset);

            // Get all executions
            var executions = await _storageProvider.GetAllAsync<FunctionCompositionExecution>(_collectionName);

            // Filter by composition ID and status, and sort by start time
            return executions
                .Where(e => e.CompositionId == compositionId && e.Status == status)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetByAccountIdAndStatusAsync(Guid accountId, string status, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by account ID: {AccountId} and status: {Status}, limit: {Limit}, offset: {Offset}", accountId, status, limit, offset);

            // Get all executions
            var executions = await _storageProvider.GetAllAsync<FunctionCompositionExecution>(_collectionName);

            // Filter by account ID and status, and sort by start time
            return executions
                .Where(e => e.AccountId == accountId && e.Status == status)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function composition execution: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionCompositionExecution, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByCompositionIdAsync(Guid compositionId)
        {
            _logger.LogInformation("Deleting function composition executions by composition ID: {CompositionId}", compositionId);

            // Get executions by composition ID
            var executions = await GetByCompositionIdAsync(compositionId, int.MaxValue, 0);

            // Delete each execution
            var success = true;
            foreach (var execution in executions)
            {
                var result = await DeleteAsync(execution.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Deleting function composition executions by account ID: {AccountId}", accountId);

            // Get executions by account ID
            var executions = await GetByAccountIdAsync(accountId, int.MaxValue, 0);

            // Delete each execution
            var success = true;
            foreach (var execution in executions)
            {
                var result = await DeleteAsync(execution.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByStatusAsync(string status, DateTime olderThan)
        {
            _logger.LogInformation("Deleting function composition executions by status: {Status} older than: {OlderThan}", status, olderThan);

            // Get all executions
            var executions = await _storageProvider.GetAllAsync<FunctionCompositionExecution>(_collectionName);

            // Filter by status and start time
            var filteredExecutions = executions.Where(e => e.Status == status && e.StartTime < olderThan);

            // Delete each execution
            var success = true;
            foreach (var execution in filteredExecutions)
            {
                var result = await DeleteAsync(execution.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by time range: {StartTime} to {EndTime}, limit: {Limit}, offset: {Offset}", startTime, endTime, limit, offset);

            // Get all executions
            var executions = await _storageProvider.GetAllAsync<FunctionCompositionExecution>(_collectionName);

            // Filter by time range and sort by start time
            return executions
                .Where(e => e.StartTime >= startTime && e.StartTime <= endTime)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetByCompositionIdAndTimeRangeAsync(Guid compositionId, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by composition ID: {CompositionId} and time range: {StartTime} to {EndTime}, limit: {Limit}, offset: {Offset}", compositionId, startTime, endTime, limit, offset);

            // Get all executions
            var executions = await _storageProvider.GetAllAsync<FunctionCompositionExecution>(_collectionName);

            // Filter by composition ID and time range, and sort by start time
            return executions
                .Where(e => e.CompositionId == compositionId && e.StartTime >= startTime && e.StartTime <= endTime)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function composition executions, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all executions
            var executions = await _storageProvider.GetAllAsync<FunctionCompositionExecution>(_collectionName);

            // Sort by start time and apply pagination
            return executions
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit);
        }
    }
}
