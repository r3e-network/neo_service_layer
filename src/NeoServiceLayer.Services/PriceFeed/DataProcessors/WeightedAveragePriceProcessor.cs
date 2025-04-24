using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.PriceFeed.Repositories;

namespace NeoServiceLayer.Services.PriceFeed.DataProcessors
{
    /// <summary>
    /// Weighted average price data processor
    /// </summary>
    public class WeightedAveragePriceProcessor : IPriceDataProcessor
    {
        private readonly ILogger<WeightedAveragePriceProcessor> _logger;
        private readonly IPriceSourceRepository _sourceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeightedAveragePriceProcessor"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="sourceRepository">Price source repository</param>
        public WeightedAveragePriceProcessor(
            ILogger<WeightedAveragePriceProcessor> logger,
            IPriceSourceRepository sourceRepository)
        {
            _logger = logger;
            _sourceRepository = sourceRepository;
        }

        /// <inheritdoc/>
        public string Name => "WeightedAverage";

        /// <inheritdoc/>
        public async Task<Price> ProcessPricesAsync(IEnumerable<Price> prices)
        {
            if (prices == null || !prices.Any())
            {
                return null;
            }

            try
            {
                var pricesList = prices.ToList();
                var firstPrice = pricesList.First();
                var symbol = firstPrice.Symbol;
                var baseCurrency = firstPrice.BaseCurrency;

                // Get source weights
                var sourceWeights = await GetSourceWeightsAsync(pricesList.Select(p => p.Source ?? "Unknown").Distinct());

                // Calculate weighted average
                decimal weightedSum = 0;
                decimal totalWeight = 0;

                foreach (var price in pricesList)
                {
                    if (sourceWeights.TryGetValue(price.Source ?? "Unknown", out var weight))
                    {
                        weightedSum += price.Value * weight;
                        totalWeight += weight;
                    }
                }

                if (totalWeight == 0)
                {
                    // If no weights found, use simple average
                    return new Price
                    {
                        Id = Guid.NewGuid(),
                        Symbol = symbol,
                        BaseCurrency = baseCurrency,
                        Value = pricesList.Average(p => p.Value),
                        Timestamp = DateTime.UtcNow,
                        ConfidenceScore = 100,
                        CreatedAt = DateTime.UtcNow,
                        SourcePrices = pricesList.Select(p => new SourcePrice
                        {
                            Id = Guid.NewGuid(),
                            SourceId = Guid.NewGuid(),
                            SourceName = p.SourcePrices.FirstOrDefault()?.SourceName ?? "Unknown",
                            Value = p.Value,
                            Timestamp = p.Timestamp,
                            Weight = 100 / pricesList.Count()
                        }).ToList()
                    };
                }

                var weightedAverage = weightedSum / totalWeight;

                return new Price
                {
                    Id = Guid.NewGuid(),
                    Symbol = symbol,
                    BaseCurrency = baseCurrency,
                    Value = weightedAverage,
                    Timestamp = DateTime.UtcNow,
                    ConfidenceScore = 100,
                    CreatedAt = DateTime.UtcNow,
                    SourcePrices = prices.Select(p => new SourcePrice
                    {
                        Id = Guid.NewGuid(),
                        SourceId = Guid.NewGuid(),
                        SourceName = p.SourcePrices.FirstOrDefault()?.SourceName ?? "Unknown",
                        Value = p.Value,
                        Timestamp = p.Timestamp,
                        Weight = (int)GetSourceWeight(p, totalWeight)
                    }).ToList()
                };
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
        public async Task<PriceHistory> GeneratePriceHistoryAsync(IEnumerable<Price> prices, string interval)
        {
            if (prices == null || !prices.Any())
            {
                return null;
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

                return new PriceHistory
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating price history");
                throw;
            }
        }

        /// <summary>
        /// Gets the weights for each source
        /// </summary>
        /// <param name="sources">List of source names</param>
        /// <returns>Dictionary of source name to weight</returns>
        private async Task<Dictionary<string, decimal>> GetSourceWeightsAsync(IEnumerable<string> sources)
        {
            var result = new Dictionary<string, decimal>();
            var sourcesList = sources.ToList();

            foreach (var sourceName in sourcesList)
            {
                var source = await _sourceRepository.GetByNameAsync(sourceName);
                if (source != null)
                {
                    result[sourceName] = source.Weight;
                }
                else
                {
                    // Default weight if source not found
                    result[sourceName] = 100;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the weight for a price source
        /// </summary>
        /// <param name="price">The price</param>
        /// <param name="totalWeight">The total weight of all sources</param>
        /// <returns>The weight for the source</returns>
        private decimal GetSourceWeight(Price price, decimal totalWeight)
        {
            if (price.SourcePrices == null || !price.SourcePrices.Any())
            {
                return 100 / totalWeight;
            }

            return price.SourcePrices.Sum(sp => sp.Weight) / totalWeight;
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
