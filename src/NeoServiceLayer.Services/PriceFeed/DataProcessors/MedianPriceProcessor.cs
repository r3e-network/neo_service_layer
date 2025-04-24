using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.PriceFeed.DataProcessors
{
    /// <summary>
    /// Median price data processor
    /// </summary>
    public class MedianPriceProcessor : IPriceDataProcessor
    {
        private readonly ILogger<MedianPriceProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedianPriceProcessor"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public MedianPriceProcessor(ILogger<MedianPriceProcessor> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public string Name => "Median";

        /// <inheritdoc/>
        public Task<Price> ProcessPricesAsync(IEnumerable<Price> prices)
        {
            if (prices == null || !prices.Any())
            {
                return Task.FromResult<Price>(null);
            }

            try
            {
                var pricesList = prices.ToList();
                var firstPrice = pricesList.First();
                var symbol = firstPrice.Symbol;
                var baseCurrency = firstPrice.BaseCurrency;

                // Calculate median
                var sortedPrices = pricesList.OrderBy(p => p.Value).ToList();
                decimal median;

                if (sortedPrices.Count % 2 == 0)
                {
                    // Even number of prices, take average of middle two
                    var middle1 = sortedPrices[sortedPrices.Count / 2 - 1].Value;
                    var middle2 = sortedPrices[sortedPrices.Count / 2].Value;
                    median = (middle1 + middle2) / 2;
                }
                else
                {
                    // Odd number of prices, take middle one
                    median = sortedPrices[sortedPrices.Count / 2].Value;
                }

                var result = new Price
                {
                    Id = Guid.NewGuid(),
                    Symbol = symbol,
                    BaseCurrency = baseCurrency,
                    Value = median,
                    Timestamp = DateTime.UtcNow,
                    ConfidenceScore = 100,
                    CreatedAt = DateTime.UtcNow,
                    SourcePrices = prices.Select(p => new SourcePrice
                    {
                        Id = Guid.NewGuid(),
                        SourceId = p.Id,
                        SourceName = p.SourcePrices.FirstOrDefault()?.SourceName ?? "Unknown",
                        Value = p.Value,
                        Timestamp = p.Timestamp,
                        Weight = 100 / prices.Count()
                    }).ToList()
                };

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing prices");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Price>> ProcessMultipleSymbolsAsync(IEnumerable<Price> prices)
        {
            if (prices == null || !prices.Any())
            {
                return new List<Price>();
            }

            try
            {
                var result = new List<Price>();
                var pricesBySymbol = prices.GroupBy(p => new { p.Symbol, p.BaseCurrency });

                foreach (var group in pricesBySymbol)
                {
                    var processedPrice = await ProcessPricesAsync(group);
                    if (processedPrice != null)
                    {
                        result.Add(processedPrice);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing multiple symbols");
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<PriceHistory> GeneratePriceHistoryAsync(IEnumerable<Price> prices, string interval)
        {
            if (prices == null || !prices.Any())
            {
                return Task.FromResult<PriceHistory>(null);
            }

            try
            {
                var pricesList = prices.ToList();
                var firstPrice = pricesList.First();
                var symbol = firstPrice.Symbol;
                var baseCurrency = firstPrice.BaseCurrency;

                // Sort prices by timestamp
                pricesList = pricesList.OrderBy(p => p.Timestamp).ToList();

                // Determine interval in seconds
                int intervalSeconds = ParseInterval(interval);
                if (intervalSeconds <= 0)
                {
                    throw new ArgumentException($"Invalid interval: {interval}");
                }

                // Group prices by interval
                var startTime = pricesList.First().Timestamp;
                var endTime = pricesList.Last().Timestamp;
                var dataPoints = new List<PriceDataPoint>();

                for (var time = startTime; time <= endTime; time = time.AddSeconds(intervalSeconds))
                {
                    var intervalEnd = time.AddSeconds(intervalSeconds);
                    var intervalPrices = pricesList.Where(p => p.Timestamp >= time && p.Timestamp < intervalEnd).ToList();

                    if (intervalPrices.Any())
                    {
                        var open = intervalPrices.First().Value;
                        var close = intervalPrices.Last().Value;
                        var high = intervalPrices.Max(p => p.Value);
                        var low = intervalPrices.Min(p => p.Value);
                        var volume = 0m; // Volume data not available

                        dataPoints.Add(new PriceDataPoint
                        {
                            Timestamp = time,
                            Open = open,
                            High = high,
                            Low = low,
                            Close = close,
                            Volume = volume
                        });
                    }
                }

                var result = new PriceHistory
                {
                    Id = Guid.NewGuid(),
                    Symbol = symbol,
                    BaseCurrency = baseCurrency,
                    Interval = interval,
                    StartTime = startTime,
                    EndTime = endTime,
                    DataPoints = dataPoints,
                    CreatedAt = DateTime.UtcNow
                };

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating price history");
                throw;
            }
        }

        /// <summary>
        /// Parses an interval string into seconds
        /// </summary>
        /// <param name="interval">Interval string (e.g., "1m", "1h", "1d")</param>
        /// <returns>Interval in seconds</returns>
        private int ParseInterval(string interval)
        {
            if (string.IsNullOrEmpty(interval))
            {
                return 0;
            }

            try
            {
                var value = int.Parse(interval.Substring(0, interval.Length - 1));
                var unit = interval.Substring(interval.Length - 1).ToLower();

                return unit switch
                {
                    "s" => value,
                    "m" => value * 60,
                    "h" => value * 60 * 60,
                    "d" => value * 60 * 60 * 24,
                    "w" => value * 60 * 60 * 24 * 7,
                    _ => 0
                };
            }
            catch
            {
                return 0;
            }
        }
    }
}
