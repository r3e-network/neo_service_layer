using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.EventMonitoring.Repositories
{
    /// <summary>
    /// Interface for event subscription repository
    /// </summary>
    public interface IEventSubscriptionRepository
    {
        /// <summary>
        /// Creates a new subscription
        /// </summary>
        /// <param name="subscription">Subscription to create</param>
        /// <returns>The created subscription</returns>
        Task<EventSubscription> CreateAsync(EventSubscription subscription);

        /// <summary>
        /// Gets a subscription by ID
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>The subscription if found, null otherwise</returns>
        Task<EventSubscription> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets subscriptions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of subscriptions for the account</returns>
        Task<IEnumerable<EventSubscription>> GetByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets subscriptions by contract hash
        /// </summary>
        /// <param name="contractHash">Contract hash</param>
        /// <returns>List of subscriptions for the contract</returns>
        Task<IEnumerable<EventSubscription>> GetByContractAsync(string contractHash);

        /// <summary>
        /// Gets subscriptions by event name
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <returns>List of subscriptions for the event</returns>
        Task<IEnumerable<EventSubscription>> GetByEventAsync(string eventName);

        /// <summary>
        /// Gets active subscriptions
        /// </summary>
        /// <returns>List of active subscriptions</returns>
        Task<IEnumerable<EventSubscription>> GetActiveAsync();

        /// <summary>
        /// Gets active subscriptions for a block range
        /// </summary>
        /// <param name="startBlockHeight">Start block height</param>
        /// <param name="endBlockHeight">End block height</param>
        /// <returns>List of active subscriptions for the block range</returns>
        Task<IEnumerable<EventSubscription>> GetActiveForBlockRangeAsync(long startBlockHeight, long endBlockHeight);

        /// <summary>
        /// Updates a subscription
        /// </summary>
        /// <param name="subscription">Subscription to update</param>
        /// <returns>The updated subscription</returns>
        Task<EventSubscription> UpdateAsync(EventSubscription subscription);

        /// <summary>
        /// Deletes a subscription
        /// </summary>
        /// <param name="id">Subscription ID</param>
        /// <returns>True if the subscription was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets the count of subscriptions by status
        /// </summary>
        /// <param name="status">Subscription status</param>
        /// <returns>Count of subscriptions with the specified status</returns>
        Task<int> GetCountByStatusAsync(EventSubscriptionStatus status);

        /// <summary>
        /// Gets the count of subscriptions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Count of subscriptions for the account</returns>
        Task<int> GetCountByAccountAsync(Guid accountId);
    }
}
