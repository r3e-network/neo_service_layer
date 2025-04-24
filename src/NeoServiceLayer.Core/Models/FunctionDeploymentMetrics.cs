using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents metrics for a function deployment
    /// </summary>
    public class FunctionDeploymentMetrics
    {
        /// <summary>
        /// Gets or sets the number of invocations
        /// </summary>
        public int Invocations { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of successful invocations
        /// </summary>
        public int SuccessfulInvocations { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of failed invocations
        /// </summary>
        public int FailedInvocations { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of throttled invocations
        /// </summary>
        public int ThrottledInvocations { get; set; } = 0;

        /// <summary>
        /// Gets or sets the average execution time in milliseconds
        /// </summary>
        public double AverageExecutionTimeMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum execution time in milliseconds
        /// </summary>
        public double MinExecutionTimeMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum execution time in milliseconds
        /// </summary>
        public double MaxExecutionTimeMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets the average memory usage in megabytes
        /// </summary>
        public double AverageMemoryUsageMb { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum memory usage in megabytes
        /// </summary>
        public double MinMemoryUsageMb { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum memory usage in megabytes
        /// </summary>
        public double MaxMemoryUsageMb { get; set; } = 0;

        /// <summary>
        /// Gets or sets the average CPU usage in percentage
        /// </summary>
        public double AverageCpuUsagePercentage { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum CPU usage in percentage
        /// </summary>
        public double MinCpuUsagePercentage { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum CPU usage in percentage
        /// </summary>
        public double MaxCpuUsagePercentage { get; set; } = 0;

        /// <summary>
        /// Gets or sets the average network usage in kilobytes per second
        /// </summary>
        public double AverageNetworkUsageKbps { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum network usage in kilobytes per second
        /// </summary>
        public double MinNetworkUsageKbps { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum network usage in kilobytes per second
        /// </summary>
        public double MaxNetworkUsageKbps { get; set; } = 0;

        /// <summary>
        /// Gets or sets the average disk usage in kilobytes per second
        /// </summary>
        public double AverageDiskUsageKbps { get; set; } = 0;

        /// <summary>
        /// Gets or sets the minimum disk usage in kilobytes per second
        /// </summary>
        public double MinDiskUsageKbps { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum disk usage in kilobytes per second
        /// </summary>
        public double MaxDiskUsageKbps { get; set; } = 0;

        /// <summary>
        /// Gets or sets the error rate
        /// </summary>
        public double ErrorRate { get; set; } = 0;

        /// <summary>
        /// Gets or sets the throttle rate
        /// </summary>
        public double ThrottleRate { get; set; } = 0;

        /// <summary>
        /// Gets or sets the average concurrent executions
        /// </summary>
        public double AverageConcurrentExecutions { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum concurrent executions
        /// </summary>
        public double MaxConcurrentExecutions { get; set; } = 0;

        /// <summary>
        /// Gets or sets the average instance count
        /// </summary>
        public double AverageInstanceCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum instance count
        /// </summary>
        public double MaxInstanceCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total execution time in milliseconds
        /// </summary>
        public double TotalExecutionTimeMs { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total memory usage in megabytes
        /// </summary>
        public double TotalMemoryUsageMb { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total CPU usage in percentage
        /// </summary>
        public double TotalCpuUsagePercentage { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total network usage in kilobytes
        /// </summary>
        public double TotalNetworkUsageKb { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total disk usage in kilobytes
        /// </summary>
        public double TotalDiskUsageKb { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total cost
        /// </summary>
        public decimal TotalCost { get; set; } = 0;

        /// <summary>
        /// Gets or sets the cost per invocation
        /// </summary>
        public decimal CostPerInvocation { get; set; } = 0;

        /// <summary>
        /// Gets or sets the last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
