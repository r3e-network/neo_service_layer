using System.Collections.Generic;

namespace NeoServiceLayer.API.HealthChecks
{
    /// <summary>
    /// Options for health checks
    /// </summary>
    public class HealthCheckOptions
    {
        /// <summary>
        /// Gets or sets whether health checks are enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the Redis health check options
        /// </summary>
        public RedisHealthCheckOptions Redis { get; set; } = new RedisHealthCheckOptions();

        /// <summary>
        /// Gets or sets the SQL Server health check options
        /// </summary>
        public SqlServerHealthCheckOptions SqlServer { get; set; } = new SqlServerHealthCheckOptions();

        /// <summary>
        /// Gets or sets the MongoDB health check options
        /// </summary>
        public MongoDbHealthCheckOptions MongoDB { get; set; } = new MongoDbHealthCheckOptions();

        /// <summary>
        /// Gets or sets the URL health check options
        /// </summary>
        public UrlHealthCheckOptions Urls { get; set; } = new UrlHealthCheckOptions();

        /// <summary>
        /// Gets or sets the UI health check options
        /// </summary>
        public UiHealthCheckOptions UI { get; set; } = new UiHealthCheckOptions();
    }

    /// <summary>
    /// Options for Redis health check
    /// </summary>
    public class RedisHealthCheckOptions
    {
        /// <summary>
        /// Gets or sets whether Redis health check is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the Redis connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;
    }

    /// <summary>
    /// Options for SQL Server health check
    /// </summary>
    public class SqlServerHealthCheckOptions
    {
        /// <summary>
        /// Gets or sets whether SQL Server health check is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the SQL Server connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;
    }

    /// <summary>
    /// Options for MongoDB health check
    /// </summary>
    public class MongoDbHealthCheckOptions
    {
        /// <summary>
        /// Gets or sets whether MongoDB health check is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the MongoDB connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;
    }

    /// <summary>
    /// Options for URL health check
    /// </summary>
    public class UrlHealthCheckOptions
    {
        /// <summary>
        /// Gets or sets whether URL health check is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the URLs to check
        /// </summary>
        public List<string> UrlsToCheck { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 5;
    }

    /// <summary>
    /// Options for UI health check
    /// </summary>
    public class UiHealthCheckOptions
    {
        /// <summary>
        /// Gets or sets whether UI health check is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the evaluation time in seconds
        /// </summary>
        public int EvaluationTimeInSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the minimum seconds between failure notifications
        /// </summary>
        public int MinimumSecondsBetweenFailureNotifications { get; set; } = 300;

        /// <summary>
        /// Gets or sets the endpoints
        /// </summary>
        public List<HealthCheckEndpoint> Endpoints { get; set; } = new List<HealthCheckEndpoint>();
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    public class HealthCheckEndpoint
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL
        /// </summary>
        public string Url { get; set; }
    }
}
