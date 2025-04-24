using System.Collections.Generic;
using CoreStorageProviderConfig = NeoServiceLayer.Core.Models.StorageProviderConfiguration;
using ServicesStorageProviderConfig = NeoServiceLayer.Services.Storage.Configuration.StorageProviderConfiguration;

namespace NeoServiceLayer.Services.Storage.Configuration
{
    /// <summary>
    /// Extension methods for StorageProviderConfiguration
    /// </summary>
    public static class StorageProviderConfigurationExtensions
    {
        /// <summary>
        /// Converts a Core.Models.StorageProviderConfiguration to a Services.Storage.Configuration.StorageProviderConfiguration
        /// </summary>
        /// <param name="config">The Core.Models.StorageProviderConfiguration to convert</param>
        /// <returns>The converted Services.Storage.Configuration.StorageProviderConfiguration</returns>
        public static ServicesStorageProviderConfig ToServicesConfig(this CoreStorageProviderConfig config)
        {
            var options = new Dictionary<string, string>(config.Options);
            
            // Add additional properties to options if they're not null
            if (!string.IsNullOrEmpty(config.ConnectionString))
            {
                options["ConnectionString"] = config.ConnectionString;
            }
            
            if (!string.IsNullOrEmpty(config.Container))
            {
                options["Container"] = config.Container;
            }
            
            return new ServicesStorageProviderConfig
            {
                Name = config.Name,
                Type = config.Type,
                BasePath = config.BasePath,
                Options = options
            };
        }
        
        /// <summary>
        /// Converts a Services.Storage.Configuration.StorageProviderConfiguration to a Core.Models.StorageProviderConfiguration
        /// </summary>
        /// <param name="config">The Services.Storage.Configuration.StorageProviderConfiguration to convert</param>
        /// <returns>The converted Core.Models.StorageProviderConfiguration</returns>
        public static CoreStorageProviderConfig ToCoreConfig(this ServicesStorageProviderConfig config)
        {
            var coreConfig = new CoreStorageProviderConfig
            {
                Name = config.Name,
                Type = config.Type,
                BasePath = config.BasePath,
                Options = new Dictionary<string, string>(config.Options)
            };
            
            // Extract additional properties from options if they exist
            if (config.Options.TryGetValue("ConnectionString", out var connectionString))
            {
                coreConfig.ConnectionString = connectionString;
            }
            
            if (config.Options.TryGetValue("Container", out var container))
            {
                coreConfig.Container = container;
            }
            
            return coreConfig;
        }
    }
}
