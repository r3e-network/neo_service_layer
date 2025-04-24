using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Enclave.Enclave.Execution
{
    /// <summary>
    /// Interface for function runtime
    /// </summary>
    public interface IFunctionRuntime
    {
        /// <summary>
        /// Compiles a function
        /// </summary>
        /// <param name="sourceCode">Source code</param>
        /// <param name="entryPoint">Entry point</param>
        /// <returns>Compiled assembly</returns>
        Task<object> CompileAsync(string sourceCode, string entryPoint);

        /// <summary>
        /// Executes a function
        /// </summary>
        /// <param name="compiledAssembly">Compiled assembly</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="parameters">Function parameters</param>
        /// <param name="context">Execution context</param>
        /// <returns>Function result</returns>
        Task<object> ExecuteAsync(object compiledAssembly, string entryPoint, Dictionary<string, object> parameters, FunctionExecutionContext context);

        /// <summary>
        /// Executes a function for an event
        /// </summary>
        /// <param name="compiledAssembly">Compiled assembly</param>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="eventData">Event data</param>
        /// <param name="context">Execution context</param>
        /// <returns>Function result</returns>
        Task<object> ExecuteForEventAsync(object compiledAssembly, string entryPoint, Event eventData, FunctionExecutionContext context);
    }
}
