using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Api;
using NeoServiceLayer.Api.Scripts;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage;
using NeoServiceLayer.Services.Storage.Configuration;
using NeoServiceLayer.Services.Storage.Migration;
using NeoServiceLayer.Services.Storage.Sharding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Initialize the database
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var databaseService = app.Services.GetRequiredService<IDatabaseService>();

    // Log database configuration
    var databaseConfig = builder.Configuration.GetSection("Database").Get<NeoServiceLayer.Core.Configuration.DatabaseConfiguration>();
    if (databaseConfig != null)
    {
        logger.LogInformation("Database configuration loaded.");
        logger.LogInformation("Connection string: {ConnectionString}", databaseConfig.ConnectionString);
        logger.LogInformation("Database name: {DatabaseName}", databaseConfig.DatabaseName);
        logger.LogInformation("Max connection pool size: {MaxConnectionPoolSize}", databaseConfig.MaxConnectionPoolSize);
        logger.LogInformation("Connection timeout: {ConnectionTimeoutSeconds} seconds", databaseConfig.ConnectionTimeoutSeconds);
    }
    else
    {
        logger.LogError("Database configuration not found in appsettings.json");
    }

    // Initialize database providers
    logger.LogInformation("Initializing database providers...");
    var result = await databaseService.InitializeProvidersAsync();
    if (result)
    {
        logger.LogInformation("Database providers initialized successfully");
    }
    else
    {
        logger.LogError("Failed to initialize database providers");
    }

    // Verify MongoDB provider is initialized
    var mongoProvider = await databaseService.GetProviderByNameAsync("MongoDB");
    if (mongoProvider != null)
    {
        logger.LogInformation("MongoDB provider retrieved successfully");
        var healthCheck = await mongoProvider.HealthCheckAsync();
        logger.LogInformation("MongoDB health check result: {HealthCheck}", healthCheck);
    }
    else
    {
        logger.LogError("MongoDB provider not found or not initialized");
    }

    // Run database migrations
    try
    {
        var migrationService = app.Services.GetRequiredService<DatabaseMigrationService>();
        logger.LogInformation("Running database migrations...");
        await migrationService.RunMigrationsAsync();
        logger.LogInformation("Database migrations completed successfully");
    }
    catch (Exception migrationEx)
    {
        logger.LogError(migrationEx, "Error running database migrations");
    }

    // Initialize MongoDB sharding
    try
    {
        var shardingManager = app.Services.GetRequiredService<MongoDbShardingManager>();
        var shardingConfig = app.Services.GetRequiredService<IOptions<MongoDbShardingConfiguration>>().Value;

        if (shardingConfig.Enabled)
        {
            logger.LogInformation("Initializing MongoDB sharding...");
            await shardingManager.InitializeAsync();

            // Enable sharding for database
            logger.LogInformation("Enabling sharding for database: {DatabaseName}", shardingConfig.DatabaseName);
            await shardingManager.EnableShardingForDatabaseAsync(shardingConfig.DatabaseName);

            // Add shards
            foreach (var shard in shardingConfig.Shards)
            {
                logger.LogInformation("Adding shard: {Shard}", shard);
                await shardingManager.AddShardAsync(shard);
            }

            // Shard collections
            foreach (var collection in shardingConfig.ShardedCollections)
            {
                logger.LogInformation("Sharding collection: {Collection} with key: {ShardKey}", collection.Name, collection.ShardKey);
                await shardingManager.ShardCollectionAsync(shardingConfig.DatabaseName, collection.Name, collection.ShardKey);
            }

            logger.LogInformation("MongoDB sharding initialized successfully");
        }
        else
        {
            logger.LogInformation("MongoDB sharding is disabled");
        }
    }
    catch (Exception shardingEx)
    {
        logger.LogError(shardingEx, "Error initializing MongoDB sharding");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during database initialization");
}

// Configure the HTTP request pipeline
startup.Configure(app, app.Environment);

app.Run();
