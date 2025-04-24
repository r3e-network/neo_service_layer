using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function composition execution repository
    /// </summary>
    public interface IFunctionCompositionExecutionRepository
    {
        /// <summary>
        /// Creates a new function composition execution
        /// </summary>
        /// <param name="execution">Function composition execution to create</param>
        /// <returns>The created function composition execution</returns>
        Task<FunctionCompositionExecution> CreateAsync(FunctionCompositionExecution execution);

        /// <summary>
        /// Updates a function composition execution
        /// </summary>
        /// <param name="execution">Function composition execution to update</param>
        /// <returns>The updated function composition execution</returns>
        Task<FunctionCompositionExecution> UpdateAsync(FunctionCompositionExecution execution);

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
        /// <returns>List of function composition executions</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByCompositionIdAsync(Guid compositionId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function composition executions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByAccountIdAsync(Guid accountId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function composition executions by status
        /// </summary>
        /// <param name="status">Status</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByStatusAsync(string status, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function composition executions by composition ID and status
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="status">Status</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByCompositionIdAndStatusAsync(Guid compositionId, string status, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function composition executions by account ID and status
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="status">Status</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetByAccountIdAndStatusAsync(Guid accountId, string status, int limit = 10, int offset = 0);

        /// <summary>
        /// Deletes a function composition execution
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>True if the execution was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function composition executions by composition ID
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <returns>True if the executions were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByCompositionIdAsync(Guid compositionId);

        /// <summary>
        /// Deletes function composition executions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>True if the executions were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Deletes function composition executions by status
        /// </summary>
        /// <param name="status">Status</param>
        /// <param name="olderThan">Delete executions older than this date</param>
        /// <returns>True if the executions were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByStatusAsync(string status, DateTime olderThan);
    }
}
