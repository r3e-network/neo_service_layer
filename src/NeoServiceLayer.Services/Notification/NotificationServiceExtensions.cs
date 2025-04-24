using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Notification.Providers;
using NeoServiceLayer.Services.Notification.Repositories;

namespace NeoServiceLayer.Services.Notification
{
    /// <summary>
    /// Extension methods for registering notification services
    /// </summary>
    public static class NotificationServiceExtensions
    {
        /// <summary>
        /// Adds notification services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddNotificationServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<INotificationRepository, NotificationRepository>();

            // Register providers
            services.AddSingleton<INotificationProvider, EmailNotificationProvider>();
            services.AddSingleton<INotificationProvider, SmsNotificationProvider>();
            services.AddSingleton<INotificationProvider, WebhookNotificationProvider>();

            // Register services
            services.AddSingleton<INotificationService, NotificationService>();

            return services;
        }
    }
}
