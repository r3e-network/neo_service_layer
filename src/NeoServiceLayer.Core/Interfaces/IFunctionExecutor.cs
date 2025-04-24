using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function executor
    /// </summary>
    public interface IFunctionExecutor
    {
        /// <summary>
        /// Executes a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="input">Function input</param>
        /// <param name="context">Execution context</param>
        /// <returns>Function execution result</returns>
        Task<FunctionExecutionResult> ExecuteAsync(Guid functionId, object input, FunctionExecutionContext context);

        /// <summary>
        /// Executes a function by name
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionName">Function name</param>
        /// <param name="input">Function input</param>
        /// <param name="context">Execution context</param>
        /// <returns>Function execution result</returns>
        Task<FunctionExecutionResult> ExecuteByNameAsync(Guid accountId, string functionName, object input, FunctionExecutionContext context);

        /// <summary>
        /// Executes a function with source code
        /// </summary>
        /// <param name="source">Function source code</param>
        /// <param name="runtime">Function runtime</param>
        /// <param name="handler">Function handler</param>
        /// <param name="input">Function input</param>
        /// <param name="context">Execution context</param>
        /// <returns>Function execution result</returns>
        Task<FunctionExecutionResult> ExecuteSourceAsync(string source, string runtime, string handler, object input, FunctionExecutionContext context);

        /// <summary>
        /// Validates a function
        /// </summary>
        /// <param name="source">Function source code</param>
        /// <param name="runtime">Function runtime</param>
        /// <param name="handler">Function handler</param>
        /// <returns>Function validation result</returns>
        Task<FunctionValidationResult> ValidateAsync(string source, string runtime, string handler);

        /// <summary>
        /// Gets the supported runtimes
        /// </summary>
        /// <returns>List of supported runtimes</returns>
        Task<IEnumerable<string>> GetSupportedRuntimesAsync();

        /// <summary>
        /// Gets the runtime details
        /// </summary>
        /// <param name="runtime">Runtime name</param>
        /// <returns>Runtime details</returns>
        Task<FunctionRuntimeDetails> GetRuntimeDetailsAsync(string runtime);

        /// <summary>
        /// Initializes the executor
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InitializeAsync();

        /// <summary>
        /// Shuts down the executor
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ShutdownAsync();
    }
}
