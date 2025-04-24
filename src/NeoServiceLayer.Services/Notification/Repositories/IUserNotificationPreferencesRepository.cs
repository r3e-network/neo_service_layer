using System;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Notification.Repositories
{
    /// <summary>
    /// Interface for user notification preferences repository
    /// </summary>
    public interface IUserNotificationPreferencesRepository
    {
        /// <summary>
        /// Gets user preferences by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>User preferences if found, null otherwise</returns>
        Task<UserNotificationPreferences> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Creates or updates user preferences
        /// </summary>
        /// <param name="preferences">User preferences</param>
        /// <returns>The created or updated preferences</returns>
        Task<UserNotificationPreferences> CreateOrUpdateAsync(UserNotificationPreferences preferences);

        /// <summary>
        /// Adds a device token for push notifications
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="deviceToken">Device token</param>
        /// <returns>True if the device token was added, false otherwise</returns>
        Task<bool> AddDeviceTokenAsync(Guid accountId, string deviceToken);

        /// <summary>
        /// Removes a device token for push notifications
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="deviceToken">Device token</param>
        /// <returns>True if the device token was removed, false otherwise</returns>
        Task<bool> RemoveDeviceTokenAsync(Guid accountId, string deviceToken);

        /// <summary>
        /// Adds a webhook URL
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="webhookUrl">Webhook URL</param>
        /// <returns>True if the webhook URL was added, false otherwise</returns>
        Task<bool> AddWebhookUrlAsync(Guid accountId, string webhookUrl);

        /// <summary>
        /// Removes a webhook URL
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="webhookUrl">Webhook URL</param>
        /// <returns>True if the webhook URL was removed, false otherwise</returns>
        Task<bool> RemoveWebhookUrlAsync(Guid accountId, string webhookUrl);

        /// <summary>
        /// Updates the enabled channels for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="channels">Enabled channels</param>
        /// <returns>True if the channels were updated, false otherwise</returns>
        Task<bool> UpdateEnabledChannelsAsync(Guid accountId, System.Collections.Generic.List<NotificationChannel> channels);

        /// <summary>
        /// Updates the notification type preferences for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="typePreferences">Type preferences</param>
        /// <returns>True if the type preferences were updated, false otherwise</returns>
        Task<bool> UpdateTypePreferencesAsync(Guid accountId, System.Collections.Generic.Dictionary<NotificationType, System.Collections.Generic.List<NotificationChannel>> typePreferences);
    }
}
