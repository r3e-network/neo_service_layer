using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function execution repository
    /// </summary>
    public interface IFunctionExecutionRepository
    {
        /// <summary>
        /// Creates a new function execution
        /// </summary>
        /// <param name="execution">Execution to create</param>
        /// <returns>The created execution</returns>
        Task<FunctionExecution> CreateAsync(FunctionExecution execution);

        /// <summary>
        /// Gets a function execution by ID
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>The execution if found, null otherwise</returns>
        Task<FunctionExecution> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function executions by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Number of executions to skip</param>
        /// <returns>List of executions for the function</returns>
        Task<IEnumerable<FunctionExecution>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function executions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Number of executions to skip</param>
        /// <returns>List of executions for the account</returns>
        Task<IEnumerable<FunctionExecution>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Updates a function execution
        /// </summary>
        /// <param name="execution">Execution to update</param>
        /// <returns>The updated execution</returns>
        Task<FunctionExecution> UpdateAsync(FunctionExecution execution);

        /// <summary>
        /// Adds a log entry to a function execution
        /// </summary>
        /// <param name="log">Log entry to add</param>
        /// <returns>The added log entry</returns>
        Task<FunctionLog> AddLogAsync(FunctionLog log);

        /// <summary>
        /// Gets logs for a function execution
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <returns>List of logs for the execution</returns>
        Task<IEnumerable<FunctionLog>> GetLogsAsync(Guid executionId);
    }
}
