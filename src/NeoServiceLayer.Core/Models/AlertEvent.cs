using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Enums;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an alert event
    /// </summary>
    public class AlertEvent
    {
        /// <summary>
        /// Gets or sets the unique identifier for the event
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the alert ID
        /// </summary>
        public Guid AlertId { get; set; }

        /// <summary>
        /// Gets or sets the event type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the event severity
        /// </summary>
        public AlertSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the event timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the metric value
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the threshold value
        /// </summary>
        public double Threshold { get; set; }

        /// <summary>
        /// Gets or sets the event message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the event dimensions
        /// </summary>
        public Dictionary<string, string> Dimensions { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the notification status for each channel
        /// </summary>
        public Dictionary<NotificationChannel, NotificationDeliveryStatus> NotificationStatus { get; set; } =
            new Dictionary<NotificationChannel, NotificationDeliveryStatus>();
    }

    /// <summary>
    /// Represents the severity of an alert
    /// </summary>
    public enum AlertSeverity
    {
        /// <summary>
        /// Informational severity
        /// </summary>
        Info,

        /// <summary>
        /// Warning severity
        /// </summary>
        Warning,

        /// <summary>
        /// Error severity
        /// </summary>
        Error,

        /// <summary>
        /// Critical severity
        /// </summary>
        Critical
    }
}
