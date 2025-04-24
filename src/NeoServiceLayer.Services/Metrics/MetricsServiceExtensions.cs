using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Metrics.Repositories;

namespace NeoServiceLayer.Services.Metrics
{
    /// <summary>
    /// Extension methods for registering metrics services
    /// </summary>
    public static class MetricsServiceExtensions
    {
        /// <summary>
        /// Adds metrics services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddMetricsServices(this IServiceCollection services)
        {
            // Register repositories
            // Note: MetricsRepository is implemented in the Analytics module

            // Register services
            services.AddSingleton<IMetricsService, MetricsService>();

            return services;
        }
    }
}
