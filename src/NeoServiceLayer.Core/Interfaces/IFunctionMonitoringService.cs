using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function monitoring service
    /// </summary>
    public interface IFunctionMonitoringService
    {
        /// <summary>
        /// Records a function execution
        /// </summary>
        /// <param name="execution">Function execution to record</param>
        /// <returns>The recorded function execution</returns>
        Task<FunctionExecution> RecordExecutionAsync(FunctionExecution execution);

        /// <summary>
        /// Records a function log
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="executionId">Execution ID</param>
        /// <param name="message">Log message</param>
        /// <param name="level">Log level</param>
        /// <param name="timestamp">Log timestamp</param>
        /// <returns>The recorded function log</returns>
        Task<FunctionLog> RecordLogAsync(Guid functionId, Guid executionId, string message, string level, DateTime timestamp);

        /// <summary>
        /// Gets function executions by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Number of executions to skip</param>
        /// <returns>List of function executions</returns>
        Task<IEnumerable<FunctionExecution>> GetExecutionsByFunctionIdAsync(Guid functionId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function executions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of executions to return</param>
        /// <param name="offset">Number of executions to skip</param>
        /// <returns>List of function executions</returns>
        Task<IEnumerable<FunctionExecution>> GetExecutionsByAccountIdAsync(Guid accountId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function execution by ID
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <returns>The function execution if found, null otherwise</returns>
        Task<FunctionExecution> GetExecutionByIdAsync(Guid executionId);

        /// <summary>
        /// Gets function logs by execution ID
        /// </summary>
        /// <param name="executionId">Execution ID</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Number of logs to skip</param>
        /// <returns>List of function logs</returns>
        Task<IEnumerable<FunctionLog>> GetLogsByExecutionIdAsync(Guid executionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function metrics by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>Function metrics</returns>
        Task<FunctionMetrics> GetMetricsByFunctionIdAsync(Guid functionId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets function metrics by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>List of function metrics</returns>
        Task<IEnumerable<FunctionMetrics>> GetMetricsByAccountIdAsync(Guid accountId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Updates function metrics
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="executionTime">Execution time in milliseconds</param>
        /// <param name="memoryUsage">Memory usage in megabytes</param>
        /// <returns>Updated function metrics</returns>
        Task<FunctionMetrics> UpdateMetricsAsync(Guid functionId, double executionTime, double memoryUsage);

        /// <summary>
        /// Sets up monitoring for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="settings">Monitoring settings</param>
        /// <returns>True if monitoring was set up successfully, false otherwise</returns>
        Task<bool> SetupMonitoringAsync(Guid functionId, FunctionMonitoringSettings settings);

        /// <summary>
        /// Gets monitoring settings for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>Monitoring settings for the function</returns>
        Task<FunctionMonitoringSettings> GetMonitoringSettingsAsync(Guid functionId);

        /// <summary>
        /// Updates monitoring settings for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="settings">Monitoring settings</param>
        /// <returns>Updated monitoring settings</returns>
        Task<FunctionMonitoringSettings> UpdateMonitoringSettingsAsync(Guid functionId, FunctionMonitoringSettings settings);
    }
}
