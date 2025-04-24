using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Enums;

namespace NeoServiceLayer.Core.Models.Analytics
{
    /// <summary>
    /// Represents an alert in the analytics system
    /// </summary>
    public class Alert
    {
        /// <summary>
        /// Gets or sets the unique identifier for the alert
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the alert
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the alert
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns this alert
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the user ID that created this alert
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the metric ID for the alert
        /// </summary>
        public Guid? MetricId { get; set; }

        /// <summary>
        /// Gets or sets the query for the alert
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the condition for the alert
        /// </summary>
        public AlertCondition Condition { get; set; }

        /// <summary>
        /// Gets or sets the evaluation frequency in seconds
        /// </summary>
        public int EvaluationFrequencySeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the evaluation window in seconds
        /// </summary>
        public int EvaluationWindowSeconds { get; set; } = 300;

        /// <summary>
        /// Gets or sets the notification settings for the alert
        /// </summary>
        public AlertNotification Notification { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the alert
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last evaluation timestamp
        /// </summary>
        public DateTime? LastEvaluationAt { get; set; }

        /// <summary>
        /// Gets or sets the status of the alert
        /// </summary>
        public AlertStatus Status { get; set; }

        /// <summary>
        /// Gets or sets whether the alert is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the silenced until timestamp
        /// </summary>
        public DateTime? SilencedUntil { get; set; }
    }

    /// <summary>
    /// Represents an alert condition
    /// </summary>
    public class AlertCondition
    {
        /// <summary>
        /// Gets or sets the type of the condition
        /// </summary>
        public ConditionType Type { get; set; }

        /// <summary>
        /// Gets or sets the operator for the condition
        /// </summary>
        public ConditionOperator Operator { get; set; }

        /// <summary>
        /// Gets or sets the threshold value for the condition
        /// </summary>
        public double Threshold { get; set; }

        /// <summary>
        /// Gets or sets the comparison value for the condition
        /// </summary>
        public string ComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the aggregation function for the condition
        /// </summary>
        public AggregationFunction AggregationFunction { get; set; }

        /// <summary>
        /// Gets or sets the number of consecutive evaluations required to trigger the alert
        /// </summary>
        public int ConsecutiveEvaluations { get; set; } = 1;

        /// <summary>
        /// Gets or sets the dimensions to group by
        /// </summary>
        public List<string> GroupBy { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the severity of the condition
        /// </summary>
        public AlertSeverity Severity { get; set; }
    }

    /// <summary>
    /// Represents an alert notification
    /// </summary>
    public class AlertNotification
    {
        /// <summary>
        /// Gets or sets the channels for the notification
        /// </summary>
        public List<NotificationChannel> Channels { get; set; } = new List<NotificationChannel>();

        /// <summary>
        /// Gets or sets the email recipients
        /// </summary>
        public List<string> EmailRecipients { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the webhook URLs
        /// </summary>
        public List<string> WebhookUrls { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the notification message template
        /// </summary>
        public string MessageTemplate { get; set; }

        /// <summary>
        /// Gets or sets the notification subject template
        /// </summary>
        public string SubjectTemplate { get; set; }

        /// <summary>
        /// Gets or sets whether to include the alert data in the notification
        /// </summary>
        public bool IncludeAlertData { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include the evaluation details in the notification
        /// </summary>
        public bool IncludeEvaluationDetails { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to send a recovery notification
        /// </summary>
        public bool SendRecoveryNotification { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum interval between notifications in seconds
        /// </summary>
        public int MinIntervalSeconds { get; set; } = 300;
    }

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
        /// Gets or sets the timestamp of the event
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the type of the event
        /// </summary>
        public AlertEventType Type { get; set; }

        /// <summary>
        /// Gets or sets the severity of the event
        /// </summary>
        public AlertSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the value that triggered the event
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the threshold value
        /// </summary>
        public double Threshold { get; set; }

        /// <summary>
        /// Gets or sets the dimensions of the event
        /// </summary>
        public Dictionary<string, string> Dimensions { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the message for the event
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the notification status
        /// </summary>
        public Dictionary<NotificationChannel, NotificationStatus> NotificationStatus { get; set; } = new Dictionary<NotificationChannel, NotificationStatus>();
    }

    /// <summary>
    /// Represents the type of a condition
    /// </summary>
    public enum ConditionType
    {
        /// <summary>
        /// Threshold condition
        /// </summary>
        Threshold,

        /// <summary>
        /// Change condition
        /// </summary>
        Change,

        /// <summary>
        /// Anomaly condition
        /// </summary>
        Anomaly,

        /// <summary>
        /// No data condition
        /// </summary>
        NoData
    }

    /// <summary>
    /// Represents the operator for a condition
    /// </summary>
    public enum ConditionOperator
    {
        /// <summary>
        /// Greater than operator
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Greater than or equal to operator
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// Less than operator
        /// </summary>
        LessThan,

        /// <summary>
        /// Less than or equal to operator
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// Equal to operator
        /// </summary>
        Equal,

        /// <summary>
        /// Not equal to operator
        /// </summary>
        NotEqual,

        /// <summary>
        /// Outside range operator
        /// </summary>
        OutsideRange,

        /// <summary>
        /// Inside range operator
        /// </summary>
        InsideRange
    }

    /// <summary>
    /// Represents the status of an alert
    /// </summary>
    public enum AlertStatus
    {
        /// <summary>
        /// OK status
        /// </summary>
        OK,

        /// <summary>
        /// Pending status
        /// </summary>
        Pending,

        /// <summary>
        /// Alerting status
        /// </summary>
        Alerting,

        /// <summary>
        /// No data status
        /// </summary>
        NoData,

        /// <summary>
        /// Error status
        /// </summary>
        Error,

        /// <summary>
        /// Silenced status
        /// </summary>
        Silenced
    }

    /// <summary>
    /// Represents the severity of an alert
    /// </summary>
    public enum AlertSeverity
    {
        /// <summary>
        /// Info severity
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

    /// <summary>
    /// Represents the type of an alert event
    /// </summary>
    public enum AlertEventType
    {
        /// <summary>
        /// Triggered event
        /// </summary>
        Triggered,

        /// <summary>
        /// Resolved event
        /// </summary>
        Resolved,

        /// <summary>
        /// No data event
        /// </summary>
        NoData,

        /// <summary>
        /// Error event
        /// </summary>
        Error
    }
}
