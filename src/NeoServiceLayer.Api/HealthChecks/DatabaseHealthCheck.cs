using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.API.HealthChecks
{
    /// <summary>
    /// Health check for database
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ILogger<DatabaseHealthCheck> _logger;
        private readonly IDatabaseService _databaseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public DatabaseHealthCheck(
            ILogger<DatabaseHealthCheck> logger,
            IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Checking database health");

                // Check database health
                var isHealthy = await _databaseService.HealthCheckAsync();

                if (isHealthy)
                {
                    _logger.LogInformation("Database is healthy");
                    return HealthCheckResult.Healthy("Database is healthy");
                }
                else
                {
                    _logger.LogWarning("Database is unhealthy");
                    return HealthCheckResult.Unhealthy("Database is unhealthy");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database health: {Message}", ex.Message);
                return HealthCheckResult.Unhealthy("Error checking database health", ex);
            }
        }
    }
}
