using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a price data point for a specific asset
    /// </summary>
    public class Price
    {
        /// <summary>
        /// Gets or sets the unique identifier for the price
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
        /// Gets or sets the price value
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the price was recorded
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the list of source prices used to calculate this price
        /// </summary>
        public List<SourcePrice> SourcePrices { get; set; } = new List<SourcePrice>();

        /// <summary>
        /// Gets or sets the confidence score (0-100) indicating the reliability of the price
        /// </summary>
        public int ConfidenceScore { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the signature of the price data (signed by the enclave)
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Gets or sets the source of the price data
        /// </summary>
        public string Source { get; set; }
    }

    /// <summary>
    /// Represents a price from a specific source
    /// </summary>
    public class SourcePrice
    {
        /// <summary>
        /// Gets or sets the unique identifier for the source price
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the source identifier
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// Gets or sets the source name
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// Gets or sets the price value from this source
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the price was fetched from the source
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the weight of this source in the aggregated price (0-100)
        /// </summary>
        public int Weight { get; set; }
    }
}
