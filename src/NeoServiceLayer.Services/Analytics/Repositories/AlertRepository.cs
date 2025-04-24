using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Services.Analytics.Repositories
{
    /// <summary>
    /// Implementation of the alert repository
    /// </summary>
    public class AlertRepository : IAlertRepository
    {
        private readonly ILogger<AlertRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string AlertsCollectionName = "analytics_alerts";
        private const string AlertEventsCollectionName = "analytics_alert_events";

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public AlertRepository(ILogger<AlertRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<Alert> CreateAsync(Alert alert)
        {
            _logger.LogInformation("Creating alert: {Name} for account: {AccountId}", alert.Name, alert.AccountId);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    await _databaseService.CreateCollectionAsync(AlertsCollectionName);
                }

                // Set default values
                if (alert.Id == Guid.Empty)
                {
                    alert.Id = Guid.NewGuid();
                }

                alert.CreatedAt = DateTime.UtcNow;
                alert.UpdatedAt = DateTime.UtcNow;
                alert.Status = AlertStatus.OK;

                // Create alert
                return await _databaseService.CreateAsync(AlertsCollectionName, alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert: {Name} for account: {AccountId}", alert.Name, alert.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Alert> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting alert: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    return null;
                }

                // Get alert
                return await _databaseService.GetByIdAsync<Alert, Guid>(AlertsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Alert>> GetByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting alerts for account: {AccountId}", accountId);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    return Enumerable.Empty<Alert>();
                }

                // Get alerts
                return await _databaseService.GetByFilterAsync<Alert>(
                    AlertsCollectionName,
                    a => a.AccountId == accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Alert>> GetByUserAsync(Guid userId)
        {
            _logger.LogInformation("Getting alerts for user: {UserId}", userId);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    return Enumerable.Empty<Alert>();
                }

                // Get alerts
                return await _databaseService.GetByFilterAsync<Alert>(
                    AlertsCollectionName,
                    a => a.CreatedBy == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for user: {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Alert>> GetByStatusAsync(AlertStatus status)
        {
            _logger.LogInformation("Getting alerts with status: {Status}", status);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    return Enumerable.Empty<Alert>();
                }

                // Get alerts
                return await _databaseService.GetByFilterAsync<Alert>(
                    AlertsCollectionName,
                    a => a.Status == status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts with status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Alert>> GetByMetricAsync(Guid metricId)
        {
            _logger.LogInformation("Getting alerts for metric: {MetricId}", metricId);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    return Enumerable.Empty<Alert>();
                }

                // Get alerts
                return await _databaseService.GetByFilterAsync<Alert>(
                    AlertsCollectionName,
                    a => a.MetricId == metricId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts for metric: {MetricId}", metricId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Alert>> GetDueForEvaluationAsync()
        {
            _logger.LogInformation("Getting alerts due for evaluation");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    return Enumerable.Empty<Alert>();
                }

                // Get alerts
                var now = DateTime.UtcNow;
                return await _databaseService.GetByFilterAsync<Alert>(
                    AlertsCollectionName,
                    a => a.IsEnabled &&
                         (!a.LastEvaluationAt.HasValue ||
                          (now - a.LastEvaluationAt.Value).TotalSeconds >= a.EvaluationFrequencySeconds) &&
                         (!a.SilencedUntil.HasValue || a.SilencedUntil.Value <= now));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts due for evaluation");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Alert> UpdateAsync(Alert alert)
        {
            _logger.LogInformation("Updating alert: {Id}", alert.Id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    throw new InvalidOperationException("Alerts collection does not exist");
                }

                // Update timestamp
                alert.UpdatedAt = DateTime.UtcNow;

                // Update alert
                return await _databaseService.UpdateAsync<Alert, Guid>(AlertsCollectionName, alert.Id, alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert: {Id}", alert.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting alert: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    return false;
                }

                // Delete alert
                return await _databaseService.DeleteAsync<Alert, Guid>(AlertsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting alert: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<AlertEvent> CreateEventAsync(AlertEvent @event)
        {
            _logger.LogInformation("Creating alert event for alert: {AlertId}, type: {Type}", @event.AlertId, @event.Type);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(AlertEventsCollectionName))
                {
                    await _databaseService.CreateCollectionAsync(AlertEventsCollectionName);
                }

                // Set default values
                if (@event.Id == Guid.Empty)
                {
                    @event.Id = Guid.NewGuid();
                }

                if (@event.Timestamp == default)
                {
                    @event.Timestamp = DateTime.UtcNow;
                }

                // Create event
                return await _databaseService.CreateAsync(AlertEventsCollectionName, @event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert event for alert: {AlertId}, type: {Type}", @event.AlertId, @event.Type);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<AlertEvent> GetEventByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting alert event: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertEventsCollectionName))
                {
                    return null;
                }

                // Get event
                return await _databaseService.GetByIdAsync<AlertEvent, Guid>(AlertEventsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert event: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AlertEvent>> GetEventsByAlertAsync(Guid alertId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting events for alert: {AlertId}, start: {StartTime}, end: {EndTime}", alertId, startTime, endTime);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertEventsCollectionName))
                {
                    return Enumerable.Empty<AlertEvent>();
                }

                // Get events
                return await _databaseService.GetByFilterAsync<AlertEvent>(
                    AlertEventsCollectionName,
                    e => e.AlertId == alertId && e.Timestamp >= startTime && e.Timestamp <= endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for alert: {AlertId}", alertId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AlertEvent>> GetEventsByTypeAsync(AlertEventType type, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting alert events by type: {Type}, start: {StartTime}, end: {EndTime}", type, startTime, endTime);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertEventsCollectionName))
                {
                    return Enumerable.Empty<AlertEvent>();
                }

                // Get events
                return await _databaseService.GetByFilterAsync<AlertEvent>(
                    AlertEventsCollectionName,
                    e => e.Type == type && e.Timestamp >= startTime && e.Timestamp <= endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert events by type: {Type}", type);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AlertEvent>> GetEventsBySeverityAsync(AlertSeverity severity, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting alert events by severity: {Severity}, start: {StartTime}, end: {EndTime}", severity, startTime, endTime);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertEventsCollectionName))
                {
                    return Enumerable.Empty<AlertEvent>();
                }

                // Get events
                return await _databaseService.GetByFilterAsync<AlertEvent>(
                    AlertEventsCollectionName,
                    e => e.Severity == severity && e.Timestamp >= startTime && e.Timestamp <= endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert events by severity: {Severity}", severity);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync()
        {
            _logger.LogInformation("Getting alert count");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertsCollectionName))
                {
                    return 0;
                }

                // Get count
                var alerts = await _databaseService.GetAllAsync<Alert>(AlertsCollectionName);
                return alerts.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert count");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetEventsCountLast24HoursAsync()
        {
            _logger.LogInformation("Getting alert events count for last 24 hours");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(AlertEventsCollectionName))
                {
                    return 0;
                }

                // Get count
                var startTime = DateTime.UtcNow.AddHours(-24);
                var events = await _databaseService.GetByFilterAsync<AlertEvent>(
                    AlertEventsCollectionName,
                    e => e.Timestamp >= startTime);

                return events.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert events count for last 24 hours");
                throw;
            }
        }
    }
}
