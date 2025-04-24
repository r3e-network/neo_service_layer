using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Enclave.Enclave;
using NeoServiceLayer.Enclave.Enclave.Models;
using NeoServiceLayer.Enclave.Enclave.Services;
using NeoServiceLayer.Tests.Mocks;
using Xunit;

namespace NeoServiceLayer.Tests.Integration
{
    public class EnclaveServicesIntegrationTests
    {
        private readonly MockEnclaveWalletService _walletService;
        private readonly MockEnclaveSecretsService _secretsService;
        private readonly MockEnclavePriceFeedService _priceFeedService;

        public EnclaveServicesIntegrationTests()
        {
            _walletService = new MockEnclaveWalletService();
            _secretsService = new MockEnclaveSecretsService();
            _priceFeedService = new MockEnclavePriceFeedService();
        }

        [Fact]
        public async Task EndToEnd_CreateWalletAndSubmitPrice_Success()
        {
            // Arrange

            // Step 1: Create a wallet
            var accountId = Guid.NewGuid();
            var createWalletRequest = new
            {
                Name = "Oracle Wallet",
                AccountId = accountId,
                Password = "StrongPassword123!",
                Tags = new { Type = "oracle" }
            };
            var createWalletPayload = JsonSerializer.SerializeToUtf8Bytes(createWalletRequest);

            // Act - Create wallet
            var walletEnclaveRequest = new NeoServiceLayer.Enclave.Enclave.Models.EnclaveRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Operation = Constants.WalletOperations.CreateWallet,
                Payload = createWalletPayload
            };
            var walletResponse = await _walletService.ProcessRequestAsync(walletEnclaveRequest);
            var walletResponseObj = JsonSerializer.Deserialize<JsonElement>(System.Text.Encoding.UTF8.GetString(walletResponse.Payload));
            var walletId = Guid.Parse(walletResponseObj.GetProperty("Id").GetString());

            // Assert - Wallet created successfully
            Assert.NotEqual(Guid.Empty, walletId);
            Assert.NotNull(walletResponseObj.GetProperty("Name").GetString());

            // Step 2: Submit a price to the oracle
            var price = new Price
            {
                Id = Guid.NewGuid(),
                Symbol = "BTC",
                BaseCurrency = "USD",
                Value = 50000.0m,
                Timestamp = DateTime.UtcNow,
                ConfidenceScore = 100,
                CreatedAt = DateTime.UtcNow
            };

            var submitPriceRequest = new
            {
                Price = price,
                WalletId = walletId,
                AccountId = accountId,
                Password = createWalletRequest.Password,
                Network = "TestNet"
            };

            // Act - Submit price
            var enclaveRequest = new NeoServiceLayer.Enclave.Enclave.Models.EnclaveRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Operation = Constants.PriceFeedOperations.SubmitToOracle,
                Payload = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(submitPriceRequest))
            };
            var priceResponse = await _priceFeedService.ProcessRequestAsync(enclaveRequest);
            var priceResponseObj = JsonSerializer.Deserialize<dynamic>(System.Text.Encoding.UTF8.GetString(priceResponse.Payload));

            // Assert - Price submitted successfully
            Assert.NotNull(priceResponse);
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(System.Text.Encoding.UTF8.GetString(priceResponse.Payload));
            Assert.True(jsonElement.TryGetProperty("TransactionHash", out var txHash));
            Assert.NotNull(txHash.GetString());
            Assert.StartsWith("0x", txHash.GetString());
        }

        [Fact]
        public async Task EndToEnd_CreateSecretAndCheckAccess_Success()
        {
            // Arrange

            // Step 1: Create a secret
            var accountId = Guid.NewGuid();
            var functionId = Guid.NewGuid();
            var createSecretRequest = new
            {
                Name = "API Key",
                Description = "API key for external service",
                Value = "secret-api-key-123",
                AccountId = accountId,
                AllowedFunctionIds = new[] { functionId },
                Tags = new Dictionary<string, string> { { "type", "api-key" } },
                RotationPeriod = 90
            };
            var createSecretPayload = JsonSerializer.SerializeToUtf8Bytes(createSecretRequest);

            // Act - Create secret
            var secretEnclaveRequest = new NeoServiceLayer.Enclave.Enclave.Models.EnclaveRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Operation = Constants.SecretsOperations.CreateSecret,
                Payload = createSecretPayload
            };
            var secretResponse = await _secretsService.ProcessRequestAsync(secretEnclaveRequest);
            var secretResponseObj = JsonSerializer.Deserialize<JsonElement>(System.Text.Encoding.UTF8.GetString(secretResponse.Payload));
            var secretId = Guid.Parse(secretResponseObj.GetProperty("Id").GetString());

            // Assert - Secret created successfully
            Assert.NotEqual(Guid.Empty, secretId);
            Assert.NotNull(secretResponseObj.GetProperty("Name").GetString());

            // Step 2: Check access to the secret
            var checkAccessRequest = new
            {
                SecretId = secretId,
                AccountId = accountId,
                FunctionId = functionId
            };
            var checkAccessPayload = JsonSerializer.SerializeToUtf8Bytes(checkAccessRequest);

            // Act - Check access
            var accessEnclaveRequest = new NeoServiceLayer.Enclave.Enclave.Models.EnclaveRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Operation = Constants.SecretsOperations.HasAccess,
                Payload = checkAccessPayload
            };
            var accessResponse = await _secretsService.ProcessRequestAsync(accessEnclaveRequest);
            var accessResponseObj = JsonSerializer.Deserialize<JsonElement>(System.Text.Encoding.UTF8.GetString(accessResponse.Payload));

            // Assert - Access check successful
            Assert.True(accessResponseObj.GetProperty("HasAccess").GetBoolean());
            Assert.NotNull(accessResponseObj.GetProperty("SecretId").GetString());
            Assert.NotNull(accessResponseObj.GetProperty("FunctionId").GetString());

            // Step 3: Get the secret value
            var getSecretRequest = new
            {
                SecretId = secretId,
                AccountId = accountId,
                FunctionId = functionId
            };
            var getSecretPayload = JsonSerializer.SerializeToUtf8Bytes(getSecretRequest);

            // Act - Get secret value
            var getEnclaveRequest = new NeoServiceLayer.Enclave.Enclave.Models.EnclaveRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Operation = Constants.SecretsOperations.GetSecretValue,
                Payload = getSecretPayload
            };
            var getResponse = await _secretsService.ProcessRequestAsync(getEnclaveRequest);
            var getResponseObj = JsonSerializer.Deserialize<JsonElement>(System.Text.Encoding.UTF8.GetString(getResponse.Payload));

            // Assert - Secret value retrieved successfully
            Assert.NotNull(getResponseObj.GetProperty("Id").GetString());
            Assert.NotNull(getResponseObj.GetProperty("Name").GetString());
            Assert.NotNull(getResponseObj.GetProperty("Value").GetString());
        }
    }
}
