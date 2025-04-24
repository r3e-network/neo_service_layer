using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Metrics.Repositories
{
    /// <summary>
    /// Interface for metric repository
    /// </summary>
    public interface IMetricRepository
    {
        /// <summary>
        /// Creates a metric
        /// </summary>
        /// <param name="metric">Metric to create</param>
        /// <returns>True if the metric was created successfully, false otherwise</returns>
        Task<bool> CreateMetricAsync(Metric metric);

        /// <summary>
        /// Gets a metric by ID
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <returns>The metric if found, null otherwise</returns>
        Task<Metric> GetMetricAsync(Guid id);

        /// <summary>
        /// Gets metrics by name
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <returns>List of metrics with the specified name</returns>
        Task<IEnumerable<Metric>> GetMetricsByNameAsync(string name);

        /// <summary>
        /// Gets metrics by type
        /// </summary>
        /// <param name="type">Metric type</param>
        /// <returns>List of metrics with the specified type</returns>
        Task<IEnumerable<Metric>> GetMetricsByTypeAsync(string type);

        /// <summary>
        /// Gets metrics by time range
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>List of metrics within the specified time range</returns>
        Task<IEnumerable<Metric>> GetMetricsByTimeRangeAsync(DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets metrics by entity
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="entityType">Entity type</param>
        /// <returns>List of metrics for the specified entity</returns>
        Task<IEnumerable<Metric>> GetMetricsByEntityAsync(Guid entityId, string entityType);

        /// <summary>
        /// Deletes a metric
        /// </summary>
        /// <param name="id">Metric ID</param>
        /// <returns>True if the metric was deleted successfully, false otherwise</returns>
        Task<bool> DeleteMetricAsync(Guid id);

        /// <summary>
        /// Deletes metrics by time range
        /// </summary>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>True if the metrics were deleted successfully, false otherwise</returns>
        Task<bool> DeleteMetricsByTimeRangeAsync(DateTime startTime, DateTime endTime);
    }
}
