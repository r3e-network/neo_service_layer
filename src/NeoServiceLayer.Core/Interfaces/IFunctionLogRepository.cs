using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function log repository
    /// </summary>
    public interface IFunctionLogRepository
    {
        /// <summary>
        /// Gets function logs by execution ID
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function logs for the execution</returns>
        Task<IEnumerable<FunctionLog>> GetByExecutionIdAsync(Guid executionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function logs by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function logs for the function</returns>
        Task<IEnumerable<FunctionLog>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function logs by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function logs for the account</returns>
        Task<IEnumerable<FunctionLog>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function logs by level
        /// </summary>
        /// <param name="level">Log level</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function logs with the specified level</returns>
        Task<IEnumerable<FunctionLog>> GetByLevelAsync(string level, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function logs by time range
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function logs in the specified time range</returns>
        Task<IEnumerable<FunctionLog>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function log
        /// </summary>
        /// <param name="log">Function log to create</param>
        /// <returns>The created function log</returns>
        Task<FunctionLog> CreateAsync(FunctionLog log);

        /// <summary>
        /// Creates multiple function logs
        /// </summary>
        /// <param name="logs">Function logs to create</param>
        /// <returns>The created function logs</returns>
        Task<IEnumerable<FunctionLog>> CreateManyAsync(IEnumerable<FunctionLog> logs);

        /// <summary>
        /// Deletes function logs by execution ID
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <returns>True if the function logs were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByExecutionIdAsync(Guid executionId);

        /// <summary>
        /// Deletes function logs by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the function logs were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Deletes function logs by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>True if the function logs were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Deletes function logs by time range
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>True if the function logs were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByTimeRangeAsync(DateTime startTime, DateTime endTime);

        /// <summary>
        /// Counts function logs by execution ID
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <returns>Number of function logs for the execution</returns>
        Task<int> CountByExecutionIdAsync(Guid executionId);

        /// <summary>
        /// Counts function logs by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>Number of function logs for the function</returns>
        Task<int> CountByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Counts function logs by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Number of function logs for the account</returns>
        Task<int> CountByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Counts function logs by level
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Number of function logs with the specified level</returns>
        Task<int> CountByLevelAsync(string level);

        /// <summary>
        /// Counts function logs by time range
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>Number of function logs in the specified time range</returns>
        Task<int> CountByTimeRangeAsync(DateTime startTime, DateTime endTime);
    }
}
