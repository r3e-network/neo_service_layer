using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function composition service
    /// </summary>
    public interface IFunctionCompositionService
    {
        /// <summary>
        /// Creates a new function composition
        /// </summary>
        /// <param name="composition">Function composition to create</param>
        /// <returns>The created function composition</returns>
        Task<FunctionComposition> CreateAsync(FunctionComposition composition);

        /// <summary>
        /// Updates a function composition
        /// </summary>
        /// <param name="composition">Function composition to update</param>
        /// <returns>The updated function composition</returns>
        Task<FunctionComposition> UpdateAsync(FunctionComposition composition);

        /// <summary>
        /// Gets a function composition by ID
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The function composition if found, null otherwise</returns>
        Task<FunctionComposition> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function compositions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of function compositions</returns>
        Task<IEnumerable<FunctionComposition>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets function compositions by tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>List of function compositions</returns>
        Task<IEnumerable<FunctionComposition>> GetByTagsAsync(List<string> tags);

        /// <summary>
        /// Deletes a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>True if the composition was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Executes a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <param name="inputParameters">Input parameters</param>
        /// <returns>The execution result</returns>
        Task<FunctionCompositionExecution> ExecuteAsync(Guid id, Dictionary<string, object> inputParameters);

        /// <summary>
        /// Gets a function composition execution by ID
        /// </summary>
        /// <param name="id">Execution ID</param>
        /// <returns>The function composition execution if found, null otherwise</returns>
        Task<FunctionCompositionExecution> GetExecutionByIdAsync(Guid id);

        /// <summary>
        /// Gets function composition executions by composition ID
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetExecutionsByCompositionIdAsync(Guid compositionId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function composition executions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetExecutionsByAccountIdAsync(Guid accountId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function composition executions by status
        /// </summary>
        /// <param name="status">Status</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function composition executions</returns>
        Task<IEnumerable<FunctionCompositionExecution>> GetExecutionsByStatusAsync(string status, int limit = 10, int offset = 0);

        /// <summary>
        /// Validates a function composition
        /// </summary>
        /// <param name="composition">Function composition to validate</param>
        /// <returns>List of validation errors, empty if valid</returns>
        Task<List<string>> ValidateAsync(FunctionComposition composition);

        /// <summary>
        /// Adds a step to a function composition
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="step">Step to add</param>
        /// <returns>The updated function composition</returns>
        Task<FunctionComposition> AddStepAsync(Guid compositionId, FunctionCompositionStep step);

        /// <summary>
        /// Updates a step in a function composition
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="step">Step to update</param>
        /// <returns>The updated function composition</returns>
        Task<FunctionComposition> UpdateStepAsync(Guid compositionId, FunctionCompositionStep step);

        /// <summary>
        /// Removes a step from a function composition
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="stepId">Step ID</param>
        /// <returns>The updated function composition</returns>
        Task<FunctionComposition> RemoveStepAsync(Guid compositionId, Guid stepId);

        /// <summary>
        /// Reorders steps in a function composition
        /// </summary>
        /// <param name="compositionId">Composition ID</param>
        /// <param name="stepIds">Ordered list of step IDs</param>
        /// <returns>The updated function composition</returns>
        Task<FunctionComposition> ReorderStepsAsync(Guid compositionId, List<Guid> stepIds);

        /// <summary>
        /// Cancels a function composition execution
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <returns>The updated function composition execution</returns>
        Task<FunctionCompositionExecution> CancelExecutionAsync(Guid executionId);

        /// <summary>
        /// Gets the logs for a function composition execution
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <returns>The logs</returns>
        Task<List<string>> GetExecutionLogsAsync(Guid executionId);

        /// <summary>
        /// Gets the logs for a function composition step execution
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <param name="stepId">Step ID</param>
        /// <returns>The logs</returns>
        Task<List<string>> GetStepExecutionLogsAsync(Guid executionId, Guid stepId);

        /// <summary>
        /// Gets the input schema for a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The input schema</returns>
        Task<string> GetInputSchemaAsync(Guid id);

        /// <summary>
        /// Gets the output schema for a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The output schema</returns>
        Task<string> GetOutputSchemaAsync(Guid id);

        /// <summary>
        /// Generates the input schema for a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The generated input schema</returns>
        Task<string> GenerateInputSchemaAsync(Guid id);

        /// <summary>
        /// Generates the output schema for a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The generated output schema</returns>
        Task<string> GenerateOutputSchemaAsync(Guid id);
    }
}
