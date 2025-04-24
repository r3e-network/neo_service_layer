using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function composition execution repository
    /// </summary>
    public interface IFunctionCompositionExecutionRepository
    {
        /// <summary>
        /// Gets a function composition execution by ID
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>The function composition execution if found, null otherwise</returns>
        Task<FunctionCompositionExecution> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function composition executions by composition ID
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions for the specified composition</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByCompositionIdAsync(Guid compositionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function composition executions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions for the specified account</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function composition executions by status
        /// </summary>
        /// <param name="status">Execution status</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions with the specified status</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByStatusAsync(string status, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function composition executions by time range
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions in the specified time range</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function composition executions by composition ID and status
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="status">Execution status</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions for the specified composition with the specified status</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByCompositionIdAndStatusAsync(Guid compositionId, string status, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function composition executions by composition ID and time range
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions for the specified composition in the specified time range</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByCompositionIdAndTimeRangeAsync(Guid compositionId, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function composition execution
        /// </summary>
        /// <param name="execution">Execution to create</param>
        /// <returns>The created function composition execution</returns>
        Task<FunctionCompositionExecution> CreateAsync(FunctionCompositionExecution execution);

        /// <summary>
        /// Updates a function composition execution
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <param name="execution">Updated execution</param>
        /// <returns>The updated function composition execution</returns>
        Task<FunctionCompositionExecution> UpdateAsync(Guid id, FunctionCompositionExecution execution);

        /// <summary>
        /// Deletes a function composition execution
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>True if the execution was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function composition executions
        /// </summary>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function composition executions</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetAllAsync(int limit = 100, int offset = 0);
    }
}
