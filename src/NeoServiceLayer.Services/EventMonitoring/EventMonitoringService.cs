using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Utilities;
using NeoServiceLayer.Core.Extensions;
using NeoServiceLayer.Services.EventMonitoring.Repositories;

namespace NeoServiceLayer.Services.EventMonitoring
{
    /// <summary>
    /// Implementation of the event monitoring service
    /// </summary>
    public class EventMonitoringService : IEventMonitoringService, IDisposable
    {
        private readonly ILogger<EventMonitoringService> _logger;
        private readonly IEventSubscriptionRepository _subscriptionRepository;
        private readonly IEventLogRepository _eventLogRepository;
        private readonly IFunctionService _functionService;
        private readonly IEnclaveService _enclaveService;
        private readonly EventMonitoringConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private Timer _monitoringTimer;
        private Timer _notificationTimer;
        private bool _isMonitoring;
        private long _lastProcessedBlockHeight;
        private DateTime? _monitoringStartTime;
        private readonly SemaphoreSlim _monitoringSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _notificationSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="EventMonitoringService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="subscriptionRepository">Subscription repository</param>
        /// <param name="eventLogRepository">Event log repository</param>
        /// <param name="functionService">Function service</param>
        /// <param name="enclaveService">Enclave service</param>
        /// <param name="configuration">Configuration</param>
        public EventMonitoringService(
            ILogger<EventMonitoringService> logger,
            IEventSubscriptionRepository subscriptionRepository,
            IEventLogRepository eventLogRepository,
            IFunctionService functionService,
            IEnclaveService enclaveService,
            IOptions<EventMonitoringConfiguration> configuration)
        {
            _logger = logger;
            _subscriptionRepository = subscriptionRepository;
            _eventLogRepository = eventLogRepository;
            _functionService = functionService;
            _enclaveService = enclaveService;
            _configuration = configuration.Value;
            _httpClient = new HttpClient();
        }

        /// <inheritdoc/>
        public async Task<bool> StartMonitoringAsync()
        {
            _logger.LogInformation("Starting event monitoring");

            try
            {
                await _monitoringSemaphore.WaitAsync();
                try
                {
                    if (_isMonitoring)
                    {
                        _logger.LogWarning("Event monitoring is already running");
                        return true;
                    }

                    // Initialize last processed block height
                    _lastProcessedBlockHeight = _configuration.StartBlockHeight;
                    _monitoringStartTime = DateTime.UtcNow;
                    _isMonitoring = true;

                    // Start monitoring timer
                    _monitoringTimer = new Timer(
                        async _ => await MonitorBlocksAsync(),
                        null,
                        TimeSpan.Zero,
                        TimeSpan.FromSeconds(_configuration.MonitoringIntervalSeconds));

                    // Start notification timer
                    _notificationTimer = new Timer(
                        async _ => await ProcessNotificationsAsync(),
                        null,
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(_configuration.NotificationIntervalSeconds));

                    _logger.LogInformation("Event monitoring started");
                    return true;
                }
                finally
                {
                    _monitoringSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting event monitoring");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> StopMonitoringAsync()
        {
            _logger.LogInformation("Stopping event monitoring");

            try
            {
                await _monitoringSemaphore.WaitAsync();
                try
                {
                    if (!_isMonitoring)
                    {
                        _logger.LogWarning("Event monitoring is not running");
                        return true;
                    }

                    // Stop timers
                    _monitoringTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    _notificationTimer?.Change(Timeout.Infinite, Timeout.Infinite);

                    _isMonitoring = false;
                    _monitoringStartTime = null;

                    _logger.LogInformation("Event monitoring stopped");
                    return true;
                }
                finally
                {
                    _monitoringSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping event monitoring");
                return false;
            }
        }

        /// <inheritdoc/>
        public Task<bool> GetMonitoringStatusAsync()
        {
            return Task.FromResult(_isMonitoring);
        }

        /// <inheritdoc/>
        public async Task<long> GetCurrentBlockHeightAsync()
        {
            try
            {
                // In a real implementation, this would query the Neo node
                // For now, we'll simulate it
                return await Task.FromResult(_lastProcessedBlockHeight + 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current block height");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventSubscription> CreateSubscriptionAsync(EventSubscription subscription)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Name"] = subscription.Name,
                ["AccountId"] = subscription.AccountId,
                ["ContractHash"] = subscription.ContractHash,
                ["EventName"] = subscription.EventName
            };

            LoggingUtility.LogOperationStart(_logger, "CreateEventSubscription", requestId, additionalData);

            try
            {
                // Validate subscription
                ValidationUtility.ValidateNotNull(subscription, nameof(subscription));
                ValidationUtility.ValidateNotNullOrEmpty(subscription.ContractHash, "Contract hash");
                ValidationUtility.ValidateNotNullOrEmpty(subscription.EventName, "Event name");

                if (string.IsNullOrEmpty(subscription.CallbackUrl) && !subscription.FunctionId.HasValue)
                {
                    throw new ArgumentException("Either callback URL or function ID is required");
                }

                if (!string.IsNullOrEmpty(subscription.CallbackUrl) && !subscription.CallbackUrl.IsValidUrl())
                {
                    throw new ArgumentException("Invalid callback URL format");
                }

                if (subscription.FunctionId.HasValue)
                {
                    ValidationUtility.ValidateGuid(subscription.FunctionId.Value, "Function ID");
                }

                // Set default values
                if (subscription.StartBlockHeight <= 0)
                {
                    subscription.StartBlockHeight = await GetCurrentBlockHeightAsync();
                }

                // Create subscription
                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<EventMonitoringService, EventSubscription>(
                    _logger,
                    async () => await _subscriptionRepository.CreateAsync(subscription),
                    "CreateEventSubscription",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new InvalidOperationException("Failed to create event subscription");
                }

                LoggingUtility.LogOperationSuccess(_logger, "CreateEventSubscription", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "CreateEventSubscription", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventSubscription> GetSubscriptionAsync(Guid id)
        {
            _logger.LogInformation("Getting event subscription: {Id}", id);

            try
            {
                return await _subscriptionRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event subscription: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventSubscription>> GetSubscriptionsByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting event subscriptions for account: {AccountId}", accountId);

            try
            {
                return await _subscriptionRepository.GetByAccountAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event subscriptions for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventSubscription>> GetSubscriptionsByContractAsync(string contractHash)
        {
            _logger.LogInformation("Getting event subscriptions for contract: {ContractHash}", contractHash);

            try
            {
                return await _subscriptionRepository.GetByContractAsync(contractHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event subscriptions for contract: {ContractHash}", contractHash);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventSubscription>> GetSubscriptionsByEventAsync(string eventName)
        {
            _logger.LogInformation("Getting event subscriptions for event: {EventName}", eventName);

            try
            {
                return await _subscriptionRepository.GetByEventAsync(eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event subscriptions for event: {EventName}", eventName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventSubscription>> GetActiveSubscriptionsAsync()
        {
            _logger.LogInformation("Getting active event subscriptions");

            try
            {
                return await _subscriptionRepository.GetActiveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active event subscriptions");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventSubscription> UpdateSubscriptionAsync(EventSubscription subscription)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = subscription.Id,
                ["Name"] = subscription.Name,
                ["AccountId"] = subscription.AccountId,
                ["ContractHash"] = subscription.ContractHash,
                ["EventName"] = subscription.EventName
            };

            LoggingUtility.LogOperationStart(_logger, "UpdateEventSubscription", requestId, additionalData);

            try
            {
                // Validate subscription
                ValidationUtility.ValidateNotNull(subscription, nameof(subscription));
                ValidationUtility.ValidateGuid(subscription.Id, "Subscription ID");
                ValidationUtility.ValidateNotNullOrEmpty(subscription.ContractHash, "Contract hash");
                ValidationUtility.ValidateNotNullOrEmpty(subscription.EventName, "Event name");

                if (string.IsNullOrEmpty(subscription.CallbackUrl) && !subscription.FunctionId.HasValue)
                {
                    throw new ArgumentException("Either callback URL or function ID is required");
                }

                if (!string.IsNullOrEmpty(subscription.CallbackUrl) && !subscription.CallbackUrl.IsValidUrl())
                {
                    throw new ArgumentException("Invalid callback URL format");
                }

                if (subscription.FunctionId.HasValue)
                {
                    ValidationUtility.ValidateGuid(subscription.FunctionId.Value, "Function ID");
                }

                // Update subscription
                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<EventMonitoringService, EventSubscription>(
                    _logger,
                    async () => await _subscriptionRepository.UpdateAsync(subscription),
                    "UpdateEventSubscription",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new InvalidOperationException("Failed to update event subscription");
                }

                LoggingUtility.LogOperationSuccess(_logger, "UpdateEventSubscription", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "UpdateEventSubscription", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteSubscriptionAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            LoggingUtility.LogOperationStart(_logger, "DeleteEventSubscription", requestId, additionalData);

            try
            {
                // Validate ID
                ValidationUtility.ValidateGuid(id, "Subscription ID");

                // Delete subscription
                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<EventMonitoringService, bool>(
                    _logger,
                    async () => await _subscriptionRepository.DeleteAsync(id),
                    "DeleteEventSubscription",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new InvalidOperationException("Failed to delete event subscription");
                }

                LoggingUtility.LogOperationSuccess(_logger, "DeleteEventSubscription", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "DeleteEventSubscription", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventSubscription> ActivateSubscriptionAsync(Guid id)
        {
            _logger.LogInformation("Activating event subscription: {Id}", id);

            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(id);
                if (subscription == null)
                {
                    throw new ArgumentException($"Subscription not found: {id}");
                }

                subscription.Status = EventSubscriptionStatus.Active;
                return await _subscriptionRepository.UpdateAsync(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating event subscription: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventSubscription> PauseSubscriptionAsync(Guid id)
        {
            _logger.LogInformation("Pausing event subscription: {Id}", id);

            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(id);
                if (subscription == null)
                {
                    throw new ArgumentException($"Subscription not found: {id}");
                }

                subscription.Status = EventSubscriptionStatus.Paused;
                return await _subscriptionRepository.UpdateAsync(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing event subscription: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetEventLogsBySubscriptionAsync(Guid subscriptionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for subscription: {SubscriptionId}, limit: {Limit}, offset: {Offset}", subscriptionId, limit, offset);

            try
            {
                return await _eventLogRepository.GetBySubscriptionAsync(subscriptionId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for subscription: {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetEventLogsByAccountAsync(Guid accountId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for account: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            try
            {
                return await _eventLogRepository.GetByAccountAsync(accountId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetEventLogsByTransactionAsync(string transactionHash)
        {
            _logger.LogInformation("Getting event logs for transaction: {TransactionHash}", transactionHash);

            try
            {
                return await _eventLogRepository.GetByTransactionAsync(transactionHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for transaction: {TransactionHash}", transactionHash);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetEventLogsByBlockAsync(long blockHeight, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for block: {BlockHeight}, limit: {Limit}, offset: {Offset}", blockHeight, limit, offset);

            try
            {
                return await _eventLogRepository.GetByBlockAsync(blockHeight, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for block: {BlockHeight}", blockHeight);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetEventLogsByContractAsync(string contractHash, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for contract: {ContractHash}, limit: {Limit}, offset: {Offset}", contractHash, limit, offset);

            try
            {
                return await _eventLogRepository.GetByContractAsync(contractHash, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for contract: {ContractHash}", contractHash);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetEventLogsByEventAsync(string eventName, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs for event: {EventName}, limit: {Limit}, offset: {Offset}", eventName, limit, offset);

            try
            {
                return await _eventLogRepository.GetByEventAsync(eventName, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for event: {EventName}", eventName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetEventLogsByNotificationStatusAsync(NotificationStatus status, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting event logs with notification status: {Status}, limit: {Limit}, offset: {Offset}", status, limit, offset);

            try
            {
                return await _eventLogRepository.GetByNotificationStatusAsync(status, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs with notification status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EventLog>> GetEventLogsForRetryAsync(int limit = 100)
        {
            _logger.LogInformation("Getting event logs for retry, limit: {Limit}", limit);

            try
            {
                return await _eventLogRepository.GetForRetryAsync(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event logs for retry");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RetryNotificationAsync(Guid eventLogId)
        {
            _logger.LogInformation("Retrying notification for event log: {EventLogId}", eventLogId);

            try
            {
                var eventLog = await _eventLogRepository.GetByIdAsync(eventLogId);
                if (eventLog == null)
                {
                    throw new ArgumentException($"Event log not found: {eventLogId}");
                }

                if (eventLog.NotificationStatus != NotificationStatus.Failed && eventLog.NotificationStatus != NotificationStatus.Retrying)
                {
                    throw new InvalidOperationException($"Cannot retry notification with status: {eventLog.NotificationStatus}");
                }

                // Reset retry count and status
                eventLog.RetryCount = 0;
                eventLog.NotificationStatus = NotificationStatus.Pending;
                eventLog.NextRetryAt = null;
                await _eventLogRepository.UpdateAsync(eventLog);

                // Process notification
                return await SendNotificationAsync(eventLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying notification for event log: {EventLogId}", eventLogId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CancelNotificationAsync(Guid eventLogId)
        {
            _logger.LogInformation("Cancelling notification for event log: {EventLogId}", eventLogId);

            try
            {
                var eventLog = await _eventLogRepository.GetByIdAsync(eventLogId);
                if (eventLog == null)
                {
                    throw new ArgumentException($"Event log not found: {eventLogId}");
                }

                if (eventLog.NotificationStatus == NotificationStatus.Sent || eventLog.NotificationStatus == NotificationStatus.Cancelled)
                {
                    throw new InvalidOperationException($"Cannot cancel notification with status: {eventLog.NotificationStatus}");
                }

                // Update status
                eventLog.NotificationStatus = NotificationStatus.Cancelled;
                eventLog.NextRetryAt = null;
                await _eventLogRepository.UpdateAsync(eventLog);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling notification for event log: {EventLogId}", eventLogId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<EventMonitoringStatistics> GetStatisticsAsync()
        {
            _logger.LogInformation("Getting event monitoring statistics");

            try
            {
                var activeSubscriptions = await _subscriptionRepository.GetCountByStatusAsync(EventSubscriptionStatus.Active);
                var pendingNotifications = await _eventLogRepository.GetCountByNotificationStatusAsync(NotificationStatus.Pending);
                var failedNotifications = await _eventLogRepository.GetCountByNotificationStatusAsync(NotificationStatus.Failed);
                var retryingNotifications = await _eventLogRepository.GetCountByNotificationStatusAsync(NotificationStatus.Retrying);
                var successfulNotifications = await _eventLogRepository.GetCountByNotificationStatusAsync(NotificationStatus.Sent);
                var eventsLast24Hours = await _eventLogRepository.GetCountLast24HoursAsync();
                var notificationsLast24Hours = await _eventLogRepository.GetSuccessfulNotificationsLast24HoursAsync();

                return new EventMonitoringStatistics
                {
                    CurrentBlockHeight = await GetCurrentBlockHeightAsync(),
                    LastProcessedBlockHeight = _lastProcessedBlockHeight,
                    ActiveSubscriptions = activeSubscriptions,
                    TotalEventsDetected = successfulNotifications + pendingNotifications + failedNotifications + retryingNotifications,
                    TotalNotificationsSent = successfulNotifications,
                    TotalNotificationsFailed = failedNotifications,
                    TotalNotificationsPending = pendingNotifications,
                    TotalNotificationsRetrying = retryingNotifications,
                    EventsDetectedLast24Hours = eventsLast24Hours,
                    NotificationsSentLast24Hours = notificationsLast24Hours,
                    IsMonitoring = _isMonitoring,
                    MonitoringStartTime = _monitoringStartTime,
                    LastUpdateTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event monitoring statistics");
                throw;
            }
        }

        /// <summary>
        /// Monitors blocks for events
        /// </summary>
        private async Task MonitorBlocksAsync()
        {
            if (!_isMonitoring)
            {
                return;
            }

            // Prevent concurrent execution
            if (!await _monitoringSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                // Get current block height
                var currentBlockHeight = await GetCurrentBlockHeightAsync();
                if (currentBlockHeight <= _lastProcessedBlockHeight)
                {
                    return;
                }

                _logger.LogInformation("Processing blocks from {LastProcessedBlockHeight} to {CurrentBlockHeight}",
                    _lastProcessedBlockHeight + 1, currentBlockHeight);

                // Process blocks
                for (var blockHeight = _lastProcessedBlockHeight + 1; blockHeight <= currentBlockHeight; blockHeight++)
                {
                    await ProcessBlockAsync(blockHeight);
                    _lastProcessedBlockHeight = blockHeight;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring blocks");
            }
            finally
            {
                _monitoringSemaphore.Release();
            }
        }

        /// <summary>
        /// Processes a block for events
        /// </summary>
        /// <param name="blockHeight">Block height to process</param>
        private async Task ProcessBlockAsync(long blockHeight)
        {
            try
            {
                // Get active subscriptions for this block
                var subscriptions = await _subscriptionRepository.GetActiveForBlockRangeAsync(blockHeight, blockHeight);
                if (!subscriptions.Any())
                {
                    return;
                }

                _logger.LogInformation("Processing block {BlockHeight} with {SubscriptionCount} active subscriptions",
                    blockHeight, subscriptions.Count());

                // Get block events from Neo node
                var blockEvents = await GetBlockEventsAsync(blockHeight);
                if (!blockEvents.Any())
                {
                    return;
                }

                // Process events
                foreach (var subscription in subscriptions)
                {
                    await ProcessSubscriptionEventsAsync(subscription, blockEvents, blockHeight);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing block {BlockHeight}", blockHeight);
            }
        }

        /// <summary>
        /// Gets events for a block
        /// </summary>
        /// <param name="blockHeight">Block height</param>
        /// <returns>List of events in the block</returns>
        private async Task<List<BlockEvent>> GetBlockEventsAsync(long blockHeight)
        {
            try
            {
                // In a real implementation, this would query the Neo node
                // For now, we'll simulate it with random events
                await Task.Delay(100); // Simulate network delay

                var events = new List<BlockEvent>();
                var random = new Random();

                // Simulate 0-3 events per block
                var eventCount = random.Next(4);
                for (var i = 0; i < eventCount; i++)
                {
                    var contractIndex = random.Next(3);
                    var eventIndex = random.Next(3);

                    var contractHash = $"0x{new string('0', 40 - (contractIndex + 1).ToString().Length)}{contractIndex + 1}";
                    var eventName = $"Event{eventIndex + 1}";

                    var eventData = new Dictionary<string, object>
                    {
                        { "param1", $"value{random.Next(100)}" },
                        { "param2", random.Next(1000) },
                        { "param3", random.NextDouble() * 100 }
                    };

                    events.Add(new BlockEvent
                    {
                        TransactionHash = $"0x{Guid.NewGuid().ToString("N")}",
                        BlockHash = $"0x{Guid.NewGuid().ToString("N")}",
                        BlockHeight = blockHeight,
                        BlockTimestamp = DateTime.UtcNow,
                        ContractHash = contractHash,
                        EventName = eventName,
                        EventData = eventData
                    });
                }

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for block {BlockHeight}", blockHeight);
                return new List<BlockEvent>();
            }
        }

        /// <summary>
        /// Processes events for a subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="blockEvents">Block events</param>
        /// <param name="blockHeight">Block height</param>
        private async Task ProcessSubscriptionEventsAsync(EventSubscription subscription, List<BlockEvent> blockEvents, long blockHeight)
        {
            try
            {
                // Filter events for this subscription
                var matchingEvents = blockEvents.Where(e =>
                    e.ContractHash.Equals(subscription.ContractHash, StringComparison.OrdinalIgnoreCase) &&
                    e.EventName.Equals(subscription.EventName, StringComparison.OrdinalIgnoreCase) &&
                    MatchesFilters(e.EventData, subscription.Filters));

                foreach (var blockEvent in matchingEvents)
                {
                    await ProcessEventAsync(subscription, blockEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing events for subscription {SubscriptionId} at block {BlockHeight}",
                    subscription.Id, blockHeight);
            }
        }

        /// <summary>
        /// Checks if event data matches filters
        /// </summary>
        /// <param name="eventData">Event data</param>
        /// <param name="filters">Filters</param>
        /// <returns>True if the event data matches all filters, false otherwise</returns>
        private bool MatchesFilters(Dictionary<string, object> eventData, List<EventFilter> filters)
        {
            if (filters == null || !filters.Any())
            {
                return true;
            }

            foreach (var filter in filters)
            {
                if (!eventData.TryGetValue(filter.ParameterName, out var paramValue))
                {
                    return false;
                }

                var paramString = paramValue?.ToString();
                var filterValue = filter.Value;

                switch (filter.Operator)
                {
                    case FilterOperator.Equals:
                        if (paramString != filterValue)
                            return false;
                        break;
                    case FilterOperator.NotEquals:
                        if (paramString == filterValue)
                            return false;
                        break;
                    case FilterOperator.GreaterThan:
                        if (!TryCompareNumeric(paramValue, filterValue, out var gtResult) || gtResult <= 0)
                            return false;
                        break;
                    case FilterOperator.GreaterThanOrEquals:
                        if (!TryCompareNumeric(paramValue, filterValue, out var gteResult) || gteResult < 0)
                            return false;
                        break;
                    case FilterOperator.LessThan:
                        if (!TryCompareNumeric(paramValue, filterValue, out var ltResult) || ltResult >= 0)
                            return false;
                        break;
                    case FilterOperator.LessThanOrEquals:
                        if (!TryCompareNumeric(paramValue, filterValue, out var lteResult) || lteResult > 0)
                            return false;
                        break;
                    case FilterOperator.Contains:
                        if (paramString == null || !paramString.Contains(filterValue))
                            return false;
                        break;
                    case FilterOperator.StartsWith:
                        if (paramString == null || !paramString.StartsWith(filterValue))
                            return false;
                        break;
                    case FilterOperator.EndsWith:
                        if (paramString == null || !paramString.EndsWith(filterValue))
                            return false;
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to compare numeric values
        /// </summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <param name="result">Comparison result</param>
        /// <returns>True if comparison was successful, false otherwise</returns>
        private bool TryCompareNumeric(object value1, string value2, out int result)
        {
            result = 0;

            if (value1 == null || value2 == null)
            {
                return false;
            }

            // Try to parse as double
            if (double.TryParse(value1.ToString(), out var double1) && double.TryParse(value2, out var double2))
            {
                result = double1.CompareTo(double2);
                return true;
            }

            // Try to parse as long
            if (long.TryParse(value1.ToString(), out var long1) && long.TryParse(value2, out var long2))
            {
                result = long1.CompareTo(long2);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processes an event for a subscription
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="blockEvent">Block event</param>
        private async Task ProcessEventAsync(EventSubscription subscription, BlockEvent blockEvent)
        {
            try
            {
                _logger.LogInformation("Processing event {EventName} for subscription {SubscriptionId}",
                    blockEvent.EventName, subscription.Id);

                // Check if subscription has reached max trigger count
                if (subscription.MaxTriggerCount > 0 && subscription.TriggerCount >= subscription.MaxTriggerCount)
                {
                    _logger.LogInformation("Subscription {SubscriptionId} has reached max trigger count",
                        subscription.Id);

                    subscription.Status = EventSubscriptionStatus.Completed;
                    await _subscriptionRepository.UpdateAsync(subscription);
                    return;
                }

                // Create event log
                var eventLog = new EventLog
                {
                    SubscriptionId = subscription.Id,
                    AccountId = subscription.AccountId,
                    TransactionHash = blockEvent.TransactionHash,
                    BlockHash = blockEvent.BlockHash,
                    BlockHeight = blockEvent.BlockHeight,
                    BlockTimestamp = blockEvent.BlockTimestamp,
                    ContractHash = blockEvent.ContractHash,
                    EventName = blockEvent.EventName,
                    EventData = blockEvent.EventData,
                    RawEventData = JsonSerializer.Serialize(blockEvent.EventData),
                    NotificationStatus = Core.Enums.NotificationStatus.Pending
                };

                // Save event log
                eventLog = await _eventLogRepository.CreateAsync(eventLog);

                // Update subscription
                subscription.LastTriggeredAt = DateTime.UtcNow;
                subscription.TriggerCount++;
                await _subscriptionRepository.UpdateAsync(subscription);

                // Send notification
                await SendNotificationAsync(eventLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventName} for subscription {SubscriptionId}",
                    blockEvent.EventName, subscription.Id);
            }
        }

        /// <summary>
        /// Processes pending notifications
        /// </summary>
        private async Task ProcessNotificationsAsync()
        {
            if (!_isMonitoring)
            {
                return;
            }

            // Prevent concurrent execution
            if (!await _notificationSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                // Get pending notifications
                var pendingLogs = await _eventLogRepository.GetByNotificationStatusAsync(NotificationStatus.Pending, 100);
                foreach (var eventLog in pendingLogs)
                {
                    await SendNotificationAsync(eventLog);
                }

                // Get notifications for retry
                var retryLogs = await _eventLogRepository.GetForRetryAsync(100);
                foreach (var eventLog in retryLogs)
                {
                    await SendNotificationAsync(eventLog);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notifications");
            }
            finally
            {
                _notificationSemaphore.Release();
            }
        }

        /// <summary>
        /// Sends a notification for an event log
        /// </summary>
        /// <param name="eventLog">Event log</param>
        /// <returns>True if the notification was sent successfully, false otherwise</returns>
        private async Task<bool> SendNotificationAsync(EventLog eventLog)
        {
            try
            {
                _logger.LogInformation("Sending notification for event log {EventLogId}", eventLog.Id);

                // Get subscription
                var subscription = await _subscriptionRepository.GetByIdAsync(eventLog.SubscriptionId);
                if (subscription == null)
                {
                    _logger.LogWarning("Subscription {SubscriptionId} not found for event log {EventLogId}",
                        eventLog.SubscriptionId, eventLog.Id);

                    eventLog.NotificationStatus = Core.Enums.NotificationStatus.Failed;
                    eventLog.ErrorMessage = "Subscription not found";
                    await _eventLogRepository.UpdateAsync(eventLog);
                    return false;
                }

                // Check if subscription is active
                if (subscription.Status != EventSubscriptionStatus.Active)
                {
                    _logger.LogWarning("Subscription {SubscriptionId} is not active for event log {EventLogId}",
                        eventLog.SubscriptionId, eventLog.Id);

                    eventLog.NotificationStatus = Core.Enums.NotificationStatus.Cancelled;
                    eventLog.ErrorMessage = $"Subscription is {subscription.Status}";
                    await _eventLogRepository.UpdateAsync(eventLog);
                    return false;
                }

                // Prepare notification payload
                var payload = PrepareNotificationPayload(subscription, eventLog);

                bool success = false;
                string response = null;

                // Send notification
                if (!string.IsNullOrEmpty(subscription.CallbackUrl))
                {
                    // Send HTTP callback
                    (success, response) = await SendHttpCallbackAsync(subscription, payload);
                }
                else if (subscription.FunctionId.HasValue)
                {
                    // Execute function
                    (success, response) = await ExecuteFunctionAsync(subscription, payload);
                }
                else
                {
                    _logger.LogWarning("No callback URL or function ID specified for subscription {SubscriptionId}",
                        subscription.Id);

                    eventLog.NotificationStatus = Core.Enums.NotificationStatus.Failed;
                    eventLog.ErrorMessage = "No callback URL or function ID specified";
                    await _eventLogRepository.UpdateAsync(eventLog);
                    return false;
                }

                // Update event log
                if (success)
                {
                    eventLog.NotificationStatus = Core.Enums.NotificationStatus.Sent;
                    eventLog.NotifiedAt = DateTime.UtcNow;
                    eventLog.NotificationResponse = response;
                    await _eventLogRepository.UpdateAsync(eventLog);
                    return true;
                }
                else
                {
                    // Handle retry
                    eventLog.RetryCount++;
                    if (eventLog.RetryCount >= subscription.MaxRetryCount)
                    {
                        eventLog.NotificationStatus = Core.Enums.NotificationStatus.Failed;
                        eventLog.ErrorMessage = $"Max retry count reached: {response}";
                        eventLog.NextRetryAt = null;
                    }
                    else
                    {
                        eventLog.NotificationStatus = Core.Enums.NotificationStatus.Retrying;
                        eventLog.ErrorMessage = response;
                        eventLog.NextRetryAt = DateTime.UtcNow.AddSeconds(subscription.RetryIntervalSeconds * Math.Pow(2, eventLog.RetryCount - 1));
                    }

                    await _eventLogRepository.UpdateAsync(eventLog);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for event log {EventLogId}", eventLog.Id);

                eventLog.NotificationStatus = Core.Enums.NotificationStatus.Failed;
                eventLog.ErrorMessage = ex.Message;
                await _eventLogRepository.UpdateAsync(eventLog);
                return false;
            }
        }

        /// <summary>
        /// Prepares the notification payload
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="eventLog">Event log</param>
        /// <returns>Notification payload</returns>
        private string PrepareNotificationPayload(EventSubscription subscription, EventLog eventLog)
        {
            var payload = new
            {
                subscription = new
                {
                    id = subscription.Id,
                    name = subscription.Name,
                    contractHash = subscription.ContractHash,
                    eventName = subscription.EventName
                },
                event_data = subscription.IncludeEventData ? eventLog.EventData : null,
                transaction = new
                {
                    hash = eventLog.TransactionHash,
                    block_hash = eventLog.BlockHash,
                    block_height = eventLog.BlockHeight,
                    block_timestamp = eventLog.BlockTimestamp
                },
                timestamp = DateTime.UtcNow
            };

            return JsonSerializer.Serialize(payload);
        }

        /// <summary>
        /// Sends an HTTP callback
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="payload">Notification payload</param>
        /// <returns>Success status and response</returns>
        private async Task<(bool success, string response)> SendHttpCallbackAsync(EventSubscription subscription, string payload)
        {
            try
            {
                _logger.LogInformation("Sending HTTP callback to {CallbackUrl} for subscription {SubscriptionId}",
                    subscription.CallbackUrl, subscription.Id);

                // Create request
                var request = new HttpRequestMessage(HttpMethod.Post, subscription.CallbackUrl)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                // Add headers
                if (subscription.CallbackHeaders != null)
                {
                    foreach (var header in subscription.CallbackHeaders)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                // Send request
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("HTTP callback successful for subscription {SubscriptionId}",
                        subscription.Id);
                    return (true, responseContent);
                }
                else
                {
                    _logger.LogWarning("HTTP callback failed for subscription {SubscriptionId}: {StatusCode} {Response}",
                        subscription.Id, response.StatusCode, responseContent);
                    return (false, $"HTTP {response.StatusCode}: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending HTTP callback for subscription {SubscriptionId}",
                    subscription.Id);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Executes a function
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <param name="payload">Notification payload</param>
        /// <returns>Success status and response</returns>
        private async Task<(bool success, string response)> ExecuteFunctionAsync(EventSubscription subscription, string payload)
        {
            try
            {
                _logger.LogInformation("Executing function {FunctionId} for subscription {SubscriptionId}",
                    subscription.FunctionId, subscription.Id);

                // In a real implementation, this would execute the function
                // For now, we'll simulate it
                await Task.Delay(100); // Simulate function execution

                // Simulate success or failure
                var random = new Random();
                var success = random.Next(10) < 9; // 90% success rate

                if (success)
                {
                    _logger.LogInformation("Function execution successful for subscription {SubscriptionId}",
                        subscription.Id);
                    return (true, "Function executed successfully");
                }
                else
                {
                    _logger.LogWarning("Function execution failed for subscription {SubscriptionId}",
                        subscription.Id);
                    return (false, "Function execution failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function for subscription {SubscriptionId}",
                    subscription.Id);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Disposes the service
        /// </summary>
        public void Dispose()
        {
            _monitoringTimer?.Dispose();
            _notificationTimer?.Dispose();
            _httpClient?.Dispose();
            _monitoringSemaphore?.Dispose();
            _notificationSemaphore?.Dispose();
        }
    }
}
