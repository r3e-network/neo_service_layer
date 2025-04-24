using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function execution repository
    /// </summary>
    public interface IFunctionExecutionRepository
    {
        /// <summary>
        /// Gets a function execution by ID
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>The function execution if found, null otherwise</returns>
        Task<FunctionExecutionResult?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function executions by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function executions for the function</returns>
        Task<IEnumerable<FunctionExecutionResult>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function executions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function executions for the account</returns>
        Task<IEnumerable<FunctionExecutionResult>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function executions by status
        /// </summary>
        /// <param name="status">Execution status</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function executions with the specified status</returns>
        Task<IEnumerable<FunctionExecutionResult>> GetByStatusAsync(string status, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function executions by time range
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function executions in the specified time range</returns>
        Task<IEnumerable<FunctionExecutionResult>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function execution
        /// </summary>
        /// <param name="execution">Function execution to create</param>
        /// <returns>The created function execution</returns>
        Task<FunctionExecutionResult> CreateAsync(FunctionExecutionResult execution);

        /// <summary>
        /// Updates a function execution
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <param name="execution">Updated function execution</param>
        /// <returns>The updated function execution</returns>
        Task<FunctionExecutionResult> UpdateAsync(Guid id, FunctionExecutionResult execution);

        /// <summary>
        /// Deletes a function execution
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>True if the function execution was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Checks if a function execution exists
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>True if the function execution exists, false otherwise</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Gets all function executions
        /// </summary>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function executions</returns>
        Task<IEnumerable<FunctionExecutionResult>> GetAllAsync(int limit = 100, int offset = 0);

        /// <summary>
        /// Counts function executions
        /// </summary>
        /// <returns>Number of function executions</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Counts function executions by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>Number of function executions for the function</returns>
        Task<int> CountByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Counts function executions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Number of function executions for the account</returns>
        Task<int> CountByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Counts function executions by status
        /// </summary>
        /// <param name="status">Execution status</param>
        /// <returns>Number of function executions with the specified status</returns>
        Task<int> CountByStatusAsync(string status);

        /// <summary>
        /// Counts function executions by time range
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>Number of function executions in the specified time range</returns>
        Task<int> CountByTimeRangeAsync(DateTime startTime, DateTime endTime);
    }
}
