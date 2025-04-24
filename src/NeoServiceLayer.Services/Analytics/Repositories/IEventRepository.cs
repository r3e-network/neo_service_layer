using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Services.Analytics.Repositories
{
    /// <summary>
    /// Interface for event repository
    /// </summary>
    public interface IEventRepository
    {
        /// <summary>
        /// Creates a new event
        /// </summary>
        /// <param name="event">Event to create</param>
        /// <returns>The created event</returns>
        Task<AnalyticsEvent> CreateAsync(AnalyticsEvent @event);

        /// <summary>
        /// Gets an event by ID
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>The event if found, null otherwise</returns>
        Task<AnalyticsEvent> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets events by filter
        /// </summary>
        /// <param name="filter">Event filter</param>
        /// <returns>List of events matching the filter</returns>
        Task<IEnumerable<AnalyticsEvent>> GetByFilterAsync(EventFilter filter);

        /// <summary>
        /// Gets event count by filter
        /// </summary>
        /// <param name="filter">Event filter</param>
        /// <returns>Count of events matching the filter</returns>
        Task<int> GetCountByFilterAsync(EventFilter filter);

        /// <summary>
        /// Gets events by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="limit">Maximum number of events to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of events for the account</returns>
        Task<IEnumerable<AnalyticsEvent>> GetByAccountAsync(Guid accountId, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets events by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="limit">Maximum number of events to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of events for the user</returns>
        Task<IEnumerable<AnalyticsEvent>> GetByUserAsync(Guid userId, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets events by name
        /// </summary>
        /// <param name="name">Event name</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="limit">Maximum number of events to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of events with the specified name</returns>
        Task<IEnumerable<AnalyticsEvent>> GetByNameAsync(string name, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets events by category
        /// </summary>
        /// <param name="category">Event category</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="limit">Maximum number of events to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of events in the category</returns>
        Task<IEnumerable<AnalyticsEvent>> GetByCategoryAsync(string category, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets events by source
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="limit">Maximum number of events to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of events from the source</returns>
        Task<IEnumerable<AnalyticsEvent>> GetBySourceAsync(string source, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets the count of events in the last 24 hours
        /// </summary>
        /// <returns>Count of events in the last 24 hours</returns>
        Task<int> GetCountLast24HoursAsync();

        /// <summary>
        /// Gets the count of events
        /// </summary>
        /// <returns>Count of events</returns>
        Task<int> GetCountAsync();

        /// <summary>
        /// Gets the storage usage in bytes
        /// </summary>
        /// <returns>Storage usage in bytes</returns>
        Task<long> GetStorageUsageAsync();

        /// <summary>
        /// Deletes events older than the specified date
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>Number of events deleted</returns>
        Task<int> DeleteOlderThanAsync(DateTime date);
    }
}
