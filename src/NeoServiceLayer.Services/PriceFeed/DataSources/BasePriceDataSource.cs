using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.PriceFeed.DataSources
{
    /// <summary>
    /// Base class for price data sources
    /// </summary>
    public abstract class BasePriceDataSource : IPriceDataSource
    {
        protected readonly ILogger _logger;
        protected readonly HttpClient _httpClient;
        protected PriceSourceConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePriceDataSource"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="httpClient">HTTP client</param>
        protected BasePriceDataSource(ILogger logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract PriceSourceType Type { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<string> SupportedAssets { get; }

        /// <inheritdoc/>
        public virtual void Initialize(PriceSourceConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <inheritdoc/>
        public abstract Task<IEnumerable<Price>> FetchPricesAsync(string baseCurrency = "USD");

        /// <inheritdoc/>
        public abstract Task<Price> FetchPriceForSymbolAsync(string symbol, string baseCurrency = "USD");

        /// <inheritdoc/>
        public virtual async Task<bool> ValidateAsync()
        {
            try
            {
                // Try to fetch a price for a supported asset
                var asset = GetFirstSupportedAsset();
                if (string.IsNullOrEmpty(asset))
                {
                    _logger.LogWarning("No supported assets found for {DataSource}", Name);
                    return false;
                }

                var price = await FetchPriceForSymbolAsync(asset);
                return price != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating data source {DataSource}", Name);
                return false;
            }
        }

        /// <summary>
        /// Gets the first supported asset
        /// </summary>
        /// <returns>First supported asset</returns>
        protected virtual string GetFirstSupportedAsset()
        {
            foreach (var asset in SupportedAssets)
            {
                return asset;
            }

            return null;
        }

        /// <summary>
        /// Creates a price object
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="baseCurrency">Base currency</param>
        /// <param name="value">Price value</param>
        /// <param name="timestamp">Timestamp</param>
        /// <returns>Price object</returns>
        protected virtual Price CreatePrice(string symbol, string baseCurrency, decimal value, DateTime timestamp)
        {
            return new Price
            {
                Id = Guid.NewGuid(),
                Symbol = symbol,
                BaseCurrency = baseCurrency,
                Value = value,
                Timestamp = timestamp,
                ConfidenceScore = 100,
                CreatedAt = DateTime.UtcNow,
                SourcePrices = new List<SourcePrice>
                {
                    new SourcePrice
                    {
                        Id = Guid.NewGuid(),
                        SourceId = Guid.NewGuid(),
                        SourceName = Name,
                        Value = value,
                        Timestamp = timestamp,
                        Weight = 100
                    }
                }
            };
        }
    }
}
