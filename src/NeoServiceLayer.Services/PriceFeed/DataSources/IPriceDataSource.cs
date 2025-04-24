using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.PriceFeed.DataSources
{
    /// <summary>
    /// Interface for price data sources
    /// </summary>
    public interface IPriceDataSource
    {
        /// <summary>
        /// Gets the name of the data source
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the data source
        /// </summary>
        PriceSourceType Type { get; }

        /// <summary>
        /// Gets the supported assets for this data source
        /// </summary>
        IEnumerable<string> SupportedAssets { get; }

        /// <summary>
        /// Initializes the data source with configuration
        /// </summary>
        /// <param name="config">Configuration for the data source</param>
        void Initialize(PriceSourceConfig config);

        /// <summary>
        /// Fetches prices for all supported assets
        /// </summary>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <returns>List of fetched prices</returns>
        Task<IEnumerable<Price>> FetchPricesAsync(string baseCurrency = "USD");

        /// <summary>
        /// Fetches price for a specific symbol
        /// </summary>
        /// <param name="symbol">Symbol to fetch price for</param>
        /// <param name="baseCurrency">Base currency for the price</param>
        /// <returns>Price for the symbol</returns>
        Task<Price> FetchPriceForSymbolAsync(string symbol, string baseCurrency = "USD");

        /// <summary>
        /// Validates the data source configuration
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        Task<bool> ValidateAsync();
    }
}
