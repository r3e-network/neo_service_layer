using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a log entry from a function execution
    /// </summary>
    public class FunctionLog
    {
        /// <summary>
        /// Gets or sets the unique identifier for the log entry
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the execution ID that generated this log
        /// </summary>
        public Guid ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the log entry
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the log level (e.g., "Info", "Warning", "Error", "Debug")
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        public string Message { get; set; }
    }
}
