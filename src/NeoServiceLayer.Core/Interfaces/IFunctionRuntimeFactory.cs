using System.Collections.Generic;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function runtime factory
    /// </summary>
    public interface IFunctionRuntimeFactory
    {
        /// <summary>
        /// Gets a runtime by type
        /// </summary>
        /// <param name="runtimeType">Runtime type</param>
        /// <returns>Function runtime</returns>
        IFunctionRuntime GetRuntime(string runtimeType);

        /// <summary>
        /// Gets a runtime for a function
        /// </summary>
        /// <param name="function">Function</param>
        /// <returns>Function runtime</returns>
        IFunctionRuntime GetRuntimeForFunction(Function function);

        /// <summary>
        /// Gets all runtimes
        /// </summary>
        /// <returns>All function runtimes</returns>
        IEnumerable<IFunctionRuntime> GetAllRuntimes();

        /// <summary>
        /// Shuts down all runtimes
        /// </summary>
        void ShutdownAllRuntimes();
    }
}
