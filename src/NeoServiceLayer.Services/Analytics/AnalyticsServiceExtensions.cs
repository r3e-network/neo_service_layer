using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Analytics.Repositories;

namespace NeoServiceLayer.Services.Analytics
{
    /// <summary>
    /// Extension methods for registering analytics services
    /// </summary>
    public static class AnalyticsServiceExtensions
    {
        /// <summary>
        /// Adds analytics services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddAnalyticsServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<IEventRepository, EventRepository>();
            services.AddSingleton<IMetricRepository, MetricRepository>();
            services.AddSingleton<IAlertRepository, AlertRepository>();
            services.AddSingleton<IDashboardRepository, DashboardRepository>();
            services.AddSingleton<IReportRepository, ReportRepository>();

            // Register services
            services.AddSingleton<IAnalyticsService, AnalyticsService>();

            return services;
        }
    }
}
