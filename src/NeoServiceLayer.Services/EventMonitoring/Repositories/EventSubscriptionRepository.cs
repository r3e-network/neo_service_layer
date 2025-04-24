using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.EventMonitoring.Repositories
{
    /// <summary>
    /// Implementation of the event subscription repository
    /// </summary>
    public class EventSubscriptionRepository : IEventSubscriptionRepository
    {
        private readonly ILogger<EventSubscriptionRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string CollectionName = "event_subscriptions";

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriptionRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public EventSubscriptionRepository(ILogger<EventSubscriptionRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<EventSubscription> CreateAsync(EventSubscription subscription)
        {
            _logger.LogInformation("Creating event subscription: {Name} for account: {AccountId}", subscription.Name, subscription.AccountId);

            try
            {
                // Set default values
                if (subscription.Id == Guid.Empty)
                {
                    subscription.Id = Guid.NewGuid();
                }

                subscription.CreatedAt = DateTime.UtcNow;
                subscription.UpdatedAt = DateTime.UtcNow;
                
                if (subscription.Status == 0)
                {
                    subscription.Status = EventSubscriptionStatus.Active;
                }

                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(CollectionName))
                {
                    await _databaseService.CreateCollectionAsync(CollectionName);
                }

                // Create subscription
                return await _databaseService.CreateAsync(CollectionName, subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event subscription: {Name} for account: {AccountId}", subscription.Name, subscription.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventSubscription> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting event subscription by ID: {Id}", id);

            try
            {
                return await _databaseService.GetByIdAsync<EventSubscription, Guid>(CollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event subscription by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventSubscription>> GetByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting event subscriptions for account: {AccountId}", accountId);

            try
            {
                return await _databaseService.GetByFilterAsync<EventSubscription>(
                    CollectionName,
                    s => s.AccountId == accountId && s.Status != EventSubscriptionStatus.Deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event subscriptions for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventSubscription>> GetByContractAsync(string contractHash)
        {
            _logger.LogInformation("Getting event subscriptions for contract: {ContractHash}", contractHash);

            try
            {
                return await _databaseService.GetByFilterAsync<EventSubscription>(
                    CollectionName,
                    s => s.ContractHash == contractHash && s.Status != EventSubscriptionStatus.Deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event subscriptions for contract: {ContractHash}", contractHash);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventSubscription>> GetByEventAsync(string eventName)
        {
            _logger.LogInformation("Getting event subscriptions for event: {EventName}", eventName);

            try
            {
                return await _databaseService.GetByFilterAsync<EventSubscription>(
                    CollectionName,
                    s => s.EventName == eventName && s.Status != EventSubscriptionStatus.Deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event subscriptions for event: {EventName}", eventName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventSubscription>> GetActiveAsync()
        {
            _logger.LogInformation("Getting active event subscriptions");

            try
            {
                return await _databaseService.GetByFilterAsync<EventSubscription>(
                    CollectionName,
                    s => s.Status == EventSubscriptionStatus.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active event subscriptions");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventSubscription>> GetActiveForBlockRangeAsync(long startBlockHeight, long endBlockHeight)
        {
            _logger.LogInformation("Getting active event subscriptions for block range: {StartBlockHeight} - {EndBlockHeight}", startBlockHeight, endBlockHeight);

            try
            {
                return await _databaseService.GetByFilterAsync<EventSubscription>(
                    CollectionName,
                    s => s.Status == EventSubscriptionStatus.Active &&
                         s.StartBlockHeight <= endBlockHeight &&
                         (s.EndBlockHeight == 0 || s.EndBlockHeight >= startBlockHeight));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active event subscriptions for block range: {StartBlockHeight} - {EndBlockHeight}", startBlockHeight, endBlockHeight);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventSubscription> UpdateAsync(EventSubscription subscription)
        {
            _logger.LogInformation("Updating event subscription: {Id}", subscription.Id);

            try
            {
                subscription.UpdatedAt = DateTime.UtcNow;
                return await _databaseService.UpdateAsync<EventSubscription, Guid>(CollectionName, subscription.Id, subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event subscription: {Id}", subscription.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting event subscription: {Id}", id);

            try
            {
                // Soft delete - update status to Deleted
                var subscription = await GetByIdAsync(id);
                if (subscription == null)
                {
                    return false;
                }

                subscription.Status = EventSubscriptionStatus.Deleted;
                subscription.UpdatedAt = DateTime.UtcNow;
                await _databaseService.UpdateAsync<EventSubscription, Guid>(CollectionName, id, subscription);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event subscription: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountByStatusAsync(EventSubscriptionStatus status)
        {
            _logger.LogInformation("Getting count of event subscriptions with status: {Status}", status);

            try
            {
                return await _databaseService.CountAsync<EventSubscription>(
                    CollectionName,
                    s => s.Status == status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of event subscriptions with status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting count of event subscriptions for account: {AccountId}", accountId);

            try
            {
                return await _databaseService.CountAsync<EventSubscription>(
                    CollectionName,
                    s => s.AccountId == accountId && s.Status != EventSubscriptionStatus.Deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of event subscriptions for account: {AccountId}", accountId);
                throw;
            }
        }
    }
}
