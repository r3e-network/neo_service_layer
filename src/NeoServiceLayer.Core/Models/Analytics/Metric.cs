using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models.Analytics
{
    /// <summary>
    /// Represents a metric in the analytics system
    /// </summary>
    public class Metric
    {
        /// <summary>
        /// Gets or sets the unique identifier for the metric
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the metric
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the metric
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category of the metric
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the type of the metric
        /// </summary>
        public MetricType Type { get; set; }

        /// <summary>
        /// Gets or sets the source of the metric
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the metric
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets whether the metric is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Represents a metric data point
    /// </summary>
    public class MetricDataPoint
    {
        /// <summary>
        /// Gets or sets the unique identifier for the data point
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the metric ID
        /// </summary>
        public Guid MetricId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the data point
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the value of the data point
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the dimensions of the data point
        /// </summary>
        public Dictionary<string, string> Dimensions { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents a metric aggregation
    /// </summary>
    public class MetricAggregation
    {
        /// <summary>
        /// Gets or sets the metric ID
        /// </summary>
        public Guid MetricId { get; set; }

        /// <summary>
        /// Gets or sets the start timestamp
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end timestamp
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the aggregation period
        /// </summary>
        public AggregationPeriod Period { get; set; }

        /// <summary>
        /// Gets or sets the aggregation function
        /// </summary>
        public AggregationFunction Function { get; set; }

        /// <summary>
        /// Gets or sets the aggregated values
        /// </summary>
        public List<AggregatedValue> Values { get; set; } = new List<AggregatedValue>();

        /// <summary>
        /// Gets or sets the dimensions to group by
        /// </summary>
        public List<string> GroupBy { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents an aggregated value
    /// </summary>
    public class AggregatedValue
    {
        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the dimensions
        /// </summary>
        public Dictionary<string, string> Dimensions { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the count of data points
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the minimum value
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// Gets or sets the sum of values
        /// </summary>
        public double Sum { get; set; }
    }

    /// <summary>
    /// Represents the type of a metric
    /// </summary>
    public enum MetricType
    {
        /// <summary>
        /// A gauge metric represents a single numerical value that can go up and down
        /// </summary>
        Gauge,

        /// <summary>
        /// A counter metric represents a cumulative value that can only increase
        /// </summary>
        Counter,

        /// <summary>
        /// A timer metric represents a duration of time
        /// </summary>
        Timer,

        /// <summary>
        /// A histogram metric represents a distribution of values
        /// </summary>
        Histogram
    }

    /// <summary>
    /// Represents the aggregation period for metrics
    /// </summary>
    public enum AggregationPeriod
    {
        /// <summary>
        /// Minute aggregation
        /// </summary>
        Minute,

        /// <summary>
        /// Hour aggregation
        /// </summary>
        Hour,

        /// <summary>
        /// Day aggregation
        /// </summary>
        Day,

        /// <summary>
        /// Week aggregation
        /// </summary>
        Week,

        /// <summary>
        /// Month aggregation
        /// </summary>
        Month
    }

    /// <summary>
    /// Represents the aggregation function for metrics
    /// </summary>
    public enum AggregationFunction
    {
        /// <summary>
        /// Average function
        /// </summary>
        Average,

        /// <summary>
        /// Sum function
        /// </summary>
        Sum,

        /// <summary>
        /// Minimum function
        /// </summary>
        Min,

        /// <summary>
        /// Maximum function
        /// </summary>
        Max,

        /// <summary>
        /// Count function
        /// </summary>
        Count,

        /// <summary>
        /// Percentile function
        /// </summary>
        Percentile
    }
}
