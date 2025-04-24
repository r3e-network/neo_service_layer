using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.ServiceTests.Mocks
{
    public class MockEnclaveService : IEnclaveService
    {
        private readonly Dictionary<string, Dictionary<string, Func<object, object>>> _handlers = new();

        public void RegisterHandler<TRequest, TResponse>(string serviceType, string operation, Func<TRequest, TResponse> handler)
        {
            if (!_handlers.ContainsKey(serviceType))
            {
                _handlers[serviceType] = new Dictionary<string, Func<object, object>>();
            }

            _handlers[serviceType][operation] = request => handler((TRequest)request);
        }

        public Task<TResponse> SendRequestAsync<TRequest, TResponse>(string serviceType, string operation, TRequest request)
        {
            if (_handlers.TryGetValue(serviceType, out var operations) &&
                operations.TryGetValue(operation, out var handler))
            {
                return Task.FromResult((TResponse)handler(request));
            }

            return Task.FromResult(default(TResponse));
        }

        public Task<bool> InitializeAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> HealthCheckAsync()
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
            return Task.FromResult<object>(new { CPU = 0.1, Memory = 100 });
        }
    }
}
