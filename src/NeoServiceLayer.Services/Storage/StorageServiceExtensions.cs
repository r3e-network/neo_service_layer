using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Storage.Providers;

namespace NeoServiceLayer.Services.Storage
{
    /// <summary>
    /// Extension methods for registering storage services
    /// </summary>
    public static class StorageServiceExtensions
    {
        /// <summary>
        /// Adds storage services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register options
            services.Configure<StorageOptions>(options => configuration.GetSection("Storage").Bind(options));

            // Register providers
            services.AddSingleton<S3StorageProvider>();
            services.AddSingleton<FileStorageProvider>();
            services.AddSingleton<InMemoryStorageProvider>();

            // Register services
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IFileStorageService, FileStorageService>();
            services.AddSingleton<IObjectStorageService, ObjectStorageService>();

            return services;
        }
    }
}
