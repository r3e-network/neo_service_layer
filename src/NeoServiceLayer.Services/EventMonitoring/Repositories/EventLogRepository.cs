using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.EventMonitoring.Repositories
{
    /// <summary>
    /// Implementation of the event log repository
    /// </summary>
    public class EventLogRepository : IEventLogRepository
    {
        private readonly ILogger<EventLogRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string CollectionName = "event_logs";

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public EventLogRepository(ILogger<EventLogRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<EventLog> CreateAsync(EventLog eventLog)
        {
            _logger.LogInformation("Creating event log for subscription: {SubscriptionId}, event: {EventName}", eventLog.SubscriptionId, eventLog.EventName);

            try
            {
                // Set default values
                if (eventLog.Id == Guid.Empty)
                {
                    eventLog.Id = Guid.NewGuid();
                }

                eventLog.DetectedAt = DateTime.UtcNow;

                if (eventLog.NotificationStatus == 0)
                {
                    eventLog.NotificationStatus = NotificationStatus.Pending;
                }

                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(CollectionName))
                {
                    await _databaseService.CreateCollectionAsync(CollectionName);
                }

                // Create event log
                return await _databaseService.CreateAsync(CollectionName, eventLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event log for subscription: {SubscriptionId}, event: {EventName}", eventLog.SubscriptionId, eventLog.EventName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventLog> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting event log by ID: {Id}", id);

            try
            {
                return await _databaseService.GetByIdAsync<EventLog, Guid>(CollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event log by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetBySubscriptionAsync(Guid subscriptionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for subscription: {SubscriptionId}, limit: {Limit}, offset: {Offset}", subscriptionId, limit, offset);

            try
            {
                var logs = await _databaseService.GetByFilterAsync<EventLog>(
                    CollectionName,
                    l => l.SubscriptionId == subscriptionId);

                return logs.OrderByDescending(l => l.BlockHeight)
                          .Skip(offset)
                          .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for subscription: {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetByAccountAsync(Guid accountId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for account: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            try
            {
                var logs = await _databaseService.GetByFilterAsync<EventLog>(
                    CollectionName,
                    l => l.AccountId == accountId);

                return logs.OrderByDescending(l => l.BlockHeight)
                          .Skip(offset)
                          .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetByTransactionAsync(string transactionHash)
        {
            _logger.LogInformation("Getting event logs for transaction: {TransactionHash}", transactionHash);

            try
            {
                return await _databaseService.GetByFilterAsync<EventLog>(
                    CollectionName,
                    l => l.TransactionHash == transactionHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for transaction: {TransactionHash}", transactionHash);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetByBlockAsync(long blockHeight, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for block: {BlockHeight}, limit: {Limit}, offset: {Offset}", blockHeight, limit, offset);

            try
            {
                var logs = await _databaseService.GetByFilterAsync<EventLog>(
                    CollectionName,
                    l => l.BlockHeight == blockHeight);

                return logs.Skip(offset).Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for block: {BlockHeight}", blockHeight);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetByContractAsync(string contractHash, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for contract: {ContractHash}, limit: {Limit}, offset: {Offset}", contractHash, limit, offset);

            try
            {
                var logs = await _databaseService.GetByFilterAsync<EventLog>(
                    CollectionName,
                    l => l.ContractHash == contractHash);

                return logs.OrderByDescending(l => l.BlockHeight)
                          .Skip(offset)
                          .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for contract: {ContractHash}", contractHash);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetByEventAsync(string eventName, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for event: {EventName}, limit: {Limit}, offset: {Offset}", eventName, limit, offset);

            try
            {
                var logs = await _databaseService.GetByFilterAsync<EventLog>(
                    CollectionName,
                    l => l.EventName == eventName);

                return logs.OrderByDescending(l => l.BlockHeight)
                          .Skip(offset)
                          .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for event: {EventName}", eventName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetByNotificationStatusAsync(NotificationStatus status, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs with notification status: {Status}, limit: {Limit}, offset: {Offset}", status, limit, offset);

            try
            {
                var logs = await _databaseService.GetByFilterAsync<EventLog>(
                    CollectionName,
                    l => l.NotificationStatus == status);

                return logs.OrderByDescending(l => l.BlockHeight)
                          .Skip(offset)
                          .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs with notification status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetForRetryAsync(int limit = 100)
        {
            _logger.LogInformation("Getting event logs for retry, limit: {Limit}", limit);

            try
            {
                var now = DateTime.UtcNow;
                var logs = await _databaseService.GetByFilterAsync<EventLog>(
                    CollectionName,
                    l => l.NotificationStatus == NotificationStatus.Retrying &&
                         l.NextRetryAt.HasValue &&
                         l.NextRetryAt.Value <= now);

                return logs.OrderBy(l => l.NextRetryAt)
                          .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for retry");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventLog> UpdateAsync(EventLog eventLog)
        {
            _logger.LogInformation("Updating event log: {Id}", eventLog.Id);

            try
            {
                return await _databaseService.UpdateAsync<EventLog, Guid>(CollectionName, eventLog.Id, eventLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event log: {Id}", eventLog.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountBySubscriptionAsync(Guid subscriptionId)
        {
            _logger.LogInformation("Getting count of event logs for subscription: {SubscriptionId}", subscriptionId);

            try
            {
                return await _databaseService.CountAsync<EventLog>(
                    CollectionName,
                    l => l.SubscriptionId == subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of event logs for subscription: {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting count of event logs for account: {AccountId}", accountId);

            try
            {
                return await _databaseService.CountAsync<EventLog>(
                    CollectionName,
                    l => l.AccountId == accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of event logs for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountByNotificationStatusAsync(NotificationStatus status)
        {
            _logger.LogInformation("Getting count of event logs with notification status: {Status}", status);

            try
            {
                return await _databaseService.CountAsync<EventLog>(
                    CollectionName,
                    l => l.NotificationStatus == status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of event logs with notification status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountLast24HoursAsync()
        {
            _logger.LogInformation("Getting count of event logs in the last 24 hours");

            try
            {
                var cutoff = DateTime.UtcNow.AddHours(-24);
                return await _databaseService.CountAsync<EventLog>(
                    CollectionName,
                    l => l.DetectedAt >= cutoff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of event logs in the last 24 hours");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetSuccessfulNotificationsLast24HoursAsync()
        {
            _logger.LogInformation("Getting count of successful notifications in the last 24 hours");

            try
            {
                var cutoff = DateTime.UtcNow.AddHours(-24);
                return await _databaseService.CountAsync<EventLog>(
                    CollectionName,
                    l => l.NotificationStatus == NotificationStatus.Sent &&
                         l.NotifiedAt.HasValue &&
                         l.NotifiedAt.Value >= cutoff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of successful notifications in the last 24 hours");
                throw;
            }
        }
    }
}
