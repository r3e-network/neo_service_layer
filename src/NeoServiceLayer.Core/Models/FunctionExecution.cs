using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a function execution in the Neo Service Layer
    /// </summary>
    public class FunctionExecution
    {
        /// <summary>
        /// Gets or sets the unique identifier for the execution
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the function ID that was executed
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID that triggered the execution
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the input parameters for the execution
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// Gets or sets the output result of the execution
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// Gets or sets the status of the execution (e.g., "Success", "Failed", "Timeout")
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the error message if the execution failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the start time of the execution
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the execution
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the duration of the execution in milliseconds
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Gets or sets the memory usage of the execution in megabytes
        /// </summary>
        public double MemoryUsageMb { get; set; }

        /// <summary>
        /// Gets or sets the CPU usage of the execution in percentage
        /// </summary>
        public double CpuUsagePercent { get; set; }

        /// <summary>
        /// Gets or sets the billing amount for the execution
        /// </summary>
        public decimal BillingAmount { get; set; }

        /// <summary>
        /// Gets or sets the logs generated during the execution
        /// </summary>
        public List<FunctionLog> Logs { get; set; }

        /// <summary>
        /// Gets or sets the blockchain transactions generated during the execution
        /// </summary>
        public List<string> Transactions { get; set; }

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
