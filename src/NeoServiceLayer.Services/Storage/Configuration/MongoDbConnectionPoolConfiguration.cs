namespace NeoServiceLayer.Services.Storage.Configuration
{
    /// <summary>
    /// Configuration for MongoDB connection pool
    /// </summary>
    public class MongoDbConnectionPoolConfiguration
    {
        /// <summary>
        /// Gets or sets the maximum number of connections
        /// </summary>
        public int MaxConnections { get; set; } = 100;

        /// <summary>
        /// Gets or sets the idle timeout in minutes
        /// </summary>
        public int IdleTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// Gets or sets whether to use connection pooling
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
