using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Caching
{
    /// <summary>
    /// Extension methods for registering caching services
    /// </summary>
    public static class CachingServiceExtensions
    {
        /// <summary>
        /// Adds caching services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add cache configuration
            services.Configure<CacheConfiguration>(options =>
                configuration.GetSection("Caching").Bind(options));

            // Get cache configuration
            var cacheConfig = configuration.GetSection("Caching").Get<CacheConfiguration>();

            if (cacheConfig == null || !cacheConfig.Enabled)
            {
                // If caching is disabled, register a no-op cache service
                services.AddSingleton<ICacheService, NoOpCacheService>();
                return services;
            }

            // Register cache service based on provider type
            switch (cacheConfig.ProviderType?.ToLower())
            {
                case "redis":
                    if (string.IsNullOrEmpty(cacheConfig.RedisConnectionString))
                    {
                        throw new ArgumentException("Redis connection string is required when using Redis cache provider");
                    }

                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = cacheConfig.RedisConnectionString;
                        options.InstanceName = "NeoServiceLayer:";
                    });
                    break;

                case "distributed":
                    services.AddDistributedMemoryCache();
                    break;

                case "memory":
                default:
                    services.AddMemoryCache();
                    services.AddSingleton<IDistributedCache, MemoryCacheAdapter>();
                    break;
            }

            // Register cache service
            services.AddSingleton<ICacheService, CacheService>();

            return services;
        }
    }
}
