using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a source of price data
    /// </summary>
    public class PriceSource
    {
        /// <summary>
        /// Gets or sets the unique identifier for the price source
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the price source
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the price source
        /// </summary>
        public PriceSourceType Type { get; set; }

        /// <summary>
        /// Gets or sets the URL of the price source API
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the API key for the price source (if required)
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the API secret for the price source (if required)
        /// </summary>
        public string ApiSecret { get; set; }

        /// <summary>
        /// Gets or sets the weight of this source in the aggregated price (0-100)
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Gets or sets the status of the price source
        /// </summary>
        public PriceSourceStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the update interval in seconds
        /// </summary>
        public int UpdateIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds for API requests
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the list of supported assets
        /// </summary>
        public List<string> SupportedAssets { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last successful fetch timestamp
        /// </summary>
        public DateTime? LastSuccessfulFetchAt { get; set; }

        /// <summary>
        /// Gets or sets the configuration for parsing the API response
        /// </summary>
        public PriceSourceConfig Config { get; set; }
    }

    /// <summary>
    /// Represents the type of a price source
    /// </summary>
    public enum PriceSourceType
    {
        /// <summary>
        /// Exchange API (e.g., Binance, Coinbase)
        /// </summary>
        Exchange,

        /// <summary>
        /// Aggregator API (e.g., CoinGecko, CoinMarketCap)
        /// </summary>
        Aggregator,

        /// <summary>
        /// Oracle (e.g., Chainlink)
        /// </summary>
        Oracle,

        /// <summary>
        /// On-chain data (e.g., DEX prices)
        /// </summary>
        OnChain,

        /// <summary>
        /// Custom API
        /// </summary>
        Custom
    }

    /// <summary>
    /// Represents the status of a price source
    /// </summary>
    public enum PriceSourceStatus
    {
        /// <summary>
        /// Active and being used for price aggregation
        /// </summary>
        Active,

        /// <summary>
        /// Inactive and not being used for price aggregation
        /// </summary>
        Inactive,

        /// <summary>
        /// Temporarily disabled due to errors
        /// </summary>
        Error,

        /// <summary>
        /// Being tested before activation
        /// </summary>
        Testing
    }

    /// <summary>
    /// Represents the configuration for parsing the API response
    /// </summary>
    public class PriceSourceConfig
    {
        /// <summary>
        /// Gets or sets the JSON path to the price value
        /// </summary>
        public string PriceJsonPath { get; set; }

        /// <summary>
        /// Gets or sets the JSON path to the timestamp
        /// </summary>
        public string TimestampJsonPath { get; set; }

        /// <summary>
        /// Gets or sets the timestamp format
        /// </summary>
        public string TimestampFormat { get; set; }

        /// <summary>
        /// Gets or sets the headers to include in the API request
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the query parameters to include in the API request
        /// </summary>
        public Dictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the HTTP method to use for the API request
        /// </summary>
        public string HttpMethod { get; set; } = "GET";

        /// <summary>
        /// Gets or sets the request body for POST requests
        /// </summary>
        public string RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the content type for the request
        /// </summary>
        public string ContentType { get; set; } = "application/json";
    }
}
