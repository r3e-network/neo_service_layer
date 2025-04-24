using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Enclave.Host
{
    /// <summary>
    /// Entry point for the enclave host application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static async Task Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                var logger = host.Services.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Starting enclave host application");

                // Initialize the enclave
                var enclaveService = host.Services.GetRequiredService<IEnclaveService>();
                var initialized = await enclaveService.InitializeAsync();

                if (!initialized)
                {
                    logger.LogError("Failed to initialize enclave");
                    Environment.Exit(1);
                }

                logger.LogInformation("Enclave initialized successfully");

                // Run the host
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error starting enclave host application: {ex}");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Creates the host builder
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Host builder</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Add enclave services
                    services.AddSingleton<VsockClient>();
                    services.AddSingleton<EnclaveManager>();
                    services.AddSingleton<IEnclaveService, EnclaveService>();

                    // Add hosted services
                    services.AddHostedService<EnclaveHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                });
    }
}
