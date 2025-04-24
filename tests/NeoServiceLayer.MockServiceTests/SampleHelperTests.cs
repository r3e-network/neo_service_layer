using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.MockServiceTests.TestFixtures;
using NeoServiceLayer.MockServiceTests.TestHelpers;
using Xunit;

namespace NeoServiceLayer.MockServiceTests
{
    public class SampleHelperTests : IClassFixture<MockServiceTestFixture>
    {
        private readonly MockServiceTestFixture _fixture;
        private readonly IAccountService _accountService;
        private readonly IWalletService _walletService;
        private readonly ISecretsService _secretsService;
        private readonly IFunctionService _functionService;
        private readonly IPriceFeedService _priceFeedService;

        public SampleHelperTests(MockServiceTestFixture fixture)
        {
            _fixture = fixture;
            _accountService = _fixture.ServiceProvider.GetRequiredService<IAccountService>();
            _walletService = _fixture.ServiceProvider.GetRequiredService<IWalletService>();
            _secretsService = _fixture.ServiceProvider.GetRequiredService<ISecretsService>();
            _functionService = _fixture.ServiceProvider.GetRequiredService<IFunctionService>();
            _priceFeedService = _fixture.ServiceProvider.GetRequiredService<IPriceFeedService>();
        }

        [Fact]
        public async Task TestDataGenerator_CreatesValidTestData()
        {
            // Generate test data
            var account = TestDataGenerator.CreateTestAccount();
            var wallet = TestDataGenerator.CreateTestWallet(accountId: account.Id);
            var secret = TestDataGenerator.CreateTestSecret(accountId: account.Id);
            var function = TestDataGenerator.CreateTestFunction(accountId: account.Id);
            var priceSource = TestDataGenerator.CreateTestPriceSource();
            var price = TestDataGenerator.CreateTestPrice();
            var priceHistory = TestDataGenerator.CreateTestPriceHistory();

            // Assert test data is valid
            Assert.NotEqual(Guid.Empty, account.Id);
            Assert.NotEmpty(account.Username);
            Assert.NotEmpty(account.Email);

            Assert.NotEqual(Guid.Empty, wallet.Id);
            Assert.NotEmpty(wallet.Name);
            Assert.Equal(account.Id, wallet.AccountId);

            Assert.NotEqual(Guid.Empty, secret.Id);
            Assert.NotEmpty(secret.Name);
            Assert.Equal(account.Id, secret.AccountId);

            Assert.NotEqual(Guid.Empty, function.Id);
            Assert.NotEmpty(function.Name);
            Assert.Equal(account.Id, function.AccountId);

            Assert.NotEqual(Guid.Empty, priceSource.Id);
            Assert.NotEmpty(priceSource.Name);
            Assert.NotEmpty(priceSource.Url);

            Assert.NotEqual(Guid.Empty, price.Id);
            Assert.NotEmpty(price.Symbol);
            Assert.NotEmpty(price.BaseCurrency);
            Assert.True(price.Value > 0);

            Assert.NotEqual(Guid.Empty, priceHistory.Id);
            Assert.NotEmpty(priceHistory.Symbol);
            Assert.NotEmpty(priceHistory.BaseCurrency);
            Assert.NotEmpty(priceHistory.Interval);
            Assert.True(priceHistory.DataPoints.Count > 0);
        }

        [Fact]
        public async Task MockSetupHelper_SetsUpMocksCorrectly()
        {
            // Generate test data
            var account = TestDataGenerator.CreateTestAccount();
            var wallet = TestDataGenerator.CreateTestWallet(accountId: account.Id);
            var secret = TestDataGenerator.CreateTestSecret(accountId: account.Id);
            var function = TestDataGenerator.CreateTestFunction(accountId: account.Id);
            var priceSource = TestDataGenerator.CreateTestPriceSource();
            var price = TestDataGenerator.CreateTestPrice();
            var priceHistory = TestDataGenerator.CreateTestPriceHistory();

            // Add function ID to secret's allowed functions
            secret.AllowedFunctionIds.Add(function.Id);

            // Setup mocks
            MockSetupHelper.SetupAccountServiceMocks(_fixture.AccountServiceMock, account);
            MockSetupHelper.SetupWalletServiceMocks(_fixture.WalletServiceMock, wallet);
            MockSetupHelper.SetupSecretsServiceMocks(_fixture.SecretsServiceMock, secret, function);
            MockSetupHelper.SetupFunctionServiceMocks(_fixture.FunctionServiceMock, function);
            MockSetupHelper.SetupPriceFeedServiceMocks(_fixture.PriceFeedServiceMock, priceSource, price, priceHistory);

            // Test account service mocks
            var retrievedAccount = await _accountService.GetByIdAsync(account.Id);
            Assert.NotNull(retrievedAccount);
            Assert.Equal(account.Id, retrievedAccount.Id);
            Assert.Equal(account.Username, retrievedAccount.Username);

            var accountByUsername = await _accountService.GetByUsernameAsync(account.Username);
            Assert.NotNull(accountByUsername);
            Assert.Equal(account.Id, accountByUsername.Id);

            var accountByEmail = await _accountService.GetByEmailAsync(account.Email);
            Assert.NotNull(accountByEmail);
            Assert.Equal(account.Id, accountByEmail.Id);

            var verificationResult = await _accountService.VerifyAccountAsync(account.Id);
            Assert.True(verificationResult);

            var creditsToAdd = 50.0m;
            var accountWithCredits = await _accountService.AddCreditsAsync(account.Id, creditsToAdd);
            Assert.Equal(account.Credits + creditsToAdd, accountWithCredits.Credits);

            var creditsToDeduct = 20.0m;
            var accountAfterDeduction = await _accountService.DeductCreditsAsync(account.Id, creditsToDeduct);
            Assert.Equal(account.Credits - creditsToDeduct, accountAfterDeduction.Credits);

            // Test wallet service mocks
            var retrievedWallet = await _walletService.GetByIdAsync(wallet.Id);
            Assert.NotNull(retrievedWallet);
            Assert.Equal(wallet.Id, retrievedWallet.Id);
            Assert.Equal(wallet.Name, retrievedWallet.Name);

            var walletByAddress = await _walletService.GetByAddressAsync(wallet.Address);
            Assert.NotNull(walletByAddress);
            Assert.Equal(wallet.Id, walletByAddress.Id);

            var walletByScriptHash = await _walletService.GetByScriptHashAsync(wallet.ScriptHash);
            Assert.NotNull(walletByScriptHash);
            Assert.Equal(wallet.Id, walletByScriptHash.Id);

            var walletsByAccount = await _walletService.GetByAccountIdAsync(account.Id);
            Assert.NotEmpty(walletsByAccount);
            Assert.Contains(walletsByAccount, w => w.Id == wallet.Id);

            var neoBalance = await _walletService.GetNeoBalanceAsync(wallet.Address);
            Assert.Equal(10.0m, neoBalance);

            var gasBalance = await _walletService.GetGasBalanceAsync(wallet.Address);
            Assert.Equal(100.0m, gasBalance);

            // Test secrets service mocks
            var retrievedSecret = await _secretsService.GetByIdAsync(secret.Id);
            Assert.NotNull(retrievedSecret);
            Assert.Equal(secret.Id, retrievedSecret.Id);
            Assert.Equal(secret.Name, retrievedSecret.Name);

            var secretByName = await _secretsService.GetByNameAsync(secret.Name, account.Id);
            Assert.NotNull(secretByName);
            Assert.Equal(secret.Id, secretByName.Id);

            var secretsByAccount = await _secretsService.GetByAccountIdAsync(account.Id);
            Assert.NotEmpty(secretsByAccount);
            Assert.Contains(secretsByAccount, s => s.Id == secret.Id);

            var hasAccess = await _secretsService.HasAccessAsync(secret.Id, function.Id);
            Assert.True(hasAccess);

            var secretValue = await _secretsService.GetSecretValueAsync(secret.Id, function.Id);
            Assert.Equal("decrypted-secret-value", secretValue);

            // Test function service mocks
            var retrievedFunction = await _functionService.GetByIdAsync(function.Id);
            Assert.NotNull(retrievedFunction);
            Assert.Equal(function.Id, retrievedFunction.Id);
            Assert.Equal(function.Name, retrievedFunction.Name);

            var functionByName = await _functionService.GetByNameAsync(function.Name, account.Id);
            Assert.NotNull(functionByName);
            Assert.Equal(function.Id, functionByName.Id);

            var functionsByAccount = await _functionService.GetByAccountIdAsync(account.Id);
            Assert.NotEmpty(functionsByAccount);
            Assert.Contains(functionsByAccount, f => f.Id == function.Id);

            var functionsByRuntime = await _functionService.GetByRuntimeAsync(FunctionRuntime.CSharp);
            Assert.NotEmpty(functionsByRuntime);
            Assert.Contains(functionsByRuntime, f => f.Id == function.Id);

            var executionResult = await _functionService.ExecuteAsync(function.Id, new Dictionary<string, object> { { "param1", "value1" } });
            Assert.NotNull(executionResult);

            // Test price feed service mocks
            var retrievedSource = await _priceFeedService.GetSourceByIdAsync(priceSource.Id);
            Assert.NotNull(retrievedSource);
            Assert.Equal(priceSource.Id, retrievedSource.Id);
            Assert.Equal(priceSource.Name, retrievedSource.Name);

            var sourceByName = await _priceFeedService.GetSourceByNameAsync(priceSource.Name);
            Assert.NotNull(sourceByName);
            Assert.Equal(priceSource.Id, sourceByName.Id);

            var allSources = await _priceFeedService.GetAllSourcesAsync();
            Assert.NotEmpty(allSources);
            Assert.Contains(allSources, s => s.Id == priceSource.Id);

            var activeSources = await _priceFeedService.GetActiveSourcesAsync();
            Assert.NotEmpty(activeSources);
            Assert.Contains(activeSources, s => s.Id == priceSource.Id);

            var latestPrice = await _priceFeedService.GetLatestPriceAsync(price.Symbol, price.BaseCurrency);
            Assert.NotNull(latestPrice);
            Assert.Equal(price.Id, latestPrice.Id);
            Assert.Equal(price.Symbol, latestPrice.Symbol);
            Assert.Equal(price.Value, latestPrice.Value);

            var allPrices = await _priceFeedService.GetAllLatestPricesAsync(price.BaseCurrency);
            Assert.NotEmpty(allPrices);
            Assert.True(allPrices.ContainsKey(price.Symbol));
            Assert.Equal(price.Id, allPrices[price.Symbol].Id);

            var retrievedHistory = await _priceFeedService.GetPriceHistoryAsync(
                priceHistory.Symbol,
                priceHistory.BaseCurrency,
                priceHistory.Interval,
                priceHistory.StartTime,
                priceHistory.EndTime);
            Assert.NotNull(retrievedHistory);
            Assert.Equal(priceHistory.Id, retrievedHistory.Id);
            Assert.Equal(priceHistory.Symbol, retrievedHistory.Symbol);
            Assert.Equal(priceHistory.Interval, retrievedHistory.Interval);
            Assert.NotEmpty(retrievedHistory.DataPoints);
        }
    }
}
