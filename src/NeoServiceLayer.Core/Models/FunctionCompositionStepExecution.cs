using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an execution of a function composition step
    /// </summary>
    public class FunctionCompositionStepExecution
    {
        /// <summary>
        /// Gets or sets the execution ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the step ID
        /// </summary>
        public Guid StepId { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the function version
        /// </summary>
        public string FunctionVersion { get; set; }

        /// <summary>
        /// Gets or sets the status of the execution
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the execution time in milliseconds
        /// </summary>
        public double? ExecutionTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the input parameters
        /// </summary>
        public Dictionary<string, object> InputParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the output result
        /// </summary>
        public object OutputResult { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the stack trace
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the retry count
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the logs
        /// </summary>
        public List<string> Logs { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
