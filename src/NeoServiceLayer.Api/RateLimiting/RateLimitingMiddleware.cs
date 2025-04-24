using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.API.RateLimiting
{
    /// <summary>
    /// Middleware for rate limiting
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RateLimitingOptions _options;
        private readonly ICacheService _cacheService;
        private static readonly ConcurrentDictionary<string, RateLimitCounter> _inMemoryCounters = new ConcurrentDictionary<string, RateLimitCounter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitingMiddleware"/> class
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger</param>
        /// <param name="options">Rate limiting options</param>
        /// <param name="cacheService">Cache service</param>
        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IOptions<RateLimitingOptions> options,
            ICacheService cacheService)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            // Skip rate limiting for excluded paths
            var path = context.Request.Path.Value;
            if (_options.ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Get client identifier
            var clientId = GetClientIdentifier(context);
            var endpoint = $"{context.Request.Method}:{context.Request.Path}";

            // Get rate limit for this client and endpoint
            var (limit, periodSeconds) = GetRateLimit(clientId, endpoint);

            // Get or create counter
            var counter = await GetCounterAsync(clientId, endpoint, limit, periodSeconds);

            // Check if rate limit is exceeded
            if (counter.Count >= limit)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", clientId, endpoint);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Add("Retry-After", counter.ResetSeconds.ToString());

                if (_options.IncludeHeaders)
                {
                    context.Response.Headers.Add(_options.RateLimitLimitHeader, limit.ToString());
                    context.Response.Headers.Add(_options.RateLimitRemainingHeader, "0");
                    context.Response.Headers.Add(_options.RateLimitResetHeader, counter.ResetSeconds.ToString());
                }

                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }

            // Increment counter
            counter.Count++;
            await SaveCounterAsync(clientId, endpoint, counter);

            // Add rate limit headers
            if (_options.IncludeHeaders)
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Add(_options.RateLimitLimitHeader, limit.ToString());
                    context.Response.Headers.Add(_options.RateLimitRemainingHeader, (limit - counter.Count).ToString());
                    context.Response.Headers.Add(_options.RateLimitResetHeader, counter.ResetSeconds.ToString());
                    return Task.CompletedTask;
                });
            }

            // Call the next middleware
            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get client ID from header
            if (context.Request.Headers.TryGetValue(_options.ClientIdHeader, out var clientId) && !string.IsNullOrEmpty(clientId))
            {
                return clientId;
            }

            // Try to get client ID from authenticated user
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                return context.User.Identity.Name;
            }

            // Fall back to IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private (int limit, int periodSeconds) GetRateLimit(string clientId, string endpoint)
        {
            // Check for endpoint-specific rate limit
            foreach (var endpointLimit in _options.EndpointRateLimits)
            {
                if (endpoint.StartsWith(endpointLimit.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return (endpointLimit.Value.Limit, endpointLimit.Value.PeriodSeconds);
                }
            }

            // Check for client-specific rate limit
            if (_options.ClientRateLimits.TryGetValue(clientId, out var clientLimit))
            {
                return (clientLimit.Limit, clientLimit.PeriodSeconds);
            }

            // Check for IP-specific rate limit
            if (_options.IpRateLimits.TryGetValue(clientId, out var ipLimit))
            {
                return (ipLimit, _options.DefaultPeriodSeconds);
            }

            // Use default rate limit
            return (_options.DefaultLimit, _options.DefaultPeriodSeconds);
        }

        private async Task<RateLimitCounter> GetCounterAsync(string clientId, string endpoint, int limit, int periodSeconds)
        {
            var key = $"ratelimit:{clientId}:{endpoint}";

            // Try to get counter from cache
            var counter = await _cacheService.GetAsync<RateLimitCounter>(key);
            if (counter != null)
            {
                // Check if counter has expired
                if (counter.Expiration < DateTime.UtcNow)
                {
                    counter = new RateLimitCounter
                    {
                        Count = 0,
                        Expiration = DateTime.UtcNow.AddSeconds(periodSeconds),
                        ResetSeconds = periodSeconds
                    };
                }
                else
                {
                    // Update reset seconds
                    counter.ResetSeconds = (int)(counter.Expiration - DateTime.UtcNow).TotalSeconds;
                }

                return counter;
            }

            // Create new counter
            counter = new RateLimitCounter
            {
                Count = 0,
                Expiration = DateTime.UtcNow.AddSeconds(periodSeconds),
                ResetSeconds = periodSeconds
            };

            return counter;
        }

        private async Task SaveCounterAsync(string clientId, string endpoint, RateLimitCounter counter)
        {
            var key = $"ratelimit:{clientId}:{endpoint}";
            var expiration = counter.Expiration - DateTime.UtcNow;

            // Save counter to cache
            await _cacheService.SetAsync(key, counter, expiration);
        }
    }

    /// <summary>
    /// Counter for rate limiting
    /// </summary>
    public class RateLimitCounter
    {
        /// <summary>
        /// Gets or sets the count
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the expiration
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Gets or sets the reset seconds
        /// </summary>
        public int ResetSeconds { get; set; }
    }
}
