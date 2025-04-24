using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for metrics collection service
    /// </summary>
    public interface IMetricsService
    {
        /// <summary>
        /// Records a function execution metric
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="executionTime">Execution time in milliseconds</param>
        /// <param name="memoryUsage">Memory usage in bytes</param>
        /// <param name="success">Indicates whether the execution was successful</param>
        /// <param name="errorMessage">Error message if the execution failed</param>
        /// <returns>True if the metric was recorded successfully, false otherwise</returns>
        Task<bool> RecordFunctionExecutionAsync(Guid functionId, long executionTime, long memoryUsage, bool success, string errorMessage = null);

        /// <summary>
        /// Records a storage operation metric
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Optional function ID</param>
        /// <param name="operationType">Type of storage operation (Read, Write, Delete)</param>
        /// <param name="size">Size of the data in bytes</param>
        /// <returns>True if the metric was recorded successfully, false otherwise</returns>
        Task<bool> RecordStorageOperationAsync(Guid accountId, Guid? functionId, string operationType, long size);

        /// <summary>
        /// Records a blockchain operation metric
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Optional function ID</param>
        /// <param name="operationType">Type of blockchain operation (Read, Write)</param>
        /// <param name="transactionHash">Transaction hash if applicable</param>
        /// <returns>True if the metric was recorded successfully, false otherwise</returns>
        Task<bool> RecordBlockchainOperationAsync(Guid accountId, Guid? functionId, string operationType, string transactionHash = null);

        /// <summary>
        /// Records a custom metric
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="value">Metric value</param>
        /// <param name="tags">Optional tags for the metric</param>
        /// <returns>True if the metric was recorded successfully, false otherwise</returns>
        Task<bool> RecordCustomMetricAsync(string name, double value, Dictionary<string, string> tags = null);

        /// <summary>
        /// Gets function execution metrics for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>List of function execution metrics</returns>
        Task<IEnumerable<object>> GetFunctionExecutionMetricsAsync(Guid functionId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets function execution metrics for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>List of function execution metrics</returns>
        Task<IEnumerable<object>> GetFunctionExecutionMetricsForAccountAsync(Guid accountId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets storage operation metrics for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>List of storage operation metrics</returns>
        Task<IEnumerable<object>> GetStorageOperationMetricsAsync(Guid accountId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets blockchain operation metrics for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>List of blockchain operation metrics</returns>
        Task<IEnumerable<object>> GetBlockchainOperationMetricsAsync(Guid accountId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets custom metrics
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <param name="tags">Optional tags to filter by</param>
        /// <returns>List of custom metrics</returns>
        Task<IEnumerable<object>> GetCustomMetricsAsync(string name, DateTime startTime, DateTime endTime, Dictionary<string, string> tags = null);

        /// <summary>
        /// Gets system metrics
        /// </summary>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>List of system metrics</returns>
        Task<IEnumerable<object>> GetSystemMetricsAsync(DateTime startTime, DateTime endTime);

        /// <summary>
        /// Creates a dashboard
        /// </summary>
        /// <param name="name">Dashboard name</param>
        /// <param name="description">Dashboard description</param>
        /// <param name="metrics">List of metrics to include in the dashboard</param>
        /// <returns>The created dashboard</returns>
        Task<object> CreateDashboardAsync(string name, string description, List<string> metrics);

        /// <summary>
        /// Gets a dashboard
        /// </summary>
        /// <param name="name">Dashboard name</param>
        /// <returns>The dashboard if found, null otherwise</returns>
        Task<object> GetDashboardAsync(string name);

        /// <summary>
        /// Creates an alert
        /// </summary>
        /// <param name="name">Alert name</param>
        /// <param name="description">Alert description</param>
        /// <param name="metricName">Metric name to monitor</param>
        /// <param name="threshold">Threshold value</param>
        /// <param name="operator">Comparison operator (>, <, =, >=, <=)</param>
        /// <param name="duration">Duration in seconds for the condition to be true before alerting</param>
        /// <param name="notificationChannels">List of notification channels</param>
        /// <returns>The created alert</returns>
        Task<object> CreateAlertAsync(string name, string description, string metricName, double threshold, string @operator, int duration, List<string> notificationChannels);

        /// <summary>
        /// Gets an alert
        /// </summary>
        /// <param name="name">Alert name</param>
        /// <returns>The alert if found, null otherwise</returns>
        Task<object> GetAlertAsync(string name);
    }
}
