using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Services.Analytics.Repositories
{
    /// <summary>
    /// Implementation of the metric repository
    /// </summary>
    public class MetricRepository : IMetricRepository
    {
        private readonly ILogger<MetricRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string MetricsCollectionName = "analytics_metrics";
        private const string DataPointsCollectionName = "analytics_metric_datapoints";

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public MetricRepository(ILogger<MetricRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<Metric> CreateAsync(Metric metric)
        {
            _logger.LogInformation("Creating metric: {Name}", metric.Name);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(MetricsCollectionName))
                {
                    await _databaseService.CreateCollectionAsync(MetricsCollectionName);
                }

                // Set default values
                if (metric.Id == Guid.Empty)
                {
                    metric.Id = Guid.NewGuid();
                }

                metric.CreatedAt = DateTime.UtcNow;
                metric.UpdatedAt = DateTime.UtcNow;

                // Create metric
                return await _databaseService.CreateAsync(MetricsCollectionName, metric);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating metric: {Name}", metric.Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Metric> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting metric: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(MetricsCollectionName))
                {
                    return null;
                }

                // Get metric
                return await _databaseService.GetByIdAsync<Metric, Guid>(MetricsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metric: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Metric>> GetByCategoryAsync(string category)
        {
            _logger.LogInformation("Getting metrics by category: {Category}", category);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(MetricsCollectionName))
                {
                    return Enumerable.Empty<Metric>();
                }

                // Get metrics
                return await _databaseService.GetByFilterAsync<Metric>(MetricsCollectionName, m => m.Category == category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics by category: {Category}", category);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Metric>> GetAllAsync()
        {
            _logger.LogInformation("Getting all metrics");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(MetricsCollectionName))
                {
                    return Enumerable.Empty<Metric>();
                }

                // Get metrics
                return await _databaseService.GetAllAsync<Metric>(MetricsCollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all metrics");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Metric> UpdateAsync(Metric metric)
        {
            _logger.LogInformation("Updating metric: {Id}", metric.Id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(MetricsCollectionName))
                {
                    throw new InvalidOperationException("Metrics collection does not exist");
                }

                // Update timestamp
                metric.UpdatedAt = DateTime.UtcNow;

                // Update metric
                return await _databaseService.UpdateAsync<Metric, Guid>(MetricsCollectionName, metric.Id, metric);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metric: {Id}", metric.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting metric: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(MetricsCollectionName))
                {
                    return false;
                }

                // Delete metric
                return await _databaseService.DeleteAsync<Metric, Guid>(MetricsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting metric: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<MetricDataPoint> CreateDataPointAsync(MetricDataPoint dataPoint)
        {
            _logger.LogInformation("Creating data point for metric: {MetricId}", dataPoint.MetricId);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(DataPointsCollectionName))
                {
                    await _databaseService.CreateCollectionAsync(DataPointsCollectionName);
                }

                // Set default values
                if (dataPoint.Id == Guid.Empty)
                {
                    dataPoint.Id = Guid.NewGuid();
                }

                if (dataPoint.Timestamp == default)
                {
                    dataPoint.Timestamp = DateTime.UtcNow;
                }

                // Create data point
                return await _databaseService.CreateAsync(DataPointsCollectionName, dataPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating data point for metric: {MetricId}", dataPoint.MetricId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MetricDataPoint>> GetDataPointsAsync(Guid metricId, DateTime startTime, DateTime endTime, Dictionary<string, string> dimensions = null)
        {
            _logger.LogInformation("Getting data points for metric: {MetricId}, start: {StartTime}, end: {EndTime}", metricId, startTime, endTime);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DataPointsCollectionName))
                {
                    return Enumerable.Empty<MetricDataPoint>();
                }

                // Get data points
                var dataPoints = await _databaseService.GetByFilterAsync<MetricDataPoint>(
                    DataPointsCollectionName,
                    dp => dp.MetricId == metricId && dp.Timestamp >= startTime && dp.Timestamp <= endTime);

                // Filter by dimensions if provided
                if (dimensions != null && dimensions.Count > 0)
                {
                    dataPoints = dataPoints.Where(dp => dimensions.All(d => dp.Dimensions.ContainsKey(d.Key) && dp.Dimensions[d.Key] == d.Value));
                }

                return dataPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data points for metric: {MetricId}", metricId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<MetricAggregation> GetAggregationAsync(Guid metricId, DateTime startTime, DateTime endTime, AggregationPeriod period, AggregationFunction function, List<string> groupBy = null)
        {
            _logger.LogInformation("Getting aggregation for metric: {MetricId}, start: {StartTime}, end: {EndTime}, period: {Period}, function: {Function}",
                metricId, startTime, endTime, period, function);

            try
            {
                // Get data points
                var dataPoints = await GetDataPointsAsync(metricId, startTime, endTime);
                if (!dataPoints.Any())
                {
                    return new MetricAggregation
                    {
                        MetricId = metricId,
                        StartTime = startTime,
                        EndTime = endTime,
                        Period = period,
                        Function = function,
                        GroupBy = groupBy ?? new List<string>(),
                        Values = new List<AggregatedValue>()
                    };
                }

                // Group by time period
                var periodGroups = GroupByTimePeriod(dataPoints, period);

                // Group by dimensions if specified
                var dimensionGroups = GroupByDimensions(periodGroups, groupBy);

                // Aggregate values
                var aggregatedValues = new List<AggregatedValue>();
                foreach (var group in dimensionGroups)
                {
                    var timestamp = group.Key.Item1;
                    var dimensions = group.Key.Item2;
                    var values = group.Value.Select(dp => dp.Value).ToList();

                    var aggregatedValue = new AggregatedValue
                    {
                        Timestamp = timestamp,
                        Dimensions = dimensions,
                        Count = values.Count,
                        Min = values.Min(),
                        Max = values.Max(),
                        Sum = values.Sum()
                    };

                    // Calculate aggregated value based on function
                    switch (function)
                    {
                        case AggregationFunction.Average:
                            aggregatedValue.Value = values.Average();
                            break;
                        case AggregationFunction.Sum:
                            aggregatedValue.Value = values.Sum();
                            break;
                        case AggregationFunction.Min:
                            aggregatedValue.Value = values.Min();
                            break;
                        case AggregationFunction.Max:
                            aggregatedValue.Value = values.Max();
                            break;
                        case AggregationFunction.Count:
                            aggregatedValue.Value = values.Count;
                            break;
                        case AggregationFunction.Percentile:
                            // Default to 95th percentile
                            aggregatedValue.Value = CalculatePercentile(values, 95);
                            break;
                        default:
                            aggregatedValue.Value = values.Average();
                            break;
                    }

                    aggregatedValues.Add(aggregatedValue);
                }

                return new MetricAggregation
                {
                    MetricId = metricId,
                    StartTime = startTime,
                    EndTime = endTime,
                    Period = period,
                    Function = function,
                    GroupBy = groupBy ?? new List<string>(),
                    Values = aggregatedValues
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting aggregation for metric: {MetricId}", metricId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetDataPointsCountLast24HoursAsync()
        {
            _logger.LogInformation("Getting data points count for last 24 hours");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DataPointsCollectionName))
                {
                    return 0;
                }

                // Get count
                var startTime = DateTime.UtcNow.AddHours(-24);
                var dataPoints = await _databaseService.GetByFilterAsync<MetricDataPoint>(
                    DataPointsCollectionName,
                    dp => dp.Timestamp >= startTime);

                return dataPoints.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data points count for last 24 hours");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetMetricsCountAsync()
        {
            _logger.LogInformation("Getting metrics count");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(MetricsCollectionName))
                {
                    return 0;
                }

                // Get count
                var metrics = await _databaseService.GetAllAsync<Metric>(MetricsCollectionName);
                return metrics.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics count");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetStorageUsageAsync()
        {
            _logger.LogInformation("Getting storage usage");

            try
            {
                // Check if collections exist
                long metricsSize = 0;
                long dataPointsSize = 0;

                if (await _databaseService.CollectionExistsAsync(MetricsCollectionName))
                {
                    metricsSize = await _databaseService.GetCollectionSizeAsync(MetricsCollectionName);
                }

                if (await _databaseService.CollectionExistsAsync(DataPointsCollectionName))
                {
                    dataPointsSize = await _databaseService.GetCollectionSizeAsync(DataPointsCollectionName);
                }

                return metricsSize + dataPointsSize;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage");
                throw;
            }
        }

        /// <summary>
        /// Groups data points by time period
        /// </summary>
        /// <param name="dataPoints">Data points</param>
        /// <param name="period">Aggregation period</param>
        /// <returns>Grouped data points</returns>
        private Dictionary<DateTime, List<MetricDataPoint>> GroupByTimePeriod(IEnumerable<MetricDataPoint> dataPoints, AggregationPeriod period)
        {
            var result = new Dictionary<DateTime, List<MetricDataPoint>>();

            foreach (var dataPoint in dataPoints)
            {
                var periodStart = GetPeriodStart(dataPoint.Timestamp, period);
                if (!result.ContainsKey(periodStart))
                {
                    result[periodStart] = new List<MetricDataPoint>();
                }

                result[periodStart].Add(dataPoint);
            }

            return result;
        }

        /// <summary>
        /// Groups data points by dimensions
        /// </summary>
        /// <param name="periodGroups">Period groups</param>
        /// <param name="groupBy">Dimensions to group by</param>
        /// <returns>Grouped data points</returns>
        private Dictionary<(DateTime, Dictionary<string, string>), List<MetricDataPoint>> GroupByDimensions(
            Dictionary<DateTime, List<MetricDataPoint>> periodGroups,
            List<string> groupBy)
        {
            var result = new Dictionary<(DateTime, Dictionary<string, string>), List<MetricDataPoint>>();

            foreach (var periodGroup in periodGroups)
            {
                var timestamp = periodGroup.Key;
                var dataPoints = periodGroup.Value;

                if (groupBy == null || !groupBy.Any())
                {
                    // No dimension grouping
                    result[(timestamp, new Dictionary<string, string>())] = dataPoints;
                }
                else
                {
                    // Group by dimensions
                    var dimensionGroups = dataPoints.GroupBy(dp => 
                    {
                        var dimensions = new Dictionary<string, string>();
                        foreach (var key in groupBy)
                        {
                            if (dp.Dimensions.TryGetValue(key, out var value))
                            {
                                dimensions[key] = value;
                            }
                        }
                        return dimensions;
                    },
                    (dimensions, points) => new { Dimensions = dimensions, Points = points.ToList() });

                    foreach (var group in dimensionGroups)
                    {
                        result[(timestamp, group.Dimensions)] = group.Points;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the start of a time period
        /// </summary>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="period">Aggregation period</param>
        /// <returns>Period start</returns>
        private DateTime GetPeriodStart(DateTime timestamp, AggregationPeriod period)
        {
            switch (period)
            {
                case AggregationPeriod.Minute:
                    return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, 0, DateTimeKind.Utc);
                case AggregationPeriod.Hour:
                    return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0, DateTimeKind.Utc);
                case AggregationPeriod.Day:
                    return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, 0, 0, 0, DateTimeKind.Utc);
                case AggregationPeriod.Week:
                    var dayOfWeek = timestamp.DayOfWeek;
                    var daysToSubtract = dayOfWeek - DayOfWeek.Sunday;
                    if (daysToSubtract < 0)
                    {
                        daysToSubtract += 7;
                    }
                    return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(-daysToSubtract);
                case AggregationPeriod.Month:
                    return new DateTime(timestamp.Year, timestamp.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                default:
                    return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Calculates a percentile value
        /// </summary>
        /// <param name="values">Values</param>
        /// <param name="percentile">Percentile (0-100)</param>
        /// <returns>Percentile value</returns>
        private double CalculatePercentile(List<double> values, int percentile)
        {
            if (values == null || !values.Any())
            {
                return 0;
            }

            var sortedValues = values.OrderBy(v => v).ToList();
            var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
            return sortedValues[Math.Max(0, index)];
        }
    }
}
