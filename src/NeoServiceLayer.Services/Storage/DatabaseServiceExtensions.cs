using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Storage
{
    /// <summary>
    /// Extension methods for registering database services
    /// </summary>
    public static class DatabaseServiceExtensions
    {
        /// <summary>
        /// Adds database services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register options
            var databaseConfig = new DatabaseConfiguration();
            configuration.GetSection("Database").Bind(databaseConfig);
            services.AddSingleton(Options.Create(databaseConfig));

            // Register service
            services.AddSingleton<IDatabaseService, DatabaseService>();

            return services;
        }
    }
}
