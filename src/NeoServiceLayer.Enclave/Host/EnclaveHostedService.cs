using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Enclave.Host
{
    /// <summary>
    /// Hosted service for managing the enclave lifecycle
    /// </summary>
    public class EnclaveHostedService : IHostedService
    {
        private readonly ILogger<EnclaveHostedService> _logger;
        private readonly IEnclaveService _enclaveService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnclaveHostedService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="enclaveService">Enclave service</param>
        public EnclaveHostedService(ILogger<EnclaveHostedService> logger, IEnclaveService enclaveService)
        {
            _logger = logger;
            _enclaveService = enclaveService;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Enclave hosted service started");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down enclave");
            
            try
            {
                await _enclaveService.ShutdownAsync();
                _logger.LogInformation("Enclave shut down successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down enclave");
            }
        }
    }
}
