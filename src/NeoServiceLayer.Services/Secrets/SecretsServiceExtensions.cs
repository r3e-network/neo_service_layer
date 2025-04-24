using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Secrets.Repositories;

namespace NeoServiceLayer.Services.Secrets
{
    /// <summary>
    /// Extension methods for registering secrets services
    /// </summary>
    public static class SecretsServiceExtensions
    {
        /// <summary>
        /// Adds secrets services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddSecretsServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<ISecretsRepository, SecretsRepository>();

            // Register services
            services.AddSingleton<ISecretsService, SecretsService>();

            return services;
        }
    }
}
