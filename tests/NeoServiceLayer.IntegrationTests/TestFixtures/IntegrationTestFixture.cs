using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Account;
using NeoServiceLayer.Services.Account.Repositories;
using NeoServiceLayer.Services.Functions;
using NeoServiceLayer.Services.Functions.Repositories;
using NeoServiceLayer.Services.PriceFeed;
using NeoServiceLayer.Services.PriceFeed.Repositories;
using NeoServiceLayer.Services.Secrets;
using NeoServiceLayer.Services.Secrets.Repositories;
using NeoServiceLayer.Services.Wallet;
using NeoServiceLayer.Services.Wallet.Repositories;

namespace NeoServiceLayer.IntegrationTests.TestFixtures
{
    public class IntegrationTestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; }
        public IConfiguration Configuration { get; }

        public IntegrationTestFixture()
        {
            // Build configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Jwt:Secret", "test-jwt-secret-key-that-is-long-enough-for-testing" },
                    { "Jwt:Issuer", "test-issuer" },
                    { "Jwt:Audience", "test-audience" },
                    { "Jwt:ExpiryMinutes", "60" }
                })
                .Build();

            // Setup DI
            var services = new ServiceCollection();

            // Add configuration
            services.AddSingleton<IConfiguration>(Configuration);

            // Add logging
            services.AddLogging(builder => builder.AddConsole());

            // Add repositories (in-memory implementations for testing)
            services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
            services.AddSingleton<ISecretsRepository, InMemorySecretsRepository>();
            services.AddSingleton<IFunctionRepository, InMemoryFunctionRepository>();
            services.AddSingleton<IPriceFeedRepository, InMemoryPriceFeedRepository>();
            services.AddSingleton<IWalletRepository, InMemoryWalletRepository>();

            // Add enclave service (mock implementation for testing)
            services.AddSingleton<IEnclaveService, MockEnclaveService>();

            // Add services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ISecretsService, SecretsService>();
            services.AddScoped<IFunctionService, FunctionService>();
            services.AddScoped<IPriceFeedService, PriceFeedService>();
            services.AddScoped<IWalletService, WalletService>();

            // Build service provider
            ServiceProvider = services.BuildServiceProvider();
        }

        public void Dispose()
        {
            // Clean up resources if needed
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    // In-memory repository implementations for testing
    public class InMemoryAccountRepository : IAccountRepository
    {
        private readonly Dictionary<Guid, Core.Models.Account> _accounts = new();
        private readonly Dictionary<string, Core.Models.Account> _accountsByUsername = new();
        private readonly Dictionary<string, Core.Models.Account> _accountsByEmail = new();
        private readonly Dictionary<string, Core.Models.Account> _accountsByNeoAddress = new();

        public Task<Core.Models.Account> AddAsync(Core.Models.Account account)
        {
            if (account.Id == Guid.Empty)
            {
                account.Id = Guid.NewGuid();
            }

            _accounts[account.Id] = account;
            _accountsByUsername[account.Username] = account;
            _accountsByEmail[account.Email] = account;
            if (!string.IsNullOrEmpty(account.NeoAddress))
            {
                _accountsByNeoAddress[account.NeoAddress] = account;
            }

            return Task.FromResult(account);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            if (_accounts.TryGetValue(id, out var account))
            {
                _accounts.Remove(id);
                _accountsByUsername.Remove(account.Username);
                _accountsByEmail.Remove(account.Email);
                if (!string.IsNullOrEmpty(account.NeoAddress))
                {
                    _accountsByNeoAddress.Remove(account.NeoAddress);
                }
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<Core.Models.Account> GetByEmailAsync(string email)
        {
            _accountsByEmail.TryGetValue(email, out var account);
            return Task.FromResult(account);
        }

        public Task<Core.Models.Account> GetByIdAsync(Guid id)
        {
            _accounts.TryGetValue(id, out var account);
            return Task.FromResult(account);
        }

        public Task<Core.Models.Account> GetByUsernameAsync(string username)
        {
            _accountsByUsername.TryGetValue(username, out var account);
            return Task.FromResult(account);
        }

        public Task<Core.Models.Account> GetByNeoAddressAsync(string neoAddress)
        {
            _accountsByNeoAddress.TryGetValue(neoAddress, out var account);
            return Task.FromResult(account);
        }

        public Task<IEnumerable<Core.Models.Account>> ListAsync(int skip = 0, int take = 100)
        {
            return Task.FromResult(_accounts.Values.Skip(skip).Take(take));
        }

        public Task<IEnumerable<Core.Models.Account>> GetAllAsync()
        {
            return Task.FromResult(_accounts.Values.AsEnumerable());
        }

        public Task<Core.Models.Account> UpdateAsync(Core.Models.Account account)
        {
            if (_accounts.TryGetValue(account.Id, out var existingAccount))
            {
                // Remove old username/email mappings if they changed
                if (existingAccount.Username != account.Username)
                {
                    _accountsByUsername.Remove(existingAccount.Username);
                    _accountsByUsername[account.Username] = account;
                }

                if (existingAccount.Email != account.Email)
                {
                    _accountsByEmail.Remove(existingAccount.Email);
                    _accountsByEmail[account.Email] = account;
                }

                if (existingAccount.NeoAddress != account.NeoAddress)
                {
                    if (!string.IsNullOrEmpty(existingAccount.NeoAddress))
                    {
                        _accountsByNeoAddress.Remove(existingAccount.NeoAddress);
                    }
                    if (!string.IsNullOrEmpty(account.NeoAddress))
                    {
                        _accountsByNeoAddress[account.NeoAddress] = account;
                    }
                }

                _accounts[account.Id] = account;
                return Task.FromResult(account);
            }

            return Task.FromResult<Core.Models.Account>(null);
        }
    }

    public class InMemorySecretsRepository : ISecretsRepository
    {
        private readonly Dictionary<Guid, Core.Models.Secret> _secrets = new();
        private readonly Dictionary<string, Dictionary<Guid, Core.Models.Secret>> _secretsByNameAndAccount = new();
        private readonly Dictionary<Guid, List<Guid>> _secretsByFunction = new();

        public Task<Core.Models.Secret> CreateAsync(Core.Models.Secret secret)
        {
            if (secret.Id == Guid.Empty)
            {
                secret.Id = Guid.NewGuid();
            }

            _secrets[secret.Id] = secret;

            if (!_secretsByNameAndAccount.ContainsKey(secret.Name))
            {
                _secretsByNameAndAccount[secret.Name] = new Dictionary<Guid, Core.Models.Secret>();
            }

            _secretsByNameAndAccount[secret.Name][secret.AccountId] = secret;

            return Task.FromResult(secret);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            if (_secrets.TryGetValue(id, out var secret))
            {
                _secrets.Remove(id);
                _secretsByNameAndAccount[secret.Name].Remove(secret.AccountId);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<Core.Models.Secret> GetByIdAsync(Guid id)
        {
            _secrets.TryGetValue(id, out var secret);
            return Task.FromResult(secret);
        }

        public Task<Core.Models.Secret> GetByNameAsync(string name, Guid accountId)
        {
            if (_secretsByNameAndAccount.TryGetValue(name, out var secretsByAccount) &&
                secretsByAccount.TryGetValue(accountId, out var secret))
            {
                return Task.FromResult(secret);
            }

            return Task.FromResult<Core.Models.Secret>(null);
        }

        public Task<IEnumerable<Core.Models.Secret>> GetByAccountIdAsync(Guid accountId)
        {
            var secrets = _secrets.Values.Where(s => s.AccountId == accountId);
            return Task.FromResult(secrets);
        }

        public Task<IEnumerable<Core.Models.Secret>> GetByFunctionIdAsync(Guid functionId)
        {
            if (_secretsByFunction.TryGetValue(functionId, out var secretIds))
            {
                var secrets = secretIds
                    .Where(id => _secrets.ContainsKey(id))
                    .Select(id => _secrets[id]);
                return Task.FromResult(secrets);
            }

            return Task.FromResult(Enumerable.Empty<Core.Models.Secret>());
        }

        public Task<Core.Models.Secret> UpdateAsync(Core.Models.Secret secret)
        {
            if (_secrets.TryGetValue(secret.Id, out var existingSecret))
            {
                // Update name mapping if it changed
                if (existingSecret.Name != secret.Name)
                {
                    _secretsByNameAndAccount[existingSecret.Name].Remove(existingSecret.AccountId);

                    if (!_secretsByNameAndAccount.ContainsKey(secret.Name))
                    {
                        _secretsByNameAndAccount[secret.Name] = new Dictionary<Guid, Core.Models.Secret>();
                    }

                    _secretsByNameAndAccount[secret.Name][secret.AccountId] = secret;
                }

                _secrets[secret.Id] = secret;
                return Task.FromResult(secret);
            }

            return Task.FromResult<Core.Models.Secret>(null);
        }
    }

    public class InMemoryFunctionRepository : IFunctionRepository
    {
        private readonly Dictionary<Guid, Core.Models.Function> _functions = new();
        private readonly Dictionary<string, Dictionary<Guid, Core.Models.Function>> _functionsByNameAndAccount = new();
        private readonly Dictionary<string, List<Core.Models.Function>> _functionsByRuntime = new();
        private readonly Dictionary<string, List<Core.Models.Function>> _functionsByTag = new();

        public Task<Core.Models.Function> CreateAsync(Core.Models.Function function)
        {
            if (function.Id == Guid.Empty)
            {
                function.Id = Guid.NewGuid();
            }

            _functions[function.Id] = function;

            if (!_functionsByNameAndAccount.ContainsKey(function.Name))
            {
                _functionsByNameAndAccount[function.Name] = new Dictionary<Guid, Core.Models.Function>();
            }

            _functionsByNameAndAccount[function.Name][function.AccountId] = function;

            // Add to runtime index
            if (!string.IsNullOrEmpty(function.Runtime))
            {
                if (!_functionsByRuntime.ContainsKey(function.Runtime))
                {
                    _functionsByRuntime[function.Runtime] = new List<Core.Models.Function>();
                }
                _functionsByRuntime[function.Runtime].Add(function);
            }

            // Add to tags index
            if (function.Tags != null)
            {
                foreach (var tag in function.Tags)
                {
                    if (!_functionsByTag.ContainsKey(tag))
                    {
                        _functionsByTag[tag] = new List<Core.Models.Function>();
                    }
                    _functionsByTag[tag].Add(function);
                }
            }

            return Task.FromResult(function);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            if (_functions.TryGetValue(id, out var function))
            {
                _functions.Remove(id);
                _functionsByNameAndAccount[function.Name].Remove(function.AccountId);

                // Remove from runtime index
                if (!string.IsNullOrEmpty(function.Runtime) && _functionsByRuntime.ContainsKey(function.Runtime))
                {
                    _functionsByRuntime[function.Runtime].Remove(function);
                }

                // Remove from tags index
                if (function.Tags != null)
                {
                    foreach (var tag in function.Tags)
                    {
                        if (_functionsByTag.ContainsKey(tag))
                        {
                            _functionsByTag[tag].Remove(function);
                        }
                    }
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<Core.Models.Function> GetByIdAsync(Guid id)
        {
            _functions.TryGetValue(id, out var function);
            return Task.FromResult(function);
        }

        public Task<Core.Models.Function> GetByNameAsync(string name, Guid accountId)
        {
            if (_functionsByNameAndAccount.TryGetValue(name, out var functionsByAccount) &&
                functionsByAccount.TryGetValue(accountId, out var function))
            {
                return Task.FromResult(function);
            }

            return Task.FromResult<Core.Models.Function>(null);
        }

        public Task<IEnumerable<Core.Models.Function>> ListByAccountAsync(Guid accountId, int skip = 0, int take = 100)
        {
            var functions = _functions.Values.Where(f => f.AccountId == accountId).Skip(skip).Take(take);
            return Task.FromResult(functions);
        }

        public Task<Core.Models.Function> UpdateAsync(Guid id, Core.Models.Function function)
        {
            if (_functions.TryGetValue(id, out var existingFunction))
            {
                // Update name mapping if it changed
                if (existingFunction.Name != function.Name)
                {
                    _functionsByNameAndAccount[existingFunction.Name].Remove(existingFunction.AccountId);

                    if (!_functionsByNameAndAccount.ContainsKey(function.Name))
                    {
                        _functionsByNameAndAccount[function.Name] = new Dictionary<Guid, Core.Models.Function>();
                    }

                    _functionsByNameAndAccount[function.Name][function.AccountId] = function;
                }

                // Update runtime index
                if (existingFunction.Runtime != function.Runtime)
                {
                    if (!string.IsNullOrEmpty(existingFunction.Runtime) && _functionsByRuntime.ContainsKey(existingFunction.Runtime))
                    {
                        _functionsByRuntime[existingFunction.Runtime].Remove(existingFunction);
                    }

                    if (!string.IsNullOrEmpty(function.Runtime))
                    {
                        if (!_functionsByRuntime.ContainsKey(function.Runtime))
                        {
                            _functionsByRuntime[function.Runtime] = new List<Core.Models.Function>();
                        }
                        _functionsByRuntime[function.Runtime].Add(function);
                    }
                }

                // Update tags index
                if (existingFunction.Tags != null)
                {
                    foreach (var tag in existingFunction.Tags)
                    {
                        if (_functionsByTag.ContainsKey(tag))
                        {
                            _functionsByTag[tag].Remove(existingFunction);
                        }
                    }
                }

                if (function.Tags != null)
                {
                    foreach (var tag in function.Tags)
                    {
                        if (!_functionsByTag.ContainsKey(tag))
                        {
                            _functionsByTag[tag] = new List<Core.Models.Function>();
                        }
                        _functionsByTag[tag].Add(function);
                    }
                }

                _functions[id] = function;
                return Task.FromResult(function);
            }

            return Task.FromResult<Core.Models.Function>(null);
        }

        public Task<Core.Models.Function> UpdateAsync(Core.Models.Function function)
        {
            return UpdateAsync(function.Id, function);
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            return Task.FromResult(_functions.ContainsKey(id));
        }

        public Task<IEnumerable<Core.Models.Function>> GetAllAsync(int skip = 0, int take = 100)
        {
            return Task.FromResult(_functions.Values.Skip(skip).Take(take));
        }

        public Task<int> CountAsync()
        {
            return Task.FromResult(_functions.Count);
        }

        public Task<int> CountByAccountIdAsync(Guid accountId)
        {
            return Task.FromResult(_functions.Values.Count(f => f.AccountId == accountId));
        }

        public Task<int> CountByRuntimeAsync(string runtime)
        {
            if (_functionsByRuntime.TryGetValue(runtime, out var functions))
            {
                return Task.FromResult(functions.Count);
            }
            return Task.FromResult(0);
        }

        public Task<int> CountByTagsAsync(List<string> tags)
        {
            var functions = new HashSet<Core.Models.Function>();
            foreach (var tag in tags)
            {
                if (_functionsByTag.TryGetValue(tag, out var taggedFunctions))
                {
                    foreach (var function in taggedFunctions)
                    {
                        functions.Add(function);
                    }
                }
            }
            return Task.FromResult(functions.Count);
        }

        public Task<IEnumerable<Core.Models.Function>> GetByAccountIdAsync(Guid accountId, int skip = 0, int take = 100)
        {
            return Task.FromResult(_functions.Values.Where(f => f.AccountId == accountId).Skip(skip).Take(take));
        }

        public Task<IEnumerable<Core.Models.Function>> GetByNameAsync(string name, int skip = 0, int take = 100)
        {
            if (_functionsByNameAndAccount.TryGetValue(name, out var functionsByAccount))
            {
                return Task.FromResult(functionsByAccount.Values.Skip(skip).Take(take));
            }
            return Task.FromResult(Enumerable.Empty<Core.Models.Function>());
        }

        public Task<Core.Models.Function> GetByNameAndAccountIdAsync(string name, Guid accountId)
        {
            return GetByNameAsync(name, accountId);
        }

        public Task<IEnumerable<Core.Models.Function>> GetByRuntimeAsync(string runtime, int skip = 0, int take = 100)
        {
            if (_functionsByRuntime.TryGetValue(runtime, out var functions))
            {
                return Task.FromResult(functions.Skip(skip).Take(take).AsEnumerable());
            }
            return Task.FromResult(Enumerable.Empty<Core.Models.Function>());
        }

        public Task<IEnumerable<Core.Models.Function>> GetByTagsAsync(List<string> tags, int skip = 0, int take = 100)
        {
            var functions = new HashSet<Core.Models.Function>();
            foreach (var tag in tags)
            {
                if (_functionsByTag.TryGetValue(tag, out var taggedFunctions))
                {
                    foreach (var function in taggedFunctions)
                    {
                        functions.Add(function);
                    }
                }
            }
            return Task.FromResult(functions.Skip(skip).Take(take).AsEnumerable());
        }
    }

    public class InMemoryPriceFeedRepository : IPriceFeedRepository
    {
        private readonly Dictionary<Guid, Core.Models.PriceSource> _sources = new();
        private readonly Dictionary<string, Core.Models.PriceSource> _sourcesByName = new();
        private readonly Dictionary<string, Dictionary<string, Core.Models.Price>> _prices = new();
        private readonly Dictionary<string, Dictionary<string, Core.Models.PriceHistory>> _priceHistories = new();

        public Task<Core.Models.PriceSource> CreateSourceAsync(Core.Models.PriceSource source)
        {
            if (source.Id == Guid.Empty)
            {
                source.Id = Guid.NewGuid();
            }

            _sources[source.Id] = source;
            _sourcesByName[source.Name] = source;

            return Task.FromResult(source);
        }

        public Task<bool> DeleteSourceAsync(Guid id)
        {
            if (_sources.TryGetValue(id, out var source))
            {
                _sources.Remove(id);
                _sourcesByName.Remove(source.Name);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<Core.Models.PriceSource> GetSourceByIdAsync(Guid id)
        {
            _sources.TryGetValue(id, out var source);
            return Task.FromResult(source);
        }

        public Task<Core.Models.PriceSource> GetSourceByNameAsync(string name)
        {
            _sourcesByName.TryGetValue(name, out var source);
            return Task.FromResult(source);
        }

        public Task<IEnumerable<Core.Models.PriceSource>> ListSourcesAsync(int skip = 0, int take = 100)
        {
            var sources = _sources.Values.Skip(skip).Take(take);
            return Task.FromResult(sources);
        }

        public Task<Core.Models.PriceSource> UpdateSourceAsync(Core.Models.PriceSource source)
        {
            if (_sources.TryGetValue(source.Id, out var existingSource))
            {
                // Update name mapping if it changed
                if (existingSource.Name != source.Name)
                {
                    _sourcesByName.Remove(existingSource.Name);
                    _sourcesByName[source.Name] = source;
                }

                _sources[source.Id] = source;
                return Task.FromResult(source);
            }

            return Task.FromResult<Core.Models.PriceSource>(null);
        }

        public Task<Core.Models.Price> SavePriceAsync(Core.Models.Price price)
        {
            if (price.Id == Guid.Empty)
            {
                price.Id = Guid.NewGuid();
            }

            if (!_prices.ContainsKey(price.Symbol))
            {
                _prices[price.Symbol] = new Dictionary<string, Core.Models.Price>();
            }

            _prices[price.Symbol][price.BaseCurrency] = price;

            return Task.FromResult(price);
        }

        public Task<Core.Models.Price> GetLatestPriceAsync(string symbol, string baseCurrency)
        {
            if (_prices.TryGetValue(symbol, out var pricesByBase) &&
                pricesByBase.TryGetValue(baseCurrency, out var price))
            {
                return Task.FromResult(price);
            }

            return Task.FromResult<Core.Models.Price>(null);
        }

        public Task<IEnumerable<Core.Models.Price>> GetLatestPricesAsync(IEnumerable<string> symbols, string baseCurrency)
        {
            var prices = new List<Core.Models.Price>();

            foreach (var symbol in symbols)
            {
                if (_prices.TryGetValue(symbol, out var pricesByBase) &&
                    pricesByBase.TryGetValue(baseCurrency, out var price))
                {
                    prices.Add(price);
                }
            }

            return Task.FromResult(prices.AsEnumerable());
        }

        public Task<Core.Models.PriceHistory> SavePriceHistoryAsync(Core.Models.PriceHistory history)
        {
            if (history.Id == Guid.Empty)
            {
                history.Id = Guid.NewGuid();
            }

            if (!_priceHistories.ContainsKey(history.Symbol))
            {
                _priceHistories[history.Symbol] = new Dictionary<string, Core.Models.PriceHistory>();
            }

            _priceHistories[history.Symbol][history.BaseCurrency] = history;

            return Task.FromResult(history);
        }

        public Task<Core.Models.PriceHistory> GetPriceHistoryAsync(string symbol, string baseCurrency, string interval, DateTime startTime, DateTime endTime)
        {
            if (_priceHistories.TryGetValue(symbol, out var historiesByBase) &&
                historiesByBase.TryGetValue(baseCurrency, out var history) &&
                history.Interval == interval &&
                history.StartTime <= startTime &&
                history.EndTime >= endTime)
            {
                // Filter data points to match the requested time range
                var filteredHistory = new Core.Models.PriceHistory
                {
                    Id = history.Id,
                    Symbol = history.Symbol,
                    BaseCurrency = history.BaseCurrency,
                    Interval = history.Interval,
                    StartTime = startTime,
                    EndTime = endTime,
                    DataPoints = history.DataPoints.Where(dp => dp.Timestamp >= startTime && dp.Timestamp <= endTime).ToList(),
                    CreatedAt = history.CreatedAt,
                    UpdatedAt = history.UpdatedAt
                };

                return Task.FromResult(filteredHistory);
            }

            return Task.FromResult<Core.Models.PriceHistory>(null);
        }
    }

    public class InMemoryWalletRepository : IWalletRepository
    {
        private readonly Dictionary<Guid, Core.Models.Wallet> _wallets = new();
        private readonly Dictionary<string, Core.Models.Wallet> _walletsByAddress = new();
        private readonly Dictionary<string, Core.Models.Wallet> _walletsByScriptHash = new();
        private readonly Dictionary<Guid, List<Core.Models.Wallet>> _walletsByAccount = new();
        private readonly List<Core.Models.Wallet> _serviceWallets = new();

        public Task<Core.Models.Wallet> CreateAsync(Core.Models.Wallet wallet)
        {
            if (wallet.Id == Guid.Empty)
            {
                wallet.Id = Guid.NewGuid();
            }

            _wallets[wallet.Id] = wallet;
            _walletsByAddress[wallet.Address] = wallet;
            _walletsByScriptHash[wallet.ScriptHash] = wallet;

            if (!_walletsByAccount.ContainsKey(wallet.AccountId))
            {
                _walletsByAccount[wallet.AccountId] = new List<Core.Models.Wallet>();
            }

            _walletsByAccount[wallet.AccountId].Add(wallet);

            // If this is a service wallet, add it to the service wallets list
            if (wallet.IsServiceWallet)
            {
                _serviceWallets.Add(wallet);
            }

            return Task.FromResult(wallet);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            if (_wallets.TryGetValue(id, out var wallet))
            {
                _wallets.Remove(id);
                _walletsByAddress.Remove(wallet.Address);
                _walletsByScriptHash.Remove(wallet.ScriptHash);
                _walletsByAccount[wallet.AccountId].Remove(wallet);

                if (wallet.IsServiceWallet)
                {
                    _serviceWallets.Remove(wallet);
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<Core.Models.Wallet> GetByAddressAsync(string address)
        {
            _walletsByAddress.TryGetValue(address, out var wallet);
            return Task.FromResult(wallet);
        }

        public Task<Core.Models.Wallet> GetByScriptHashAsync(string scriptHash)
        {
            _walletsByScriptHash.TryGetValue(scriptHash, out var wallet);
            return Task.FromResult(wallet);
        }

        public Task<Core.Models.Wallet> GetByIdAsync(Guid id)
        {
            _wallets.TryGetValue(id, out var wallet);
            return Task.FromResult(wallet);
        }

        public Task<IEnumerable<Core.Models.Wallet>> GetByAccountIdAsync(Guid accountId)
        {
            if (_walletsByAccount.TryGetValue(accountId, out var wallets))
            {
                return Task.FromResult(wallets.AsEnumerable());
            }

            return Task.FromResult(Enumerable.Empty<Core.Models.Wallet>());
        }

        public Task<IEnumerable<Core.Models.Wallet>> GetServiceWalletsAsync()
        {
            return Task.FromResult(_serviceWallets.AsEnumerable());
        }

        public Task<IEnumerable<Core.Models.Wallet>> ListByAccountAsync(Guid accountId, int skip = 0, int take = 100)
        {
            if (_walletsByAccount.TryGetValue(accountId, out var wallets))
            {
                return Task.FromResult(wallets.Skip(skip).Take(take).AsEnumerable());
            }

            return Task.FromResult(Enumerable.Empty<Core.Models.Wallet>());
        }

        public Task<Core.Models.Wallet> UpdateAsync(Core.Models.Wallet wallet)
        {
            if (_wallets.TryGetValue(wallet.Id, out var existingWallet))
            {
                // Update address mapping if it changed
                if (existingWallet.Address != wallet.Address)
                {
                    _walletsByAddress.Remove(existingWallet.Address);
                    _walletsByAddress[wallet.Address] = wallet;
                }

                // Update script hash mapping if it changed
                if (existingWallet.ScriptHash != wallet.ScriptHash)
                {
                    _walletsByScriptHash.Remove(existingWallet.ScriptHash);
                    _walletsByScriptHash[wallet.ScriptHash] = wallet;
                }

                // Update account mapping if it changed
                if (existingWallet.AccountId != wallet.AccountId)
                {
                    _walletsByAccount[existingWallet.AccountId].Remove(existingWallet);

                    if (!_walletsByAccount.ContainsKey(wallet.AccountId))
                    {
                        _walletsByAccount[wallet.AccountId] = new List<Core.Models.Wallet>();
                    }

                    _walletsByAccount[wallet.AccountId].Add(wallet);
                }

                // Update service wallet status if it changed
                if (existingWallet.IsServiceWallet != wallet.IsServiceWallet)
                {
                    if (wallet.IsServiceWallet)
                    {
                        _serviceWallets.Add(wallet);
                    }
                    else
                    {
                        _serviceWallets.Remove(existingWallet);
                    }
                }

                _wallets[wallet.Id] = wallet;
                return Task.FromResult(wallet);
            }

            return Task.FromResult<Core.Models.Wallet>(null);
        }
    }

    public class MockEnclaveService : IEnclaveService
    {
        private readonly Dictionary<string, Dictionary<string, Func<object, object>>> _handlers = new();

        public MockEnclaveService()
        {
            // Register default handlers for common operations
            RegisterHandler<object, object>(
                Core.Constants.EnclaveServiceTypes.Account,
                Core.Constants.AccountOperations.Register,
                request => new { Success = true });

            RegisterHandler<object, object>(
                Core.Constants.EnclaveServiceTypes.Account,
                Core.Constants.AccountOperations.Authenticate,
                request => new { Success = true });

            RegisterHandler<object, object>(
                Core.Constants.EnclaveServiceTypes.Secrets,
                Core.Constants.SecretsOperations.CreateSecret,
                request => new { Success = true });

            RegisterHandler<object, object>(
                Core.Constants.EnclaveServiceTypes.Secrets,
                Core.Constants.SecretsOperations.GetSecretValue,
                request => new { Value = "decrypted-secret-value" });
        }

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
            return Task.FromResult<object>(new { CPU = 10, Memory = 100 });
        }
    }
}
