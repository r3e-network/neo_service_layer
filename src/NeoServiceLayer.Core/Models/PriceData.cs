using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents price data in the Neo Service Layer
    /// </summary>
    public class PriceData
    {
        /// <summary>
        /// Unique identifier for the price data
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Symbol for the price data (e.g., BTC/USD)
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Price value
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Timestamp when the price was recorded
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Source of the price data
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Confidence score for the price data (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }

        /// <summary>
        /// Transaction hash if submitted to the blockchain
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Indicates whether the price data has been submitted to the blockchain
        /// </summary>
        public bool IsSubmitted { get; set; }

        /// <summary>
        /// Date and time when the price data was submitted to the blockchain
        /// </summary>
        public DateTime? SubmittedAt { get; set; }

        /// <summary>
        /// Additional metadata for the price data
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }
    }
}
