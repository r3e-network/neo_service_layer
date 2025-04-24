using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Services.Analytics.Repositories
{
    /// <summary>
    /// Interface for metric repository
    /// </summary>
    public interface IMetricRepository
    {
        /// <summary>
        /// Creates a new metric
        /// </summary>
        /// <param name="metric">Metric to create</param>
        /// <returns>The created metric</returns>
        Task<Metric> CreateAsync(Metric metric);

        /// <summary>
        /// Gets a metric by ID
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <returns>The metric if found, null otherwise</returns>
        Task<Metric> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets metrics by category
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>List of metrics in the category</returns>
        Task<IEnumerable<Metric>> GetByCategoryAsync(string category);

        /// <summary>
        /// Gets all metrics
        /// </summary>
        /// <returns>List of all metrics</returns>
        Task<IEnumerable<Metric>> GetAllAsync();

        /// <summary>
        /// Updates a metric
        /// </summary>
        /// <param name="metric">Metric to update</param>
        /// <returns>The updated metric</returns>
        Task<Metric> UpdateAsync(Metric metric);

        /// <summary>
        /// Deletes a metric
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <returns>True if the metric was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Creates a metric data point
        /// </summary>
        /// <param name="dataPoint">Data point to create</param>
        /// <returns>The created data point</returns>
        Task<MetricDataPoint> CreateDataPointAsync(MetricDataPoint dataPoint);

        /// <summary>
        /// Gets metric data points
        /// </summary>
        /// <param name="metricId">Metric ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="dimensions">Dimensions to filter by</param>
        /// <returns>List of metric data points</returns>
        Task<IEnumerable<MetricDataPoint>> GetDataPointsAsync(Guid metricId, DateTime startTime, DateTime endTime, Dictionary<string, string> dimensions = null);

        /// <summary>
        /// Gets metric aggregation
        /// </summary>
        /// <param name="metricId">Metric ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="period">Aggregation period</param>
        /// <param name="function">Aggregation function</param>
        /// <param name="groupBy">Dimensions to group by</param>
        /// <returns>Metric aggregation</returns>
        Task<MetricAggregation> GetAggregationAsync(Guid metricId, DateTime startTime, DateTime endTime, AggregationPeriod period, AggregationFunction function, List<string> groupBy = null);

        /// <summary>
        /// Gets the count of data points in the last 24 hours
        /// </summary>
        /// <returns>Count of data points in the last 24 hours</returns>
        Task<int> GetDataPointsCountLast24HoursAsync();

        /// <summary>
        /// Gets the count of metrics
        /// </summary>
        /// <returns>Count of metrics</returns>
        Task<int> GetMetricsCountAsync();

        /// <summary>
        /// Gets the storage usage in bytes
        /// </summary>
        /// <returns>Storage usage in bytes</returns>
        Task<long> GetStorageUsageAsync();
    }
}
