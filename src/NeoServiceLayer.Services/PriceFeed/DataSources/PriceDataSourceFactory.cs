using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.PriceFeed.DataSources
{
    /// <summary>
    /// Factory for creating price data sources
    /// </summary>
    public class PriceDataSourceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PriceDataSourceFactory> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceDataSourceFactory"/> class
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="logger">Logger</param>
        public PriceDataSourceFactory(
            IServiceProvider serviceProvider,
            ILogger<PriceDataSourceFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Creates a price data source
        /// </summary>
        /// <param name="source">Price source configuration</param>
        /// <returns>Price data source</returns>
        public IPriceDataSource CreateDataSource(PriceSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            try
            {
                IPriceDataSource dataSource = source.Type switch
                {
                    PriceSourceType.Aggregator when source.Name.Equals("CoinGecko", StringComparison.OrdinalIgnoreCase) =>
                        _serviceProvider.GetRequiredService<CoinGeckoDataSource>(),
                    PriceSourceType.Exchange when source.Name.Equals("Binance", StringComparison.OrdinalIgnoreCase) =>
                        _serviceProvider.GetRequiredService<BinanceDataSource>(),
                    PriceSourceType.Custom when source.Name.Equals("Custom", StringComparison.OrdinalIgnoreCase) =>
                        _serviceProvider.GetRequiredService<CustomPriceDataSource>(),
                    _ => throw new ArgumentException($"Unsupported price source type: {source.Type} - {source.Name}")
                };

                // Initialize the data source with the configuration
                dataSource.Initialize(source.Config);

                return dataSource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating data source for {SourceName}", source.Name);
                throw;
            }
        }

        /// <summary>
        /// Creates multiple price data sources
        /// </summary>
        /// <param name="sources">List of price source configurations</param>
        /// <returns>List of price data sources</returns>
        public IEnumerable<IPriceDataSource> CreateDataSources(IEnumerable<PriceSource> sources)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            var result = new List<IPriceDataSource>();

            foreach (var source in sources)
            {
                try
                {
                    var dataSource = CreateDataSource(source);
                    result.Add(dataSource);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating data source for {SourceName}", source.Name);
                    // Continue with other sources
                }
            }

            return result;
        }
    }
}
