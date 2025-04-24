using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage.CircuitBreaker;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for health checks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly ILogger<HealthCheckController> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly CircuitBreakerFactory _circuitBreakerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        /// <param name="circuitBreakerFactory">Circuit breaker factory</param>
        public HealthCheckController(
            ILogger<HealthCheckController> logger,
            IDatabaseService databaseService,
            CircuitBreakerFactory circuitBreakerFactory)
        {
            _logger = logger;
            _databaseService = databaseService;
            _circuitBreakerFactory = circuitBreakerFactory;
        }

        /// <summary>
        /// Gets the health status of the API
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = new HealthCheckResult
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = GetType().Assembly.GetName().Version.ToString(),
                    Services = new Dictionary<string, string>()
                };

                // Check database service
                var databaseHealthy = await _databaseService.HealthCheckAsync();
                result.Services["Database"] = databaseHealthy ? "Healthy" : "Unhealthy";

                // If any service is unhealthy, mark the overall status as unhealthy
                if (result.Services.Values.Any(v => v != "Healthy"))
                {
                    result.Status = "Unhealthy";
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking health");
                return StatusCode(500, new { Status = "Unhealthy", Error = ex.Message });
            }
        }

        /// <summary>
        /// Gets detailed health status of the API
        /// </summary>
        /// <returns>Detailed health status</returns>
        [HttpGet("detailed")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDetailed()
        {
            try
            {
                var result = new DetailedHealthCheckResult
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = GetType().Assembly.GetName().Version.ToString(),
                    Services = new Dictionary<string, ServiceHealthStatus>()
                };

                // Check database service
                var databaseHealthy = await _databaseService.HealthCheckAsync();
                var databaseProviders = await _databaseService.GetProvidersAsync();
                
                var databaseStatus = new ServiceHealthStatus
                {
                    Status = databaseHealthy ? "Healthy" : "Unhealthy",
                    Components = new Dictionary<string, string>()
                };

                foreach (var provider in databaseProviders)
                {
                    var providerHealthy = await provider.HealthCheckAsync();
                    databaseStatus.Components[provider.Name] = providerHealthy ? "Healthy" : "Unhealthy";
                }

                result.Services["Database"] = databaseStatus;

                // Check circuit breakers
                var circuitBreakerStatus = new ServiceHealthStatus
                {
                    Status = "Healthy",
                    Components = new Dictionary<string, string>()
                };

                // Get all circuit breakers
                var circuitBreakers = _circuitBreakerFactory.GetAll();
                foreach (var circuitBreaker in circuitBreakers)
                {
                    var status = circuitBreaker.State == CircuitBreakerState.Closed ? "Healthy" : 
                                 circuitBreaker.State == CircuitBreakerState.HalfOpen ? "Degraded" : "Unhealthy";
                    
                    circuitBreakerStatus.Components[circuitBreaker.Name] = status;
                    
                    if (status != "Healthy" && circuitBreakerStatus.Status == "Healthy")
                    {
                        circuitBreakerStatus.Status = status;
                    }
                    
                    if (status == "Unhealthy")
                    {
                        circuitBreakerStatus.Status = "Unhealthy";
                    }
                }

                result.Services["CircuitBreakers"] = circuitBreakerStatus;

                // If any service is unhealthy, mark the overall status as unhealthy
                if (result.Services.Values.Any(v => v.Status == "Unhealthy"))
                {
                    result.Status = "Unhealthy";
                }
                else if (result.Services.Values.Any(v => v.Status == "Degraded"))
                {
                    result.Status = "Degraded";
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking detailed health");
                return StatusCode(500, new { Status = "Unhealthy", Error = ex.Message });
            }
        }

        /// <summary>
        /// Resets all circuit breakers
        /// </summary>
        /// <returns>Result of the operation</returns>
        [HttpPost("reset-circuit-breakers")]
        [Authorize(Roles = "Admin")]
        public IActionResult ResetCircuitBreakers()
        {
            try
            {
                _circuitBreakerFactory.ResetAll();
                return Ok(new { Status = "Success", Message = "All circuit breakers reset" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting circuit breakers");
                return StatusCode(500, new { Status = "Error", Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Health check result
    /// </summary>
    public class HealthCheckResult
    {
        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the services
        /// </summary>
        public Dictionary<string, string> Services { get; set; }
    }

    /// <summary>
    /// Detailed health check result
    /// </summary>
    public class DetailedHealthCheckResult
    {
        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the services
        /// </summary>
        public Dictionary<string, ServiceHealthStatus> Services { get; set; }
    }

    /// <summary>
    /// Service health status
    /// </summary>
    public class ServiceHealthStatus
    {
        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the components
        /// </summary>
        public Dictionary<string, string> Components { get; set; }
    }
}
