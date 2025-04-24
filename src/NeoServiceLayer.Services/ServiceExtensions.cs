using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Services.Account;
using NeoServiceLayer.Services.Analytics;
using NeoServiceLayer.Services.Deployment;
using NeoServiceLayer.Services.Enclave;
using NeoServiceLayer.Services.EventMonitoring;
using NeoServiceLayer.Services.Function;
using NeoServiceLayer.Services.Metrics;
using NeoServiceLayer.Services.Notification;
using NeoServiceLayer.Services.PriceFeed;
using NeoServiceLayer.Services.Secrets;
using NeoServiceLayer.Services.Storage;
using NeoServiceLayer.Services.Wallet;

namespace NeoServiceLayer.Services
{
    /// <summary>
    /// Extension methods for registering all services
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds all services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddNeoServiceLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add storage services first as they are dependencies for other services
            services.AddStorageServices(configuration);
            services.AddDatabaseServices(configuration);

            // Add enclave service as it's a dependency for other services
            services.AddEnclaveServices();

            // Add core services
            services.AddAccountServices();
            services.AddWalletServices();
            services.AddSecretsServices();
            services.AddFunctionServices();

            // Add monitoring and analytics services
            services.AddEventMonitoringServices();
            services.AddMetricsServices();
            services.AddAnalyticsServices();
            services.AddPriceFeedServices();
            services.AddNotificationServices();

            // Add deployment services
            services.AddDeploymentServices();

            return services;
        }
    }
}
