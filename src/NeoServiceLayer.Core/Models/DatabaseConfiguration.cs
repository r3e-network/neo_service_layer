using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Configuration for database service
    /// </summary>
    public class DatabaseConfiguration
    {
        /// <summary>
        /// Gets or sets the default provider name
        /// </summary>
        public string DefaultProvider { get; set; }

        /// <summary>
        /// Gets or sets the providers configuration
        /// </summary>
        public List<DatabaseProviderConfiguration> Providers { get; set; } = new List<DatabaseProviderConfiguration>();
    }

    /// <summary>
    /// Configuration for a database provider
    /// </summary>
    public class DatabaseProviderConfiguration
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the provider type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the database name
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the schema name (for SQL databases)
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the maximum connection pool size
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the connection timeout in seconds
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// Gets or sets the command timeout in seconds
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Gets or sets whether to enable query logging
        /// </summary>
        public bool EnableLogging { get; set; } = false;

        /// <summary>
        /// Gets or sets additional options
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
    }
}
