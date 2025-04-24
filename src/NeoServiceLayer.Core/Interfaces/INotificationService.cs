using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for notification service
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification
        /// </summary>
        /// <param name="notification">Notification to send</param>
        /// <returns>The sent notification</returns>
        Task<Notification> SendNotificationAsync(Notification notification);

        /// <summary>
        /// Sends a notification using a template
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="templateId">Template ID</param>
        /// <param name="templateData">Template data</param>
        /// <param name="channels">Channels to send the notification through (null to use template defaults)</param>
        /// <returns>The sent notification</returns>
        Task<Notification> SendTemplateNotificationAsync(Guid accountId, Guid templateId, Dictionary<string, object> templateData, List<Enums.NotificationChannel> channels = null);

        /// <summary>
        /// Schedules a notification for later delivery
        /// </summary>
        /// <param name="notification">Notification to schedule</param>
        /// <param name="scheduledTime">Scheduled time</param>
        /// <returns>The scheduled notification</returns>
        Task<Notification> ScheduleNotificationAsync(Notification notification, DateTime scheduledTime);

        /// <summary>
        /// Cancels a scheduled notification
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>True if the notification was cancelled, false otherwise</returns>
        Task<bool> CancelNotificationAsync(Guid notificationId);

        /// <summary>
        /// Gets a notification by ID
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>The notification if found, null otherwise</returns>
        Task<Notification> GetNotificationAsync(Guid notificationId);

        /// <summary>
        /// Gets notifications by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of notifications to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of notifications for the account</returns>
        Task<IEnumerable<Notification>> GetNotificationsByAccountAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets notifications by status
        /// </summary>
        /// <param name="status">Notification status</param>
        /// <param name="limit">Maximum number of notifications to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of notifications with the specified status</returns>
        Task<IEnumerable<Notification>> GetNotificationsByStatusAsync(NotificationStatus status, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets notifications that need to be retried
        /// </summary>
        /// <param name="limit">Maximum number of notifications to return</param>
        /// <returns>List of notifications that need to be retried</returns>
        Task<IEnumerable<Notification>> GetNotificationsForRetryAsync(int limit = 100);

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>True if the notification was marked as read, false otherwise</returns>
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId);

        /// <summary>
        /// Marks all notifications for an account as read
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Number of notifications marked as read</returns>
        Task<int> MarkAllNotificationsAsReadAsync(Guid accountId);

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>True if the notification was deleted, false otherwise</returns>
        Task<bool> DeleteNotificationAsync(Guid notificationId);

        /// <summary>
        /// Creates a notification template
        /// </summary>
        /// <param name="template">Template to create</param>
        /// <returns>The created template</returns>
        Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template);

        /// <summary>
        /// Gets a template by ID
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>The template if found, null otherwise</returns>
        Task<NotificationTemplate> GetTemplateAsync(Guid templateId);

        /// <summary>
        /// Gets templates by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of templates for the account</returns>
        Task<IEnumerable<NotificationTemplate>> GetTemplatesByAccountAsync(Guid accountId);

        /// <summary>
        /// Updates a template
        /// </summary>
        /// <param name="template">Template to update</param>
        /// <returns>The updated template</returns>
        Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplate template);

        /// <summary>
        /// Deletes a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>True if the template was deleted, false otherwise</returns>
        Task<bool> DeleteTemplateAsync(Guid templateId);

        /// <summary>
        /// Gets user notification preferences
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>User notification preferences</returns>
        Task<UserNotificationPreferences> GetUserPreferencesAsync(Guid accountId);

        /// <summary>
        /// Updates user notification preferences
        /// </summary>
        /// <param name="preferences">User notification preferences</param>
        /// <returns>The updated preferences</returns>
        Task<UserNotificationPreferences> UpdateUserPreferencesAsync(UserNotificationPreferences preferences);

        /// <summary>
        /// Gets notification statistics
        /// </summary>
        /// <returns>Notification statistics</returns>
        Task<NotificationStatistics> GetStatisticsAsync();
    }

    /// <summary>
    /// Statistics for notifications
    /// </summary>
    public class NotificationStatistics
    {
        /// <summary>
        /// Gets or sets the total number of notifications
        /// </summary>
        public int TotalNotifications { get; set; }

        /// <summary>
        /// Gets or sets the total number of notifications sent
        /// </summary>
        public int TotalNotificationsSent { get; set; }

        /// <summary>
        /// Gets or sets the total number of notifications failed
        /// </summary>
        public int TotalNotificationsFailed { get; set; }

        /// <summary>
        /// Gets or sets the total number of notifications pending
        /// </summary>
        public int TotalNotificationsPending { get; set; }

        /// <summary>
        /// Gets or sets the total number of notifications scheduled
        /// </summary>
        public int TotalNotificationsScheduled { get; set; }

        /// <summary>
        /// Gets or sets the total number of notifications retrying
        /// </summary>
        public int TotalNotificationsRetrying { get; set; }

        /// <summary>
        /// Gets or sets the notifications sent in the last 24 hours
        /// </summary>
        public int NotificationsSentLast24Hours { get; set; }

        /// <summary>
        /// Gets or sets the notifications by type
        /// </summary>
        public Dictionary<Models.NotificationType, int> NotificationsByType { get; set; } = new Dictionary<Models.NotificationType, int>();

        /// <summary>
        /// Gets or sets the notifications by channel
        /// </summary>
        public Dictionary<Enums.NotificationChannel, int> NotificationsByChannel { get; set; } = new Dictionary<Enums.NotificationChannel, int>();

        /// <summary>
        /// Gets or sets the delivery success rate
        /// </summary>
        public double DeliverySuccessRate { get; set; }

        /// <summary>
        /// Gets or sets the last update time
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
    }
}
