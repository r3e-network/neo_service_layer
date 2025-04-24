using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents metrics for a function
    /// </summary>
    public class FunctionMetrics
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the time period for the metrics
        /// </summary>
        public string TimePeriod { get; set; }

        /// <summary>
        /// Gets or sets the start time for the metrics
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for the metrics
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the number of invocations
        /// </summary>
        public int Invocations { get; set; }

        /// <summary>
        /// Gets or sets the number of successful invocations
        /// </summary>
        public int SuccessfulInvocations { get; set; }

        /// <summary>
        /// Gets or sets the number of failed invocations
        /// </summary>
        public int FailedInvocations { get; set; }

        /// <summary>
        /// Gets or sets the number of throttled invocations
        /// </summary>
        public int ThrottledInvocations { get; set; }

        /// <summary>
        /// Gets or sets the average execution time in milliseconds
        /// </summary>
        public double AverageExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the minimum execution time in milliseconds
        /// </summary>
        public double MinExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum execution time in milliseconds
        /// </summary>
        public double MaxExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the average memory usage in megabytes
        /// </summary>
        public double AverageMemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets the minimum memory usage in megabytes
        /// </summary>
        public double MinMemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets the maximum memory usage in megabytes
        /// </summary>
        public double MaxMemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets the execution time percentiles
        /// </summary>
        public Dictionary<string, double> ExecutionTimePercentiles { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets or sets the memory usage percentiles
        /// </summary>
        public Dictionary<string, double> MemoryUsagePercentiles { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets or sets the error counts by error type
        /// </summary>
        public Dictionary<string, int> ErrorCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the invocation counts by status
        /// </summary>
        public Dictionary<string, int> InvocationCountsByStatus { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the invocation counts by hour
        /// </summary>
        public Dictionary<int, int> InvocationCountsByHour { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Gets or sets the invocation counts by day
        /// </summary>
        public Dictionary<string, int> InvocationCountsByDay { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the total execution time in milliseconds
        /// </summary>
        public double TotalExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the total memory usage in megabytes
        /// </summary>
        public double TotalMemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets the total cost
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the cost per invocation
        /// </summary>
        public decimal CostPerInvocation { get; set; }

        /// <summary>
        /// Gets or sets the last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
