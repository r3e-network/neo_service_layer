using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Repositories;
using NeoServiceLayer.Core.Utilities;

namespace NeoServiceLayer.Services.GasBank.Repositories
{
    /// <summary>
    /// Implementation of the GasBank account repository
    /// </summary>
    public class GasBankAccountRepository : IGasBankAccountRepository
    {
        private readonly ILogger<GasBankAccountRepository> _logger;
        private readonly IGenericRepository<GasBankAccount, Guid> _repository;
        private const string CollectionName = "gasbank_accounts";

        /// <summary>
        /// Initializes a new instance of the <see cref="GasBankAccountRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public GasBankAccountRepository(ILogger<GasBankAccountRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _repository = new GenericRepository<GasBankAccount, Guid>(logger, storageProvider, CollectionName);
        }

        /// <inheritdoc/>
        public async Task<GasBankAccount> CreateAsync(GasBankAccount account)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = account.Id,
                ["AccountId"] = account.AccountId,
                ["Name"] = account.Name
            };

            LoggingUtility.LogOperationStart(_logger, "CreateGasBankAccount", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateNotNull(account, nameof(account));
                ValidationUtility.ValidateGuid(account.Id, "GasBank account ID");
                ValidationUtility.ValidateGuid(account.AccountId, "Account ID");
                ValidationUtility.ValidateNotNullOrEmpty(account.Name, "Name");

                // Set timestamps
                account.CreatedAt = DateTime.UtcNow;
                account.UpdatedAt = DateTime.UtcNow;

                // Create account
                var result = await _repository.CreateAsync(account);

                LoggingUtility.LogOperationSuccess(_logger, "CreateGasBankAccount", requestId, 0, additionalData);

                return result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "CreateGasBankAccount", requestId, ex, 0, additionalData);
                throw;
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

                // Get account
                var account = await _repository.GetByIdAsync(id);

                if (account != null)
                {
                    additionalData["AccountId"] = account.AccountId;
                    additionalData["Name"] = account.Name;
                }

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankAccountById", requestId, 0, additionalData);

                return account;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankAccountById", requestId, ex, 0, additionalData);
                throw;
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

                // Get accounts
                var accounts = await _repository.FindAsync(account => account.AccountId == accountId);

                additionalData["Count"] = accounts.Count();

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankAccountsByAccountId", requestId, 0, additionalData);

                return accounts;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankAccountsByAccountId", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<GasBankAccount> UpdateAsync(GasBankAccount account)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = account.Id,
                ["AccountId"] = account.AccountId,
                ["Name"] = account.Name
            };

            LoggingUtility.LogOperationStart(_logger, "UpdateGasBankAccount", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateNotNull(account, nameof(account));
                ValidationUtility.ValidateGuid(account.Id, "GasBank account ID");
                ValidationUtility.ValidateGuid(account.AccountId, "Account ID");
                ValidationUtility.ValidateNotNullOrEmpty(account.Name, "Name");

                // Update timestamp
                account.UpdatedAt = DateTime.UtcNow;

                // Update account
                var result = await _repository.UpdateAsync(account.Id, account);

                LoggingUtility.LogOperationSuccess(_logger, "UpdateGasBankAccount", requestId, 0, additionalData);

                return result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "UpdateGasBankAccount", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            LoggingUtility.LogOperationStart(_logger, "DeleteGasBankAccount", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank account ID");

                // Delete account
                var result = await _repository.DeleteAsync(id);

                LoggingUtility.LogOperationSuccess(_logger, "DeleteGasBankAccount", requestId, 0, additionalData);

                return result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "DeleteGasBankAccount", requestId, ex, 0, additionalData);
                throw;
            }
        }
    }
}
