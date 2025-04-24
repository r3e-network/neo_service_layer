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
    /// Implementation of the function execution repository
    /// </summary>
    public class FunctionExecutionRepository : Core.Interfaces.IFunctionExecutionRepository
    {
        private readonly ILogger<FunctionExecutionRepository> _logger;
        private readonly Dictionary<Guid, FunctionExecution> _executions;
        private readonly Dictionary<Guid, List<FunctionLog>> _logs;
        private readonly Dictionary<Guid, FunctionExecutionResult> _executionResults;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionExecutionRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public FunctionExecutionRepository(ILogger<FunctionExecutionRepository> logger)
        {
            _logger = logger;
            _executions = new Dictionary<Guid, FunctionExecution>();
            _logs = new Dictionary<Guid, List<FunctionLog>>();
            _executionResults = new Dictionary<Guid, FunctionExecutionResult>();
        }

        // This method is now implemented in the Core.Interfaces.IFunctionExecutionRepository region below

        // These methods are now implemented in the Core.Interfaces.IFunctionExecutionRepository region below

        /// <inheritdoc/>
        public Task<FunctionExecution> UpdateAsync(FunctionExecution execution)
        {
            _logger.LogInformation("Updating function execution: {Id}", execution.Id);

            if (!_executions.ContainsKey(execution.Id))
            {
                return Task.FromResult<FunctionExecution?>(null);
            }

            _executions[execution.Id] = execution;

            return Task.FromResult(execution);
        }

        /// <inheritdoc/>
        public Task<FunctionLog> AddLogAsync(FunctionLog log)
        {
            _logger.LogInformation("Adding log to function execution: {ExecutionId}", log.ExecutionId);

            if (log.Id == Guid.Empty)
            {
                log.Id = Guid.NewGuid();
            }

            if (!_logs.ContainsKey(log.ExecutionId))
            {
                _logs[log.ExecutionId] = new List<FunctionLog>();
            }

            _logs[log.ExecutionId].Add(log);

            return Task.FromResult(log);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FunctionLog>> GetLogsAsync(Guid executionId)
        {
            _logger.LogInformation("Getting logs for function execution: {ExecutionId}", executionId);

            if (!_logs.ContainsKey(executionId))
            {
                return Task.FromResult<IEnumerable<FunctionLog>>(new List<FunctionLog>());
            }

            return Task.FromResult<IEnumerable<FunctionLog>>(_logs[executionId].OrderBy(l => l.Timestamp).ToList());
        }

        #region Core.Interfaces.IFunctionExecutionRepository Implementation

        /// <inheritdoc/>
        public Task<FunctionExecutionResult?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function execution result by ID: {Id}", id);

            _executionResults.TryGetValue(id, out var executionResult);
            return Task.FromResult<FunctionExecutionResult?>(executionResult);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FunctionExecutionResult>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function execution results by function ID: {FunctionId}, Limit: {Limit}, Offset: {Offset}", functionId, limit, offset);

            var executionResults = _executionResults.Values
                .Where(e => e.FunctionId == functionId)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<FunctionExecutionResult>>(executionResults);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FunctionExecutionResult>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function execution results by account ID: {AccountId}, Limit: {Limit}, Offset: {Offset}", accountId, limit, offset);

            // Note: FunctionExecutionResult doesn't have AccountId property, so we can't filter by it
            // This is a placeholder implementation
            var executionResults = _executionResults.Values
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<FunctionExecutionResult>>(executionResults);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FunctionExecutionResult>> GetByStatusAsync(string status, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function execution results by status: {Status}, Limit: {Limit}, Offset: {Offset}", status, limit, offset);

            var executionResults = _executionResults.Values
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<FunctionExecutionResult>>(executionResults);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FunctionExecutionResult>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function execution results by time range: {StartTime} to {EndTime}, Limit: {Limit}, Offset: {Offset}", startTime, endTime, limit, offset);

            var executionResults = _executionResults.Values
                .Where(e => e.StartTime >= startTime && e.StartTime <= endTime)
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<FunctionExecutionResult>>(executionResults);
        }

        /// <inheritdoc/>
        public Task<FunctionExecutionResult> CreateAsync(FunctionExecutionResult execution)
        {
            _logger.LogInformation("Creating function execution result: {Id}, FunctionId: {FunctionId}", execution.Id, execution.FunctionId);

            if (execution.Id == Guid.Empty)
            {
                execution.Id = Guid.NewGuid();
            }

            _executionResults[execution.Id] = execution;

            return Task.FromResult(execution);
        }

        /// <inheritdoc/>
        public Task<FunctionExecutionResult> UpdateAsync(Guid id, FunctionExecutionResult execution)
        {
            _logger.LogInformation("Updating function execution result: {Id}", id);

            if (!_executionResults.ContainsKey(id))
            {
                return Task.FromResult<FunctionExecutionResult?>(null);
            }

            execution.Id = id;
            _executionResults[id] = execution;

            return Task.FromResult(execution);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function execution result: {Id}", id);

            return Task.FromResult(_executionResults.Remove(id));
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(Guid id)
        {
            _logger.LogInformation("Checking if function execution result exists: {Id}", id);

            return Task.FromResult(_executionResults.ContainsKey(id));
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FunctionExecutionResult>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function execution results, Limit: {Limit}, Offset: {Offset}", limit, offset);

            var executionResults = _executionResults.Values
                .OrderByDescending(e => e.StartTime)
                .Skip(offset)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<FunctionExecutionResult>>(executionResults);
        }

        /// <inheritdoc/>
        public Task<int> CountAsync()
        {
            _logger.LogInformation("Counting function execution results");

            return Task.FromResult(_executionResults.Count);
        }

        /// <inheritdoc/>
        public Task<int> CountByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Counting function execution results by function ID: {FunctionId}", functionId);

            var count = _executionResults.Values.Count(e => e.FunctionId == functionId);
            return Task.FromResult(count);
        }

        /// <inheritdoc/>
        public Task<int> CountByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Counting function execution results by account ID: {AccountId}", accountId);

            // Note: FunctionExecutionResult doesn't have AccountId property, so we can't filter by it
            // This is a placeholder implementation
            return Task.FromResult(_executionResults.Count);
        }

        /// <inheritdoc/>
        public Task<int> CountByStatusAsync(string status)
        {
            _logger.LogInformation("Counting function execution results by status: {Status}", status);

            var count = _executionResults.Values.Count(e => e.Status == status);
            return Task.FromResult(count);
        }

        /// <inheritdoc/>
        public Task<int> CountByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Counting function execution results by time range: {StartTime} to {EndTime}", startTime, endTime);

            var count = _executionResults.Values.Count(e => e.StartTime >= startTime && e.StartTime <= endTime);
            return Task.FromResult(count);
        }

        #endregion
    }
}
