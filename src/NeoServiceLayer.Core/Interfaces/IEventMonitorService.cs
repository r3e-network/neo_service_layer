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
    public interface IEventMonitorService
    {
        /// <summary>
        /// Starts monitoring for events
        /// </summary>
        /// <returns>True if monitoring was started successfully, false otherwise</returns>
        Task<bool> StartMonitoringAsync();

        /// <summary>
        /// Stops monitoring for events
        /// </summary>
        /// <returns>True if monitoring was stopped successfully, false otherwise</returns>
        Task<bool> StopMonitoringAsync();

        /// <summary>
        /// Adds a blockchain event to monitor
        /// </summary>
        /// <param name="contractHash">Contract hash to monitor</param>
        /// <param name="eventName">Event name to monitor</param>
        /// <param name="functionIds">List of function IDs to trigger when the event occurs</param>
        /// <returns>The created event monitor configuration</returns>
        Task<Event> AddBlockchainEventMonitorAsync(string contractHash, string eventName, List<Guid> functionIds);

        /// <summary>
        /// Adds a time-based event to monitor
        /// </summary>
        /// <param name="name">Name for the event</param>
        /// <param name="cronExpression">Cron expression for the event schedule</param>
        /// <param name="functionIds">List of function IDs to trigger when the event occurs</param>
        /// <returns>The created event monitor configuration</returns>
        Task<Event> AddTimeEventMonitorAsync(string name, string cronExpression, List<Guid> functionIds);

        /// <summary>
        /// Adds a custom event to monitor
        /// </summary>
        /// <param name="name">Name for the event</param>
        /// <param name="source">Source of the event</param>
        /// <param name="functionIds">List of function IDs to trigger when the event occurs</param>
        /// <returns>The created event monitor configuration</returns>
        Task<Event> AddCustomEventMonitorAsync(string name, string source, List<Guid> functionIds);

        /// <summary>
        /// Removes an event monitor
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>True if the event monitor was removed successfully, false otherwise</returns>
        Task<bool> RemoveEventMonitorAsync(Guid id);

        /// <summary>
        /// Gets all event monitors
        /// </summary>
        /// <returns>List of all event monitors</returns>
        Task<IEnumerable<Event>> GetAllEventMonitorsAsync();

        /// <summary>
        /// Gets event monitors by type
        /// </summary>
        /// <param name="type">Event type</param>
        /// <returns>List of event monitors of the specified type</returns>
        Task<IEnumerable<Event>> GetEventMonitorsByTypeAsync(EventType type);

        /// <summary>
        /// Gets event monitors for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of event monitors that trigger the function</returns>
        Task<IEnumerable<Event>> GetEventMonitorsByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Triggers a custom event
        /// </summary>
        /// <param name="name">Event name</param>
        /// <param name="source">Event source</param>
        /// <param name="data">Event data</param>
        /// <returns>The triggered event</returns>
        Task<Event> TriggerCustomEventAsync(string name, string source, Dictionary<string, object> data);

        /// <summary>
        /// Gets event history
        /// </summary>
        /// <param name="startTime">Start time for the history</param>
        /// <param name="endTime">End time for the history</param>
        /// <param name="type">Optional event type filter</param>
        /// <param name="status">Optional event status filter</param>
        /// <returns>List of events in the specified time range</returns>
        Task<IEnumerable<Event>> GetEventHistoryAsync(DateTime startTime, DateTime endTime, EventType? type = null, EventStatus? status = null);

        /// <summary>
        /// Gets event history for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="startTime">Start time for the history</param>
        /// <param name="endTime">End time for the history</param>
        /// <returns>List of events that triggered the function in the specified time range</returns>
        Task<IEnumerable<Event>> GetEventHistoryForFunctionAsync(Guid functionId, DateTime startTime, DateTime endTime);
    }
}
