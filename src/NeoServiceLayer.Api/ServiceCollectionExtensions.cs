using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage;
using System;
using System.Threading.Tasks;

namespace NeoServiceLayer.Api
{
    public static class ServiceCollectionExtensions
    {
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<DatabaseService>>();
            var databaseService = serviceProvider.GetRequiredService<IDatabaseService>();

            try
            {
                logger.LogInformation("Initializing database providers...");
                await databaseService.InitializeProvidersAsync();
                logger.LogInformation("Database providers initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initializing database providers");
                throw;
            }
        }
    }
}
