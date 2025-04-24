using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.PriceFeed.DataProcessors
{
    /// <summary>
    /// Interface for price data processors
    /// </summary>
    public interface IPriceDataProcessor
    {
        /// <summary>
        /// Gets the name of the processor
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Processes a list of prices for the same symbol and returns a single aggregated price
        /// </summary>
        /// <param name="prices">List of prices to process</param>
        /// <returns>Aggregated price</returns>
        Task<Price> ProcessPricesAsync(IEnumerable<Price> prices);

        /// <summary>
        /// Processes a list of prices for multiple symbols and returns a list of aggregated prices
        /// </summary>
        /// <param name="prices">List of prices to process</param>
        /// <returns>List of aggregated prices</returns>
        Task<IEnumerable<Price>> ProcessMultipleSymbolsAsync(IEnumerable<Price> prices);

        /// <summary>
        /// Generates price history from a list of prices
        /// </summary>
        /// <param name="prices">List of prices</param>
        /// <param name="interval">Interval for the price history</param>
        /// <returns>Price history</returns>
        Task<PriceHistory> GeneratePriceHistoryAsync(IEnumerable<Price> prices, string interval);
    }
}
