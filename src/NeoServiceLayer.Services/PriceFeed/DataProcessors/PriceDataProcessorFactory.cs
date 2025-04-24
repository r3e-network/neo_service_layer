using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NeoServiceLayer.Services.PriceFeed.DataProcessors
{
    /// <summary>
    /// Factory for creating price data processors
    /// </summary>
    public class PriceDataProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PriceDataProcessorFactory> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceDataProcessorFactory"/> class
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="logger">Logger</param>
        public PriceDataProcessorFactory(
            IServiceProvider serviceProvider,
            ILogger<PriceDataProcessorFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Creates a price data processor
        /// </summary>
        /// <param name="processorName">Name of the processor</param>
        /// <returns>Price data processor</returns>
        public IPriceDataProcessor CreateProcessor(string processorName)
        {
            if (string.IsNullOrEmpty(processorName))
            {
                throw new ArgumentNullException(nameof(processorName));
            }

            try
            {
                return processorName.ToLower() switch
                {
                    "weightedaverage" => _serviceProvider.GetRequiredService<WeightedAveragePriceProcessor>(),
                    "median" => _serviceProvider.GetRequiredService<MedianPriceProcessor>(),
                    _ => throw new ArgumentException($"Unsupported price processor: {processorName}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating price processor: {ProcessorName}", processorName);
                throw;
            }
        }

        /// <summary>
        /// Gets the default processor
        /// </summary>
        /// <returns>Default price data processor</returns>
        public IPriceDataProcessor GetDefaultProcessor()
        {
            try
            {
                return _serviceProvider.GetRequiredService<WeightedAveragePriceProcessor>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default price processor");
                throw;
            }
        }
    }
}
