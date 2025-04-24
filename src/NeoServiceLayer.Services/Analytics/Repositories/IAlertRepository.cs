using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Services.Analytics.Repositories
{
    /// <summary>
    /// Interface for alert repository
    /// </summary>
    public interface IAlertRepository
    {
        /// <summary>
        /// Creates a new alert
        /// </summary>
        /// <param name="alert">Alert to create</param>
        /// <returns>The created alert</returns>
        Task<Alert> CreateAsync(Alert alert);

        /// <summary>
        /// Gets an alert by ID
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>The alert if found, null otherwise</returns>
        Task<Alert> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets alerts by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of alerts for the account</returns>
        Task<IEnumerable<Alert>> GetByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets alerts by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of alerts created by the user</returns>
        Task<IEnumerable<Alert>> GetByUserAsync(Guid userId);

        /// <summary>
        /// Gets alerts by status
        /// </summary>
        /// <param name="status">Alert status</param>
        /// <returns>List of alerts with the specified status</returns>
        Task<IEnumerable<Alert>> GetByStatusAsync(AlertStatus status);

        /// <summary>
        /// Gets alerts by metric ID
        /// </summary>
        /// <param name="metricId">Metric ID</param>
        /// <returns>List of alerts for the metric</returns>
        Task<IEnumerable<Alert>> GetByMetricAsync(Guid metricId);

        /// <summary>
        /// Gets alerts due for evaluation
        /// </summary>
        /// <returns>List of alerts due for evaluation</returns>
        Task<IEnumerable<Alert>> GetDueForEvaluationAsync();

        /// <summary>
        /// Updates an alert
        /// </summary>
        /// <param name="alert">Alert to update</param>
        /// <returns>The updated alert</returns>
        Task<Alert> UpdateAsync(Alert alert);

        /// <summary>
        /// Deletes an alert
        /// </summary>
        /// <param name="id">Alert ID</param>
        /// <returns>True if the alert was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Creates an alert event
        /// </summary>
        /// <param name="event">Event to create</param>
        /// <returns>The created event</returns>
        Task<AlertEvent> CreateEventAsync(AlertEvent @event);

        /// <summary>
        /// Gets an alert event by ID
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>The event if found, null otherwise</returns>
        Task<AlertEvent> GetEventByIdAsync(Guid id);

        /// <summary>
        /// Gets alert events by alert ID
        /// </summary>
        /// <param name="alertId">Alert ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>List of events for the alert</returns>
        Task<IEnumerable<AlertEvent>> GetEventsByAlertAsync(Guid alertId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets alert events by type
        /// </summary>
        /// <param name="type">Event type</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>List of events with the specified type</returns>
        Task<IEnumerable<AlertEvent>> GetEventsByTypeAsync(AlertEventType type, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets alert events by severity
        /// </summary>
        /// <param name="severity">Event severity</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>List of events with the specified severity</returns>
        Task<IEnumerable<AlertEvent>> GetEventsBySeverityAsync(AlertSeverity severity, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets the count of alerts
        /// </summary>
        /// <returns>Count of alerts</returns>
        Task<int> GetCountAsync();

        /// <summary>
        /// Gets the count of alert events in the last 24 hours
        /// </summary>
        /// <returns>Count of alert events in the last 24 hours</returns>
        Task<int> GetEventsCountLast24HoursAsync();
    }
}
