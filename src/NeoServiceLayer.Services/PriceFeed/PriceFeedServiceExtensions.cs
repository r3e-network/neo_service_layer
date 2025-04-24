using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.PriceFeed.DataProcessors;
using NeoServiceLayer.Services.PriceFeed.DataSources;
using NeoServiceLayer.Services.PriceFeed.Repositories;

namespace NeoServiceLayer.Services.PriceFeed
{
    /// <summary>
    /// Extension methods for registering price feed services
    /// </summary>
    public static class PriceFeedServiceExtensions
    {
        /// <summary>
        /// Adds price feed services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddPriceFeedServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<IPriceRepository, PriceRepository>();
            services.AddSingleton<IPriceSourceRepository, PriceSourceRepository>();
            services.AddSingleton<IPriceHistoryRepository, PriceHistoryRepository>();

            // Register data sources
            services.AddSingleton<HttpClient>();
            services.AddSingleton<CoinGeckoDataSource>();
            services.AddSingleton<BinanceDataSource>();
            services.AddSingleton<CustomPriceDataSource>();
            services.AddSingleton<PriceDataSourceFactory>();

            // Register data processors
            services.AddSingleton<WeightedAveragePriceProcessor>();
            services.AddSingleton<MedianPriceProcessor>();
            services.AddSingleton<PriceDataProcessorFactory>();

            // Register main service
            services.AddSingleton<IPriceFeedService, PriceFeedService>();

            return services;
        }
    }
}
