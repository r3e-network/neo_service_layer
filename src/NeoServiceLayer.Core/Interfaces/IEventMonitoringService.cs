using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for event monitoring service
    /// </summary>
    public interface IEventMonitoringService
    {
        /// <summary>
        /// Starts monitoring for events
        /// </summary>
        /// <returns>True if monitoring started successfully, false otherwise</returns>
        Task<bool> StartMonitoringAsync();

        /// <summary>
        /// Stops monitoring for events
        /// </summary>
        /// <returns>True if monitoring stopped successfully, false otherwise</returns>
        Task<bool> StopMonitoringAsync();

        /// <summary>
        /// Gets the monitoring status
        /// </summary>
        /// <returns>True if monitoring is active, false otherwise</returns>
        Task<bool> GetMonitoringStatusAsync();

        /// <summary>
        /// Gets the current block height
        /// </summary>
        /// <returns>The current block height</returns>
        Task<long> GetCurrentBlockHeightAsync();

        /// <summary>
        /// Creates a new event subscription
        /// </summary>
        /// <param name="subscription">Subscription to create</param>
        /// <returns>The created subscription</returns>
        Task<EventSubscription> CreateSubscriptionAsync(EventSubscription subscription);

        /// <summary>
        /// Gets a subscription by ID
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>The subscription if found, null otherwise</returns>
        Task<EventSubscription> GetSubscriptionAsync(Guid id);

        /// <summary>
        /// Gets subscriptions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of subscriptions for the account</returns>
        Task<IEnumerable<EventSubscription>> GetSubscriptionsByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets subscriptions by contract hash
        /// </summary>
        /// <param name="contractHash">Contract hash</param>
        /// <returns>List of subscriptions for the contract</returns>
        Task<IEnumerable<EventSubscription>> GetSubscriptionsByContractAsync(string contractHash);

        /// <summary>
        /// Gets subscriptions by event name
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <returns>List of subscriptions for the event</returns>
        Task<IEnumerable<EventSubscription>> GetSubscriptionsByEventAsync(string eventName);

        /// <summary>
        /// Gets active subscriptions
        /// </summary>
        /// <returns>List of active subscriptions</returns>
        Task<IEnumerable<EventSubscription>> GetActiveSubscriptionsAsync();

        /// <summary>
        /// Updates a subscription
        /// </summary>
        /// <param name="subscription">Subscription to update</param>
        /// <returns>The updated subscription</returns>
        Task<EventSubscription> UpdateSubscriptionAsync(EventSubscription subscription);

        /// <summary>
        /// Deletes a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>True if the subscription was deleted, false otherwise</returns>
        Task<bool> DeleteSubscriptionAsync(Guid id);

        /// <summary>
        /// Activates a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>The activated subscription</returns>
        Task<EventSubscription> ActivateSubscriptionAsync(Guid id);

        /// <summary>
        /// Pauses a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>The paused subscription</returns>
        Task<EventSubscription> PauseSubscriptionAsync(Guid id);

        /// <summary>
        /// Gets event logs by subscription ID
        /// </summary>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the subscription</returns>
        Task<IEnumerable<EventLog>> GetEventLogsBySubscriptionAsync(Guid subscriptionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the account</returns>
        Task<IEnumerable<EventLog>> GetEventLogsByAccountAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by transaction hash
        /// </summary>
        /// <param name="transactionHash">Transaction hash</param>
        /// <returns>List of event logs for the transaction</returns>
        Task<IEnumerable<EventLog>> GetEventLogsByTransactionAsync(string transactionHash);

        /// <summary>
        /// Gets event logs by block height
        /// </summary>
        /// <param name="blockHeight">Block height</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the block</returns>
        Task<IEnumerable<EventLog>> GetEventLogsByBlockAsync(long blockHeight, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by contract hash
        /// </summary>
        /// <param name="contractHash">Contract hash</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the contract</returns>
        Task<IEnumerable<EventLog>> GetEventLogsByContractAsync(string contractHash, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by event name
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs for the event</returns>
        Task<IEnumerable<EventLog>> GetEventLogsByEventAsync(string eventName, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs by notification status
        /// </summary>
        /// <param name="status">Notification status</param>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of event logs with the specified notification status</returns>
        Task<IEnumerable<EventLog>> GetEventLogsByNotificationStatusAsync(NotificationStatus status, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets event logs that need to be retried
        /// </summary>
        /// <param name="limit">Maximum number of logs to return</param>
        /// <returns>List of event logs that need to be retried</returns>
        Task<IEnumerable<EventLog>> GetEventLogsForRetryAsync(int limit = 100);

        /// <summary>
        /// Retries sending a notification for an event log
        /// </summary>
        /// <param name="eventLogId">Event log ID</param>
        /// <returns>True if the notification was sent successfully, false otherwise</returns>
        Task<bool> RetryNotificationAsync(Guid eventLogId);

        /// <summary>
        /// Cancels a notification for an event log
        /// </summary>
        /// <param name="eventLogId">Event log ID</param>
        /// <returns>True if the notification was cancelled successfully, false otherwise</returns>
        Task<bool> CancelNotificationAsync(Guid eventLogId);

        /// <summary>
        /// Gets monitoring statistics
        /// </summary>
        /// <returns>Monitoring statistics</returns>
        Task<EventMonitoringStatistics> GetStatisticsAsync();
    }

    /// <summary>
    /// Statistics for event monitoring
    /// </summary>
    public class EventMonitoringStatistics
    {
        /// <summary>
        /// Gets or sets the current block height
        /// </summary>
        public long CurrentBlockHeight { get; set; }

        /// <summary>
        /// Gets or sets the last processed block height
        /// </summary>
        public long LastProcessedBlockHeight { get; set; }

        /// <summary>
        /// Gets or sets the total number of active subscriptions
        /// </summary>
        public int ActiveSubscriptions { get; set; }

        /// <summary>
        /// Gets or sets the total number of events detected
        /// </summary>
        public int TotalEventsDetected { get; set; }

        /// <summary>
        /// Gets or sets the total number of notifications sent
        /// </summary>
        public int TotalNotificationsSent { get; set; }

        /// <summary>
        /// Gets or sets the total number of notifications failed
        /// </summary>
        public int TotalNotificationsFailed { get; set; }

        /// <summary>
        /// Gets or sets the total number of notifications pending
        /// </summary>
        public int TotalNotificationsPending { get; set; }

        /// <summary>
        /// Gets or sets the total number of notifications retrying
        /// </summary>
        public int TotalNotificationsRetrying { get; set; }

        /// <summary>
        /// Gets or sets the events detected in the last 24 hours
        /// </summary>
        public int EventsDetectedLast24Hours { get; set; }

        /// <summary>
        /// Gets or sets the notifications sent in the last 24 hours
        /// </summary>
        public int NotificationsSentLast24Hours { get; set; }

        /// <summary>
        /// Gets or sets the monitoring status
        /// </summary>
        public bool IsMonitoring { get; set; }

        /// <summary>
        /// Gets or sets the monitoring start time
        /// </summary>
        public DateTime? MonitoringStartTime { get; set; }

        /// <summary>
        /// Gets or sets the last update time
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
    }
}
