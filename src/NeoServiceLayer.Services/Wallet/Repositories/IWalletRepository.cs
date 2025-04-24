using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Wallet.Repositories
{
    /// <summary>
    /// Interface for wallet repository
    /// </summary>
    public interface IWalletRepository
    {
        /// <summary>
        /// Creates a new wallet
        /// </summary>
        /// <param name="wallet">Wallet to create</param>
        /// <returns>The created wallet</returns>
        Task<Core.Models.Wallet> CreateAsync(Core.Models.Wallet wallet);

        /// <summary>
        /// Gets a wallet by ID
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <returns>The wallet if found, null otherwise</returns>
        Task<Core.Models.Wallet> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a wallet by address
        /// </summary>
        /// <param name="address">Neo N3 address</param>
        /// <returns>The wallet if found, null otherwise</returns>
        Task<Core.Models.Wallet> GetByAddressAsync(string address);

        /// <summary>
        /// Gets a wallet by script hash
        /// </summary>
        /// <param name="scriptHash">Script hash</param>
        /// <returns>The wallet if found, null otherwise</returns>
        Task<Core.Models.Wallet> GetByScriptHashAsync(string scriptHash);

        /// <summary>
        /// Gets wallets by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of wallets associated with the account</returns>
        Task<IEnumerable<Core.Models.Wallet>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets all service wallets
        /// </summary>
        /// <returns>List of service wallets</returns>
        Task<IEnumerable<Core.Models.Wallet>> GetServiceWalletsAsync();

        /// <summary>
        /// Updates a wallet
        /// </summary>
        /// <param name="wallet">Wallet to update</param>
        /// <returns>The updated wallet</returns>
        Task<Core.Models.Wallet> UpdateAsync(Core.Models.Wallet wallet);

        /// <summary>
        /// Deletes a wallet
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <returns>True if wallet was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
