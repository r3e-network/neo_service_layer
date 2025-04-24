using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Extensions;
using NeoServiceLayer.Services.Wallet.Repositories;
using NeoServiceLayer.Services.Common.Utilities;
using NeoServiceLayer.Services.Common.Extensions;

namespace NeoServiceLayer.Services.Wallet
{
    /// <summary>
    /// Implementation of the wallet service
    /// </summary>
    public class WalletService : IWalletService
    {
        private readonly ILogger<WalletService> _logger;
        private readonly IWalletRepository _walletRepository;
        private readonly IEnclaveService _enclaveService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="walletRepository">Wallet repository</param>
        /// <param name="enclaveService">Enclave service</param>
        public WalletService(ILogger<WalletService> logger, IWalletRepository walletRepository, IEnclaveService enclaveService)
        {
            _logger = logger;
            _walletRepository = walletRepository;
            _enclaveService = enclaveService;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Wallet> CreateWalletAsync(string name, string password, Guid? accountId = null, bool isServiceWallet = false)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Name"] = name,
                ["AccountId"] = accountId,
                ["IsServiceWallet"] = isServiceWallet
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "CreateWallet", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(name, "Wallet name");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(password, "Password");

                if (accountId.HasValue)
                {
                    Common.Utilities.ValidationUtility.ValidateGuid(accountId.Value, "Account ID");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, Core.Models.Wallet>(
                    _logger,
                    async () =>
                    {
                        // Send wallet creation request to enclave
                        var walletRequest = new
                        {
                            Name = name,
                            Password = password,
                            AccountId = accountId,
                            IsServiceWallet = isServiceWallet
                        };

                        var wallet = await _enclaveService.SendRequestAsync<object, Core.Models.Wallet>(
                            Constants.EnclaveServiceTypes.Wallet,
                            Constants.WalletOperations.CreateWallet,
                            walletRequest);

                        // Save wallet to repository
                        await _walletRepository.CreateAsync(wallet);

                        additionalData["WalletId"] = wallet.Id;
                        additionalData["Address"] = wallet.Address;

                        return wallet;
                    },
                    "CreateWallet",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new WalletException("Failed to create wallet");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "CreateWallet", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "CreateWallet", requestId, ex, 0, additionalData);
                throw new WalletException("Error creating wallet", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Wallet> GetByIdAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetWalletById", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Wallet ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, Core.Models.Wallet>(
                    _logger,
                    async () => await _walletRepository.GetByIdAsync(id),
                    "GetWalletById",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["Address"] = result.result.Address;
                    additionalData["AccountId"] = result.result.AccountId;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetWalletById", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetWalletById", requestId, ex, 0, additionalData);
                throw new WalletException($"Error getting wallet by ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Wallet> GetByAddressAsync(string address)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Address"] = address
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetWalletByAddress", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(address, "Wallet address");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(address))
                {
                    throw new WalletException("Invalid Neo address format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, Core.Models.Wallet>(
                    _logger,
                    async () => await _walletRepository.GetByAddressAsync(address),
                    "GetWalletByAddress",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["Id"] = result.result.Id;
                    additionalData["AccountId"] = result.result.AccountId;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetWalletByAddress", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetWalletByAddress", requestId, ex, 0, additionalData);
                throw new WalletException($"Error getting wallet by address {address}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Wallet> GetByScriptHashAsync(string scriptHash)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["ScriptHash"] = scriptHash
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetWalletByScriptHash", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(scriptHash, "Script hash");

                if (!scriptHash.IsValidScriptHash())
                {
                    throw new WalletException("Invalid script hash format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, Core.Models.Wallet>(
                    _logger,
                    async () => await _walletRepository.GetByScriptHashAsync(scriptHash),
                    "GetWalletByScriptHash",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["Id"] = result.result.Id;
                    additionalData["Address"] = result.result.Address;
                    additionalData["AccountId"] = result.result.AccountId;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetWalletByScriptHash", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetWalletByScriptHash", requestId, ex, 0, additionalData);
                throw new WalletException($"Error getting wallet by script hash {scriptHash}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Wallet>> GetByAccountIdAsync(Guid accountId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = accountId
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetWalletsByAccountId", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(accountId, "Account ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, IEnumerable<Core.Models.Wallet>>(
                    _logger,
                    async () => await _walletRepository.GetByAccountIdAsync(accountId),
                    "GetWalletsByAccountId",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["WalletCount"] = result.result.Count();
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetWalletsByAccountId", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetWalletsByAccountId", requestId, ex, 0, additionalData);
                throw new WalletException($"Error getting wallets by account ID {accountId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Wallet>> GetServiceWalletsAsync()
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>();

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetServiceWallets", requestId, additionalData);

            try
            {
                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, IEnumerable<Core.Models.Wallet>>(
                    _logger,
                    async () => await _walletRepository.GetServiceWalletsAsync(),
                    "GetServiceWallets",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["WalletCount"] = result.result.Count();
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetServiceWallets", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetServiceWallets", requestId, ex, 0, additionalData);
                throw new WalletException("Error getting service wallets", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Wallet> UpdateAsync(Core.Models.Wallet wallet)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = wallet.Id,
                ["Address"] = wallet.Address
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "UpdateWallet", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNull(wallet, nameof(wallet));
                Common.Utilities.ValidationUtility.ValidateGuid(wallet.Id, "Wallet ID");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(wallet.Address, "Wallet address");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(wallet.Address))
                {
                    throw new WalletException("Invalid Neo address format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, Core.Models.Wallet>(
                    _logger,
                    async () => await _walletRepository.UpdateAsync(wallet),
                    "UpdateWallet",
                    requestId,
                    additionalData);

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "UpdateWallet", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "UpdateWallet", requestId, ex, 0, additionalData);
                throw new WalletException($"Error updating wallet {wallet.Id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Wallet> ImportFromWIFAsync(string wif, string password, string name, Guid? accountId = null, bool isServiceWallet = false)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Name"] = name,
                ["AccountId"] = accountId,
                ["IsServiceWallet"] = isServiceWallet
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "ImportWalletFromWIF", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(wif, "WIF");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(password, "Password");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(name, "Wallet name");

                if (accountId.HasValue)
                {
                    Common.Utilities.ValidationUtility.ValidateGuid(accountId.Value, "Account ID");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, Core.Models.Wallet>(
                    _logger,
                    async () =>
                    {
                        // Send wallet import request to enclave
                        var importRequest = new
                        {
                            WIF = wif,
                            Password = password,
                            Name = name,
                            AccountId = accountId,
                            IsServiceWallet = isServiceWallet
                        };

                        var wallet = await _enclaveService.SendRequestAsync<object, Core.Models.Wallet>(
                            Constants.EnclaveServiceTypes.Wallet,
                            Constants.WalletOperations.ImportFromWIF,
                            importRequest);

                        // Save wallet to repository
                        await _walletRepository.CreateAsync(wallet);

                        additionalData["WalletId"] = wallet.Id;
                        additionalData["Address"] = wallet.Address;

                        return wallet;
                    },
                    "ImportWalletFromWIF",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new WalletException("Failed to import wallet from WIF");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "ImportWalletFromWIF", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "ImportWalletFromWIF", requestId, ex, 0, additionalData);
                throw new WalletException("Error importing wallet from WIF", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<decimal> GetNeoBalanceAsync(string address)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Address"] = address
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetNeoBalance", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(address, "Address");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(address))
                {
                    throw new WalletException("Invalid Neo address format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, decimal>(
                    _logger,
                    async () =>
                    {
                        // TODO: Implement Neo RPC client to get balance
                        // For now, return a placeholder value
                        await Task.Delay(100);
                        return 0;
                    },
                    "GetNeoBalance",
                    requestId,
                    additionalData);

                additionalData["Balance"] = result.result;

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetNeoBalance", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetNeoBalance", requestId, ex, 0, additionalData);
                throw new WalletException("Error getting NEO balance", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<decimal> GetGasBalanceAsync(string address)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Address"] = address
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetGasBalance", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(address, "Address");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(address))
                {
                    throw new WalletException("Invalid Neo address format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, decimal>(
                    _logger,
                    async () =>
                    {
                        // TODO: Implement Neo RPC client to get balance
                        // For now, return a placeholder value
                        await Task.Delay(100);
                        return 0;
                    },
                    "GetGasBalance",
                    requestId,
                    additionalData);

                additionalData["Balance"] = result.result;

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetGasBalance", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetGasBalance", requestId, ex, 0, additionalData);
                throw new WalletException("Error getting GAS balance", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<decimal> GetTokenBalanceAsync(string address, string tokenHash)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Address"] = address,
                ["TokenHash"] = tokenHash
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetTokenBalance", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(address, "Address");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(tokenHash, "Token hash");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(address))
                {
                    throw new WalletException("Invalid Neo address format");
                }

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidScriptHash(tokenHash))
                {
                    throw new WalletException("Invalid token hash format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, decimal>(
                    _logger,
                    async () =>
                    {
                        // TODO: Implement Neo RPC client to get token balance
                        // For now, return a placeholder value
                        await Task.Delay(100);
                        return 0;
                    },
                    "GetTokenBalance",
                    requestId,
                    additionalData);

                additionalData["Balance"] = result.result;

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetTokenBalance", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetTokenBalance", requestId, ex, 0, additionalData);
                throw new WalletException("Error getting token balance", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> TransferNeoAsync(Guid walletId, string password, string toAddress, decimal amount)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["WalletId"] = walletId,
                ["ToAddress"] = toAddress,
                ["Amount"] = amount
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "TransferNeo", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(walletId, "Wallet ID");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(password, "Password");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(toAddress, "To address");
                Common.Utilities.ValidationUtility.ValidateGreaterThanZero(amount, "Amount");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(toAddress))
                {
                    throw new WalletException("Invalid Neo address format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, string>(
                    _logger,
                    async () =>
                    {
                        // Get wallet from repository
                        var wallet = await _walletRepository.GetByIdAsync(walletId);
                        if (wallet == null)
                        {
                            throw new WalletException("Wallet not found");
                        }

                        additionalData["FromAddress"] = wallet.Address;

                        // Send transfer request to enclave
                        var transferRequest = new
                        {
                            WalletId = walletId,
                            Password = password,
                            ToAddress = toAddress,
                            Amount = amount
                        };

                        var transferResult = await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Wallet,
                            Constants.WalletOperations.TransferNeo,
                            transferRequest);

                        // Extract transaction hash from result
                        var transactionHash = transferResult.GetType().GetProperty("TransactionHash")?.GetValue(transferResult)?.ToString();

                        if (string.IsNullOrEmpty(transactionHash))
                        {
                            throw new WalletException("Failed to get transaction hash from transfer result");
                        }

                        additionalData["TransactionHash"] = transactionHash;

                        return transactionHash;
                    },
                    "TransferNeo",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new WalletException("Failed to transfer NEO");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "TransferNeo", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "TransferNeo", requestId, ex, 0, additionalData);
                throw new WalletException("Error transferring NEO", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> TransferGasAsync(Guid walletId, string password, string toAddress, decimal amount)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["WalletId"] = walletId,
                ["ToAddress"] = toAddress,
                ["Amount"] = amount
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "TransferGas", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(walletId, "Wallet ID");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(password, "Password");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(toAddress, "To address");
                Common.Utilities.ValidationUtility.ValidateGreaterThanZero(amount, "Amount");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(toAddress))
                {
                    throw new WalletException("Invalid Neo address format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, string>(
                    _logger,
                    async () =>
                    {
                        // Get wallet from repository
                        var wallet = await _walletRepository.GetByIdAsync(walletId);
                        if (wallet == null)
                        {
                            throw new WalletException("Wallet not found");
                        }

                        additionalData["FromAddress"] = wallet.Address;

                        // Send transfer request to enclave
                        var transferRequest = new
                        {
                            WalletId = walletId,
                            Password = password,
                            ToAddress = toAddress,
                            Amount = amount
                        };

                        var transferResult = await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Wallet,
                            Constants.WalletOperations.TransferGas,
                            transferRequest);

                        // Extract transaction hash from result
                        var transactionHash = transferResult.GetType().GetProperty("TransactionHash")?.GetValue(transferResult)?.ToString();

                        if (string.IsNullOrEmpty(transactionHash))
                        {
                            throw new WalletException("Failed to get transaction hash from transfer result");
                        }

                        additionalData["TransactionHash"] = transactionHash;

                        return transactionHash;
                    },
                    "TransferGas",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new WalletException("Failed to transfer GAS");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "TransferGas", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "TransferGas", requestId, ex, 0, additionalData);
                throw new WalletException("Error transferring GAS", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> TransferTokenAsync(Guid walletId, string password, string toAddress, string tokenHash, decimal amount)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["WalletId"] = walletId,
                ["ToAddress"] = toAddress,
                ["TokenHash"] = tokenHash,
                ["Amount"] = amount
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "TransferToken", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(walletId, "Wallet ID");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(password, "Password");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(toAddress, "To address");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(tokenHash, "Token hash");
                Common.Utilities.ValidationUtility.ValidateGreaterThanZero(amount, "Amount");

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidNeoAddress(toAddress))
                {
                    throw new WalletException("Invalid Neo address format");
                }

                if (!NeoServiceLayer.Core.Extensions.StringExtensions.IsValidScriptHash(tokenHash))
                {
                    throw new WalletException("Invalid token hash format");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, string>(
                    _logger,
                    async () =>
                    {
                        // Get wallet from repository
                        var wallet = await _walletRepository.GetByIdAsync(walletId);
                        if (wallet == null)
                        {
                            throw new WalletException("Wallet not found");
                        }

                        additionalData["FromAddress"] = wallet.Address;

                        // Send transfer request to enclave
                        var transferRequest = new
                        {
                            WalletId = walletId,
                            Password = password,
                            ToAddress = toAddress,
                            TokenHash = tokenHash,
                            Amount = amount
                        };

                        var transferResult = await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Wallet,
                            Constants.WalletOperations.TransferToken,
                            transferRequest);

                        // Extract transaction hash from result
                        var transactionHash = transferResult.GetType().GetProperty("TransactionHash")?.GetValue(transferResult)?.ToString();

                        if (string.IsNullOrEmpty(transactionHash))
                        {
                            throw new WalletException("Failed to get transaction hash from transfer result");
                        }

                        additionalData["TransactionHash"] = transactionHash;

                        return transactionHash;
                    },
                    "TransferToken",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new WalletException("Failed to transfer token");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "TransferToken", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "TransferToken", requestId, ex, 0, additionalData);
                throw new WalletException("Error transferring token", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> SignDataAsync(Guid walletId, string password, byte[] data)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["WalletId"] = walletId,
                ["DataLength"] = data?.Length ?? 0
            };

            LoggingUtility.LogOperationStart(_logger, "SignData", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(walletId, "Wallet ID");
                ValidationUtility.ValidateNotNullOrEmpty(password, "Password");
                ValidationUtility.ValidateNotNull(data, "Data");

                if (data.Length == 0)
                {
                    throw new WalletException("Data to sign cannot be empty");
                }

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, string>(
                    _logger,
                    async () =>
                    {
                        // Get wallet from repository
                        var wallet = await _walletRepository.GetByIdAsync(walletId);
                        if (wallet == null)
                        {
                            throw new WalletException("Wallet not found");
                        }

                        additionalData["Address"] = wallet.Address;

                        // Send signing request to enclave
                        var signingRequest = new
                        {
                            WalletId = walletId,
                            Password = password,
                            Data = data
                        };

                        var signingResult = await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Wallet,
                            Constants.WalletOperations.SignData,
                            signingRequest);

                        // Extract signature from result
                        var signature = signingResult.GetType().GetProperty("Signature")?.GetValue(signingResult)?.ToString();

                        if (string.IsNullOrEmpty(signature))
                        {
                            throw new WalletException("Failed to get signature from signing result");
                        }

                        additionalData["SignatureLength"] = signature.Length;

                        return signature;
                    },
                    "SignData",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new WalletException("Failed to sign data");
                }

                LoggingUtility.LogOperationSuccess(_logger, "SignData", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "SignData", requestId, ex, 0, additionalData);
                throw new WalletException("Error signing data", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid walletId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["WalletId"] = walletId
            };

            LoggingUtility.LogOperationStart(_logger, "DeleteWallet", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(walletId, "Wallet ID");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<WalletService, bool>(
                    _logger,
                    async () =>
                    {
                        // Get wallet first to check if it exists
                        var wallet = await _walletRepository.GetByIdAsync(walletId);
                        if (wallet == null)
                        {
                            throw new WalletException("Wallet not found");
                        }

                        additionalData["Address"] = wallet.Address;

                        // Delete wallet
                        return await _walletRepository.DeleteAsync(walletId);
                    },
                    "DeleteWallet",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new WalletException("Failed to delete wallet");
                }

                LoggingUtility.LogOperationSuccess(_logger, "DeleteWallet", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "DeleteWallet", requestId, ex, 0, additionalData);
                throw new WalletException($"Error deleting wallet {walletId}", ex);
            }
        }
    }
}
