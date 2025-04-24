using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Enclave.Enclave;
using NeoServiceLayer.Enclave.Enclave.Services;
using NeoServiceLayer.Enclave.Enclave.Execution;

namespace NeoServiceLayer.Enclave
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Neo Service Layer Enclave...");
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            try
            {
                // Setup dependency injection
                var services = new ServiceCollection();

                // Add logging
                services.AddLogging(configure => configure.AddConsole());

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

                // Add VsockServer
                services.AddSingleton<VsockServer>();

                // Build service provider
                var serviceProvider = services.BuildServiceProvider();

                // Get logger
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Starting Neo Service Layer Enclave...");

                // Get VsockServer
                var vsockServer = serviceProvider.GetRequiredService<VsockServer>();

                // Start VsockServer
                vsockServer.Start();

                logger.LogInformation("Neo Service Layer Enclave started. Press Ctrl+C to exit.");

                // Wait for cancellation
                var cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) =>
                {
                    logger.LogInformation("Stopping Neo Service Layer Enclave...");
                    cancellationTokenSource.Cancel();
                    e.Cancel = true;
                };

                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // Expected when cancellation is requested
                }

                // Stop VsockServer
                vsockServer.Stop();

                logger.LogInformation("Neo Service Layer Enclave stopped.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error starting Neo Service Layer Enclave: {ex}");
                Environment.Exit(1);
            }
        }
    }
}
