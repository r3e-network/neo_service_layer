using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Storage.Monitoring
{
    /// <summary>
    /// Collects metrics for database operations
    /// </summary>
    public class DatabaseMetricsCollector : IDisposable
    {
        private readonly ILogger<DatabaseMetricsCollector> _logger;
        private readonly ConcurrentDictionary<string, OperationMetrics> _metrics = new ConcurrentDictionary<string, OperationMetrics>();
        private readonly Timer _reportingTimer;
        private readonly TimeSpan _reportingInterval;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseMetricsCollector"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="reportingIntervalSeconds">Reporting interval in seconds</param>
        public DatabaseMetricsCollector(ILogger<DatabaseMetricsCollector> logger, int reportingIntervalSeconds = 60)
        {
            _logger = logger;
            _reportingInterval = TimeSpan.FromSeconds(reportingIntervalSeconds);
            _reportingTimer = new Timer(ReportMetrics, null, _reportingInterval, _reportingInterval);
        }

        /// <summary>
        /// Records the execution time of a database operation
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <param name="operationName">Operation name</param>
        /// <param name="collectionName">Collection name</param>
        /// <param name="executionTime">Execution time in milliseconds</param>
        /// <param name="success">Whether the operation was successful</param>
        public void RecordOperation(string providerName, string operationName, string collectionName, long executionTime, bool success)
        {
            var key = $"{providerName}:{operationName}:{collectionName}";
            var metrics = _metrics.GetOrAdd(key, _ => new OperationMetrics
            {
                ProviderName = providerName,
                OperationName = operationName,
                CollectionName = collectionName
            });

            metrics.TotalOperations++;
            metrics.TotalExecutionTime += executionTime;

            if (executionTime > metrics.MaxExecutionTime)
            {
                metrics.MaxExecutionTime = executionTime;
            }

            if (executionTime < metrics.MinExecutionTime || metrics.MinExecutionTime == 0)
            {
                metrics.MinExecutionTime = executionTime;
            }

            if (!success)
            {
                metrics.FailedOperations++;
            }
        }

        /// <summary>
        /// Gets all collected metrics
        /// </summary>
        /// <returns>Collection of operation metrics</returns>
        public IEnumerable<OperationMetrics> GetAllMetrics()
        {
            return _metrics.Values;
        }

        /// <summary>
        /// Clears all collected metrics
        /// </summary>
        public void ClearMetrics()
        {
            _metrics.Clear();
        }

        /// <summary>
        /// Reports metrics to the logger
        /// </summary>
        /// <param name="state">Timer state</param>
        private void ReportMetrics(object state)
        {
            try
            {
                _logger.LogInformation("Database metrics report:");

                foreach (var metrics in _metrics.Values)
                {
                    if (metrics.TotalOperations > 0)
                    {
                        var avgExecutionTime = metrics.TotalOperations > 0 ? metrics.TotalExecutionTime / metrics.TotalOperations : 0;
                        var successRate = metrics.TotalOperations > 0 ? 100.0 * (metrics.TotalOperations - metrics.FailedOperations) / metrics.TotalOperations : 0;

                        _logger.LogInformation(
                            "Provider: {ProviderName}, Operation: {OperationName}, Collection: {CollectionName}, " +
                            "Total: {TotalOperations}, Failed: {FailedOperations}, Success Rate: {SuccessRate:F2}%, " +
                            "Avg Time: {AvgExecutionTime}ms, Min Time: {MinExecutionTime}ms, Max Time: {MaxExecutionTime}ms",
                            metrics.ProviderName,
                            metrics.OperationName,
                            metrics.CollectionName,
                            metrics.TotalOperations,
                            metrics.FailedOperations,
                            successRate,
                            avgExecutionTime,
                            metrics.MinExecutionTime,
                            metrics.MaxExecutionTime);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting database metrics");
            }
        }

        /// <summary>
        /// Disposes the metrics collector
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the metrics collector
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _reportingTimer?.Dispose();
                }

                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Metrics for a database operation
    /// </summary>
    public class OperationMetrics
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the operation name
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// Gets or sets the collection name
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// Gets or sets the total number of operations
        /// </summary>
        public long TotalOperations { get; set; }

        /// <summary>
        /// Gets or sets the total execution time in milliseconds
        /// </summary>
        public long TotalExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the minimum execution time in milliseconds
        /// </summary>
        public long MinExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum execution time in milliseconds
        /// </summary>
        public long MaxExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the number of failed operations
        /// </summary>
        public long FailedOperations { get; set; }
    }
}
