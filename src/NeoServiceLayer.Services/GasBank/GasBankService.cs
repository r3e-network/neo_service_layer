using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Utilities;
using NeoServiceLayer.Core.Extensions;
using NeoServiceLayer.Services.GasBank.Repositories;

namespace NeoServiceLayer.Services.GasBank
{
    /// <summary>
    /// Implementation of the GasBank service
    /// </summary>
    public class GasBankService : IGasBankService
    {
        private readonly ILogger<GasBankService> _logger;
        private readonly IGasBankAccountRepository _accountRepository;
        private readonly IGasBankAllocationRepository _allocationRepository;
        private readonly IGasBankTransactionRepository _transactionRepository;
        private readonly IWalletService _walletService;
        private readonly IEnclaveService _enclaveService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GasBankService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="accountRepository">GasBank account repository</param>
        /// <param name="allocationRepository">GasBank allocation repository</param>
        /// <param name="transactionRepository">GasBank transaction repository</param>
        /// <param name="walletService">Wallet service</param>
        /// <param name="enclaveService">Enclave service</param>
        public GasBankService(
            ILogger<GasBankService> logger,
            IGasBankAccountRepository accountRepository,
            IGasBankAllocationRepository allocationRepository,
            IGasBankTransactionRepository transactionRepository,
            IWalletService walletService,
            IEnclaveService enclaveService)
        {
            _logger = logger;
            _accountRepository = accountRepository;
            _allocationRepository = allocationRepository;
            _transactionRepository = transactionRepository;
            _walletService = walletService;
            _enclaveService = enclaveService;
        }

        /// <inheritdoc/>
        public async Task<GasBankAccount> CreateAccountAsync(Guid accountId, string name, decimal initialDeposit = 0)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = accountId,
                ["Name"] = name,
                ["InitialDeposit"] = initialDeposit
            };

            LoggingUtility.LogOperationStart(_logger, "CreateGasBankAccount", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(accountId, "Account ID");
                ValidationUtility.ValidateNotNullOrEmpty(name, "Name");

                if (initialDeposit < 0)
                {
                    throw new ArgumentException("Initial deposit cannot be negative");
                }

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, GasBankAccount>(
                    _logger,
                    async () =>
                    {
                        // Create wallet for GasBank account
                        var wallet = await _walletService.CreateWalletAsync(
                            $"GasBank-{name}",
                            Guid.NewGuid().ToString(), // Generate a random password
                            accountId,
                            true); // Service wallet

                        // Create GasBank account
                        var gasBankAccount = new GasBankAccount
                        {
                            Id = Guid.NewGuid(),
                            AccountId = accountId,
                            Name = name,
                            Balance = initialDeposit,
                            AllocatedAmount = 0,
                            WalletId = wallet.Id,
                            NeoAddress = wallet.Address,
                            Tags = new Dictionary<string, string>(),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _accountRepository.CreateAsync(gasBankAccount);

                        additionalData["GasBankAccountId"] = gasBankAccount.Id;
                        additionalData["NeoAddress"] = gasBankAccount.NeoAddress;

                        // Create initial deposit transaction if needed
                        if (initialDeposit > 0)
                        {
                            var transaction = new GasBankTransaction
                            {
                                Id = Guid.NewGuid(),
                                GasBankAccountId = gasBankAccount.Id,
                                Type = GasBankTransactionType.Deposit,
                                Amount = initialDeposit,
                                BalanceAfter = initialDeposit,
                                TransactionHash = null, // No blockchain transaction for initial deposit
                                RelatedEntityId = null,
                                NeoAddress = null,
                                Timestamp = DateTime.UtcNow,
                                Description = "Initial deposit"
                            };

                            await _transactionRepository.CreateAsync(transaction);
                        }

                        return gasBankAccount;
                    },
                    "CreateGasBankAccount",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new Exception("Failed to create GasBank account");
                }

                LoggingUtility.LogOperationSuccess(_logger, "CreateGasBankAccount", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "CreateGasBankAccount", requestId, ex, 0, additionalData);
                throw new GasBankException("Error creating GasBank account", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<GasBankAccount> GetByIdAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankAccountById", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank account ID");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, GasBankAccount>(
                    _logger,
                    async () => await _accountRepository.GetByIdAsync(id),
                    "GetGasBankAccountById",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["AccountId"] = result.result.AccountId;
                    additionalData["Name"] = result.result.Name;
                    additionalData["Balance"] = result.result.Balance;
                }

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankAccountById", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankAccountById", requestId, ex, 0, additionalData);
                throw new GasBankException($"Error getting GasBank account by ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GasBankAccount>> GetByAccountIdAsync(Guid accountId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = accountId
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankAccountsByAccountId", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(accountId, "Account ID");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, IEnumerable<GasBankAccount>>(
                    _logger,
                    async () => await _accountRepository.GetByAccountIdAsync(accountId),
                    "GetGasBankAccountsByAccountId",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["Count"] = result.result.Count();
                }

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankAccountsByAccountId", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankAccountsByAccountId", requestId, ex, 0, additionalData);
                throw new GasBankException($"Error getting GasBank accounts by account ID {accountId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<GasBankAccount> DepositAsync(Guid id, decimal amount)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["Amount"] = amount
            };

            LoggingUtility.LogOperationStart(_logger, "DepositToGasBankAccount", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank account ID");
                ValidationUtility.ValidateGreaterThanZero(amount, "Amount");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, GasBankAccount>(
                    _logger,
                    async () =>
                    {
                        // Get GasBank account
                        var gasBankAccount = await _accountRepository.GetByIdAsync(id);
                        if (gasBankAccount == null)
                        {
                            throw new GasBankException("GasBank account not found");
                        }

                        additionalData["AccountId"] = gasBankAccount.AccountId;
                        additionalData["Name"] = gasBankAccount.Name;
                        additionalData["PreviousBalance"] = gasBankAccount.Balance;

                        // Update balance
                        gasBankAccount.Balance += amount;
                        gasBankAccount.UpdatedAt = DateTime.UtcNow;

                        // Create transaction record
                        var transaction = new GasBankTransaction
                        {
                            Id = Guid.NewGuid(),
                            GasBankAccountId = gasBankAccount.Id,
                            Type = GasBankTransactionType.Deposit,
                            Amount = amount,
                            BalanceAfter = gasBankAccount.Balance,
                            TransactionHash = null, // No blockchain transaction for manual deposit
                            RelatedEntityId = null,
                            NeoAddress = null,
                            Timestamp = DateTime.UtcNow,
                            Description = "Manual deposit"
                        };

                        // Update account and create transaction
                        await _accountRepository.UpdateAsync(gasBankAccount);
                        await _transactionRepository.CreateAsync(transaction);

                        additionalData["NewBalance"] = gasBankAccount.Balance;
                        additionalData["TransactionId"] = transaction.Id;

                        return gasBankAccount;
                    },
                    "DepositToGasBankAccount",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new Exception("Failed to deposit to GasBank account");
                }

                LoggingUtility.LogOperationSuccess(_logger, "DepositToGasBankAccount", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "DepositToGasBankAccount", requestId, ex, 0, additionalData);
                throw new GasBankException("Error depositing to GasBank account", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> WithdrawAsync(Guid id, decimal amount, string toAddress)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["Amount"] = amount,
                ["ToAddress"] = toAddress
            };

            LoggingUtility.LogOperationStart(_logger, "WithdrawFromGasBankAccount", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank account ID");
                ValidationUtility.ValidateGreaterThanZero(amount, "Amount");
                ValidationUtility.ValidateNotNullOrEmpty(toAddress, "To address");

                if (!toAddress.IsValidNeoAddress())
                {
                    throw new ArgumentException("Invalid Neo address format");
                }

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, string>(
                    _logger,
                    async () =>
                    {
                        // Get GasBank account
                        var gasBankAccount = await _accountRepository.GetByIdAsync(id);
                        if (gasBankAccount == null)
                        {
                            throw new GasBankException("GasBank account not found");
                        }

                        additionalData["AccountId"] = gasBankAccount.AccountId;
                        additionalData["Name"] = gasBankAccount.Name;
                        additionalData["PreviousBalance"] = gasBankAccount.Balance;

                        // Check if there's enough balance
                        if (gasBankAccount.Balance < amount)
                        {
                            throw new GasBankException("Insufficient balance");
                        }

                        // Check if there's enough unallocated balance
                        decimal availableBalance = gasBankAccount.Balance - gasBankAccount.AllocatedAmount;
                        if (availableBalance < amount)
                        {
                            throw new GasBankException("Insufficient unallocated balance");
                        }

                        // Transfer GAS from wallet to the specified address
                        string transactionHash = await _walletService.TransferGasAsync(
                            gasBankAccount.WalletId,
                            Guid.NewGuid().ToString(), // Password is not used for service wallets
                            toAddress,
                            amount);

                        // Update balance
                        gasBankAccount.Balance -= amount;
                        gasBankAccount.UpdatedAt = DateTime.UtcNow;

                        // Create transaction record
                        var transaction = new GasBankTransaction
                        {
                            Id = Guid.NewGuid(),
                            GasBankAccountId = gasBankAccount.Id,
                            Type = GasBankTransactionType.Withdrawal,
                            Amount = amount,
                            BalanceAfter = gasBankAccount.Balance,
                            TransactionHash = transactionHash,
                            RelatedEntityId = null,
                            NeoAddress = toAddress,
                            Timestamp = DateTime.UtcNow,
                            Description = "Withdrawal to external address"
                        };

                        // Update account and create transaction
                        await _accountRepository.UpdateAsync(gasBankAccount);
                        await _transactionRepository.CreateAsync(transaction);

                        additionalData["NewBalance"] = gasBankAccount.Balance;
                        additionalData["TransactionId"] = transaction.Id;
                        additionalData["TransactionHash"] = transactionHash;

                        return transactionHash;
                    },
                    "WithdrawFromGasBankAccount",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new Exception("Failed to withdraw from GasBank account");
                }

                LoggingUtility.LogOperationSuccess(_logger, "WithdrawFromGasBankAccount", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "WithdrawFromGasBankAccount", requestId, ex, 0, additionalData);
                throw new GasBankException("Error withdrawing from GasBank account", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<GasBankAllocation> AllocateToFunctionAsync(Guid id, Guid functionId, decimal amount)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["FunctionId"] = functionId,
                ["Amount"] = amount
            };

            LoggingUtility.LogOperationStart(_logger, "AllocateToFunction", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank account ID");
                ValidationUtility.ValidateGuid(functionId, "Function ID");
                ValidationUtility.ValidateGreaterThanZero(amount, "Amount");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, GasBankAllocation>(
                    _logger,
                    async () =>
                    {
                        // Get GasBank account
                        var gasBankAccount = await _accountRepository.GetByIdAsync(id);
                        if (gasBankAccount == null)
                        {
                            throw new GasBankException("GasBank account not found");
                        }

                        additionalData["AccountId"] = gasBankAccount.AccountId;
                        additionalData["Name"] = gasBankAccount.Name;
                        additionalData["CurrentBalance"] = gasBankAccount.Balance;
                        additionalData["CurrentAllocatedAmount"] = gasBankAccount.AllocatedAmount;

                        // Check if there's enough unallocated balance
                        decimal availableBalance = gasBankAccount.Balance - gasBankAccount.AllocatedAmount;
                        if (availableBalance < amount)
                        {
                            throw new GasBankException("Insufficient unallocated balance");
                        }

                        // Check if there's already an allocation for this function
                        var existingAllocations = await _allocationRepository.GetByFunctionIdAsync(functionId);
                        var existingAllocation = existingAllocations.FirstOrDefault(a => a.GasBankAccountId == id);

                        if (existingAllocation != null)
                        {
                            throw new GasBankException("Allocation already exists for this function");
                        }

                        // Create allocation
                        var allocation = new GasBankAllocation
                        {
                            Id = Guid.NewGuid(),
                            GasBankAccountId = id,
                            FunctionId = functionId,
                            Amount = amount,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        // Update GasBank account allocated amount
                        gasBankAccount.AllocatedAmount += amount;
                        gasBankAccount.UpdatedAt = DateTime.UtcNow;

                        // Create transaction record
                        var transaction = new GasBankTransaction
                        {
                            Id = Guid.NewGuid(),
                            GasBankAccountId = gasBankAccount.Id,
                            Type = GasBankTransactionType.Allocation,
                            Amount = amount,
                            BalanceAfter = gasBankAccount.Balance,
                            TransactionHash = null,
                            RelatedEntityId = functionId,
                            NeoAddress = null,
                            Timestamp = DateTime.UtcNow,
                            Description = $"Allocation to function {functionId}"
                        };

                        // Save changes
                        await _accountRepository.UpdateAsync(gasBankAccount);
                        await _allocationRepository.CreateAsync(allocation);
                        await _transactionRepository.CreateAsync(transaction);

                        additionalData["AllocationId"] = allocation.Id;
                        additionalData["NewAllocatedAmount"] = gasBankAccount.AllocatedAmount;
                        additionalData["TransactionId"] = transaction.Id;

                        return allocation;
                    },
                    "AllocateToFunction",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new Exception("Failed to allocate to function");
                }

                LoggingUtility.LogOperationSuccess(_logger, "AllocateToFunction", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "AllocateToFunction", requestId, ex, 0, additionalData);
                throw new GasBankException("Error allocating to function", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GasBankAllocation>> GetAllocationsAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankAllocations", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank account ID");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, IEnumerable<GasBankAllocation>>(
                    _logger,
                    async () =>
                    {
                        // Verify GasBank account exists
                        var gasBankAccount = await _accountRepository.GetByIdAsync(id);
                        if (gasBankAccount == null)
                        {
                            throw new GasBankException("GasBank account not found");
                        }

                        additionalData["AccountId"] = gasBankAccount.AccountId;
                        additionalData["Name"] = gasBankAccount.Name;

                        // Get allocations
                        var allocations = await _allocationRepository.GetByGasBankAccountIdAsync(id);

                        additionalData["Count"] = allocations.Count();

                        return allocations;
                    },
                    "GetGasBankAllocations",
                    requestId,
                    additionalData);

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankAllocations", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankAllocations", requestId, ex, 0, additionalData);
                throw new GasBankException($"Error getting allocations for GasBank account {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<GasBankAllocation> UpdateAllocationAsync(Guid id, decimal amount)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["Amount"] = amount
            };

            LoggingUtility.LogOperationStart(_logger, "UpdateGasBankAllocation", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "Allocation ID");
                ValidationUtility.ValidateGreaterThanZero(amount, "Amount");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, GasBankAllocation>(
                    _logger,
                    async () =>
                    {
                        // Get allocation
                        var allocation = await _allocationRepository.GetByIdAsync(id);
                        if (allocation == null)
                        {
                            throw new GasBankException("Allocation not found");
                        }

                        additionalData["GasBankAccountId"] = allocation.GasBankAccountId;
                        additionalData["FunctionId"] = allocation.FunctionId;
                        additionalData["PreviousAmount"] = allocation.Amount;

                        // Get GasBank account
                        var gasBankAccount = await _accountRepository.GetByIdAsync(allocation.GasBankAccountId);
                        if (gasBankAccount == null)
                        {
                            throw new GasBankException("GasBank account not found");
                        }

                        additionalData["AccountId"] = gasBankAccount.AccountId;
                        additionalData["Name"] = gasBankAccount.Name;
                        additionalData["CurrentBalance"] = gasBankAccount.Balance;
                        additionalData["CurrentAllocatedAmount"] = gasBankAccount.AllocatedAmount;

                        // Calculate new allocated amount
                        decimal difference = amount - allocation.Amount;
                        decimal newAllocatedAmount = gasBankAccount.AllocatedAmount + difference;

                        // Check if there's enough unallocated balance if increasing allocation
                        if (difference > 0)
                        {
                            decimal availableBalance = gasBankAccount.Balance - gasBankAccount.AllocatedAmount;
                            if (availableBalance < difference)
                            {
                                throw new GasBankException("Insufficient unallocated balance");
                            }
                        }

                        // Update allocation
                        allocation.Amount = amount;
                        allocation.UpdatedAt = DateTime.UtcNow;

                        // Update GasBank account allocated amount
                        gasBankAccount.AllocatedAmount = newAllocatedAmount;
                        gasBankAccount.UpdatedAt = DateTime.UtcNow;

                        // Create transaction record
                        var transaction = new GasBankTransaction
                        {
                            Id = Guid.NewGuid(),
                            GasBankAccountId = gasBankAccount.Id,
                            Type = difference > 0 ? GasBankTransactionType.Allocation : GasBankTransactionType.Deallocation,
                            Amount = Math.Abs(difference),
                            BalanceAfter = gasBankAccount.Balance,
                            TransactionHash = null,
                            RelatedEntityId = allocation.FunctionId,
                            NeoAddress = null,
                            Timestamp = DateTime.UtcNow,
                            Description = $"Updated allocation for function {allocation.FunctionId}"
                        };

                        // Save changes
                        await _accountRepository.UpdateAsync(gasBankAccount);
                        await _allocationRepository.UpdateAsync(allocation);
                        await _transactionRepository.CreateAsync(transaction);

                        additionalData["NewAmount"] = allocation.Amount;
                        additionalData["NewAllocatedAmount"] = gasBankAccount.AllocatedAmount;
                        additionalData["TransactionId"] = transaction.Id;

                        return allocation;
                    },
                    "UpdateGasBankAllocation",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new Exception("Failed to update allocation");
                }

                LoggingUtility.LogOperationSuccess(_logger, "UpdateGasBankAllocation", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "UpdateGasBankAllocation", requestId, ex, 0, additionalData);
                throw new GasBankException("Error updating allocation", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveAllocationAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            LoggingUtility.LogOperationStart(_logger, "RemoveGasBankAllocation", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "Allocation ID");

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, bool>(
                    _logger,
                    async () =>
                    {
                        // Get allocation
                        var allocation = await _allocationRepository.GetByIdAsync(id);
                        if (allocation == null)
                        {
                            throw new GasBankException("Allocation not found");
                        }

                        additionalData["GasBankAccountId"] = allocation.GasBankAccountId;
                        additionalData["FunctionId"] = allocation.FunctionId;
                        additionalData["Amount"] = allocation.Amount;

                        // Get GasBank account
                        var gasBankAccount = await _accountRepository.GetByIdAsync(allocation.GasBankAccountId);
                        if (gasBankAccount == null)
                        {
                            throw new GasBankException("GasBank account not found");
                        }

                        additionalData["AccountId"] = gasBankAccount.AccountId;
                        additionalData["Name"] = gasBankAccount.Name;
                        additionalData["CurrentAllocatedAmount"] = gasBankAccount.AllocatedAmount;

                        // Update GasBank account allocated amount
                        gasBankAccount.AllocatedAmount -= allocation.Amount;
                        gasBankAccount.UpdatedAt = DateTime.UtcNow;

                        // Create transaction record
                        var transaction = new GasBankTransaction
                        {
                            Id = Guid.NewGuid(),
                            GasBankAccountId = gasBankAccount.Id,
                            Type = GasBankTransactionType.Deallocation,
                            Amount = allocation.Amount,
                            BalanceAfter = gasBankAccount.Balance,
                            TransactionHash = null,
                            RelatedEntityId = allocation.FunctionId,
                            NeoAddress = null,
                            Timestamp = DateTime.UtcNow,
                            Description = $"Removed allocation for function {allocation.FunctionId}"
                        };

                        // Save changes
                        await _accountRepository.UpdateAsync(gasBankAccount);
                        await _allocationRepository.DeleteAsync(id);
                        await _transactionRepository.CreateAsync(transaction);

                        additionalData["NewAllocatedAmount"] = gasBankAccount.AllocatedAmount;
                        additionalData["TransactionId"] = transaction.Id;

                        return true;
                    },
                    "RemoveGasBankAllocation",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new Exception("Failed to remove allocation");
                }

                LoggingUtility.LogOperationSuccess(_logger, "RemoveGasBankAllocation", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "RemoveGasBankAllocation", requestId, ex, 0, additionalData);
                throw new GasBankException("Error removing allocation", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GasBankTransaction>> GetTransactionHistoryAsync(Guid id, DateTime startTime, DateTime endTime)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["StartTime"] = startTime,
                ["EndTime"] = endTime
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankTransactionHistory", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank account ID");

                if (endTime < startTime)
                {
                    throw new ArgumentException("End time must be greater than or equal to start time");
                }

                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<GasBankService, IEnumerable<GasBankTransaction>>(
                    _logger,
                    async () =>
                    {
                        // Verify GasBank account exists
                        var gasBankAccount = await _accountRepository.GetByIdAsync(id);
                        if (gasBankAccount == null)
                        {
                            throw new GasBankException("GasBank account not found");
                        }

                        additionalData["AccountId"] = gasBankAccount.AccountId;
                        additionalData["Name"] = gasBankAccount.Name;

                        // Get transactions
                        var transactions = await _transactionRepository.GetByGasBankAccountIdAndTimeRangeAsync(id, startTime, endTime);

                        additionalData["Count"] = transactions.Count();

                        return transactions;
                    },
                    "GetGasBankTransactionHistory",
                    requestId,
                    additionalData);

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankTransactionHistory", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankTransactionHistory", requestId, ex, 0, additionalData);
                throw new GasBankException($"Error getting transaction history for GasBank account {id}", ex);
            }
        }
    }
}