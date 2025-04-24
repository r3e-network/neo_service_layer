using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using NeoServiceLayer.Core;
using NeoServiceLayer.Enclave.Enclave.Models;

namespace NeoServiceLayer.Tests.Mocks
{
    public class MockEnclavePriceFeedService
    {
        public async Task<EnclaveResponse> ProcessRequestAsync(EnclaveRequest request)
        {
            var requestId = request.RequestId ?? Guid.NewGuid().ToString();

            try
            {
                // Create a mock response based on the operation
                switch (request.Operation)
                {
                    case Constants.PriceFeedOperations.SubmitToOracle:
                        return SubmitToOracleResponse(requestId);
                    case Constants.PriceFeedOperations.SubmitBatchToOracle:
                        return SubmitBatchToOracleResponse(requestId);
                    case Constants.PriceFeedOperations.ValidateSource:
                        return ValidateSourceResponse(requestId);
                    default:
                        return new EnclaveResponse
                        {
                            RequestId = requestId,
                            Success = false,
                            ErrorMessage = $"Failed to handle price feed request: {request.Operation}"
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

        private EnclaveResponse SubmitToOracleResponse(string requestId)
        {
            var response = new
            {
                Symbol = "BTC/USD",
                Price = 50000.00m,
                Timestamp = DateTime.UtcNow,
                Source = "TestSource",
                TransactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
                Signature = "0x9876543210fedcba9876543210fedcba9876543210fedcba9876543210fedcba"
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse SubmitBatchToOracleResponse(string requestId)
        {
            var response = new
            {
                Prices = new List<object>
                {
                    new
                    {
                        Symbol = "BTC/USD",
                        Price = 50000.00m,
                        Timestamp = DateTime.UtcNow,
                        Source = "TestSource",
                        TransactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
                        Signature = "0x9876543210fedcba9876543210fedcba9876543210fedcba9876543210fedcba"
                    },
                    new
                    {
                        Symbol = "ETH/USD",
                        Price = 3000.00m,
                        Timestamp = DateTime.UtcNow,
                        Source = "TestSource",
                        TransactionHash = "0xabcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890",
                        Signature = "0xfedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210"
                    }
                },
                BatchTransactionHash = "0x0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
            };

            return new EnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(response)
            };
        }

        private EnclaveResponse ValidateSourceResponse(string requestId)
        {
            var response = new
            {
                SourceId = Guid.NewGuid(),
                Name = "TestSource",
                IsValid = true,
                Message = "Source is valid and can be used for price feeds"
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
