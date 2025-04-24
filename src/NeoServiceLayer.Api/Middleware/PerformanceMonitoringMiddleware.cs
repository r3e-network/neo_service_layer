using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NeoServiceLayer.API.Middleware
{
    /// <summary>
    /// Middleware for monitoring API performance
    /// </summary>
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitoringMiddleware"/> class
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger</param>
        public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            _stopwatch.Restart();

            // Add a unique request ID to the response headers
            var requestId = Guid.NewGuid().ToString();
            context.Response.Headers.Add("X-Request-ID", requestId);

            // Log the request
            _logger.LogInformation("Request {RequestId} started: {Method} {Path}{QueryString}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString);

            // Call the next middleware
            await _next(context);

            // Stop the stopwatch
            _stopwatch.Stop();

            // Log the response
            _logger.LogInformation("Request {RequestId} completed: {StatusCode} in {ElapsedMilliseconds}ms",
                requestId,
                context.Response.StatusCode,
                _stopwatch.ElapsedMilliseconds);

            // Add performance metrics to the response headers
            context.Response.Headers.Add("X-Response-Time-Ms", _stopwatch.ElapsedMilliseconds.ToString());
        }
    }
}
