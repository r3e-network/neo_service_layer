using System;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NeoServiceLayer.Core.Interfaces;


namespace NeoServiceLayer.API.HealthChecks
{
    /// <summary>
    /// Extension methods for health checks
    /// </summary>
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Adds health check services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            // Add health check options
            services.Configure<HealthCheckOptions>(configuration.GetSection("HealthChecks"));

            // Get health check options
            var healthCheckOptions = configuration.GetSection("HealthChecks").Get<HealthCheckOptions>();

            if (healthCheckOptions == null || !healthCheckOptions.Enabled)
            {
                return services;
            }

            // Add health checks
            var healthChecksBuilder = services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "self" });

            // Add database health check
            healthChecksBuilder.AddCheck<DatabaseHealthCheck>("database", tags: new[] { "database" });

            // Add Redis health check if enabled
            if (healthCheckOptions.Redis.Enabled)
            {
                healthChecksBuilder.AddRedis(
                    healthCheckOptions.Redis.ConnectionString,
                    name: "redis",
                    tags: new[] { "redis" },
                    timeout: TimeSpan.FromSeconds(healthCheckOptions.Redis.TimeoutSeconds));
            }

            // Add SQL Server health check if enabled
            if (healthCheckOptions.SqlServer.Enabled)
            {
                healthChecksBuilder.AddSqlServer(
                    healthCheckOptions.SqlServer.ConnectionString,
                    name: "sqlserver",
                    tags: new[] { "sqlserver" },
                    timeout: TimeSpan.FromSeconds(healthCheckOptions.SqlServer.TimeoutSeconds));
            }

            // Add MongoDB health check if enabled
            if (healthCheckOptions.MongoDB.Enabled)
            {
                healthChecksBuilder.AddMongoDb(
                    healthCheckOptions.MongoDB.ConnectionString,
                    name: "mongodb",
                    tags: new[] { "mongodb" },
                    timeout: TimeSpan.FromSeconds(healthCheckOptions.MongoDB.TimeoutSeconds));
            }

            // Add URL health check if enabled
            if (healthCheckOptions.Urls.Enabled)
            {
                foreach (var url in healthCheckOptions.Urls.UrlsToCheck)
                {
                    healthChecksBuilder.AddUrlGroup(
                        new Uri(url),
                        name: $"url-{url}",
                        tags: new[] { "url" },
                        timeout: TimeSpan.FromSeconds(healthCheckOptions.Urls.TimeoutSeconds));
                }
            }

            // Add health checks UI if enabled
            if (healthCheckOptions.UI.Enabled)
            {
                services.AddHealthChecksUI(setup =>
                {
                    setup.SetEvaluationTimeInSeconds(healthCheckOptions.UI.EvaluationTimeInSeconds);
                    setup.SetMinimumSecondsBetweenFailureNotifications(healthCheckOptions.UI.MinimumSecondsBetweenFailureNotifications);

                    // Add health check endpoints
                    setup.AddHealthCheckEndpoint("self", "/health");

                    foreach (var endpoint in healthCheckOptions.UI.Endpoints)
                    {
                        setup.AddHealthCheckEndpoint(endpoint.Name, endpoint.Url);
                    }
                })
                .AddInMemoryStorage();
            }

            return services;
        }

        /// <summary>
        /// Adds health check endpoints to the application
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseHealthChecks(this IApplicationBuilder app, IConfiguration configuration)
        {
            // Get health check options
            var healthCheckOptions = configuration.GetSection("HealthChecks").Get<HealthCheckOptions>();

            if (healthCheckOptions == null || !healthCheckOptions.Enabled)
            {
                return app;
            }

            // Add health check endpoint
            app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            // Add health check endpoints for specific tags
            app.UseHealthChecks("/health/self", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("self"),
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseHealthChecks("/health/database", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("database"),
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseHealthChecks("/health/redis", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("redis"),
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseHealthChecks("/health/sqlserver", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("sqlserver"),
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseHealthChecks("/health/mongodb", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("mongodb"),
                ResponseWriter = WriteHealthCheckResponse
            });

            app.UseHealthChecks("/health/url", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("url"),
                ResponseWriter = WriteHealthCheckResponse
            });

            // Add health checks UI if enabled
            if (healthCheckOptions.UI.Enabled)
            {
                app.UseHealthChecksUI(options =>
                {
                    options.UIPath = "/health-ui";
                    options.ApiPath = "/health-api";
                });
            }

            return app;
        }

        private static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;

            var response = new
            {
                status = report.Status.ToString(),
                results = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    data = entry.Value.Data,
                    duration = entry.Value.Duration.TotalMilliseconds
                })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
