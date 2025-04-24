using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Enclave.Enclave.Models;

namespace NeoServiceLayer.Tests.Mocks
{
    public class MockEnclaveService : IEnclaveService
    {
        private readonly Dictionary<string, Dictionary<string, Func<object, object>>> _handlers = new();
        private readonly MockEnclaveWalletService _walletService = new MockEnclaveWalletService();
        private readonly MockEnclaveSecretsService _secretsService = new MockEnclaveSecretsService();
        private readonly MockEnclavePriceFeedService _priceFeedService = new MockEnclavePriceFeedService();

        public void RegisterHandler<TRequest, TResponse>(string serviceType, string operation, Func<TRequest, TResponse> handler)
        {
            if (!_handlers.ContainsKey(serviceType))
            {
                _handlers[serviceType] = new Dictionary<string, Func<object, object>>();
            }

            _handlers[serviceType][operation] = request => handler((TRequest)request);
        }

        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(string serviceType, string operation, TRequest request)
        {
            // First check if there's a registered handler
            if (_handlers.TryGetValue(serviceType, out var operations) &&
                operations.TryGetValue(operation, out var handler))
            {
                return (TResponse)handler(request);
            }

            // If no handler is registered, use the mock services
            var requestJson = JsonSerializer.Serialize(request);
            var requestBytes = System.Text.Encoding.UTF8.GetBytes(requestJson);

            var enclaveRequest = new EnclaveRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Operation = operation,
                Payload = requestBytes
            };

            EnclaveResponse response;
            switch (serviceType.ToLower())
            {
                case "wallet":
                    response = await _walletService.ProcessRequestAsync(enclaveRequest);
                    break;
                case "secrets":
                    response = await _secretsService.ProcessRequestAsync(enclaveRequest);
                    break;
                case "pricefeed":
                    response = await _priceFeedService.ProcessRequestAsync(enclaveRequest);
                    break;
                default:
                    return default;
            }

            if (!response.Success || response.Payload == null)
            {
                return default;
            }

            try
            {
                if (typeof(TResponse) == typeof(object))
                {
                    return (TResponse)JsonSerializer.Deserialize<object>(System.Text.Encoding.UTF8.GetString(response.Payload));
                }
                return JsonSerializer.Deserialize<TResponse>(System.Text.Encoding.UTF8.GetString(response.Payload));
            }
            catch
            {
                return default;
            }
        }

        public Task<bool> InitializeAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> ShutdownAsync()
        {
            return Task.FromResult(true);
        }

        public Task<byte[]> GetAttestationDocumentAsync()
        {
            return Task.FromResult(new byte[0]);
        }

        public Task<bool> VerifyAttestationDocumentAsync(byte[] attestationDocument)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetStatusAsync()
        {
            return Task.FromResult("Running");
        }

        public Task<object> GetMetricsAsync()
        {
            return Task.FromResult<object>(new { CpuUsage = 0, MemoryUsage = 0 });
        }
    }
}
