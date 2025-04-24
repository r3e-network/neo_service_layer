using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a subscription to blockchain events
    /// </summary>
    public class EventSubscription
    {
        /// <summary>
        /// Gets or sets the unique identifier for the subscription
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns this subscription
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the name of the subscription
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the subscription
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the contract hash to monitor
        /// </summary>
        public string ContractHash { get; set; }

        /// <summary>
        /// Gets or sets the event name to monitor
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the event filters
        /// </summary>
        public List<EventFilter> Filters { get; set; } = new List<EventFilter>();

        /// <summary>
        /// Gets or sets the callback URL to notify when the event is triggered
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets the function ID to execute when the event is triggered
        /// </summary>
        public Guid? FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the status of the subscription
        /// </summary>
        public EventSubscriptionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the start block height for monitoring
        /// </summary>
        public long StartBlockHeight { get; set; }

        /// <summary>
        /// Gets or sets the end block height for monitoring (0 means indefinite)
        /// </summary>
        public long EndBlockHeight { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last triggered timestamp
        /// </summary>
        public DateTime? LastTriggeredAt { get; set; }

        /// <summary>
        /// Gets or sets the number of times the subscription has been triggered
        /// </summary>
        public int TriggerCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times the subscription can be triggered (0 means unlimited)
        /// </summary>
        public int MaxTriggerCount { get; set; }

        /// <summary>
        /// Gets or sets the retry count for failed notifications
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum retry count for failed notifications
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// Gets or sets the retry interval in seconds
        /// </summary>
        public int RetryIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the headers to include in the callback request
        /// </summary>
        public Dictionary<string, string> CallbackHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets whether to include the full event data in the callback
        /// </summary>
        public bool IncludeEventData { get; set; } = true;
    }

    /// <summary>
    /// Represents a filter for event data
    /// </summary>
    public class EventFilter
    {
        /// <summary>
        /// Gets or sets the parameter name to filter on
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the operator for the filter
        /// </summary>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Gets or sets the value to compare against
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Represents the operator for a filter
    /// </summary>
    public enum FilterOperator
    {
        /// <summary>
        /// Equal to
        /// </summary>
        Equals,

        /// <summary>
        /// Not equal to
        /// </summary>
        NotEquals,

        /// <summary>
        /// Greater than
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Greater than or equal to
        /// </summary>
        GreaterThanOrEquals,

        /// <summary>
        /// Less than
        /// </summary>
        LessThan,

        /// <summary>
        /// Less than or equal to
        /// </summary>
        LessThanOrEquals,

        /// <summary>
        /// Contains
        /// </summary>
        Contains,

        /// <summary>
        /// Starts with
        /// </summary>
        StartsWith,

        /// <summary>
        /// Ends with
        /// </summary>
        EndsWith
    }

    /// <summary>
    /// Represents the status of an event subscription
    /// </summary>
    public enum EventSubscriptionStatus
    {
        /// <summary>
        /// The subscription is active and monitoring events
        /// </summary>
        Active,

        /// <summary>
        /// The subscription is paused and not monitoring events
        /// </summary>
        Paused,

        /// <summary>
        /// The subscription has completed (reached end block or max trigger count)
        /// </summary>
        Completed,

        /// <summary>
        /// The subscription has failed and is not monitoring events
        /// </summary>
        Failed,

        /// <summary>
        /// The subscription has been deleted
        /// </summary>
        Deleted
    }
}
