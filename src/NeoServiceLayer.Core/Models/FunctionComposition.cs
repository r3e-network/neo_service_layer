using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a composition of functions
    /// </summary>
    public class FunctionComposition
    {
        /// <summary>
        /// Gets or sets the composition ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the composition
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the composition
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the steps in the composition
        /// </summary>
        public List<FunctionCompositionStep> Steps { get; set; } = new List<FunctionCompositionStep>();

        /// <summary>
        /// Gets or sets the input schema
        /// </summary>
        public string InputSchema { get; set; }

        /// <summary>
        /// Gets or sets the output schema
        /// </summary>
        public string OutputSchema { get; set; }

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the created by user ID
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the updated by user ID
        /// </summary>
        public Guid UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the tags for the composition
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether the composition is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum execution time in milliseconds
        /// </summary>
        public int MaxExecutionTime { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the environment variables
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the error handling strategy
        /// </summary>
        public string ErrorHandlingStrategy { get; set; } = "stop";

        /// <summary>
        /// Gets or sets the execution mode
        /// </summary>
        public string ExecutionMode { get; set; } = "sequential";

        /// <summary>
        /// Gets or sets the version of the composition
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the retry policy
        /// </summary>
        public FunctionRetryPolicy RetryPolicy { get; set; } = new FunctionRetryPolicy();
    }
}
