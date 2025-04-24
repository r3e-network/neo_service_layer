using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Notification.Repositories
{
    /// <summary>
    /// Implementation of the user notification preferences repository
    /// </summary>
    public class UserNotificationPreferencesRepository : IUserNotificationPreferencesRepository
    {
        private readonly ILogger<UserNotificationPreferencesRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string CollectionName = "user_notification_preferences";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotificationPreferencesRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public UserNotificationPreferencesRepository(ILogger<UserNotificationPreferencesRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<UserNotificationPreferences> GetByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Getting user notification preferences for account: {AccountId}", accountId);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(CollectionName))
                {
                    await _databaseService.CreateCollectionAsync(CollectionName);
                }

                var preferences = await _databaseService.GetByIdAsync<UserNotificationPreferences, Guid>(CollectionName, accountId);
                if (preferences == null)
                {
                    // Create default preferences
                    preferences = new UserNotificationPreferences
                    {
                        AccountId = accountId,
                        EnabledChannels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.InApp },
                        TypePreferences = new Dictionary<NotificationType, List<NotificationChannel>>(),
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Add default type preferences
                    foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
                    {
                        preferences.TypePreferences[type] = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.InApp };
                    }

                    await _databaseService.CreateAsync(CollectionName, preferences);
                }

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notification preferences for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserNotificationPreferences> CreateOrUpdateAsync(UserNotificationPreferences preferences)
        {
            _logger.LogInformation("Creating or updating user notification preferences for account: {AccountId}", preferences.AccountId);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(CollectionName))
                {
                    await _databaseService.CreateCollectionAsync(CollectionName);
                }

                preferences.UpdatedAt = DateTime.UtcNow;

                // Check if preferences exist
                var existingPreferences = await _databaseService.GetByIdAsync<UserNotificationPreferences, Guid>(CollectionName, preferences.AccountId);
                if (existingPreferences == null)
                {
                    // Create preferences
                    return await _databaseService.CreateAsync(CollectionName, preferences);
                }
                else
                {
                    // Update preferences
                    return await _databaseService.UpdateAsync<UserNotificationPreferences, Guid>(CollectionName, preferences.AccountId, preferences);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or updating user notification preferences for account: {AccountId}", preferences.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> AddDeviceTokenAsync(Guid accountId, string deviceToken)
        {
            _logger.LogInformation("Adding device token for account: {AccountId}", accountId);

            try
            {
                var preferences = await GetByAccountIdAsync(accountId);
                if (!preferences.DeviceTokens.Contains(deviceToken))
                {
                    preferences.DeviceTokens.Add(deviceToken);
                    preferences.UpdatedAt = DateTime.UtcNow;
                    await _databaseService.UpdateAsync<UserNotificationPreferences, Guid>(CollectionName, accountId, preferences);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding device token for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveDeviceTokenAsync(Guid accountId, string deviceToken)
        {
            _logger.LogInformation("Removing device token for account: {AccountId}", accountId);

            try
            {
                var preferences = await GetByAccountIdAsync(accountId);
                if (preferences.DeviceTokens.Contains(deviceToken))
                {
                    preferences.DeviceTokens.Remove(deviceToken);
                    preferences.UpdatedAt = DateTime.UtcNow;
                    await _databaseService.UpdateAsync<UserNotificationPreferences, Guid>(CollectionName, accountId, preferences);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing device token for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> AddWebhookUrlAsync(Guid accountId, string webhookUrl)
        {
            _logger.LogInformation("Adding webhook URL for account: {AccountId}", accountId);

            try
            {
                var preferences = await GetByAccountIdAsync(accountId);
                if (!preferences.WebhookUrls.Contains(webhookUrl))
                {
                    preferences.WebhookUrls.Add(webhookUrl);
                    preferences.UpdatedAt = DateTime.UtcNow;
                    await _databaseService.UpdateAsync<UserNotificationPreferences, Guid>(CollectionName, accountId, preferences);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding webhook URL for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveWebhookUrlAsync(Guid accountId, string webhookUrl)
        {
            _logger.LogInformation("Removing webhook URL for account: {AccountId}", accountId);

            try
            {
                var preferences = await GetByAccountIdAsync(accountId);
                if (preferences.WebhookUrls.Contains(webhookUrl))
                {
                    preferences.WebhookUrls.Remove(webhookUrl);
                    preferences.UpdatedAt = DateTime.UtcNow;
                    await _databaseService.UpdateAsync<UserNotificationPreferences, Guid>(CollectionName, accountId, preferences);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing webhook URL for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateEnabledChannelsAsync(Guid accountId, List<NotificationChannel> channels)
        {
            _logger.LogInformation("Updating enabled channels for account: {AccountId}", accountId);

            try
            {
                var preferences = await GetByAccountIdAsync(accountId);
                preferences.EnabledChannels = channels;
                preferences.UpdatedAt = DateTime.UtcNow;
                await _databaseService.UpdateAsync<UserNotificationPreferences, Guid>(CollectionName, accountId, preferences);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating enabled channels for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateTypePreferencesAsync(Guid accountId, Dictionary<NotificationType, List<NotificationChannel>> typePreferences)
        {
            _logger.LogInformation("Updating type preferences for account: {AccountId}", accountId);

            try
            {
                var preferences = await GetByAccountIdAsync(accountId);
                preferences.TypePreferences = typePreferences;
                preferences.UpdatedAt = DateTime.UtcNow;
                await _databaseService.UpdateAsync<UserNotificationPreferences, Guid>(CollectionName, accountId, preferences);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating type preferences for account: {AccountId}", accountId);
                throw;
            }
        }
    }
}
