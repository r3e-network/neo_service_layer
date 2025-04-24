using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Storage.Repositories;

namespace NeoServiceLayer.Services.PriceFeed.Repositories
{
    /// <summary>
    /// Implementation of the price history repository
    /// </summary>
    public class PriceHistoryRepository : BaseRepository<PriceHistory, Guid>, IPriceHistoryRepository
    {
        private readonly ILogger<PriceHistoryRepository> _logger;
        private readonly IStorageService _storageService;
        private readonly Guid _systemAccountId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceHistoryRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageService">Storage service</param>
        public PriceHistoryRepository(
            ILogger<PriceHistoryRepository> logger,
            IStorageService storageService)
            : base(logger, storageService, Guid.Parse("00000000-0000-0000-0000-000000000001"), "priceHistories")
        {
            _logger = logger;
            _storageService = storageService;
            _systemAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        /// <inheritdoc/>
        protected override Guid GetId(PriceHistory entity)
        {
            return entity.Id;
        }

        /// <inheritdoc/>
        protected override void SetId(PriceHistory entity, Guid id)
        {
            entity.Id = id;
        }

        /// <inheritdoc/>
        public override async Task<PriceHistory> CreateAsync(PriceHistory history)
        {
            _logger.LogInformation("Creating price history: Symbol: {Symbol}, BaseCurrency: {BaseCurrency}, Interval: {Interval}",
                history.Symbol, history.BaseCurrency, history.Interval);

            if (history.Id == Guid.Empty)
            {
                history.Id = Guid.NewGuid();
            }

            history.CreatedAt = DateTime.UtcNow;

            return await base.CreateAsync(history);
        }

        /// <inheritdoc/>
        public async Task<PriceHistory> GetHistoryAsync(string symbol, string baseCurrency, string interval, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting price history for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}, Interval: {Interval}, StartTime: {StartTime}, EndTime: {EndTime}",
                symbol, baseCurrency, interval, startTime, endTime);

            try
            {
                var histories = await GetAllAsync();
                var matchingHistories = histories
                    .Where(h => h.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) &&
                           h.BaseCurrency.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase) &&
                           h.Interval.Equals(interval, StringComparison.OrdinalIgnoreCase) &&
                           h.StartTime <= endTime && h.EndTime >= startTime)
                    .ToList();

                if (matchingHistories.Count == 0)
                {
                    return null;
                }

                // If multiple histories match, merge them
                if (matchingHistories.Count > 1)
                {
                    return MergeHistories(matchingHistories, startTime, endTime);
                }

                var history = matchingHistories[0];

                // Filter data points to the requested time range
                var filteredDataPoints = history.DataPoints
                    .Where(dp => dp.Timestamp >= startTime && dp.Timestamp <= endTime)
                    .OrderBy(dp => dp.Timestamp)
                    .ToList();

                var filteredHistory = new PriceHistory
                {
                    Id = history.Id,
                    Symbol = history.Symbol,
                    BaseCurrency = history.BaseCurrency,
                    Interval = history.Interval,
                    StartTime = startTime,
                    EndTime = endTime,
                    DataPoints = filteredDataPoints,
                    CreatedAt = history.CreatedAt
                };

                return filteredHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price history for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}, Interval: {Interval}",
                    symbol, baseCurrency, interval);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetAvailableIntervalsAsync(string symbol, string baseCurrency)
        {
            _logger.LogInformation("Getting available intervals for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}",
                symbol, baseCurrency);

            try
            {
                var histories = await GetAllAsync();
                var intervals = histories
                    .Where(h => h.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) &&
                           h.BaseCurrency.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase))
                    .Select(h => h.Interval)
                    .Distinct()
                    .ToList();

                return intervals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available intervals for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}",
                    symbol, baseCurrency);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<PriceHistory> UpdateAsync(PriceHistory history)
        {
            _logger.LogInformation("Updating price history: {Id}", history.Id);

            try
            {
                // Check if history exists
                var exists = await ExistsAsync(history.Id);
                if (!exists)
                {
                    return null;
                }

                return await base.UpdateAsync(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating price history: {Id}", history.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PriceHistory> AddDataPointsAsync(Guid id, IEnumerable<PriceDataPoint> dataPoints)
        {
            _logger.LogInformation("Adding data points to price history: {Id}", id);

            try
            {
                var history = await GetByIdAsync(id);
                if (history == null)
                {
                    return null;
                }

                // Add new data points
                var newDataPoints = dataPoints.ToList();
                var existingTimestamps = history.DataPoints.Select(dp => dp.Timestamp).ToHashSet();
                var uniqueNewDataPoints = newDataPoints.Where(dp => !existingTimestamps.Contains(dp.Timestamp)).ToList();

                history.DataPoints = history.DataPoints.Concat(uniqueNewDataPoints).ToList();

                // Update start and end times if necessary
                if (uniqueNewDataPoints.Any())
                {
                    var minTimestamp = uniqueNewDataPoints.Min(dp => dp.Timestamp);
                    var maxTimestamp = uniqueNewDataPoints.Max(dp => dp.Timestamp);

                    if (minTimestamp < history.StartTime)
                    {
                        history.StartTime = minTimestamp;
                    }

                    if (maxTimestamp > history.EndTime)
                    {
                        history.EndTime = maxTimestamp;
                    }
                }

                return await UpdateAsync(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding data points to price history: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Merges multiple price histories into a single history
        /// </summary>
        /// <param name="histories">Price histories to merge</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>Merged price history</returns>
        private PriceHistory MergeHistories(List<PriceHistory> histories, DateTime startTime, DateTime endTime)
        {
            var symbol = histories[0].Symbol;
            var baseCurrency = histories[0].BaseCurrency;
            var interval = histories[0].Interval;

            // Collect all data points and remove duplicates
            var allDataPoints = new Dictionary<DateTime, PriceDataPoint>();
            foreach (var history in histories)
            {
                foreach (var dataPoint in history.DataPoints)
                {
                    if (dataPoint.Timestamp >= startTime && dataPoint.Timestamp <= endTime)
                    {
                        allDataPoints[dataPoint.Timestamp] = dataPoint;
                    }
                }
            }

            var mergedDataPoints = allDataPoints.Values.OrderBy(dp => dp.Timestamp).ToList();

            return new PriceHistory
            {
                Id = Guid.NewGuid(),
                Symbol = symbol,
                BaseCurrency = baseCurrency,
                Interval = interval,
                StartTime = startTime,
                EndTime = endTime,
                DataPoints = mergedDataPoints,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
