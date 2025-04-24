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
    /// Implementation of the event repository
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly ILogger<EventRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string EventsCollectionName = "analytics_events";

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public EventRepository(ILogger<EventRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<AnalyticsEvent> CreateAsync(AnalyticsEvent @event)
        {
            _logger.LogInformation("Creating event: {Name}, category: {Category}", @event.Name, @event.Category);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    await _databaseService.CreateCollectionAsync(EventsCollectionName);
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
                return await _databaseService.CreateAsync(EventsCollectionName, @event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event: {Name}, category: {Category}", @event.Name, @event.Category);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<AnalyticsEvent> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting event: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return null;
                }

                // Get event
                return await _databaseService.GetByIdAsync<AnalyticsEvent, Guid>(EventsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AnalyticsEvent>> GetByFilterAsync(EventFilter filter)
        {
            _logger.LogInformation("Getting events by filter");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return Enumerable.Empty<AnalyticsEvent>();
                }

                // Build filter
                Func<AnalyticsEvent, bool> predicate = e => true;

                if (filter.EventNames != null && filter.EventNames.Any())
                {
                    predicate = predicate.And(e => filter.EventNames.Contains(e.Name));
                }

                if (filter.Categories != null && filter.Categories.Any())
                {
                    predicate = predicate.And(e => filter.Categories.Contains(e.Category));
                }

                if (filter.Sources != null && filter.Sources.Any())
                {
                    predicate = predicate.And(e => filter.Sources.Contains(e.Source));
                }

                if (filter.AccountIds != null && filter.AccountIds.Any())
                {
                    predicate = predicate.And(e => e.AccountId.HasValue && filter.AccountIds.Contains(e.AccountId.Value));
                }

                if (filter.UserIds != null && filter.UserIds.Any())
                {
                    predicate = predicate.And(e => e.UserId.HasValue && filter.UserIds.Contains(e.UserId.Value));
                }

                if (filter.StartTime.HasValue)
                {
                    predicate = predicate.And(e => e.Timestamp >= filter.StartTime.Value);
                }

                if (filter.EndTime.HasValue)
                {
                    predicate = predicate.And(e => e.Timestamp <= filter.EndTime.Value);
                }

                if (filter.PropertyFilters != null && filter.PropertyFilters.Any())
                {
                    foreach (var property in filter.PropertyFilters)
                    {
                        predicate = predicate.And(e => e.Properties.ContainsKey(property.Key) && e.Properties[property.Key].Equals(property.Value));
                    }
                }

                if (filter.TagFilters != null && filter.TagFilters.Any())
                {
                    foreach (var tag in filter.TagFilters)
                    {
                        predicate = predicate.And(e => e.Tags.ContainsKey(tag.Key) && e.Tags[tag.Key] == tag.Value);
                    }
                }

                if (!string.IsNullOrEmpty(filter.IpAddressFilter))
                {
                    predicate = predicate.And(e => e.IpAddress == filter.IpAddressFilter);
                }

                if (!string.IsNullOrEmpty(filter.CountryFilter))
                {
                    predicate = predicate.And(e => e.Location != null && e.Location.Country == filter.CountryFilter);
                }

                // Get events
                var events = await _databaseService.GetByFilterAsync<AnalyticsEvent>(EventsCollectionName, predicate);

                // Apply sorting
                if (!string.IsNullOrEmpty(filter.SortField))
                {
                    events = filter.SortDirection == SortDirection.Ascending
                        ? events.OrderBy(e => GetPropertyValue(e, filter.SortField))
                        : events.OrderByDescending(e => GetPropertyValue(e, filter.SortField));
                }

                // Apply pagination
                events = events.Skip(filter.Offset).Take(filter.Limit);

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events by filter");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountByFilterAsync(EventFilter filter)
        {
            _logger.LogInformation("Getting event count by filter");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return 0;
                }

                // Build filter
                Func<AnalyticsEvent, bool> predicate = e => true;

                if (filter.EventNames != null && filter.EventNames.Any())
                {
                    predicate = predicate.And(e => filter.EventNames.Contains(e.Name));
                }

                if (filter.Categories != null && filter.Categories.Any())
                {
                    predicate = predicate.And(e => filter.Categories.Contains(e.Category));
                }

                if (filter.Sources != null && filter.Sources.Any())
                {
                    predicate = predicate.And(e => filter.Sources.Contains(e.Source));
                }

                if (filter.AccountIds != null && filter.AccountIds.Any())
                {
                    predicate = predicate.And(e => e.AccountId.HasValue && filter.AccountIds.Contains(e.AccountId.Value));
                }

                if (filter.UserIds != null && filter.UserIds.Any())
                {
                    predicate = predicate.And(e => e.UserId.HasValue && filter.UserIds.Contains(e.UserId.Value));
                }

                if (filter.StartTime.HasValue)
                {
                    predicate = predicate.And(e => e.Timestamp >= filter.StartTime.Value);
                }

                if (filter.EndTime.HasValue)
                {
                    predicate = predicate.And(e => e.Timestamp <= filter.EndTime.Value);
                }

                if (filter.PropertyFilters != null && filter.PropertyFilters.Any())
                {
                    foreach (var property in filter.PropertyFilters)
                    {
                        predicate = predicate.And(e => e.Properties.ContainsKey(property.Key) && e.Properties[property.Key].Equals(property.Value));
                    }
                }

                if (filter.TagFilters != null && filter.TagFilters.Any())
                {
                    foreach (var tag in filter.TagFilters)
                    {
                        predicate = predicate.And(e => e.Tags.ContainsKey(tag.Key) && e.Tags[tag.Key] == tag.Value);
                    }
                }

                if (!string.IsNullOrEmpty(filter.IpAddressFilter))
                {
                    predicate = predicate.And(e => e.IpAddress == filter.IpAddressFilter);
                }

                if (!string.IsNullOrEmpty(filter.CountryFilter))
                {
                    predicate = predicate.And(e => e.Location != null && e.Location.Country == filter.CountryFilter);
                }

                // Get events
                var events = await _databaseService.GetByFilterAsync<AnalyticsEvent>(EventsCollectionName, predicate);
                return events.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event count by filter");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AnalyticsEvent>> GetByAccountAsync(Guid accountId, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting events for account: {AccountId}, start: {StartTime}, end: {EndTime}", accountId, startTime, endTime);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return Enumerable.Empty<AnalyticsEvent>();
                }

                // Get events
                var events = await _databaseService.GetByFilterAsync<AnalyticsEvent>(
                    EventsCollectionName,
                    e => e.AccountId == accountId && e.Timestamp >= startTime && e.Timestamp <= endTime);

                // Apply pagination
                events = events.OrderByDescending(e => e.Timestamp).Skip(offset).Take(limit);

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AnalyticsEvent>> GetByUserAsync(Guid userId, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting events for user: {UserId}, start: {StartTime}, end: {EndTime}", userId, startTime, endTime);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return Enumerable.Empty<AnalyticsEvent>();
                }

                // Get events
                var events = await _databaseService.GetByFilterAsync<AnalyticsEvent>(
                    EventsCollectionName,
                    e => e.UserId == userId && e.Timestamp >= startTime && e.Timestamp <= endTime);

                // Apply pagination
                events = events.OrderByDescending(e => e.Timestamp).Skip(offset).Take(limit);

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for user: {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AnalyticsEvent>> GetByNameAsync(string name, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting events by name: {Name}, start: {StartTime}, end: {EndTime}", name, startTime, endTime);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return Enumerable.Empty<AnalyticsEvent>();
                }

                // Get events
                var events = await _databaseService.GetByFilterAsync<AnalyticsEvent>(
                    EventsCollectionName,
                    e => e.Name == name && e.Timestamp >= startTime && e.Timestamp <= endTime);

                // Apply pagination
                events = events.OrderByDescending(e => e.Timestamp).Skip(offset).Take(limit);

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events by name: {Name}", name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AnalyticsEvent>> GetByCategoryAsync(string category, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting events by category: {Category}, start: {StartTime}, end: {EndTime}", category, startTime, endTime);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return Enumerable.Empty<AnalyticsEvent>();
                }

                // Get events
                var events = await _databaseService.GetByFilterAsync<AnalyticsEvent>(
                    EventsCollectionName,
                    e => e.Category == category && e.Timestamp >= startTime && e.Timestamp <= endTime);

                // Apply pagination
                events = events.OrderByDescending(e => e.Timestamp).Skip(offset).Take(limit);

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events by category: {Category}", category);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AnalyticsEvent>> GetBySourceAsync(string source, DateTime startTime, DateTime endTime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting events by source: {Source}, start: {StartTime}, end: {EndTime}", source, startTime, endTime);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return Enumerable.Empty<AnalyticsEvent>();
                }

                // Get events
                var events = await _databaseService.GetByFilterAsync<AnalyticsEvent>(
                    EventsCollectionName,
                    e => e.Source == source && e.Timestamp >= startTime && e.Timestamp <= endTime);

                // Apply pagination
                events = events.OrderByDescending(e => e.Timestamp).Skip(offset).Take(limit);

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events by source: {Source}", source);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountLast24HoursAsync()
        {
            _logger.LogInformation("Getting event count for last 24 hours");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return 0;
                }

                // Get count
                var startTime = DateTime.UtcNow.AddHours(-24);
                var events = await _databaseService.GetByFilterAsync<AnalyticsEvent>(
                    EventsCollectionName,
                    e => e.Timestamp >= startTime);

                return events.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event count for last 24 hours");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync()
        {
            _logger.LogInformation("Getting event count");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return 0;
                }

                // Get count
                var events = await _databaseService.GetAllAsync<AnalyticsEvent>(EventsCollectionName);
                return events.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event count");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<long> GetStorageUsageAsync()
        {
            _logger.LogInformation("Getting storage usage");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return 0;
                }

                // Get size
                return await _databaseService.GetCollectionSizeAsync(EventsCollectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting storage usage");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> DeleteOlderThanAsync(DateTime date)
        {
            _logger.LogInformation("Deleting events older than: {Date}", date);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(EventsCollectionName))
                {
                    return 0;
                }

                // Get events to delete
                var events = await _databaseService.GetByFilterAsync<AnalyticsEvent>(
                    EventsCollectionName,
                    e => e.Timestamp < date);

                // Delete events
                int count = 0;
                foreach (var @event in events)
                {
                    if (await _databaseService.DeleteAsync<AnalyticsEvent, Guid>(EventsCollectionName, @event.Id))
                    {
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting events older than: {Date}", date);
                throw;
            }
        }

        /// <summary>
        /// Gets a property value from an event
        /// </summary>
        /// <param name="event">Event</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value</returns>
        private object GetPropertyValue(AnalyticsEvent @event, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            // Check if it's a property in the event object
            var property = typeof(AnalyticsEvent).GetProperty(propertyName);
            if (property != null)
            {
                return property.GetValue(@event);
            }

            // Check if it's a property in the Properties dictionary
            if (@event.Properties.TryGetValue(propertyName, out var value))
            {
                return value;
            }

            // Check if it's a tag
            if (@event.Tags.TryGetValue(propertyName, out var tag))
            {
                return tag;
            }

            return null;
        }
    }

    /// <summary>
    /// Extension methods for predicates
    /// </summary>
    internal static class PredicateExtensions
    {
        /// <summary>
        /// Combines two predicates with AND
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="first">First predicate</param>
        /// <param name="second">Second predicate</param>
        /// <returns>Combined predicate</returns>
        public static Func<T, bool> And<T>(this Func<T, bool> first, Func<T, bool> second)
        {
            return x => first(x) && second(x);
        }

        /// <summary>
        /// Combines two predicates with OR
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="first">First predicate</param>
        /// <param name="second">Second predicate</param>
        /// <returns>Combined predicate</returns>
        public static Func<T, bool> Or<T>(this Func<T, bool> first, Func<T, bool> second)
        {
            return x => first(x) || second(x);
        }
    }
}
