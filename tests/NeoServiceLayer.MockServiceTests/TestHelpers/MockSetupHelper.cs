using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.MockServiceTests.TestHelpers
{
    /// <summary>
    /// Helper class for setting up mocks
    /// </summary>
    public static class MockSetupHelper
    {
        /// <summary>
        /// Sets up common account service mocks
        /// </summary>
        public static void SetupAccountServiceMocks(Mock<IAccountService> mock, Account account)
        {
            // Setup GetByIdAsync
            mock.Setup(x => x.GetByIdAsync(account.Id))
                .ReturnsAsync(account);

            // Setup GetByUsernameAsync
            mock.Setup(x => x.GetByUsernameAsync(account.Username))
                .ReturnsAsync(account);

            // Setup GetByEmailAsync
            mock.Setup(x => x.GetByEmailAsync(account.Email))
                .ReturnsAsync(account);

            // Setup VerifyAccountAsync
            mock.Setup(x => x.VerifyAccountAsync(account.Id))
                .ReturnsAsync(true);

            // Setup AddCreditsAsync
            mock.Setup(x => x.AddCreditsAsync(account.Id, It.IsAny<decimal>()))
                .ReturnsAsync((Guid id, decimal amount) =>
                {
                    var updatedAccount = new Account
                    {
                        Id = account.Id,
                        Username = account.Username,
                        Email = account.Email,
                        PasswordHash = account.PasswordHash,
                        PasswordSalt = account.PasswordSalt,
                        NeoAddress = account.NeoAddress,
                        IsVerified = account.IsVerified,
                        IsActive = account.IsActive,
                        Credits = account.Credits + amount,
                        CreatedAt = account.CreatedAt,
                        UpdatedAt = DateTime.UtcNow
                    };
                    return updatedAccount;
                });

            // Setup DeductCreditsAsync
            mock.Setup(x => x.DeductCreditsAsync(account.Id, It.Is<decimal>(a => a <= account.Credits)))
                .ReturnsAsync((Guid id, decimal amount) =>
                {
                    var updatedAccount = new Account
                    {
                        Id = account.Id,
                        Username = account.Username,
                        Email = account.Email,
                        PasswordHash = account.PasswordHash,
                        PasswordSalt = account.PasswordSalt,
                        NeoAddress = account.NeoAddress,
                        IsVerified = account.IsVerified,
                        IsActive = account.IsActive,
                        Credits = account.Credits - amount,
                        CreatedAt = account.CreatedAt,
                        UpdatedAt = DateTime.UtcNow
                    };
                    return updatedAccount;
                });
        }

        /// <summary>
        /// Sets up common wallet service mocks
        /// </summary>
        public static void SetupWalletServiceMocks(Mock<IWalletService> mock, Wallet wallet)
        {
            // Setup GetByIdAsync
            mock.Setup(x => x.GetByIdAsync(wallet.Id))
                .ReturnsAsync(wallet);

            // Setup GetByAddressAsync
            mock.Setup(x => x.GetByAddressAsync(wallet.Address))
                .ReturnsAsync(wallet);

            // Setup GetByScriptHashAsync
            mock.Setup(x => x.GetByScriptHashAsync(wallet.ScriptHash))
                .ReturnsAsync(wallet);

            // Setup GetByAccountIdAsync
            mock.Setup(x => x.GetByAccountIdAsync(It.Is<Guid>(id => id == wallet.AccountId)))
                .ReturnsAsync(new List<Wallet> { wallet });

            // Setup GetNeoBalanceAsync
            mock.Setup(x => x.GetNeoBalanceAsync(wallet.Address))
                .ReturnsAsync(10.0m);

            // Setup GetGasBalanceAsync
            mock.Setup(x => x.GetGasBalanceAsync(wallet.Address))
                .ReturnsAsync(100.0m);
        }

        /// <summary>
        /// Sets up common secrets service mocks
        /// </summary>
        public static void SetupSecretsServiceMocks(Mock<ISecretsService> mock, Secret secret, Function function = null)
        {
            // Setup GetByIdAsync
            mock.Setup(x => x.GetByIdAsync(secret.Id))
                .ReturnsAsync(secret);

            // Setup GetByNameAsync
            mock.Setup(x => x.GetByNameAsync(secret.Name, secret.AccountId))
                .ReturnsAsync(secret);

            // Setup GetByAccountIdAsync
            mock.Setup(x => x.GetByAccountIdAsync(secret.AccountId))
                .ReturnsAsync(new List<Secret> { secret });

            if (function != null)
            {
                // Setup HasAccessAsync
                mock.Setup(x => x.HasAccessAsync(secret.Id, function.Id))
                    .ReturnsAsync(secret.AllowedFunctionIds.Contains(function.Id));

                // Setup GetSecretValueAsync
                mock.Setup(x => x.GetSecretValueAsync(secret.Id, function.Id))
                    .ReturnsAsync((Guid secretId, Guid functionId) =>
                    {
                        if (secret.AllowedFunctionIds.Contains(functionId))
                        {
                            return "decrypted-secret-value";
                        }
                        throw new InvalidOperationException("Function does not have access to this secret");
                    });
            }
        }

        /// <summary>
        /// Sets up common function service mocks
        /// </summary>
        public static void SetupFunctionServiceMocks(Mock<IFunctionService> mock, Function function)
        {
            // Setup GetByIdAsync
            mock.Setup(x => x.GetByIdAsync(function.Id))
                .ReturnsAsync(function);

            // Setup GetFunctionAsync
            mock.Setup(x => x.GetFunctionAsync(function.Id))
                .ReturnsAsync(function);

            // Setup GetByNameAsync
            mock.Setup(x => x.GetByNameAsync(function.Name, function.AccountId))
                .ReturnsAsync(function);

            // Setup GetByAccountIdAsync
            mock.Setup(x => x.GetByAccountIdAsync(function.AccountId))
                .ReturnsAsync(new List<Function> { function });

            // Setup GetByRuntimeAsync
            mock.Setup(x => x.GetByRuntimeAsync(It.IsAny<FunctionRuntime>()))
                .ReturnsAsync(new List<Function> { function });

            // Setup ExecuteAsync
            mock.Setup(x => x.ExecuteAsync(function.Id, It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync((Guid id, Dictionary<string, object> parameters) =>
                {
                    return new { Result = "Function executed successfully", Parameters = parameters };
                });
        }

        /// <summary>
        /// Sets up common price feed service mocks
        /// </summary>
        public static void SetupPriceFeedServiceMocks(Mock<IPriceFeedService> mock, PriceSource source, Price price, PriceHistory history = null)
        {
            // Setup AddSourceAsync
            mock.Setup(x => x.AddSourceAsync(It.Is<PriceSource>(s => s.Name == source.Name)))
                .ReturnsAsync(source);

            // Setup UpdateSourceAsync
            mock.Setup(x => x.UpdateSourceAsync(It.Is<PriceSource>(s => s.Id == source.Id)))
                .ReturnsAsync((PriceSource s) => s);

            // Setup GetSourceByIdAsync
            mock.Setup(x => x.GetSourceByIdAsync(source.Id))
                .ReturnsAsync(source);

            // Setup GetSourceByNameAsync
            mock.Setup(x => x.GetSourceByNameAsync(source.Name))
                .ReturnsAsync(source);

            // Setup GetAllSourcesAsync
            mock.Setup(x => x.GetAllSourcesAsync())
                .ReturnsAsync(new List<PriceSource> { source });

            // Setup GetActiveSourcesAsync
            mock.Setup(x => x.GetActiveSourcesAsync())
                .ReturnsAsync(new List<PriceSource> { source });

            // Setup GetLatestPriceAsync
            mock.Setup(x => x.GetLatestPriceAsync(price.Symbol, price.BaseCurrency))
                .ReturnsAsync(price);

            // Setup GetAllLatestPricesAsync
            mock.Setup(x => x.GetAllLatestPricesAsync(price.BaseCurrency))
                .ReturnsAsync(new Dictionary<string, Price> { { price.Symbol, price } });

            if (history != null)
            {
                // Setup GetPriceHistoryAsync
                mock.Setup(x => x.GetPriceHistoryAsync(
                    history.Symbol,
                    history.BaseCurrency,
                    history.Interval,
                    history.StartTime,
                    history.EndTime))
                    .ReturnsAsync(history);
            }
        }
    }
}
