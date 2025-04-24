using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Enums;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a notification to be sent to a user
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets the unique identifier for the notification
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the account ID that the notification is for
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the template ID used for the notification
        /// </summary>
        public Guid? TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the notification type
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Gets or sets the notification priority
        /// </summary>
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the notification subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the notification content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the notification data
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the channels to send the notification through
        /// </summary>
        public List<NotificationChannel> Channels { get; set; } = new List<NotificationChannel>();

        /// <summary>
        /// Gets or sets the notification status
        /// </summary>
        public NotificationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the scheduled timestamp
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// Gets or sets the sent timestamp
        /// </summary>
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// Gets or sets the delivery status for each channel
        /// </summary>
        public Dictionary<NotificationChannel, NotificationDeliveryStatus> DeliveryStatus { get; set; } =
            new Dictionary<NotificationChannel, NotificationDeliveryStatus>();

        /// <summary>
        /// Gets or sets the error message if delivery failed
        /// </summary>
        public string ErrorMessage { get; set; }

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
        /// Gets or sets whether the notification is read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets the read timestamp
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Gets or sets the expiration timestamp
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Represents a notification template
    /// </summary>
    public class NotificationTemplate
    {
        /// <summary>
        /// Gets or sets the unique identifier for the template
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns this template
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the template name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the template description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the notification type
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Gets or sets the template subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the template content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the template parameters
        /// </summary>
        public List<string> Parameters { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the default channels to send notifications through
        /// </summary>
        public List<NotificationChannel> DefaultChannels { get; set; } = new List<NotificationChannel>();

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets whether the template is active
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents user notification preferences
    /// </summary>
    public class UserNotificationPreferences
    {
        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the device tokens for push notifications
        /// </summary>
        public List<string> DeviceTokens { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the webhook URLs
        /// </summary>
        public List<string> WebhookUrls { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the enabled channels
        /// </summary>
        public List<NotificationChannel> EnabledChannels { get; set; } = new List<NotificationChannel>();

        /// <summary>
        /// Gets or sets the notification type preferences
        /// </summary>
        public Dictionary<NotificationType, List<NotificationChannel>> TypePreferences { get; set; } =
            new Dictionary<NotificationType, List<NotificationChannel>>();

        /// <summary>
        /// Gets or sets the quiet hours start time (UTC)
        /// </summary>
        public TimeSpan? QuietHoursStart { get; set; }

        /// <summary>
        /// Gets or sets the quiet hours end time (UTC)
        /// </summary>
        public TimeSpan? QuietHoursEnd { get; set; }

        /// <summary>
        /// Gets or sets the time zone
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the type of notification
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// System notification
        /// </summary>
        System,

        /// <summary>
        /// Account notification
        /// </summary>
        Account,

        /// <summary>
        /// Wallet notification
        /// </summary>
        Wallet,

        /// <summary>
        /// Transaction notification
        /// </summary>
        Transaction,

        /// <summary>
        /// Function notification
        /// </summary>
        Function,

        /// <summary>
        /// Event notification
        /// </summary>
        Event,

        /// <summary>
        /// Price alert notification
        /// </summary>
        PriceAlert,

        /// <summary>
        /// Security notification
        /// </summary>
        Security,

        /// <summary>
        /// Marketing notification
        /// </summary>
        Marketing
    }

    /// <summary>
    /// Represents the priority of a notification
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// Low priority
        /// </summary>
        Low,

        /// <summary>
        /// Normal priority
        /// </summary>
        Normal,

        /// <summary>
        /// High priority
        /// </summary>
        High,

        /// <summary>
        /// Critical priority
        /// </summary>
        Critical
    }

    /// <summary>
    /// Represents the channel for sending notifications
    /// </summary>
    public enum NotificationChannel
    {
        /// <summary>
        /// Email channel
        /// </summary>
        Email,

        /// <summary>
        /// SMS channel
        /// </summary>
        SMS,

        /// <summary>
        /// Push notification channel
        /// </summary>
        Push,

        /// <summary>
        /// In-app notification channel
        /// </summary>
        InApp,

        /// <summary>
        /// Webhook channel
        /// </summary>
        Webhook
    }



    /// <summary>
    /// Represents the delivery status of a notification
    /// </summary>
    public enum NotificationDeliveryStatus
    {
        /// <summary>
        /// The notification is pending delivery
        /// </summary>
        Pending,

        /// <summary>
        /// The notification has been sent
        /// </summary>
        Sent,

        /// <summary>
        /// The notification has been delivered
        /// </summary>
        Delivered,

        /// <summary>
        /// The notification has been read
        /// </summary>
        Read,

        /// <summary>
        /// The notification has failed delivery
        /// </summary>
        Failed,

        /// <summary>
        /// The notification has been rejected
        /// </summary>
        Rejected,

        /// <summary>
        /// The notification has bounced
        /// </summary>
        Bounced
    }
}
