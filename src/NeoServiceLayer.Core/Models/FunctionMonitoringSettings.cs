using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents monitoring settings for a function
    /// </summary>
    public class FunctionMonitoringSettings
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether logging is enabled
        /// </summary>
        public bool LoggingEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the log retention period in days
        /// </summary>
        public int LogRetentionDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets the log level
        /// </summary>
        public string LogLevel { get; set; } = "INFO";

        /// <summary>
        /// Gets or sets a value indicating whether metrics are enabled
        /// </summary>
        public bool MetricsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the metrics retention period in days
        /// </summary>
        public int MetricsRetentionDays { get; set; } = 90;

        /// <summary>
        /// Gets or sets the metrics collection interval in seconds
        /// </summary>
        public int MetricsCollectionIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets a value indicating whether alerts are enabled
        /// </summary>
        public bool AlertsEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the alert recipients
        /// </summary>
        public List<string> AlertRecipients { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the alert thresholds
        /// </summary>
        public Dictionary<string, double> AlertThresholds { get; set; } = new Dictionary<string, double>
        {
            { "ErrorRate", 0.1 },
            { "AverageExecutionTime", 5000 },
            { "MaxExecutionTime", 10000 },
            { "AverageMemoryUsage", 256 },
            { "MaxMemoryUsage", 512 }
        };

        /// <summary>
        /// Gets or sets a value indicating whether tracing is enabled
        /// </summary>
        public bool TracingEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the tracing sample rate
        /// </summary>
        public double TracingSampleRate { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets a value indicating whether profiling is enabled
        /// </summary>
        public bool ProfilingEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the profiling sample rate
        /// </summary>
        public double ProfilingSampleRate { get; set; } = 0.01;

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
