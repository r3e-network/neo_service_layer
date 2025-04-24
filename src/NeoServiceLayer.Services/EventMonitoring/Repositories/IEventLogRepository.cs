using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.EventMonitoring.Repositories
{
    /// <summary>
    /// Interface for event log repository
    /// </summary>
    public interface IEventLogRepository
    {
        /// <summary>
        /// Creates a new event log
        /// </summary>
        /// <param name="eventLog">Event log to create</param>
        /// <returns>The created event log</returns>
        Task<EventLog> CreateAsync(EventLog eventLog);

        /// <summary>
        /// Gets an event log by ID
        /// </summary>
        /// <param name="id">Event log ID</param>
        /// <returns>The event log if found, null otherwise</returns>
        Task<EventLog> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets event logs by subscription ID
        /// </summary>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the subscription</returns>
        Task<IEnumerable<EventLog>> GetBySubscriptionAsync(Guid subscriptionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the account</returns>
        Task<IEnumerable<EventLog>> GetByAccountAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by transaction hash
        /// </summary>
        /// <param name="transactionHash">Transaction hash</param>
        /// <returns>List of event logs for the transaction</returns>
        Task<IEnumerable<EventLog>> GetByTransactionAsync(string transactionHash);

        /// <summary>
        /// Gets event logs by block height
        /// </summary>
        /// <param name="blockHeight">Block height</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the block</returns>
        Task<IEnumerable<EventLog>> GetByBlockAsync(long blockHeight, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by contract hash
        /// </summary>
        /// <param name="contractHash">Contract hash</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the contract</returns>
        Task<IEnumerable<EventLog>> GetByContractAsync(string contractHash, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by event name
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the event</returns>
        Task<IEnumerable<EventLog>> GetByEventAsync(string eventName, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by notification status
        /// </summary>
        /// <param name="status">Notification status</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs with the specified notification status</returns>
        Task<IEnumerable<EventLog>> GetByNotificationStatusAsync(NotificationStatus status, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs that need to be retried
        /// </summary>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <returns>List of event logs that need to be retried</returns>
        Task<IEnumerable<EventLog>> GetForRetryAsync(int limit = 100);

        /// <summary>
        /// Updates an event log
        /// </summary>
        /// <param name="eventLog">Event log to update</param>
        /// <returns>The updated event log</returns>
        Task<EventLog> UpdateAsync(EventLog eventLog);

        /// <summary>
        /// Gets the count of event logs by subscription ID
        /// </summary>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <returns>Count of event logs for the subscription</returns>
        Task<int> GetCountBySubscriptionAsync(Guid subscriptionId);

        /// <summary>
        /// Gets the count of event logs by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Count of event logs for the account</returns>
        Task<int> GetCountByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets the count of event logs by notification status
        /// </summary>
        /// <param name="status">Notification status</param>
        /// <returns>Count of event logs with the specified notification status</returns>
        Task<int> GetCountByNotificationStatusAsync(NotificationStatus status);

        /// <summary>
        /// Gets the count of event logs in the last 24 hours
        /// </summary>
        /// <returns>Count of event logs in the last 24 hours</returns>
        Task<int> GetCountLast24HoursAsync();

        /// <summary>
        /// Gets the count of successful notifications in the last 24 hours
        /// </summary>
        /// <returns>Count of successful notifications in the last 24 hours</returns>
        Task<int> GetSuccessfulNotificationsLast24HoursAsync();
    }
}
