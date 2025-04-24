using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an execution of a function composition
    /// </summary>
    public class FunctionCompositionExecution
    {
        /// <summary>
        /// Gets or sets the execution ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the composition ID
        /// </summary>
        public Guid CompositionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

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
        /// Gets or sets the step executions
        /// </summary>
        public List<FunctionCompositionStepExecution> StepExecutions { get; set; } = new List<FunctionCompositionStepExecution>();

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
