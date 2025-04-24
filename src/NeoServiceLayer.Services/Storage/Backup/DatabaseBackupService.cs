using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage.Configuration;

namespace NeoServiceLayer.Services.Storage.Backup
{
    /// <summary>
    /// Service for backing up database data
    /// </summary>
    public class DatabaseBackupService : IDisposable
    {
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly IStorageService _storageService;
        private readonly DatabaseBackupConfiguration _configuration;
        private readonly Timer _backupTimer;
        private readonly TimeSpan _backupInterval;
        private readonly string _backupDirectory;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseBackupService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        /// <param name="storageService">Storage service</param>
        /// <param name="configuration">Backup configuration</param>
        public DatabaseBackupService(
            ILogger<DatabaseBackupService> logger,
            IDatabaseService databaseService,
            IStorageService storageService,
            IOptions<DatabaseBackupConfiguration> configuration)
        {
            _logger = logger;
            _databaseService = databaseService;
            _storageService = storageService;
            _configuration = configuration.Value;
            _backupInterval = TimeSpan.FromHours(_configuration.BackupIntervalHours);
            _backupDirectory = _configuration.BackupDirectory;

            // Create backup directory if it doesn't exist
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }

            // Start backup timer
            _backupTimer = new Timer(BackupDatabases, null, _backupInterval, _backupInterval);
        }

        /// <summary>
        /// Performs a backup of all databases
        /// </summary>
        /// <param name="state">Timer state</param>
        private async void BackupDatabases(object state)
        {
            try
            {
                _logger.LogInformation("Starting database backup");

                // Get MongoDB provider
                var mongoProvider = await _databaseService.GetProviderByNameAsync("MongoDB");
                if (mongoProvider == null)
                {
                    _logger.LogWarning("MongoDB provider not found, skipping backup");
                    return;
                }

                // Get all collections
                var collections = new List<string>();
                collections.AddRange(new[] 
                { 
                    "accounts", 
                    "wallets", 
                    "functions", 
                    "secrets", 
                    "prices", 
                    "priceSources",
                    "gasbank_accounts",
                    "gasbank_allocations",
                    "gasbank_transactions",
                    "event_subscriptions",
                    "event_logs",
                    "notifications",
                    "notification_templates",
                    "user_notification_preferences",
                    "metrics",
                    "events",
                    "dashboards",
                    "reports",
                    "alerts"
                });

                // Create backup timestamp
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var backupPath = Path.Combine(_backupDirectory, $"backup_{timestamp}");
                Directory.CreateDirectory(backupPath);

                // Backup each collection
                foreach (var collection in collections)
                {
                    try
                    {
                        if (await mongoProvider.CollectionExistsAsync(collection))
                        {
                            _logger.LogInformation("Backing up collection: {Collection}", collection);

                            // Get all entities
                            var entities = await mongoProvider.GetAllAsync<object>(collection);
                            var entitiesList = entities.ToList();

                            // Skip empty collections
                            if (entitiesList.Count == 0)
                            {
                                _logger.LogInformation("Collection {Collection} is empty, skipping", collection);
                                continue;
                            }

                            // Serialize entities to JSON
                            var json = System.Text.Json.JsonSerializer.Serialize(entitiesList);

                            // Write to file
                            var collectionPath = Path.Combine(backupPath, $"{collection}.json");
                            await File.WriteAllTextAsync(collectionPath, json);

                            _logger.LogInformation("Backed up {Count} entities from collection {Collection}", entitiesList.Count, collection);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error backing up collection: {Collection}", collection);
                    }
                }

                // Upload backup to S3 if configured
                if (_configuration.UploadToS3)
                {
                    try
                    {
                        _logger.LogInformation("Uploading backup to S3");

                        // Create a zip file
                        var zipPath = $"{backupPath}.zip";
                        System.IO.Compression.ZipFile.CreateFromDirectory(backupPath, zipPath);

                        // Upload to S3
                        using (var fileStream = File.OpenRead(zipPath))
                        {
                            var s3Path = $"backups/database_{timestamp}.zip";
                            await _storageService.StoreFileAsync(Guid.Empty, null, s3Path, "application/zip", fileStream, false);
                        }

                        _logger.LogInformation("Backup uploaded to S3");

                        // Delete local zip file if configured
                        if (_configuration.DeleteLocalBackupAfterUpload)
                        {
                            File.Delete(zipPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading backup to S3");
                    }
                }

                // Clean up old backups
                CleanupOldBackups();

                _logger.LogInformation("Database backup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error backing up databases");
            }
        }

        /// <summary>
        /// Cleans up old backups
        /// </summary>
        private void CleanupOldBackups()
        {
            try
            {
                _logger.LogInformation("Cleaning up old backups");

                // Get all backup directories
                var backupDirs = Directory.GetDirectories(_backupDirectory)
                    .Where(d => Path.GetFileName(d).StartsWith("backup_"))
                    .OrderByDescending(d => d)
                    .ToList();

                // Keep only the configured number of backups
                if (backupDirs.Count > _configuration.BackupsToKeep)
                {
                    var dirsToDelete = backupDirs.Skip(_configuration.BackupsToKeep).ToList();
                    foreach (var dir in dirsToDelete)
                    {
                        _logger.LogInformation("Deleting old backup: {Directory}", dir);
                        Directory.Delete(dir, true);
                    }
                }

                // Get all backup zip files
                var backupZips = Directory.GetFiles(_backupDirectory, "backup_*.zip")
                    .OrderByDescending(f => f)
                    .ToList();

                // Keep only the configured number of backup zips
                if (backupZips.Count > _configuration.BackupsToKeep)
                {
                    var zipsToDelete = backupZips.Skip(_configuration.BackupsToKeep).ToList();
                    foreach (var zip in zipsToDelete)
                    {
                        _logger.LogInformation("Deleting old backup zip: {File}", zip);
                        File.Delete(zip);
                    }
                }

                _logger.LogInformation("Cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old backups");
            }
        }

        /// <summary>
        /// Performs a manual backup
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task PerformManualBackupAsync()
        {
            await Task.Run(() => BackupDatabases(null));
        }

        /// <summary>
        /// Disposes the backup service
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the backup service
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _backupTimer?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
