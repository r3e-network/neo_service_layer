using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Enums;
using NotificationPriorityEnum = NeoServiceLayer.Core.Enums.NotificationPriority;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Notification.Repositories;

namespace NeoServiceLayer.Services.Notification
{
    /// <summary>
    /// Implementation of the notification service
    /// </summary>
    public class NotificationService : INotificationService, IDisposable
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationTemplateRepository _templateRepository;
        private readonly IUserNotificationPreferencesRepository _preferencesRepository;
        private readonly IEnumerable<INotificationProvider> _providers;
        private readonly NotificationConfiguration _configuration;

        private Timer _processingTimer;
        private readonly SemaphoreSlim _processingSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="notificationRepository">Notification repository</param>
        /// <param name="templateRepository">Template repository</param>
        /// <param name="preferencesRepository">Preferences repository</param>
        /// <param name="providers">Notification providers</param>
        /// <param name="configuration">Configuration</param>
        public NotificationService(
            ILogger<NotificationService> logger,
            INotificationRepository notificationRepository,
            INotificationTemplateRepository templateRepository,
            IUserNotificationPreferencesRepository preferencesRepository,
            IEnumerable<INotificationProvider> providers,
            IOptions<NotificationConfiguration> configuration)
        {
            _logger = logger;
            _notificationRepository = notificationRepository;
            _templateRepository = templateRepository;
            _preferencesRepository = preferencesRepository;
            _providers = providers;
            _configuration = configuration.Value;

            // Start processing timer
            _processingTimer = new Timer(
                async _ => await ProcessNotificationsAsync(),
                null,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(_configuration.ProcessingIntervalSeconds));
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Notification> SendNotificationAsync(Core.Models.Notification notification)
        {
            _logger.LogInformation("Sending notification for account: {AccountId}, type: {Type}", notification.AccountId, notification.Type);

            try
            {
                // Validate notification
                if (notification.AccountId == Guid.Empty)
                {
                    throw new ArgumentException("Account ID is required");
                }

                if (notification.Type == 0)
                {
                    notification.Type = (Core.Models.NotificationType)(int)Core.Enums.NotificationType.System;
                }

                if (notification.Priority == 0)
                {
                    notification.Priority = Core.Models.NotificationPriority.Normal;
                }

                if (string.IsNullOrEmpty(notification.Subject))
                {
                    throw new ArgumentException("Subject is required");
                }

                if (string.IsNullOrEmpty(notification.Content))
                {
                    throw new ArgumentException("Content is required");
                }

                if (notification.Channels == null || !notification.Channels.Any())
                {
                    // Get user preferences
                    var preferences = await _preferencesRepository.GetByAccountIdAsync(notification.AccountId);

                    // Use enabled channels from preferences
                    notification.Channels = preferences.EnabledChannels.ToList();

                    // If no channels are enabled, use default channels
                    if (!notification.Channels.Any())
                    {
                        notification.Channels = new List<Core.Models.NotificationChannel> {
                            (Core.Models.NotificationChannel)(int)Core.Enums.NotificationChannel.Email,
                            (Core.Models.NotificationChannel)(int)Core.Enums.NotificationChannel.InApp
                        };
                    }
                }

                // Set default values
                notification.Status = NotificationStatus.Pending;
                notification.CreatedAt = DateTime.UtcNow;
                notification.MaxRetryCount = _configuration.DefaultMaxRetryCount;

                // Save notification
                notification = await _notificationRepository.CreateAsync(notification);

                // Process notification immediately if high priority
                if (notification.Priority == Core.Models.NotificationPriority.High || notification.Priority == Core.Models.NotificationPriority.Critical)
                {
                    await ProcessNotificationAsync(notification);
                }

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for account: {AccountId}, type: {Type}", notification.AccountId, notification.Type);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Notification> SendTemplateNotificationAsync(Guid accountId, Guid templateId, Dictionary<string, object> templateData, List<Core.Enums.NotificationChannel> channels = null)
        {
            _logger.LogInformation("Sending template notification for account: {AccountId}, template: {TemplateId}", accountId, templateId);

            try
            {
                // Get template
                var template = await _templateRepository.GetByIdAsync(templateId);
                if (template == null)
                {
                    throw new ArgumentException($"Template not found: {templateId}");
                }

                // Check if template is active
                if (!template.IsActive)
                {
                    throw new InvalidOperationException($"Template is not active: {templateId}");
                }

                // Process template
                var subject = ProcessTemplate(template.Subject, templateData);
                var content = ProcessTemplate(template.Content, templateData);

                // Create notification
                var notification = new Core.Models.Notification
                {
                    AccountId = accountId,
                    TemplateId = templateId,
                    Type = template.Type,
                    Priority = Core.Models.NotificationPriority.Normal,
                    Subject = subject,
                    Content = content,
                    Data = templateData ?? new Dictionary<string, object>(),
                    Channels = channels != null ? channels.Select(c => (Core.Models.NotificationChannel)(int)c).ToList() : (template.DefaultChannels != null ? template.DefaultChannels : new List<Core.Models.NotificationChannel>())
                };

                // Send notification
                return await SendNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending template notification for account: {AccountId}, template: {TemplateId}", accountId, templateId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Notification> ScheduleNotificationAsync(Core.Models.Notification notification, DateTime scheduledTime)
        {
            _logger.LogInformation("Scheduling notification for account: {AccountId}, type: {Type}, scheduled time: {ScheduledTime}", notification.AccountId, notification.Type, scheduledTime);

            try
            {
                // Validate scheduled time
                if (scheduledTime <= DateTime.UtcNow)
                {
                    throw new ArgumentException("Scheduled time must be in the future");
                }

                // Set scheduled time
                notification.ScheduledAt = scheduledTime;
                notification.Status = NotificationStatus.Scheduled;

                // Send notification (will be saved with scheduled status)
                return await SendNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling notification for account: {AccountId}, type: {Type}", notification.AccountId, notification.Type);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CancelNotificationAsync(Guid notificationId)
        {
            _logger.LogInformation("Cancelling notification: {Id}", notificationId);

            try
            {
                // Get notification
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    return false;
                }

                // Check if notification can be cancelled
                if (notification.Status != NotificationStatus.Pending && notification.Status != NotificationStatus.Scheduled && notification.Status != NotificationStatus.Retrying)
                {
                    return false;
                }

                // Update status
                notification.Status = NotificationStatus.Cancelled;
                await _notificationRepository.UpdateAsync(notification);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling notification: {Id}", notificationId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Notification> GetNotificationAsync(Guid notificationId)
        {
            _logger.LogInformation("Getting notification: {Id}", notificationId);

            try
            {
                return await _notificationRepository.GetByIdAsync(notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification: {Id}", notificationId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Notification>> GetNotificationsByAccountAsync(Guid accountId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting notifications for account: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            try
            {
                return await _notificationRepository.GetByAccountAsync(accountId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Notification>> GetNotificationsByStatusAsync(NotificationStatus status, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting notifications with status: {Status}, limit: {Limit}, offset: {Offset}", status, limit, offset);

            try
            {
                return await _notificationRepository.GetByStatusAsync(status, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications with status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Notification>> GetNotificationsForRetryAsync(int limit = 100)
        {
            _logger.LogInformation("Getting notifications for retry, limit: {Limit}", limit);

            try
            {
                return await _notificationRepository.GetForRetryAsync(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for retry");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId)
        {
            _logger.LogInformation("Marking notification as read: {Id}", notificationId);

            try
            {
                return await _notificationRepository.MarkAsReadAsync(notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read: {Id}", notificationId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> MarkAllNotificationsAsReadAsync(Guid accountId)
        {
            _logger.LogInformation("Marking all notifications as read for account: {AccountId}", accountId);

            try
            {
                return await _notificationRepository.MarkAllAsReadAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteNotificationAsync(Guid notificationId)
        {
            _logger.LogInformation("Deleting notification: {Id}", notificationId);

            try
            {
                return await _notificationRepository.DeleteAsync(notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification: {Id}", notificationId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template)
        {
            _logger.LogInformation("Creating notification template: {Name} for account: {AccountId}", template.Name, template.AccountId);

            try
            {
                // Validate template
                if (template.AccountId == Guid.Empty)
                {
                    throw new ArgumentException("Account ID is required");
                }

                if (string.IsNullOrEmpty(template.Name))
                {
                    throw new ArgumentException("Name is required");
                }

                if (template.Type == 0)
                {
                    template.Type = (Core.Models.NotificationType)(int)Core.Enums.NotificationType.System;
                }

                if (string.IsNullOrEmpty(template.Subject))
                {
                    throw new ArgumentException("Subject is required");
                }

                if (string.IsNullOrEmpty(template.Content))
                {
                    throw new ArgumentException("Content is required");
                }

                // Extract parameters from template
                template.Parameters = ExtractTemplateParameters(template.Subject + " " + template.Content);

                // Set default values
                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;
                template.IsActive = true;

                if (template.DefaultChannels == null || !template.DefaultChannels.Any())
                {
                    template.DefaultChannels = new List<Core.Models.NotificationChannel> {
                        (Core.Models.NotificationChannel)(int)Core.Enums.NotificationChannel.Email,
                        (Core.Models.NotificationChannel)(int)Core.Enums.NotificationChannel.InApp
                    };
                }

                // Create template
                return await _templateRepository.CreateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification template: {Name} for account: {AccountId}", template.Name, template.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NotificationTemplate> GetTemplateAsync(Guid templateId)
        {
            _logger.LogInformation("Getting notification template: {Id}", templateId);

            try
            {
                return await _templateRepository.GetByIdAsync(templateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification template: {Id}", templateId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<NotificationTemplate>> GetTemplatesByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting notification templates for account: {AccountId}", accountId);

            try
            {
                return await _templateRepository.GetByAccountAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification templates for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplate template)
        {
            _logger.LogInformation("Updating notification template: {Id}", template.Id);

            try
            {
                // Validate template
                if (string.IsNullOrEmpty(template.Name))
                {
                    throw new ArgumentException("Name is required");
                }

                if (string.IsNullOrEmpty(template.Subject))
                {
                    throw new ArgumentException("Subject is required");
                }

                if (string.IsNullOrEmpty(template.Content))
                {
                    throw new ArgumentException("Content is required");
                }

                // Extract parameters from template
                template.Parameters = ExtractTemplateParameters(template.Subject + " " + template.Content);

                // Update template
                template.UpdatedAt = DateTime.UtcNow;
                return await _templateRepository.UpdateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification template: {Id}", template.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteTemplateAsync(Guid templateId)
        {
            _logger.LogInformation("Deleting notification template: {Id}", templateId);

            try
            {
                return await _templateRepository.DeleteAsync(templateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification template: {Id}", templateId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserNotificationPreferences> GetUserPreferencesAsync(Guid accountId)
        {
            _logger.LogInformation("Getting user notification preferences for account: {AccountId}", accountId);

            try
            {
                return await _preferencesRepository.GetByAccountIdAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notification preferences for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserNotificationPreferences> UpdateUserPreferencesAsync(UserNotificationPreferences preferences)
        {
            _logger.LogInformation("Updating user notification preferences for account: {AccountId}", preferences.AccountId);

            try
            {
                // Validate preferences
                if (preferences.AccountId == Guid.Empty)
                {
                    throw new ArgumentException("Account ID is required");
                }

                // Update preferences
                preferences.UpdatedAt = DateTime.UtcNow;
                return await _preferencesRepository.CreateOrUpdateAsync(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user notification preferences for account: {AccountId}", preferences.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NotificationStatistics> GetStatisticsAsync()
        {
            _logger.LogInformation("Getting notification statistics");

            try
            {
                var pendingCount = await _notificationRepository.GetCountByStatusAsync(NotificationStatus.Pending);
                var scheduledCount = await _notificationRepository.GetCountByStatusAsync(NotificationStatus.Scheduled);
                var sentCount = await _notificationRepository.GetCountByStatusAsync(NotificationStatus.Sent);
                var failedCount = await _notificationRepository.GetCountByStatusAsync(NotificationStatus.Failed);
                var retryingCount = await _notificationRepository.GetCountByStatusAsync(NotificationStatus.Retrying);
                var sentLast24Hours = await _notificationRepository.GetSentCountLast24HoursAsync();
                var byType = await _notificationRepository.GetCountByTypeAsync();
                var byChannel = await _notificationRepository.GetCountByChannelAsync();

                var totalCount = pendingCount + scheduledCount + sentCount + failedCount + retryingCount;
                var successRate = totalCount > 0 ? (double)sentCount / totalCount : 0;

                return new NotificationStatistics
                {
                    TotalNotifications = totalCount,
                    TotalNotificationsSent = sentCount,
                    TotalNotificationsFailed = failedCount,
                    TotalNotificationsPending = pendingCount,
                    TotalNotificationsScheduled = scheduledCount,
                    TotalNotificationsRetrying = retryingCount,
                    NotificationsSentLast24Hours = sentLast24Hours,
                    NotificationsByType = byType,
                    NotificationsByChannel = byChannel,
                    DeliverySuccessRate = successRate,
                    LastUpdateTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification statistics");
                throw;
            }
        }

        /// <summary>
        /// Processes notifications
        /// </summary>
        private async Task ProcessNotificationsAsync()
        {
            // Prevent concurrent execution
            if (!await _processingSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                // Process pending notifications
                var pendingNotifications = await _notificationRepository.GetByStatusAsync(NotificationStatus.Pending, _configuration.MaxBatchSize);
                foreach (var notification in pendingNotifications)
                {
                    await ProcessNotificationAsync(notification);
                }

                // Process scheduled notifications that are due
                var dueNotifications = await _notificationRepository.GetDueScheduledNotificationsAsync(_configuration.MaxBatchSize);
                foreach (var notification in dueNotifications)
                {
                    notification.Status = NotificationStatus.Pending;
                    await _notificationRepository.UpdateAsync(notification);
                    await ProcessNotificationAsync(notification);
                }

                // Process notifications for retry
                var retryNotifications = await _notificationRepository.GetForRetryAsync(_configuration.MaxBatchSize);
                foreach (var notification in retryNotifications)
                {
                    await ProcessNotificationAsync(notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notifications");
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }

        /// <summary>
        /// Processes a notification
        /// </summary>
        /// <param name="notification">Notification to process</param>
        private async Task ProcessNotificationAsync(Core.Models.Notification notification)
        {
            _logger.LogInformation("Processing notification: {Id}", notification.Id);

            try
            {
                // Update status
                notification.Status = NotificationStatus.Processing;
                await _notificationRepository.UpdateAsync(notification);

                // Get user preferences
                var preferences = await _preferencesRepository.GetByAccountIdAsync(notification.AccountId);

                // Check if in quiet hours
                if (IsInQuietHours(preferences) && notification.Priority != Core.Models.NotificationPriority.Critical)
                {
                    // Reschedule for after quiet hours
                    notification.Status = NotificationStatus.Scheduled;
                    notification.ScheduledAt = GetNextActiveTime(preferences);
                    await _notificationRepository.UpdateAsync(notification);
                    return;
                }

                // Filter channels based on user preferences
                var channels = notification.Channels
                    .Where(c => preferences.EnabledChannels.Contains(c))
                    .ToList();

                // Filter channels based on notification type preferences
                if (preferences.TypePreferences.TryGetValue(notification.Type, out var typeChannels))
                {
                    channels = channels.Intersect(typeChannels).ToList();
                }

                if (!channels.Any())
                {
                    // No channels to send to
                    notification.Status = NotificationStatus.Cancelled;
                    notification.ErrorMessage = "No enabled channels for this notification type";
                    await _notificationRepository.UpdateAsync(notification);
                    return;
                }

                // Add recipient data
                if (!notification.Data.ContainsKey("Email") && !string.IsNullOrEmpty(preferences.Email))
                {
                    notification.Data["Email"] = preferences.Email;
                }

                if (!notification.Data.ContainsKey("PhoneNumber") && !string.IsNullOrEmpty(preferences.PhoneNumber))
                {
                    notification.Data["PhoneNumber"] = preferences.PhoneNumber;
                }

                if (!notification.Data.ContainsKey("DeviceTokens") && preferences.DeviceTokens.Any())
                {
                    notification.Data["DeviceTokens"] = preferences.DeviceTokens;
                }

                if (!notification.Data.ContainsKey("WebhookUrl") && preferences.WebhookUrls.Any())
                {
                    notification.Data["WebhookUrl"] = preferences.WebhookUrls.First();
                }

                // Send notification through each channel
                var success = true;
                var errors = new List<string>();

                foreach (var channel in channels)
                {
                    var provider = _providers.FirstOrDefault(p => p.Channel == channel && p.IsEnabled);
                    if (provider == null)
                    {
                        errors.Add($"No provider available for channel: {channel}");
                        continue;
                    }

                    // Send notification
                    var (status, errorMessage) = await provider.SendAsync(notification);

                    // Update delivery status
                    notification.DeliveryStatus[channel] = status;

                    if (status == NotificationDeliveryStatus.Failed || status == NotificationDeliveryStatus.Rejected || status == NotificationDeliveryStatus.Bounced)
                    {
                        success = false;
                        errors.Add($"{channel}: {errorMessage}");
                    }
                }

                // Update notification status
                if (success)
                {
                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;
                }
                else
                {
                    notification.RetryCount++;
                    if (notification.RetryCount >= notification.MaxRetryCount)
                    {
                        notification.Status = NotificationStatus.Failed;
                        notification.ErrorMessage = string.Join("; ", errors);
                    }
                    else
                    {
                        notification.Status = NotificationStatus.Retrying;
                        notification.ErrorMessage = string.Join("; ", errors);
                        notification.NextRetryAt = DateTime.UtcNow.AddSeconds(_configuration.RetryIntervalSeconds * Math.Pow(2, notification.RetryCount - 1));
                    }
                }

                await _notificationRepository.UpdateAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification: {Id}", notification.Id);

                // Update notification status
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        /// <summary>
        /// Checks if the current time is in quiet hours
        /// </summary>
        /// <param name="preferences">User preferences</param>
        /// <returns>True if in quiet hours, false otherwise</returns>
        private bool IsInQuietHours(UserNotificationPreferences preferences)
        {
            if (!preferences.QuietHoursStart.HasValue || !preferences.QuietHoursEnd.HasValue)
            {
                return false;
            }

            var now = DateTime.UtcNow.TimeOfDay;
            var start = preferences.QuietHoursStart.Value;
            var end = preferences.QuietHoursEnd.Value;

            if (start < end)
            {
                return now >= start && now <= end;
            }
            else
            {
                return now >= start || now <= end;
            }
        }

        /// <summary>
        /// Gets the next active time after quiet hours
        /// </summary>
        /// <param name="preferences">User preferences</param>
        /// <returns>Next active time</returns>
        private DateTime GetNextActiveTime(UserNotificationPreferences preferences)
        {
            if (!preferences.QuietHoursEnd.HasValue)
            {
                return DateTime.UtcNow.AddHours(8);
            }

            var now = DateTime.UtcNow;
            var today = now.Date;
            var end = today.Add(preferences.QuietHoursEnd.Value);

            if (end <= now)
            {
                end = end.AddDays(1);
            }

            return end;
        }

        /// <summary>
        /// Extracts parameters from a template
        /// </summary>
        /// <param name="template">Template</param>
        /// <returns>List of parameters</returns>
        private List<string> ExtractTemplateParameters(string template)
        {
            var parameters = new List<string>();
            var regex = new Regex(@"\{\{([^{}]+)\}\}");
            var matches = regex.Matches(template);

            foreach (Match match in matches)
            {
                var parameter = match.Groups[1].Value.Trim();
                if (!parameters.Contains(parameter))
                {
                    parameters.Add(parameter);
                }
            }

            return parameters;
        }

        /// <summary>
        /// Processes a template with data
        /// </summary>
        /// <param name="template">Template</param>
        /// <param name="data">Data</param>
        /// <returns>Processed template</returns>
        private string ProcessTemplate(string template, Dictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(template) || data == null)
            {
                return template;
            }

            var result = template;
            var regex = new Regex(@"\{\{([^{}]+)\}\}");
            var matches = regex.Matches(template);

            foreach (Match match in matches)
            {
                var parameter = match.Groups[1].Value.Trim();
                if (data.TryGetValue(parameter, out var value))
                {
                    result = result.Replace(match.Value, value?.ToString() ?? string.Empty);
                }
            }

            return result;
        }

        /// <summary>
        /// Disposes the service
        /// </summary>
        public void Dispose()
        {
            _processingTimer?.Dispose();
            _processingSemaphore?.Dispose();
        }
    }
}
