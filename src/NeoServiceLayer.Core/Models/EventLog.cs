using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Enums;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a log of a blockchain event
    /// </summary>
    public class EventLog
    {
        /// <summary>
        /// Gets or sets the unique identifier for the event log
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the subscription ID that triggered this event
        /// </summary>
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns the subscription
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the transaction hash that contained the event
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Gets or sets the block hash that contained the event
        /// </summary>
        public string BlockHash { get; set; }

        /// <summary>
        /// Gets or sets the block height that contained the event
        /// </summary>
        public long BlockHeight { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the block that contained the event
        /// </summary>
        public DateTime BlockTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the contract hash that emitted the event
        /// </summary>
        public string ContractHash { get; set; }

        /// <summary>
        /// Gets or sets the event name
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the event data
        /// </summary>
        public Dictionary<string, object> EventData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the raw event data
        /// </summary>
        public string RawEventData { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the event was detected
        /// </summary>
        public DateTime DetectedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the notification was sent
        /// </summary>
        public DateTime? NotifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the notification status
        /// </summary>
        public NotificationStatus NotificationStatus { get; set; }

        /// <summary>
        /// Gets or sets the notification response
        /// </summary>
        public string NotificationResponse { get; set; }

        /// <summary>
        /// Gets or sets the retry count for failed notifications
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the next retry timestamp
        /// </summary>
        public DateTime? NextRetryAt { get; set; }

        /// <summary>
        /// Gets or sets the error message if notification failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }



    /// <summary>
    /// Represents a notification to be sent for an event
    /// </summary>
    public class EventNotification
    {
        /// <summary>
        /// Gets or sets the unique identifier for the notification
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the event log ID
        /// </summary>
        public Guid EventLogId { get; set; }

        /// <summary>
        /// Gets or sets the subscription ID
        /// </summary>
        public Guid SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the callback URL
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets the function ID to execute
        /// </summary>
        public Guid? FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the notification payload
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// Gets or sets the headers to include in the callback request
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the timestamp when the notification was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the notification was sent
        /// </summary>
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// Gets or sets the notification status
        /// </summary>
        public NotificationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the response from the callback
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Gets or sets the retry count
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum retry count
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// Gets or sets the next retry timestamp
        /// </summary>
        public DateTime? NextRetryAt { get; set; }

        /// <summary>
        /// Gets or sets the error message if notification failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
