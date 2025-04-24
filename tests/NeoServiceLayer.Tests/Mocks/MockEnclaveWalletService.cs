using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NeoServiceLayer.Core;
using NeoServiceLayer.Enclave.Enclave.Models;

namespace NeoServiceLayer.Tests.Mocks
{
    public class MockEnclaveWalletService
    {
        public async Task<EnclaveResponse> ProcessRequestAsync(EnclaveRequest request)
        {
            var requestId = request.RequestId ?? Guid.NewGuid().ToString();

            try
            {
                // Create a mock response based on the operation
                switch (request.Operation)
                {
                    case Constants.WalletOperations.CreateWallet:
                        return CreateWalletResponse(requestId);
                    case Constants.WalletOperations.ImportFromWIF:
                        return ImportFromWIFResponse(requestId);
                    case Constants.WalletOperations.SignData:
                        return SignDataResponse(requestId);
                    case Constants.WalletOperations.TransferNeo:
                        return TransferNeoResponse(requestId);
                    case Constants.WalletOperations.TransferGas:
                        return TransferGasResponse(requestId);
                    case Constants.WalletOperations.TransferToken:
                        return TransferTokenResponse(requestId);
                    default:
                        return new EnclaveResponse
                        {
                            RequestId = requestId,
                            Success = false,
                            ErrorMessage = $"Failed to process wallet request: {request.Operation}"
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

        private EnclaveResponse CreateWalletResponse(string requestId)
        {
            var response = new
            {
                Id = Guid.NewGuid(),
                Name = "Test Wallet",
                AccountId = Guid.NewGuid(),
                Address = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                ScriptHash = "0x1234567890abcdef1234567890abcdef12345678",
                PublicKey = "02a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2",
                Tags = new { Type = "personal" },
                CreatedAt = DateTime.UtcNow
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse ImportFromWIFResponse(string requestId)
        {
            var response = new
            {
                Id = Guid.NewGuid(),
                Name = "Imported Wallet",
                AccountId = Guid.NewGuid(),
                Address = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                ScriptHash = "0x1234567890abcdef1234567890abcdef12345678",
                PublicKey = "02a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2",
                Tags = new { Type = "imported" },
                CreatedAt = DateTime.UtcNow
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse SignDataResponse(string requestId)
        {
            var response = new
            {
                WalletId = Guid.NewGuid(),
                Address = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                Signature = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse TransferNeoResponse(string requestId)
        {
            var response = new
            {
                WalletId = Guid.NewGuid(),
                FromAddress = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                ToAddress = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                Amount = 10.0m,
                Asset = "NEO",
                TransactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
                Network = "TestNet"
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse TransferGasResponse(string requestId)
        {
            var response = new
            {
                WalletId = Guid.NewGuid(),
                FromAddress = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                ToAddress = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                Amount = 5.5m,
                Asset = "GAS",
                TransactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
                Network = "TestNet"
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse TransferTokenResponse(string requestId)
        {
            var response = new
            {
                WalletId = Guid.NewGuid(),
                FromAddress = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                ToAddress = "NXV7ZhHiyMn9SLdRcgYE8S7GZY4PjuLxrA",
                TokenScriptHash = "0x1234567890abcdef1234567890abcdef12345678",
                Amount = 100.0m,
                TransactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
                Network = "TestNet"
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
