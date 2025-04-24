using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for storage provider operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StorageProviderController : ControllerBase
    {
        private readonly ILogger<StorageProviderController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOptionsMonitor<StorageConfiguration> _storageOptions;
        private readonly IOptionsMonitor<DatabaseConfiguration> _databaseOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageProviderController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="storageOptions">Storage options</param>
        /// <param name="databaseOptions">Database options</param>
        public StorageProviderController(
            ILogger<StorageProviderController> logger,
            IConfiguration configuration,
            IOptionsMonitor<StorageConfiguration> storageOptions,
            IOptionsMonitor<DatabaseConfiguration> databaseOptions)
        {
            _logger = logger;
            _configuration = configuration;
            _storageOptions = storageOptions;
            _databaseOptions = databaseOptions;
        }

        /// <summary>
        /// Gets the current storage provider configuration
        /// </summary>
        /// <returns>Storage provider configuration</returns>
        [HttpGet]
        public IActionResult GetStorageProviderConfiguration()
        {
            _logger.LogInformation("Getting storage provider configuration");

            var storageConfig = _storageOptions.CurrentValue;
            var databaseConfig = _databaseOptions.CurrentValue;

            return Ok(new
            {
                Storage = new
                {
                    DefaultProvider = storageConfig.DefaultProvider,
                    Providers = storageConfig.Providers.Select(p => new
                    {
                        p.Name,
                        p.Type,
                        p.BasePath,
                        p.MaxFileSize,
                        p.MaxStorageSize
                    })
                },
                Database = new
                {
                    DefaultProvider = databaseConfig.DefaultProvider,
                    Providers = databaseConfig.Providers.Select(p => new
                    {
                        p.Name,
                        p.Type,
                        p.Database,
                        p.Schema
                    })
                }
            });
        }

        /// <summary>
        /// Sets the default storage provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <returns>Success status</returns>
        [HttpPost("storage/{providerName}")]
        public IActionResult SetDefaultStorageProvider(string providerName)
        {
            _logger.LogInformation("Setting default storage provider: {ProviderName}", providerName);

            var storageConfig = _storageOptions.CurrentValue;
            var provider = storageConfig.Providers.FirstOrDefault(p => p.Name == providerName);
            if (provider == null)
            {
                return NotFound(new { Message = $"Storage provider not found: {providerName}" });
            }

            // Update configuration
            // Note: In a real implementation, this would update the configuration file
            // and trigger a service restart or reconfiguration
            storageConfig.DefaultProvider = providerName;

            return Ok(new { Message = $"Default storage provider set to {providerName}" });
        }

        /// <summary>
        /// Sets the default database provider
        /// </summary>
        /// <param name="providerName">Provider name</param>
        /// <returns>Success status</returns>
        [HttpPost("database/{providerName}")]
        public IActionResult SetDefaultDatabaseProvider(string providerName)
        {
            _logger.LogInformation("Setting default database provider: {ProviderName}", providerName);

            var databaseConfig = _databaseOptions.CurrentValue;
            var provider = databaseConfig.Providers.FirstOrDefault(p => p.Name == providerName);
            if (provider == null)
            {
                return NotFound(new { Message = $"Database provider not found: {providerName}" });
            }

            // Update configuration
            // Note: In a real implementation, this would update the configuration file
            // and trigger a service restart or reconfiguration
            databaseConfig.DefaultProvider = providerName;

            return Ok(new { Message = $"Default database provider set to {providerName}" });
        }
    }
}
