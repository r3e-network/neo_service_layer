using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.PriceFeed.Repositories
{
    /// <summary>
    /// Interface for price history repository
    /// </summary>
    public interface IPriceHistoryRepository
    {
        /// <summary>
        /// Creates a new price history
        /// </summary>
        /// <param name="history">Price history to create</param>
        /// <returns>The created price history</returns>
        Task<PriceHistory> CreateAsync(PriceHistory history);

        /// <summary>
        /// Gets a price history by ID
        /// </summary>
        /// <param name="id">Price history ID</param>
        /// <returns>The price history if found, null otherwise</returns>
        Task<PriceHistory> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets price history for a symbol, base currency, and interval
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <param name="baseCurrency">Base currency</param>
        /// <param name="interval">Interval</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>The price history if found, null otherwise</returns>
        Task<PriceHistory> GetHistoryAsync(string symbol, string baseCurrency, string interval, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets all available intervals for a symbol and base currency
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <param name="baseCurrency">Base currency</param>
        /// <returns>List of available intervals</returns>
        Task<IEnumerable<string>> GetAvailableIntervalsAsync(string symbol, string baseCurrency);

        /// <summary>
        /// Updates a price history
        /// </summary>
        /// <param name="history">Price history to update</param>
        /// <returns>The updated price history</returns>
        Task<PriceHistory> UpdateAsync(PriceHistory history);

        /// <summary>
        /// Adds data points to a price history
        /// </summary>
        /// <param name="id">Price history ID</param>
        /// <param name="dataPoints">Data points to add</param>
        /// <returns>The updated price history</returns>
        Task<PriceHistory> AddDataPointsAsync(Guid id, IEnumerable<PriceDataPoint> dataPoints);
    }
}
