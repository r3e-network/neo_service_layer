using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage;

namespace NeoServiceLayer.Api.Scripts
{
    /// <summary>
    /// Script to initialize the database
    /// </summary>
    public class InitializeDatabase
    {
        private readonly ILogger<InitializeDatabase> _logger;
        private readonly IDatabaseService _databaseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeDatabase"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public InitializeDatabase(ILogger<InitializeDatabase> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <summary>
        /// Runs the database initialization script
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task RunAsync()
        {
            _logger.LogInformation("Initializing database...");

            try
            {
                // Get the default storage provider
                var storageProvider = await _databaseService.GetDefaultProviderAsync();
                if (storageProvider == null)
                {
                    _logger.LogError("Default storage provider not found");
                    return;
                }

                _logger.LogInformation("Using storage provider: {ProviderName} ({ProviderType})", storageProvider.Name, storageProvider.Type);

                // Create collections
                await CreateCollectionsAsync(storageProvider);

                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
            }
        }

        /// <summary>
        /// Creates the required collections
        /// </summary>
        /// <param name="storageProvider">Storage provider</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task CreateCollectionsAsync(IStorageProvider storageProvider)
        {
            // Create collections for each service
            var collections = new[]
            {
                // Account service
                "accounts",
                "account_settings",
                "account_roles",
                "account_permissions",
                
                // Wallet service
                "wallets",
                "wallet_transactions",
                
                // Secrets service
                "secrets",
                "secret_versions",
                
                // Function service
                "functions",
                "function_versions",
                "function_executions",
                "function_logs",
                
                // Price feed service
                "price_sources",
                "prices",
                "price_history",
                
                // GasBank service
                "gasbank_accounts",
                "gasbank_allocations",
                "gasbank_transactions",
                
                // Event monitoring service
                "event_subscriptions",
                "event_logs",
                
                // Notification service
                "notifications",
                "notification_templates",
                "user_notification_preferences",
                
                // Analytics service
                "metrics",
                "events",
                "dashboards",
                "reports",
                "alerts"
            };

            foreach (var collection in collections)
            {
                _logger.LogInformation("Creating collection: {Collection}", collection);
                
                try
                {
                    // Check if collection exists
                    var exists = await storageProvider.CollectionExistsAsync(collection);
                    if (!exists)
                    {
                        // Create collection
                        await storageProvider.CreateCollectionAsync(collection);
                        _logger.LogInformation("Collection created: {Collection}", collection);
                    }
                    else
                    {
                        _logger.LogInformation("Collection already exists: {Collection}", collection);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating collection: {Collection}", collection);
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for database initialization
    /// </summary>
    public static class InitializeDatabaseExtensions
    {
        /// <summary>
        /// Initializes the database
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<InitializeDatabase>>();
            var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
            
            var initializer = new InitializeDatabase(logger, databaseService);
            await initializer.RunAsync();
        }
    }
}
