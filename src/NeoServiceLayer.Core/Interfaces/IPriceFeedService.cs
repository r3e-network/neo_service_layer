using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for price feed service
    /// </summary>
    public interface IPriceFeedService
    {
        /// <summary>
        /// Fetches price data from all configured sources
        /// </summary>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <returns>List of fetched prices</returns>
        Task<IEnumerable<Price>> FetchPricesAsync(string baseCurrency = "USD");

        /// <summary>
        /// Fetches price data for a specific symbol from all configured sources
        /// </summary>
        /// <param name="symbol">Symbol to fetch price data for</param>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <returns>List of fetched prices for the symbol</returns>
        Task<IEnumerable<Price>> FetchPriceForSymbolAsync(string symbol, string baseCurrency = "USD");

        /// <summary>
        /// Fetches price data from a specific source
        /// </summary>
        /// <param name="sourceId">ID of the source to fetch price data from</param>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <returns>List of fetched prices from the source</returns>
        Task<IEnumerable<Price>> FetchPriceFromSourceAsync(Guid sourceId, string baseCurrency = "USD");

        /// <summary>
        /// Gets the latest price for a symbol
        /// </summary>
        /// <param name="symbol">Symbol to get price for</param>
        /// <param name="baseCurrency">Base currency for the price</param>
        /// <returns>Latest price for the symbol</returns>
        Task<Price> GetLatestPriceAsync(string symbol, string baseCurrency = "USD");

        /// <summary>
        /// Gets the latest prices for all symbols
        /// </summary>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <returns>Dictionary of symbol to latest price</returns>
        Task<Dictionary<string, Price>> GetAllLatestPricesAsync(string baseCurrency = "USD");

        /// <summary>
        /// Gets historical prices for a symbol
        /// </summary>
        /// <param name="symbol">Symbol to get prices for</param>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <param name="startTime">Start time for the historical data</param>
        /// <param name="endTime">End time for the historical data</param>
        /// <returns>List of historical prices for the symbol</returns>
        Task<IEnumerable<Price>> GetHistoricalPricesAsync(string symbol, string baseCurrency, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets price history for a symbol with OHLCV data
        /// </summary>
        /// <param name="symbol">Symbol to get price history for</param>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <param name="interval">Interval for the price data (e.g., "1m", "1h", "1d")</param>
        /// <param name="startTime">Start time for the historical data</param>
        /// <param name="endTime">End time for the historical data</param>
        /// <returns>Price history with OHLCV data</returns>
        Task<PriceHistory> GetPriceHistoryAsync(string symbol, string baseCurrency, string interval, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Submits price data to the Neo N3 oracle contract
        /// </summary>
        /// <param name="price">Price to submit</param>
        /// <returns>Transaction hash</returns>
        Task<string> SubmitToOracleAsync(Price price);

        /// <summary>
        /// Submits multiple prices to the Neo N3 oracle contract
        /// </summary>
        /// <param name="prices">List of prices to submit</param>
        /// <returns>List of transaction hashes</returns>
        Task<IEnumerable<string>> SubmitBatchToOracleAsync(IEnumerable<Price> prices);

        /// <summary>
        /// Gets a price by ID
        /// </summary>
        /// <param name="id">Price ID</param>
        /// <returns>The price if found, null otherwise</returns>
        Task<Price> GetPriceByIdAsync(Guid id);

        /// <summary>
        /// Adds a new price source
        /// </summary>
        /// <param name="source">Price source to add</param>
        /// <returns>The added price source</returns>
        Task<PriceSource> AddSourceAsync(PriceSource source);

        /// <summary>
        /// Updates a price source
        /// </summary>
        /// <param name="source">Price source to update</param>
        /// <returns>The updated price source</returns>
        Task<PriceSource> UpdateSourceAsync(PriceSource source);

        /// <summary>
        /// Gets a price source by ID
        /// </summary>
        /// <param name="id">Price source ID</param>
        /// <returns>The price source if found, null otherwise</returns>
        Task<PriceSource> GetSourceByIdAsync(Guid id);

        /// <summary>
        /// Gets a price source by name
        /// </summary>
        /// <param name="name">Price source name</param>
        /// <returns>The price source if found, null otherwise</returns>
        Task<PriceSource> GetSourceByNameAsync(string name);

        /// <summary>
        /// Gets all price sources
        /// </summary>
        /// <returns>List of all price sources</returns>
        Task<IEnumerable<PriceSource>> GetAllSourcesAsync();

        /// <summary>
        /// Gets active price sources
        /// </summary>
        /// <returns>List of active price sources</returns>
        Task<IEnumerable<PriceSource>> GetActiveSourcesAsync();

        /// <summary>
        /// Removes a price source
        /// </summary>
        /// <param name="id">Price source ID</param>
        /// <returns>True if the source was removed successfully, false otherwise</returns>
        Task<bool> RemoveSourceAsync(Guid id);

        /// <summary>
        /// Gets all supported symbols
        /// </summary>
        /// <returns>List of supported symbols</returns>
        Task<IEnumerable<string>> GetSupportedSymbolsAsync();

        /// <summary>
        /// Gets all supported base currencies
        /// </summary>
        /// <returns>List of supported base currencies</returns>
        Task<IEnumerable<string>> GetSupportedBaseCurrenciesAsync();
    }
}
