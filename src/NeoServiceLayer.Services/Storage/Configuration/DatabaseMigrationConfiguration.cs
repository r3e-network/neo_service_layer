namespace NeoServiceLayer.Services.Storage.Configuration
{
    /// <summary>
    /// Configuration for database migrations
    /// </summary>
    public class DatabaseMigrationConfiguration
    {
        /// <summary>
        /// Gets or sets whether to run migrations on startup
        /// </summary>
        public bool RunMigrationsOnStartup { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to allow downgrade migrations
        /// </summary>
        public bool AllowDowngradeMigrations { get; set; } = false;

        /// <summary>
        /// Gets or sets the migration timeout in seconds
        /// </summary>
        public int MigrationTimeoutSeconds { get; set; } = 60;
    }
}
