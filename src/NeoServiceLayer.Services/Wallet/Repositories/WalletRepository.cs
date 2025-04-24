using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Wallet.Repositories
{
    /// <summary>
    /// Implementation of the wallet repository
    /// </summary>
    public class WalletRepository : IWalletRepository
    {
        private readonly ILogger<WalletRepository> _logger;
        private readonly Dictionary<Guid, Core.Models.Wallet> _wallets;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public WalletRepository(ILogger<WalletRepository> logger)
        {
            _logger = logger;
            _wallets = new Dictionary<Guid, Core.Models.Wallet>();
        }

        /// <inheritdoc/>
        public Task<Core.Models.Wallet> CreateAsync(Core.Models.Wallet wallet)
        {
            _logger.LogInformation("Creating wallet: {Id}", wallet.Id);

            if (wallet.Id == Guid.Empty)
            {
                wallet.Id = Guid.NewGuid();
            }

            wallet.CreatedAt = DateTime.UtcNow;
            wallet.UpdatedAt = DateTime.UtcNow;

            _wallets[wallet.Id] = wallet;

            return Task.FromResult(wallet);
        }

        /// <inheritdoc/>
        public Task<Core.Models.Wallet> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting wallet by ID: {Id}", id);

            _wallets.TryGetValue(id, out var wallet);
            return Task.FromResult(wallet);
        }

        /// <inheritdoc/>
        public Task<Core.Models.Wallet> GetByAddressAsync(string address)
        {
            _logger.LogInformation("Getting wallet by address: {Address}", address);

            var wallet = _wallets.Values.FirstOrDefault(w => w.Address.Equals(address, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(wallet);
        }

        /// <inheritdoc/>
        public Task<Core.Models.Wallet> GetByScriptHashAsync(string scriptHash)
        {
            _logger.LogInformation("Getting wallet by script hash: {ScriptHash}", scriptHash);

            var wallet = _wallets.Values.FirstOrDefault(w => w.ScriptHash.Equals(scriptHash, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(wallet);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Core.Models.Wallet>> GetByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Getting wallets by account ID: {AccountId}", accountId);

            var wallets = _wallets.Values.Where(w => w.AccountId == accountId).ToList();
            return Task.FromResult<IEnumerable<Core.Models.Wallet>>(wallets);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Core.Models.Wallet>> GetServiceWalletsAsync()
        {
            _logger.LogInformation("Getting service wallets");

            var wallets = _wallets.Values.Where(w => w.IsServiceWallet).ToList();
            return Task.FromResult<IEnumerable<Core.Models.Wallet>>(wallets);
        }

        /// <inheritdoc/>
        public Task<Core.Models.Wallet> UpdateAsync(Core.Models.Wallet wallet)
        {
            _logger.LogInformation("Updating wallet: {Id}", wallet.Id);

            if (!_wallets.ContainsKey(wallet.Id))
            {
                return Task.FromResult<Core.Models.Wallet>(null);
            }

            wallet.UpdatedAt = DateTime.UtcNow;
            _wallets[wallet.Id] = wallet;

            return Task.FromResult(wallet);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting wallet: {Id}", id);

            return Task.FromResult(_wallets.Remove(id));
        }
    }
}
