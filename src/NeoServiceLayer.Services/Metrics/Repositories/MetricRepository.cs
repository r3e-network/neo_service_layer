using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Metrics.Repositories
{
    /// <summary>
    /// Repository for metrics
    /// </summary>
    public class MetricRepository : IMetricRepository
    {
        private readonly ILogger<MetricRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "metrics";

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public MetricRepository(ILogger<MetricRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<bool> CreateMetricAsync(Metric metric)
        {
            try
            {
                await _storageProvider.CreateAsync(_collectionName, metric);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating metric: {MetricName}", metric.Name);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<Metric> GetMetricAsync(Guid id)
        {
            try
            {
                return await _storageProvider.GetByIdAsync<Metric, Guid>(_collectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metric: {MetricId}", id);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Metric>> GetMetricsByNameAsync(string name)
        {
            try
            {
                var metrics = await _storageProvider.GetByFilterAsync<Metric>(_collectionName, m => m.Name == name);
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics by name: {MetricName}", name);
                return Enumerable.Empty<Metric>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Metric>> GetMetricsByTypeAsync(string type)
        {
            try
            {
                var metrics = await _storageProvider.GetByFilterAsync<Metric>(_collectionName, m => m.Type == type);
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics by type: {MetricType}", type);
                return Enumerable.Empty<Metric>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Metric>> GetMetricsByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            try
            {
                var metrics = await _storageProvider.GetByFilterAsync<Metric>(_collectionName, 
                    m => m.Timestamp >= startTime && m.Timestamp <= endTime);
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics by time range: {StartTime} - {EndTime}", startTime, endTime);
                return Enumerable.Empty<Metric>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Metric>> GetMetricsByEntityAsync(Guid entityId, string entityType)
        {
            try
            {
                var metrics = await _storageProvider.GetByFilterAsync<Metric>(_collectionName, 
                    m => m.EntityId == entityId && m.EntityType == entityType);
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics by entity: {EntityId}, {EntityType}", entityId, entityType);
                return Enumerable.Empty<Metric>();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteMetricAsync(Guid id)
        {
            try
            {
                return await _storageProvider.DeleteAsync<Metric, Guid>(_collectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting metric: {MetricId}", id);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteMetricsByTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            try
            {
                var metrics = await GetMetricsByTimeRangeAsync(startTime, endTime);
                foreach (var metric in metrics)
                {
                    await _storageProvider.DeleteAsync<Metric, Guid>(_collectionName, metric.Id);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting metrics by time range: {StartTime} - {EndTime}", startTime, endTime);
                return false;
            }
        }
    }
}
