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

            _handlers[serviceType][operation] = request =>
            {
                try
                {
                    return handler((TRequest)request);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in handler for {serviceType}/{operation}: {ex.Message}");
                    throw;
                }
            };
        }

        public Task<TResponse> SendRequestAsync<TRequest, TResponse>(string serviceType, string operation, TRequest request)
        {
            if (_handlers.TryGetValue(serviceType, out var operations) &&
                operations.TryGetValue(operation, out var handler))
            {
                try
                {
                    var result = handler(request);
                    return Task.FromResult((TResponse)result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in SendRequestAsync for {serviceType}/{operation}: {ex.Message}");
                    throw;
                }
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

        public Task<FunctionExecutionResult> ExecuteAsync(Guid functionId, object input, FunctionExecutionContext context)
        {
            return Task.FromResult(new FunctionExecutionResult
            {
                Success = true,
                Result = new { Message = "Function executed successfully" }
            });
        }

        public Task<FunctionExecutionResult> ExecuteByNameAsync(Guid accountId, string functionName, object input, FunctionExecutionContext context)
        {
            return Task.FromResult(new FunctionExecutionResult
            {
                Success = true,
                Result = new { Message = "Function executed successfully" }
            });
        }

        public Task<FunctionExecutionResult> ExecuteSourceAsync(string source, string runtime, string handler, object input, FunctionExecutionContext context)
        {
            return Task.FromResult(new FunctionExecutionResult
            {
                Success = true,
                Result = new { Message = "Function executed successfully" }
            });
        }

        public Task<FunctionValidationResult> ValidateAsync(string source, string runtime, string handler)
        {
            return Task.FromResult(new FunctionValidationResult
            {
                IsValid = true,
                Messages = new List<string> { "Validation successful" }
            });
        }

        public Task<IEnumerable<string>> GetSupportedRuntimesAsync()
        {
            return Task.FromResult<IEnumerable<string>>(new[] { "node", "dotnet", "python" });
        }

        public Task<FunctionRuntimeDetails> GetRuntimeDetailsAsync(string runtime)
        {
            return Task.FromResult(new FunctionRuntimeDetails
            {
                Name = runtime,
                Version = "1.0.0",
                Description = $"{runtime} runtime",
                SupportedLanguages = new[] { runtime }
            });
        }
    }
}
