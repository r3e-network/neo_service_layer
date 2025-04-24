using System.Collections.Generic;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for executing a function
    /// </summary>
    public class ExecuteFunctionRequest
    {
        /// <summary>
        /// Parameters for the function execution
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
    }
}
