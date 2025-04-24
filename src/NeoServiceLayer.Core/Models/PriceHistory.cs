using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents historical price data for a specific asset
    /// </summary>
    public class PriceHistory
    {
        /// <summary>
        /// Gets or sets the unique identifier for the price history
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the asset symbol (e.g., "BTC", "NEO", "GAS")
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the base currency (e.g., "USD", "EUR")
        /// </summary>
        public string BaseCurrency { get; set; }

        /// <summary>
        /// Gets or sets the interval for the price data (e.g., "1m", "1h", "1d")
        /// </summary>
        public string Interval { get; set; }

        /// <summary>
        /// Gets or sets the start time of the history
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the history
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the list of price data points
        /// </summary>
        public List<PriceDataPoint> DataPoints { get; set; } = new List<PriceDataPoint>();

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents a single price data point in a price history
    /// </summary>
    public class PriceDataPoint
    {
        /// <summary>
        /// Gets or sets the timestamp of the data point
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the open price
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// Gets or sets the high price
        /// </summary>
        public decimal High { get; set; }

        /// <summary>
        /// Gets or sets the low price
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// Gets or sets the close price
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// Gets or sets the volume
        /// </summary>
        public decimal Volume { get; set; }
    }
}
