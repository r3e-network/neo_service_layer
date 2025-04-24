namespace NeoServiceLayer.Services.Caching
{
    /// <summary>
    /// Configuration for caching
    /// </summary>
    public class CacheConfiguration
    {
        /// <summary>
        /// Gets or sets the default expiration time in seconds
        /// </summary>
        public int DefaultExpirationSeconds { get; set; } = 300; // 5 minutes

        /// <summary>
        /// Gets or sets whether caching is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the Redis connection string
        /// </summary>
        public string RedisConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the cache provider type
        /// </summary>
        public string ProviderType { get; set; } = "Memory"; // Memory, Redis, or Distributed
    }
}
