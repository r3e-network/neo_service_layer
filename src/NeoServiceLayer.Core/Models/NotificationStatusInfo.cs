using System;
using NeoServiceLayer.Core.Enums;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents notification status information with timestamp
    /// </summary>
    public class NotificationStatusInfo
    {
        /// <summary>
        /// Gets or sets the notification status
        /// </summary>
        public NotificationDeliveryStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the status was updated
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the error message if any
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Implicit conversion from NotificationStatus to NotificationStatusInfo
        /// </summary>
        /// <param name="status">The notification status</param>
        public static implicit operator NotificationStatusInfo(NotificationDeliveryStatus status)
        {
            return new NotificationStatusInfo
            {
                Status = status,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Implicit conversion from NotificationStatusInfo to NotificationStatus
        /// </summary>
        /// <param name="statusInfo">The notification status info</param>
        public static implicit operator NotificationDeliveryStatus(NotificationStatusInfo statusInfo)
        {
            return statusInfo.Status;
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        /// <param name="statusInfo">The notification status info</param>
        /// <param name="status">The notification status</param>
        /// <returns>True if equal, false otherwise</returns>
        public static bool operator ==(NotificationStatusInfo statusInfo, NotificationDeliveryStatus status)
        {
            if (statusInfo is null)
                return false;

            return statusInfo.Status == status;
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="statusInfo">The notification status info</param>
        /// <param name="status">The notification status</param>
        /// <returns>True if not equal, false otherwise</returns>
        public static bool operator !=(NotificationStatusInfo statusInfo, NotificationDeliveryStatus status)
        {
            return !(statusInfo == status);
        }

        /// <summary>
        /// Equality override
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is NotificationDeliveryStatus status)
                return Status == status;

            if (obj is NotificationStatusInfo statusInfo)
                return Status == statusInfo.Status;

            return false;
        }

        /// <summary>
        /// GetHashCode override
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return Status.GetHashCode();
        }
    }
}
