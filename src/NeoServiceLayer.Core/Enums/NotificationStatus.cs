namespace NeoServiceLayer.Core.Enums
{
    /// <summary>
    /// Represents the status of a notification
    /// </summary>
    public enum NotificationStatus
    {
        /// <summary>
        /// The notification is pending
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The notification is scheduled for delivery
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// The notification is being processed
        /// </summary>
        Processing = 2,

        /// <summary>
        /// The notification was sent successfully
        /// </summary>
        Sent = 3,

        /// <summary>
        /// The notification failed
        /// </summary>
        Failed = 4,

        /// <summary>
        /// The notification is being retried
        /// </summary>
        Retrying = 5,

        /// <summary>
        /// The notification has been cancelled
        /// </summary>
        Cancelled = 6
    }
}
