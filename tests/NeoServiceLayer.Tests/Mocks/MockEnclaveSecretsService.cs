using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NeoServiceLayer.Core;
using NeoServiceLayer.Enclave.Enclave.Models;

namespace NeoServiceLayer.Tests.Mocks
{
    public class MockEnclaveSecretsService
    {
        public async Task<EnclaveResponse> ProcessRequestAsync(EnclaveRequest request)
        {
            var requestId = request.RequestId ?? Guid.NewGuid().ToString();

            try
            {
                // Create a mock response based on the operation
                switch (request.Operation)
                {
                    case Constants.SecretsOperations.CreateSecret:
                        return CreateSecretResponse(requestId);
                    case Constants.SecretsOperations.GetSecretValue:
                        return GetSecretValueResponse(requestId);
                    case Constants.SecretsOperations.UpdateValue:
                        return UpdateValueResponse(requestId);
                    case Constants.SecretsOperations.RotateSecret:
                        return RotateSecretResponse(requestId);
                    case Constants.SecretsOperations.HasAccess:
                        return HasAccessResponse(requestId);
                    default:
                        return new EnclaveResponse
                        {
                            RequestId = requestId,
                            Success = false,
                            ErrorMessage = $"Failed to process secrets request: {request.Operation}"
                        };
                }
            }
            catch (Exception ex)
            {
                return new EnclaveResponse
                {
                    RequestId = requestId,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private EnclaveResponse CreateSecretResponse(string requestId)
        {
            var response = new
            {
                Id = Guid.NewGuid(),
                Name = "test-secret",
                Description = "Test secret for unit tests",
                Version = 1,
                RotationPeriod = 90,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastRotatedAt = DateTime.UtcNow,
                NextRotationAt = DateTime.UtcNow.AddDays(90)
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse GetSecretValueResponse(string requestId)
        {
            var response = new
            {
                Id = Guid.NewGuid(),
                Name = "test-secret-get",
                Value = "secret-value-456",
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse UpdateValueResponse(string requestId)
        {
            var response = new
            {
                Id = Guid.NewGuid(),
                Name = "test-secret-update",
                Description = "Updated test secret",
                Version = 2,
                RotationPeriod = 90,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse RotateSecretResponse(string requestId)
        {
            var response = new
            {
                Id = Guid.NewGuid(),
                Name = "test-secret-rotate",
                Description = "Test secret for rotation operation",
                Version = 2,
                RotationPeriod = 90,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                LastRotatedAt = DateTime.UtcNow,
                NextRotationAt = DateTime.UtcNow.AddDays(90)
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse HasAccessResponse(string requestId)
        {
            var response = new
            {
                SecretId = Guid.NewGuid(),
                FunctionId = Guid.NewGuid(),
                HasAccess = true
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }
    }
}
