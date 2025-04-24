using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Enclave
{
    /// <summary>
    /// Extension methods for registering enclave services
    /// </summary>
    public static class EnclaveServiceExtensions
    {
        /// <summary>
        /// Adds enclave services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddEnclaveServices(this IServiceCollection services)
        {
            // Register services
            services.AddSingleton<IEnclaveService, EnclaveService>();

            return services;
        }
    }
}
