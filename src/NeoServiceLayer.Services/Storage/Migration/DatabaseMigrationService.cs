using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Storage.Configuration;

namespace NeoServiceLayer.Services.Storage.Migration
{
    /// <summary>
    /// Service for managing database migrations
    /// </summary>
    public class DatabaseMigrationService
    {
        private readonly ILogger<DatabaseMigrationService> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly DatabaseMigrationConfiguration _configuration;
        private readonly List<IMigration> _migrations = new List<IMigration>();
        private const string MigrationCollectionName = "database_migrations";

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseMigrationService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        /// <param name="configuration">Migration configuration</param>
        public DatabaseMigrationService(
            ILogger<DatabaseMigrationService> logger,
            IDatabaseService databaseService,
            IOptions<DatabaseMigrationConfiguration> configuration)
        {
            _logger = logger;
            _databaseService = databaseService;
            _configuration = configuration.Value;

            // Register migrations
            RegisterMigrations();
        }

        /// <summary>
        /// Registers all migrations
        /// </summary>
        private void RegisterMigrations()
        {
            try
            {
                _logger.LogInformation("Registering migrations");

                // Find all migration classes in the assembly
                var migrationTypes = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => typeof(IMigration).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .OrderBy(t => t.Name)
                    .ToList();

                foreach (var migrationType in migrationTypes)
                {
                    try
                    {
                        var migration = (IMigration)Activator.CreateInstance(migrationType);
                        _migrations.Add(migration);
                        _logger.LogInformation("Registered migration: {Name}, Version: {Version}", migration.Name, migration.Version);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating migration instance: {Type}", migrationType.Name);
                    }
                }

                _logger.LogInformation("Registered {Count} migrations", _migrations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering migrations");
            }
        }

        /// <summary>
        /// Runs all pending migrations
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task RunMigrationsAsync()
        {
            try
            {
                _logger.LogInformation("Running migrations");

                // Get default provider
                var provider = await _databaseService.GetDefaultProviderAsync();
                if (provider == null)
                {
                    _logger.LogError("Default storage provider not found");
                    return;
                }

                // Create migrations collection if it doesn't exist
                if (!await provider.CollectionExistsAsync(MigrationCollectionName))
                {
                    _logger.LogInformation("Creating migrations collection");
                    await provider.CreateCollectionAsync(MigrationCollectionName);
                }

                // Get applied migrations
                var appliedMigrations = await provider.GetAllAsync<MigrationRecord>(MigrationCollectionName);
                var appliedMigrationVersions = appliedMigrations.Select(m => m.Version).ToHashSet();

                // Run pending migrations
                foreach (var migration in _migrations.OrderBy(m => m.Version))
                {
                    if (!appliedMigrationVersions.Contains(migration.Version))
                    {
                        await RunMigrationAsync(migration, provider);
                    }
                    else
                    {
                        _logger.LogInformation("Migration already applied: {Name}, Version: {Version}", migration.Name, migration.Version);
                    }
                }

                _logger.LogInformation("Migrations completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running migrations");
                throw;
            }
        }

        /// <summary>
        /// Runs a single migration
        /// </summary>
        /// <param name="migration">Migration to run</param>
        /// <param name="provider">Storage provider</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task RunMigrationAsync(IMigration migration, IStorageProvider provider)
        {
            _logger.LogInformation("Running migration: {Name}, Version: {Version}", migration.Name, migration.Version);

            try
            {
                // Run migration
                await migration.UpAsync(provider);

                // Record migration
                var record = new MigrationRecord
                {
                    Id = Guid.NewGuid(),
                    Name = migration.Name,
                    Version = migration.Version,
                    AppliedAt = DateTime.UtcNow
                };

                await provider.CreateAsync(MigrationCollectionName, record);

                _logger.LogInformation("Migration completed: {Name}, Version: {Version}", migration.Name, migration.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running migration: {Name}, Version: {Version}", migration.Name, migration.Version);
                throw;
            }
        }

        /// <summary>
        /// Gets all migrations
        /// </summary>
        /// <returns>List of migrations</returns>
        public IEnumerable<IMigration> GetMigrations()
        {
            return _migrations;
        }

        /// <summary>
        /// Gets all applied migrations
        /// </summary>
        /// <returns>List of applied migrations</returns>
        public async Task<IEnumerable<MigrationRecord>> GetAppliedMigrationsAsync()
        {
            try
            {
                var provider = await _databaseService.GetDefaultProviderAsync();
                if (provider == null)
                {
                    _logger.LogError("Default storage provider not found");
                    return Enumerable.Empty<MigrationRecord>();
                }

                if (!await provider.CollectionExistsAsync(MigrationCollectionName))
                {
                    return Enumerable.Empty<MigrationRecord>();
                }

                var migrations = await provider.GetAllAsync<MigrationRecord>(MigrationCollectionName);
                return migrations.OrderByDescending(m => m.AppliedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applied migrations");
                return Enumerable.Empty<MigrationRecord>();
            }
        }

        /// <summary>
        /// Gets all pending migrations
        /// </summary>
        /// <returns>List of pending migrations</returns>
        public async Task<IEnumerable<IMigration>> GetPendingMigrationsAsync()
        {
            try
            {
                var appliedMigrations = await GetAppliedMigrationsAsync();
                var appliedVersions = appliedMigrations.Select(m => m.Version).ToHashSet();

                return _migrations.Where(m => !appliedVersions.Contains(m.Version)).OrderBy(m => m.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending migrations");
                return Enumerable.Empty<IMigration>();
            }
        }
    }

    /// <summary>
    /// Record of an applied migration
    /// </summary>
    public class MigrationRecord
    {
        /// <summary>
        /// Gets or sets the migration ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the migration name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the migration version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets when the migration was applied
        /// </summary>
        public DateTime AppliedAt { get; set; }
    }
}
