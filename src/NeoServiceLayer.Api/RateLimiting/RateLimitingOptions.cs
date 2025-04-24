using System.Collections.Generic;

namespace NeoServiceLayer.API.RateLimiting
{
    /// <summary>
    /// Options for rate limiting
    /// </summary>
    public class RateLimitingOptions
    {
        /// <summary>
        /// Gets or sets whether rate limiting is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the default rate limit period in seconds
        /// </summary>
        public int DefaultPeriodSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the default rate limit
        /// </summary>
        public int DefaultLimit { get; set; } = 100;

        /// <summary>
        /// Gets or sets the client ID header name
        /// </summary>
        public string ClientIdHeader { get; set; } = "X-ClientId";

        /// <summary>
        /// Gets or sets the rate limit remaining header name
        /// </summary>
        public string RateLimitRemainingHeader { get; set; } = "X-RateLimit-Remaining";

        /// <summary>
        /// Gets or sets the rate limit limit header name
        /// </summary>
        public string RateLimitLimitHeader { get; set; } = "X-RateLimit-Limit";

        /// <summary>
        /// Gets or sets the rate limit reset header name
        /// </summary>
        public string RateLimitResetHeader { get; set; } = "X-RateLimit-Reset";

        /// <summary>
        /// Gets or sets the endpoint-specific rate limits
        /// </summary>
        public Dictionary<string, EndpointRateLimit> EndpointRateLimits { get; set; } = new Dictionary<string, EndpointRateLimit>();

        /// <summary>
        /// Gets or sets the client-specific rate limits
        /// </summary>
        public Dictionary<string, ClientRateLimit> ClientRateLimits { get; set; } = new Dictionary<string, ClientRateLimit>();

        /// <summary>
        /// Gets or sets the IP-specific rate limits
        /// </summary>
        public Dictionary<string, int> IpRateLimits { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets whether to include headers in the response
        /// </summary>
        public bool IncludeHeaders { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of paths to exclude from rate limiting
        /// </summary>
        public List<string> ExcludedPaths { get; set; } = new List<string>();
    }

    /// <summary>
    /// Rate limit for a specific endpoint
    /// </summary>
    public class EndpointRateLimit
    {
        /// <summary>
        /// Gets or sets the endpoint path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the rate limit
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the rate limit period in seconds
        /// </summary>
        public int PeriodSeconds { get; set; }
    }

    /// <summary>
    /// Rate limit for a specific client
    /// </summary>
    public class ClientRateLimit
    {
        /// <summary>
        /// Gets or sets the client ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the rate limit
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the rate limit period in seconds
        /// </summary>
        public int PeriodSeconds { get; set; }
    }
}
