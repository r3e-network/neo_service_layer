using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.PriceFeed.Repositories
{
    /// <summary>
    /// Interface for price repository
    /// </summary>
    public interface IPriceRepository
    {
        /// <summary>
        /// Creates a new price
        /// </summary>
        /// <param name="price">Price to create</param>
        /// <returns>The created price</returns>
        Task<Price> CreateAsync(Price price);

        /// <summary>
        /// Gets a price by ID
        /// </summary>
        /// <param name="id">Price ID</param>
        /// <returns>The price if found, null otherwise</returns>
        Task<Price> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets the latest price for a symbol and base currency
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <param name="baseCurrency">Base currency</param>
        /// <returns>The latest price if found, null otherwise</returns>
        Task<Price> GetLatestPriceAsync(string symbol, string baseCurrency);

        /// <summary>
        /// Gets prices for a symbol and base currency within a time range
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <param name="baseCurrency">Base currency</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>List of prices within the time range</returns>
        Task<IEnumerable<Price>> GetPricesInRangeAsync(string symbol, string baseCurrency, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets the latest prices for all symbols
        /// </summary>
        /// <param name="baseCurrency">Base currency</param>
        /// <returns>Dictionary of symbol to latest price</returns>
        Task<Dictionary<string, Price>> GetAllLatestPricesAsync(string baseCurrency);

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
