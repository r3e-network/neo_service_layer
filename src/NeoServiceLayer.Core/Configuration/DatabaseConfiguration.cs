using System;

namespace NeoServiceLayer.Core.Configuration
{
    /// <summary>
    /// Configuration for database connections
    /// </summary>
    public class DatabaseConfiguration
    {
        /// <summary>
        /// Gets or sets the connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the database name
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the maximum connection pool size
        /// </summary>
        public int MaxConnectionPoolSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the connection timeout in seconds
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets a value indicating whether to enable SSL
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to retry on connection failure
        /// </summary>
        public bool RetryOnConnectionFailure { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of retries
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the retry interval in seconds
        /// </summary>
        public int RetryIntervalSeconds { get; set; } = 5;
    }
}
