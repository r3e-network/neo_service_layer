using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Common.Repositories;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Repository for function logs
    /// </summary>
    public interface IFunctionLogRepository
    {
        /// <summary>
        /// Creates a new function log
        /// </summary>
        /// <param name="log">Function log</param>
        /// <returns>Created function log</returns>
        Task<FunctionLog> CreateAsync(FunctionLog log);

        /// <summary>
        /// Gets function logs by execution ID
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <returns>Function logs</returns>
        Task<IEnumerable<FunctionLog>> GetByExecutionIdAsync(Guid executionId);

        /// <summary>
        /// Gets function logs by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>Function logs</returns>
        Task<IEnumerable<FunctionLog>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function logs by function ID and time range
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>Function logs</returns>
        Task<IEnumerable<FunctionLog>> GetByFunctionIdAndTimeRangeAsync(Guid functionId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Deletes function logs by execution ID
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteByExecutionIdAsync(Guid executionId);

        /// <summary>
        /// Deletes function logs by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);
    }

    /// <summary>
    /// Repository for function logs
    /// </summary>
    public class FunctionLogRepository : GenericRepository<FunctionLog, Guid>, IFunctionLogRepository
    {
        private const string CollectionName = "function_logs";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionLogRepository"/> class
        /// </summary>
        /// <param name="databaseService">Database service</param>
        /// <param name="logger">Logger</param>
        public FunctionLogRepository(ILogger<FunctionLogRepository> logger, IDatabaseService databaseService)
            : base(logger, databaseService, CollectionName)
        {
        }

        /// <inheritdoc/>
        public async Task<FunctionLog> CreateAsync(FunctionLog log)
        {
            if (log.Id == Guid.Empty)
            {
                log.Id = Guid.NewGuid();
            }

            if (log.Timestamp == default)
            {
                log.Timestamp = DateTime.UtcNow;
            }

            return await base.AddAsync(log);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionLog>> GetByExecutionIdAsync(Guid executionId)
        {
            var allLogs = await base.GetAllAsync();
            return allLogs.Where(log => log.ExecutionId == executionId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionLog>> GetByFunctionIdAsync(Guid functionId)
        {
            var allLogs = await base.GetAllAsync();
            return allLogs.Where(log => log.ExecutionId == functionId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionLog>> GetByFunctionIdAndTimeRangeAsync(Guid functionId, DateTime startTime, DateTime endTime)
        {
            var allLogs = await base.GetAllAsync();
            return allLogs.Where(log =>
                log.ExecutionId == functionId &&
                log.Timestamp >= startTime &&
                log.Timestamp <= endTime);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByExecutionIdAsync(Guid executionId)
        {
            var logs = await GetByExecutionIdAsync(executionId);
            var success = true;

            foreach (var log in logs)
            {
                var result = await base.DeleteAsync(log.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByFunctionIdAsync(Guid functionId)
        {
            var logs = await GetByFunctionIdAsync(functionId);
            var success = true;

            foreach (var log in logs)
            {
                var result = await base.DeleteAsync(log.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }
    }
}
