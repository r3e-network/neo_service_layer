using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Notification.Repositories
{
    /// <summary>
    /// Interface for notification repository
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="notification">Notification to create</param>
        /// <returns>The created notification</returns>
        Task<Core.Models.Notification> CreateAsync(Core.Models.Notification notification);

        /// <summary>
        /// Gets a notification by ID
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>The notification if found, null otherwise</returns>
        Task<Core.Models.Notification> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets notifications by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of notifications to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of notifications for the account</returns>
        Task<IEnumerable<Core.Models.Notification>> GetByAccountAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets notifications by status
        /// </summary>
        /// <param name="status">Notification status</param>
        /// <param name="limit">Maximum number of notifications to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of notifications with the specified status</returns>
        Task<IEnumerable<Core.Models.Notification>> GetByStatusAsync(NotificationStatus status, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets notifications that need to be retried
        /// </summary>
        /// <param name="limit">Maximum number of notifications to return</param>
        /// <returns>List of notifications that need to be retried</returns>
        Task<IEnumerable<Core.Models.Notification>> GetForRetryAsync(int limit = 100);

        /// <summary>
        /// Gets scheduled notifications that are due
        /// </summary>
        /// <param name="limit">Maximum number of notifications to return</param>
        /// <returns>List of scheduled notifications that are due</returns>
        Task<IEnumerable<Core.Models.Notification>> GetDueScheduledNotificationsAsync(int limit = 100);

        /// <summary>
        /// Updates a notification
        /// </summary>
        /// <param name="notification">Notification to update</param>
        /// <returns>The updated notification</returns>
        Task<Core.Models.Notification> UpdateAsync(Core.Models.Notification notification);

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>True if the notification was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>True if the notification was marked as read, false otherwise</returns>
        Task<bool> MarkAsReadAsync(Guid id);

        /// <summary>
        /// Marks all notifications for an account as read
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Number of notifications marked as read</returns>
        Task<int> MarkAllAsReadAsync(Guid accountId);

        /// <summary>
        /// Gets the count of notifications by status
        /// </summary>
        /// <param name="status">Notification status</param>
        /// <returns>Count of notifications with the specified status</returns>
        Task<int> GetCountByStatusAsync(NotificationStatus status);

        /// <summary>
        /// Gets the count of notifications by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Count of notifications for the account</returns>
        Task<int> GetCountByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets the count of unread notifications by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Count of unread notifications for the account</returns>
        Task<int> GetUnreadCountByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets the count of notifications sent in the last 24 hours
        /// </summary>
        /// <returns>Count of notifications sent in the last 24 hours</returns>
        Task<int> GetSentCountLast24HoursAsync();

        /// <summary>
        /// Gets the count of notifications by type
        /// </summary>
        /// <returns>Count of notifications by type</returns>
        Task<Dictionary<Core.Models.NotificationType, int>> GetCountByTypeAsync();

        /// <summary>
        /// Gets the count of notifications by channel
        /// </summary>
        /// <returns>Count of notifications by channel</returns>
        Task<Dictionary<Core.Enums.NotificationChannel, int>> GetCountByChannelAsync();
    }
}
