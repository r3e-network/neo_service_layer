using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Metric model
    /// </summary>
    public class Metric
    {
        /// <summary>
        /// Gets or sets the metric ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the metric name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the metric type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the metric value
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the metric timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the entity ID
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity type
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the metric tags
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the metric dimensions
        /// </summary>
        public Dictionary<string, string> Dimensions { get; set; }

        /// <summary>
        /// Gets or sets the metric unit
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Gets or sets the metric description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the metric source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the metric namespace
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the metric resolution
        /// </summary>
        public string Resolution { get; set; }

        /// <summary>
        /// Gets or sets the metric statistics
        /// </summary>
        public MetricStatistics Statistics { get; set; }
    }

    /// <summary>
    /// Metric statistics
    /// </summary>
    public class MetricStatistics
    {
        /// <summary>
        /// Gets or sets the minimum value
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// Gets or sets the average value
        /// </summary>
        public double Average { get; set; }

        /// <summary>
        /// Gets or sets the sum
        /// </summary>
        public double Sum { get; set; }

        /// <summary>
        /// Gets or sets the count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation
        /// </summary>
        public double StandardDeviation { get; set; }

        /// <summary>
        /// Gets or sets the percentiles
        /// </summary>
        public Dictionary<string, double> Percentiles { get; set; }
    }
}
