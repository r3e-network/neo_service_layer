using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Utilities;

namespace NeoServiceLayer.Services.GasBank.Repositories
{
    /// <summary>
    /// Implementation of the GasBank transaction repository
    /// </summary>
    public class GasBankTransactionRepository : IGasBankTransactionRepository
    {
        private readonly ILogger<GasBankTransactionRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private const string CollectionName = "gasbank_transactions";

        /// <summary>
        /// Initializes a new instance of the <see cref="GasBankTransactionRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public GasBankTransactionRepository(ILogger<GasBankTransactionRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<GasBankTransaction> CreateAsync(GasBankTransaction transaction)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = transaction.Id,
                ["GasBankAccountId"] = transaction.GasBankAccountId,
                ["Type"] = transaction.Type.ToString(),
                ["Amount"] = transaction.Amount
            };

            LoggingUtility.LogOperationStart(_logger, "CreateGasBankTransaction", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateNotNull(transaction, nameof(transaction));
                ValidationUtility.ValidateGuid(transaction.Id, "GasBank transaction ID");
                ValidationUtility.ValidateGuid(transaction.GasBankAccountId, "GasBank account ID");

                if (transaction.RelatedEntityId.HasValue)
                {
                    ValidationUtility.ValidateGuid(transaction.RelatedEntityId.Value, "Related entity ID");
                    additionalData["RelatedEntityId"] = transaction.RelatedEntityId.Value;
                }

                // Set timestamp if not already set
                if (transaction.Timestamp == default)
                {
                    transaction.Timestamp = DateTime.UtcNow;
                }

                // Create transaction
                await _storageProvider.CreateAsync(CollectionName, transaction);

                LoggingUtility.LogOperationSuccess(_logger, "CreateGasBankTransaction", requestId, 0, additionalData);

                return transaction;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "CreateGasBankTransaction", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<GasBankTransaction> GetByIdAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankTransactionById", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(id, "GasBank transaction ID");

                // Get transaction
                var transaction = await _storageProvider.GetByIdAsync<GasBankTransaction, Guid>(CollectionName, id);

                if (transaction != null)
                {
                    additionalData["GasBankAccountId"] = transaction.GasBankAccountId;
                    additionalData["Type"] = transaction.Type.ToString();
                    additionalData["Amount"] = transaction.Amount;
                }

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankTransactionById", requestId, 0, additionalData);

                return transaction;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankTransactionById", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GasBankTransaction>> GetByGasBankAccountIdAsync(Guid gasBankAccountId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["GasBankAccountId"] = gasBankAccountId
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankTransactionsByGasBankAccountId", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(gasBankAccountId, "GasBank account ID");

                // Get transactions
                var transactions = await _storageProvider.GetByFilterAsync<GasBankTransaction>(
                    CollectionName,
                    transaction => transaction.GasBankAccountId == gasBankAccountId);

                additionalData["Count"] = transactions.Count();

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankTransactionsByGasBankAccountId", requestId, 0, additionalData);

                return transactions.OrderByDescending(t => t.Timestamp);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankTransactionsByGasBankAccountId", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GasBankTransaction>> GetByGasBankAccountIdAndTimeRangeAsync(Guid gasBankAccountId, DateTime startTime, DateTime endTime)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["GasBankAccountId"] = gasBankAccountId,
                ["StartTime"] = startTime,
                ["EndTime"] = endTime
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankTransactionsByGasBankAccountIdAndTimeRange", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(gasBankAccountId, "GasBank account ID");

                if (endTime < startTime)
                {
                    throw new ArgumentException("End time must be greater than or equal to start time");
                }

                // Get transactions
                var transactions = await _storageProvider.GetByFilterAsync<GasBankTransaction>(
                    CollectionName,
                    transaction => transaction.GasBankAccountId == gasBankAccountId &&
                                  transaction.Timestamp >= startTime &&
                                  transaction.Timestamp <= endTime);

                additionalData["Count"] = transactions.Count();

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankTransactionsByGasBankAccountIdAndTimeRange", requestId, 0, additionalData);

                return transactions.OrderByDescending(t => t.Timestamp);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankTransactionsByGasBankAccountIdAndTimeRange", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GasBankTransaction>> GetByFunctionIdAsync(Guid functionId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = functionId
            };

            LoggingUtility.LogOperationStart(_logger, "GetGasBankTransactionsByFunctionId", requestId, additionalData);

            try
            {
                // Validate input
                ValidationUtility.ValidateGuid(functionId, "Function ID");

                // Get transactions
                var transactions = await _storageProvider.GetByFilterAsync<GasBankTransaction>(
                    CollectionName,
                    transaction => transaction.RelatedEntityId == functionId);

                additionalData["Count"] = transactions.Count();

                LoggingUtility.LogOperationSuccess(_logger, "GetGasBankTransactionsByFunctionId", requestId, 0, additionalData);

                return transactions.OrderByDescending(t => t.Timestamp);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetGasBankTransactionsByFunctionId", requestId, ex, 0, additionalData);
                throw;
            }
        }
    }
}
