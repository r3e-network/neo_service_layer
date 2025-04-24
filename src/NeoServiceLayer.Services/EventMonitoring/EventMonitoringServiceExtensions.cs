using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.EventMonitoring.Repositories;

namespace NeoServiceLayer.Services.EventMonitoring
{
    /// <summary>
    /// Extension methods for registering event monitoring services
    /// </summary>
    public static class EventMonitoringServiceExtensions
    {
        /// <summary>
        /// Adds event monitoring services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddEventMonitoringServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<IEventSubscriptionRepository, EventSubscriptionRepository>();
            services.AddSingleton<IEventLogRepository, EventLogRepository>();

            // Register services
            services.AddSingleton<IEventMonitoringService, EventMonitoringService>();

            return services;
        }
    }
}
