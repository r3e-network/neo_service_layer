using System;
using System.Linq;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NeoServiceLayer.API.Auth;
using NeoServiceLayer.API.HealthChecks;
using NeoServiceLayer.API.Middleware;
using NeoServiceLayer.API.RateLimiting;
using NeoServiceLayer.API.Swagger;
using NeoServiceLayer.API.Tracing;
using NeoServiceLayer.API.Validation;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Models.Analytics;
using NeoServiceLayer.Services.Account;
using NeoServiceLayer.Services.Account.Repositories;
using NeoServiceLayer.Services.Enclave;
using NeoServiceLayer.Services.Function;
using NeoServiceLayer.Services.Function.Repositories;
using NeoServiceLayer.Services.PriceFeed;
using NeoServiceLayer.Services.PriceFeed.Repositories;
using NeoServiceLayer.Services.Secrets;
using NeoServiceLayer.Services.Secrets.Repositories;
using NeoServiceLayer.Services.Storage;
using NeoServiceLayer.Services.Storage.Backup;
using NeoServiceLayer.Services.Storage.CircuitBreaker;
using NeoServiceLayer.Services.Storage.Configuration;
using NeoServiceLayer.Services.Storage.ConnectionPool;
using NeoServiceLayer.Services.Storage.Migration;
using NeoServiceLayer.Services.Storage.Monitoring;
using NeoServiceLayer.Services.Storage.Providers;
using NeoServiceLayer.Services.Storage.Sharding;
using NeoServiceLayer.Services.EventMonitoring;
using NeoServiceLayer.Services.EventMonitoring.Repositories;
using NeoServiceLayer.Services.Notification;
using NeoServiceLayer.Services.Notification.Providers;
using NeoServiceLayer.Services.Notification.Repositories;
using NeoServiceLayer.Services.Analytics;
using NeoServiceLayer.Services.Analytics.Repositories;
using NeoServiceLayer.Services.Caching;
using NeoServiceLayer.Services.Wallet;
using NeoServiceLayer.Services.Wallet.Repositories;

namespace NeoServiceLayer.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IDatabaseService _databaseService;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers with FluentValidation
            services.AddControllers()
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<FunctionTestValidator>();
                    fv.ImplicitlyValidateChildProperties = true;
                    fv.ImplicitlyValidateRootCollectionElements = true;
                });

            // Add health checks
            services.AddHealthChecks(Configuration);

            // Add authentication and authorization services
            services.AddAuthServices(Configuration);

            // Add configuration
            services.Configure<StorageConfiguration>(Configuration.GetSection("Storage"));
            services.Configure<DatabaseConfiguration>(Configuration.GetSection("Database"));
            services.Configure<DatabaseBackupConfiguration>(Configuration.GetSection("DatabaseBackup"));
            services.Configure<DatabaseMigrationConfiguration>(Configuration.GetSection("DatabaseMigration"));
            services.Configure<MongoDbShardingConfiguration>(Configuration.GetSection("MongoDbSharding"));
            services.Configure<MongoDbConnectionPoolConfiguration>(Configuration.GetSection("MongoDbConnectionPool"));
            services.Configure<CircuitBreakerConfiguration>(Configuration.GetSection("CircuitBreaker"));

            // Add caching services
            services.AddCachingServices(Configuration);

            // Add rate limiting services
            services.AddRateLimiting(Configuration);

            // Add distributed tracing services
            services.AddDistributedTracing(Configuration);

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Neo Service Layer API", Version = "v1" });

                // Add extended function API documentation
                c.AddFunctionExtendedApiDocumentation();

                // Enable XML comments
                var xmlFiles = System.IO.Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
                foreach (var xmlFile in xmlFiles)
                {
                    c.IncludeXmlComments(xmlFile);
                }
            });

            // Add service registrations
            // Core services
            services.AddSingleton<IEnclaveService, EnclaveService>();

            // Account services
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountService, AccountService>();

            // Wallet services
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IWalletService, WalletService>();

            // Secrets services
            services.AddScoped<ISecretsRepository>(provider => {
                var logger = provider.GetRequiredService<ILogger<SecretsRepository>>();
                var databaseService = provider.GetRequiredService<IDatabaseService>();
                var storageProvider = databaseService.GetDefaultProvider();
                return new SecretsRepository(logger, storageProvider);
            });
            services.AddScoped<ISecretsService, SecretsService>();

            // Function services
            services.AddFunctionServices(Configuration);

            // Price feed services
            services.AddScoped<PriceRepository>();
            services.AddScoped<IPriceRepository>(serviceProvider => {
                var logger = serviceProvider.GetRequiredService<ILogger<CachedPriceRepository>>();
                var repository = serviceProvider.GetRequiredService<PriceRepository>();
                var databaseService = serviceProvider.GetRequiredService<IDatabaseService>();
                var storageProvider = databaseService.GetProviderByName("Redis").GetAwaiter().GetResult();

                if (storageProvider != null && storageProvider is ICacheStorageProvider cacheProvider)
                {
                    return new CachedPriceRepository(logger, repository, cacheProvider);
                }
                else
                {
                    return repository;
                }
            });
            services.AddScoped<IPriceSourceRepository, PriceSourceRepository>();
            services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
            services.AddScoped<IPriceFeedService, PriceFeedService>();

            // Storage services
            var storageConfig = Configuration.GetSection("Storage").Get<StorageConfiguration>();
            if (storageConfig?.DefaultProvider == "S3Storage")
            {
                services.AddSingleton<IFileStorageService, S3FileStorageService>();
            }
            else
            {
                services.AddSingleton<IFileStorageService, FileStorageService>();
            }
            services.AddSingleton<IDatabaseService, DatabaseService>();

            // Register database metrics collector
            services.AddSingleton<DatabaseMetricsCollector>();

            // Register database backup service
            services.AddSingleton<DatabaseBackupService>();

            // Register database migration service
            services.AddSingleton<DatabaseMigrationService>();

            // Register circuit breaker factory
            services.AddSingleton<CircuitBreakerFactory>();

            // Register MongoDB connection pool
            services.AddSingleton<MongoDbConnectionPool>();

            // Register MongoDB sharding manager
            services.AddSingleton<MongoDbShardingManager>();

            // Register storage providers for database service
            var databaseConfig = Configuration.GetSection("Database").Get<DatabaseConfiguration>();
            if (databaseConfig?.Providers != null)
            {
                // Register S3 storage provider
                if (databaseConfig.Providers.Any(p => p.Type.Equals("S3", StringComparison.OrdinalIgnoreCase)))
                {
                    services.AddSingleton<S3StorageProvider>();
                    services.AddSingleton<IStorageProvider>(provider =>
                    {
                        var innerProvider = provider.GetRequiredService<S3StorageProvider>();
                        var metricsCollector = provider.GetRequiredService<DatabaseMetricsCollector>();
                        var logger = provider.GetRequiredService<ILogger<MetricsStorageProviderDecorator>>();
                        return new MetricsStorageProviderDecorator(innerProvider, metricsCollector, logger);
                    });
                }

                // Register MongoDB storage provider
                if (databaseConfig.Providers.Any(p => p.Type.Equals("MongoDB", StringComparison.OrdinalIgnoreCase)))
                {
                    services.AddSingleton<MongoDbStorageProvider>();
                    services.AddSingleton<IStorageProvider>(provider =>
                    {
                        var innerProvider = provider.GetRequiredService<MongoDbStorageProvider>();
                        var metricsCollector = provider.GetRequiredService<DatabaseMetricsCollector>();
                        var metricsLogger = provider.GetRequiredService<ILogger<MetricsStorageProviderDecorator>>();
                        var metricsDecorator = new MetricsStorageProviderDecorator(innerProvider, metricsCollector, metricsLogger);

                        var circuitBreakerFactory = provider.GetRequiredService<CircuitBreakerFactory>();
                        var circuitBreakerLogger = provider.GetRequiredService<ILogger<CircuitBreakerStorageProviderDecorator>>();
                        var circuitBreakerConfig = provider.GetRequiredService<IOptions<CircuitBreakerConfiguration>>();
                        return new CircuitBreakerStorageProviderDecorator(metricsDecorator, circuitBreakerFactory, circuitBreakerLogger, circuitBreakerConfig);
                    });
                }

                // Register Redis storage provider
                if (databaseConfig.Providers.Any(p => p.Type.Equals("Redis", StringComparison.OrdinalIgnoreCase)))
                {
                    services.AddSingleton<RedisStorageProvider>();
                    services.AddSingleton<IStorageProvider>(provider =>
                    {
                        var innerProvider = provider.GetRequiredService<RedisStorageProvider>();
                        var metricsCollector = provider.GetRequiredService<DatabaseMetricsCollector>();
                        var metricsLogger = provider.GetRequiredService<ILogger<MetricsStorageProviderDecorator>>();
                        var metricsDecorator = new MetricsStorageProviderDecorator(innerProvider, metricsCollector, metricsLogger);

                        var circuitBreakerFactory = provider.GetRequiredService<CircuitBreakerFactory>();
                        var circuitBreakerLogger = provider.GetRequiredService<ILogger<CircuitBreakerStorageProviderDecorator>>();
                        var circuitBreakerConfig = provider.GetRequiredService<IOptions<CircuitBreakerConfiguration>>();
                        return new CircuitBreakerStorageProviderDecorator(metricsDecorator, circuitBreakerFactory, circuitBreakerLogger, circuitBreakerConfig);
                    });
                }

                // Register InMemory storage provider
                if (databaseConfig.Providers.Any(p => p.Type.Equals("InMemory", StringComparison.OrdinalIgnoreCase)))
                {
                    services.AddSingleton<InMemoryStorageProvider>();
                    services.AddSingleton<IStorageProvider>(provider =>
                    {
                        var innerProvider = provider.GetRequiredService<InMemoryStorageProvider>();
                        var metricsCollector = provider.GetRequiredService<DatabaseMetricsCollector>();
                        var metricsLogger = provider.GetRequiredService<ILogger<MetricsStorageProviderDecorator>>();
                        var metricsDecorator = new MetricsStorageProviderDecorator(innerProvider, metricsCollector, metricsLogger);

                        var circuitBreakerFactory = provider.GetRequiredService<CircuitBreakerFactory>();
                        var circuitBreakerLogger = provider.GetRequiredService<ILogger<CircuitBreakerStorageProviderDecorator>>();
                        var circuitBreakerConfig = provider.GetRequiredService<IOptions<CircuitBreakerConfiguration>>();
                        return new CircuitBreakerStorageProviderDecorator(metricsDecorator, circuitBreakerFactory, circuitBreakerLogger, circuitBreakerConfig);
                    });
                }

                // Register File storage provider
                if (databaseConfig.Providers.Any(p => p.Type.Equals("File", StringComparison.OrdinalIgnoreCase)))
                {
                    services.AddSingleton<FileStorageProvider>();
                    services.AddSingleton<IStorageProvider>(provider =>
                    {
                        var innerProvider = provider.GetRequiredService<FileStorageProvider>();
                        var metricsCollector = provider.GetRequiredService<DatabaseMetricsCollector>();
                        var metricsLogger = provider.GetRequiredService<ILogger<MetricsStorageProviderDecorator>>();
                        var metricsDecorator = new MetricsStorageProviderDecorator(innerProvider, metricsCollector, metricsLogger);

                        var circuitBreakerFactory = provider.GetRequiredService<CircuitBreakerFactory>();
                        var circuitBreakerLogger = provider.GetRequiredService<ILogger<CircuitBreakerStorageProviderDecorator>>();
                        var circuitBreakerConfig = provider.GetRequiredService<IOptions<CircuitBreakerConfiguration>>();
                        return new CircuitBreakerStorageProviderDecorator(metricsDecorator, circuitBreakerFactory, circuitBreakerLogger, circuitBreakerConfig);
                    });
                }
            }

            // Event monitoring services
            services.AddScoped<IEventSubscriptionRepository, EventSubscriptionRepository>();
            services.AddScoped<IEventLogRepository, EventLogRepository>();
            services.AddSingleton<IEventMonitoringService, EventMonitoringService>();
            services.Configure<EventMonitoringConfiguration>(Configuration.GetSection("EventMonitoring"));

            // Notification services
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
            services.AddScoped<IUserNotificationPreferencesRepository, UserNotificationPreferencesRepository>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.Configure<NotificationConfiguration>(Configuration.GetSection("Notification"));

            // Notification providers
            services.AddSingleton<INotificationProvider, EmailNotificationProvider>();
            services.AddSingleton<INotificationProvider, SmsNotificationProvider>();
            services.AddSingleton<INotificationProvider, PushNotificationProvider>();
            services.AddSingleton<INotificationProvider, WebhookNotificationProvider>();
            services.AddSingleton<INotificationProvider, InAppNotificationProvider>();

            // Analytics services
            services.AddScoped<IMetricRepository, MetricRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IAlertRepository, AlertRepository>();
            services.AddSingleton<IAnalyticsService, AnalyticsService>();
            services.Configure<AnalyticsConfiguration>(Configuration.GetSection("Analytics"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Initialize database service
            _databaseService = app.ApplicationServices.GetRequiredService<IDatabaseService>();

            // Use performance monitoring middleware
            app.UsePerformanceMonitoring();

            // Use request logging middleware
            app.UseRequestLogging();

            // Use rate limiting middleware
            app.UseRateLimiting();

            // Use distributed tracing middleware
            app.UseDistributedTracing();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Neo Service Layer API v1");

                    // Add extended function API endpoints
                    app.UseFunctionExtendedApiSwaggerUI();
                });
            }
            else
            {
                // Use global error handling middleware in production
                app.UseGlobalErrorHandling();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Use health checks
            app.UseHealthChecks(Configuration);
        }
    }
}
