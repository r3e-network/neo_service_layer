using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Api.Models.Requests
{
    /// <summary>
    /// Request model for executing a function
    /// </summary>
    public class ExecuteFunctionRequest
    {
        /// <summary>
        /// Gets or sets the parameters for the function execution
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the version of the function to execute
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to execute the function asynchronously
        /// </summary>
        public bool Async { get; set; } = false;

        /// <summary>
        /// Gets or sets the callback URL for asynchronous execution
        /// </summary>
        public string CallbackUrl { get; set; }
    }
}
