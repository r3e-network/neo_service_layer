using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Enclave.Enclave.Execution;
using NeoServiceLayer.Enclave.Enclave.Services;

namespace NeoServiceLayer.Enclave.Enclave
{
    /// <summary>
    /// Entry point for the enclave application
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

                logger.LogInformation("Starting enclave application");

                // Start the VSOCK server
                var vsockServer = host.Services.GetRequiredService<VsockServer>();
                vsockServer.Start();

                logger.LogInformation("Enclave application started");

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error starting enclave application: {ex}");
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
                    // Add VSOCK server
                    services.AddSingleton<VsockServer>();

                    // Add enclave services
                    services.AddSingleton<EnclaveAccountService>();
                    services.AddSingleton<EnclaveWalletService>();
                    services.AddSingleton<EnclaveSecretsService>();
                    services.AddSingleton<EnclavePriceFeedService>();

                    // Add function execution services
                    services.AddSingleton<NodeJsRuntime>();
                    services.AddSingleton<DotNetRuntime>();
                    services.AddSingleton<PythonRuntime>();
                    services.AddSingleton<FunctionExecutor>();
                    services.AddSingleton<EnclaveFunctionService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                });
    }
}
