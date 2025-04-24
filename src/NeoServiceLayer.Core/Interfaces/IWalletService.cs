using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for wallet management service
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// Creates a new wallet
        /// </summary>
        /// <param name="name">Name for the wallet</param>
        /// <param name="password">Password for the wallet</param>
        /// <param name="accountId">Optional account ID to associate with the wallet</param>
        /// <param name="isServiceWallet">Indicates whether this is a service wallet</param>
        /// <returns>The created wallet</returns>
        Task<Wallet> CreateWalletAsync(string name, string password, Guid? accountId = null, bool isServiceWallet = false);

        /// <summary>
        /// Gets a wallet by ID
        /// </summary>
        /// <param name="id">Wallet ID</param>
        /// <returns>The wallet if found, null otherwise</returns>
        Task<Wallet> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a wallet by address
        /// </summary>
        /// <param name="address">Neo N3 address</param>
        /// <returns>The wallet if found, null otherwise</returns>
        Task<Wallet> GetByAddressAsync(string address);

        /// <summary>
        /// Gets a wallet by script hash
        /// </summary>
        /// <param name="scriptHash">Script hash</param>
        /// <returns>The wallet if found, null otherwise</returns>
        Task<Wallet> GetByScriptHashAsync(string scriptHash);

        /// <summary>
        /// Gets wallets by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of wallets associated with the account</returns>
        Task<IEnumerable<Wallet>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets all service wallets
        /// </summary>
        /// <returns>List of service wallets</returns>
        Task<IEnumerable<Wallet>> GetServiceWalletsAsync();

        /// <summary>
        /// Updates a wallet
        /// </summary>
        /// <param name="wallet">Wallet to update</param>
        /// <returns>The updated wallet</returns>
        Task<Wallet> UpdateAsync(Wallet wallet);

        /// <summary>
        /// Imports a wallet from WIF
        /// </summary>
        /// <param name="wif">WIF (Wallet Import Format)</param>
        /// <param name="password">Password for the wallet</param>
        /// <param name="name">Name for the wallet</param>
        /// <param name="accountId">Optional account ID to associate with the wallet</param>
        /// <param name="isServiceWallet">Indicates whether this is a service wallet</param>
        /// <returns>The imported wallet</returns>
        Task<Wallet> ImportFromWIFAsync(string wif, string password, string name, Guid? accountId = null, bool isServiceWallet = false);

        /// <summary>
        /// Gets the NEO balance for a wallet
        /// </summary>
        /// <param name="address">Neo N3 address</param>
        /// <returns>NEO balance</returns>
        Task<decimal> GetNeoBalanceAsync(string address);

        /// <summary>
        /// Gets the GAS balance for a wallet
        /// </summary>
        /// <param name="address">Neo N3 address</param>
        /// <returns>GAS balance</returns>
        Task<decimal> GetGasBalanceAsync(string address);

        /// <summary>
        /// Gets the token balance for a wallet
        /// </summary>
        /// <param name="address">Neo N3 address</param>
        /// <param name="tokenHash">Token script hash</param>
        /// <returns>Token balance</returns>
        Task<decimal> GetTokenBalanceAsync(string address, string tokenHash);

        /// <summary>
        /// Transfers NEO tokens
        /// </summary>
        /// <param name="walletId">Wallet ID</param>
        /// <param name="password">Wallet password</param>
        /// <param name="toAddress">Recipient address</param>
        /// <param name="amount">Amount to transfer</param>
        /// <returns>Transaction hash</returns>
        Task<string> TransferNeoAsync(Guid walletId, string password, string toAddress, decimal amount);

        /// <summary>
        /// Transfers GAS tokens
        /// </summary>
        /// <param name="walletId">Wallet ID</param>
        /// <param name="password">Wallet password</param>
        /// <param name="toAddress">Recipient address</param>
        /// <param name="amount">Amount to transfer</param>
        /// <returns>Transaction hash</returns>
        Task<string> TransferGasAsync(Guid walletId, string password, string toAddress, decimal amount);

        /// <summary>
        /// Transfers NEP-17 tokens
        /// </summary>
        /// <param name="walletId">Wallet ID</param>
        /// <param name="password">Wallet password</param>
        /// <param name="toAddress">Recipient address</param>
        /// <param name="tokenHash">Token script hash</param>
        /// <param name="amount">Amount to transfer</param>
        /// <returns>Transaction hash</returns>
        Task<string> TransferTokenAsync(Guid walletId, string password, string toAddress, string tokenHash, decimal amount);

        /// <summary>
        /// Signs data with a wallet's private key
        /// </summary>
        /// <param name="walletId">Wallet ID</param>
        /// <param name="password">Wallet password</param>
        /// <param name="data">Data to sign</param>
        /// <returns>Signature</returns>
        Task<string> SignDataAsync(Guid walletId, string password, byte[] data);

        /// <summary>
        /// Deletes a wallet
        /// </summary>
        /// <param name="walletId">Wallet ID</param>
        /// <returns>True if wallet was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid walletId);
    }
}
