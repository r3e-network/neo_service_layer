using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Api.Models.Responses
{
    /// <summary>
    /// Response model for a function execution
    /// </summary>
    public class FunctionExecutionResponse
    {
        /// <summary>
        /// Gets or sets the execution ID
        /// </summary>
        public Guid ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the execution result
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Gets or sets the execution time
        /// </summary>
        public DateTime ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the execution status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the execution duration in milliseconds
        /// </summary>
        public double? Duration { get; set; }

        /// <summary>
        /// Gets or sets the memory usage in megabytes
        /// </summary>
        public double? MemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets the execution logs
        /// </summary>
        public List<string> Logs { get; set; }

        /// <summary>
        /// Gets or sets the error message if the execution failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the function version that was executed
        /// </summary>
        public string FunctionVersion { get; set; }

        /// <summary>
        /// Gets or sets the event ID if the execution was triggered by an event
        /// </summary>
        public Guid? EventId { get; set; }

        /// <summary>
        /// Gets or sets the event type if the execution was triggered by an event
        /// </summary>
        public string EventType { get; set; }
    }
}
