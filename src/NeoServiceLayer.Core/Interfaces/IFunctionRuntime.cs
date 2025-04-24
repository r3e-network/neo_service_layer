using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function runtime
    /// </summary>
    public interface IFunctionRuntime
    {
        /// <summary>
        /// Gets the runtime type
        /// </summary>
        string RuntimeType { get; }

        /// <summary>
        /// Gets the runtime version
        /// </summary>
        string RuntimeVersion { get; }

        /// <summary>
        /// Gets the supported file extensions
        /// </summary>
        IEnumerable<string> SupportedFileExtensions { get; }

        /// <summary>
        /// Initializes the runtime
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InitializeAsync();

        /// <summary>
        /// Executes a function
        /// </summary>
        /// <param name="function">Function</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="context">Execution context</param>
        /// <returns>Execution result</returns>
        Task<FunctionExecutionResult> ExecuteAsync(Function function, Dictionary<string, object> parameters, FunctionExecutionContext context);

        /// <summary>
        /// Validates a function
        /// </summary>
        /// <param name="function">Function</param>
        /// <returns>Validation result</returns>
        Task<FunctionValidationResult> ValidateAsync(Function function);

        /// <summary>
        /// Compiles a function
        /// </summary>
        /// <param name="function">Function</param>
        /// <returns>Compilation result</returns>
        Task<FunctionCompilationResult> CompileAsync(Function function);

        /// <summary>
        /// Gets the runtime status
        /// </summary>
        /// <returns>Runtime status</returns>
        Task<FunctionRuntimeStatus> GetStatusAsync();

        /// <summary>
        /// Shuts down the runtime
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ShutdownAsync();
    }
}
