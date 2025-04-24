using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using CoreStorageProviderConfig = NeoServiceLayer.Core.Models.StorageProviderConfiguration;
using ServicesStorageProviderConfig = NeoServiceLayer.Services.Storage.Configuration.StorageProviderConfiguration;

namespace NeoServiceLayer.Services.Storage.Configuration
{
    /// <summary>
    /// Extension methods for DatabaseProviderConfiguration
    /// </summary>
    public static class DatabaseProviderConfigurationExtensions
    {
        /// <summary>
        /// Converts a DatabaseProviderConfiguration to a StorageProviderConfiguration
        /// </summary>
        /// <param name="config">The DatabaseProviderConfiguration to convert</param>
        /// <returns>The converted StorageProviderConfiguration</returns>
        public static CoreStorageProviderConfig ToStorageProviderConfig(this DatabaseProviderConfiguration config)
        {
            var options = new Dictionary<string, string>(config.Options);
            
            // Add additional properties to options
            if (!string.IsNullOrEmpty(config.ConnectionString))
            {
                options["ConnectionString"] = config.ConnectionString;
            }
            
            if (!string.IsNullOrEmpty(config.Database))
            {
                options["Database"] = config.Database;
            }
            
            if (!string.IsNullOrEmpty(config.Schema))
            {
                options["Schema"] = config.Schema;
            }
            
            options["MaxPoolSize"] = config.MaxPoolSize.ToString();
            options["ConnectionTimeout"] = config.ConnectionTimeout.ToString();
            options["CommandTimeout"] = config.CommandTimeout.ToString();
            options["EnableLogging"] = config.EnableLogging.ToString();
            
            return new CoreStorageProviderConfig
            {
                Name = config.Name,
                Type = config.Type,
                Options = options
            };
        }
        
        /// <summary>
        /// Converts a DatabaseProviderConfiguration to a Services.Storage.Configuration.StorageProviderConfiguration
        /// </summary>
        /// <param name="config">The DatabaseProviderConfiguration to convert</param>
        /// <returns>The converted Services.Storage.Configuration.StorageProviderConfiguration</returns>
        public static ServicesStorageProviderConfig ToServicesConfig(this DatabaseProviderConfiguration config)
        {
            var options = new Dictionary<string, string>(config.Options);
            
            // Add additional properties to options
            if (!string.IsNullOrEmpty(config.ConnectionString))
            {
                options["ConnectionString"] = config.ConnectionString;
            }
            
            if (!string.IsNullOrEmpty(config.Database))
            {
                options["Database"] = config.Database;
            }
            
            if (!string.IsNullOrEmpty(config.Schema))
            {
                options["Schema"] = config.Schema;
            }
            
            options["MaxPoolSize"] = config.MaxPoolSize.ToString();
            options["ConnectionTimeout"] = config.ConnectionTimeout.ToString();
            options["CommandTimeout"] = config.CommandTimeout.ToString();
            options["EnableLogging"] = config.EnableLogging.ToString();
            
            return new ServicesStorageProviderConfig
            {
                Name = config.Name,
                Type = config.Type,
                Options = options
            };
        }
    }
}
