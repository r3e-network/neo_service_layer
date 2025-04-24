using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Api.Models.Responses
{
    /// <summary>
    /// Response model for a function
    /// </summary>
    public class FunctionResponse
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the function
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the function
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the runtime for the function
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// Gets or sets the handler for the function
        /// </summary>
        public string Handler { get; set; }

        /// <summary>
        /// Gets or sets the entry point for the function
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the function was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the function was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the status of the function
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the version of the function
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the function was last executed
        /// </summary>
        public DateTime? LastExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the number of times the function has been executed
        /// </summary>
        public int ExecutionCount { get; set; }

        /// <summary>
        /// Gets or sets the average execution time in milliseconds
        /// </summary>
        public double AverageExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum memory usage in megabytes
        /// </summary>
        public double MaxMemoryUsage { get; set; }
    }
}
