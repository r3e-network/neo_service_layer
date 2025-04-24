using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Notification.Repositories
{
    /// <summary>
    /// Implementation of the notification repository
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly ILogger<NotificationRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string CollectionName = "notifications";

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public NotificationRepository(ILogger<NotificationRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Notification> CreateAsync(Core.Models.Notification notification)
        {
            _logger.LogInformation("Creating notification for account: {AccountId}, type: {Type}", notification.AccountId, notification.Type);

            try
            {
                // Set default values
                if (notification.Id == Guid.Empty)
                {
                    notification.Id = Guid.NewGuid();
                }

                notification.CreatedAt = DateTime.UtcNow;

                if (notification.Status == 0)
                {
                    notification.Status = NotificationStatus.Pending;
                }

                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(CollectionName))
                {
                    await _databaseService.CreateCollectionAsync(CollectionName);
                }

                // Create notification
                return await _databaseService.CreateAsync(CollectionName, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for account: {AccountId}, type: {Type}", notification.AccountId, notification.Type);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Notification> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting notification by ID: {Id}", id);

            try
            {
                return await _databaseService.GetByIdAsync<Core.Models.Notification, Guid>(CollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Notification>> GetByAccountAsync(Guid accountId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting notifications for account: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            try
            {
                var notifications = await _databaseService.GetByFilterAsync<Core.Models.Notification>(
                    CollectionName,
                    n => n.AccountId == accountId);

                return notifications.OrderByDescending(n => n.CreatedAt)
                                   .Skip(offset)
                                   .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Notification>> GetByStatusAsync(NotificationStatus status, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting notifications with status: {Status}, limit: {Limit}, offset: {Offset}", status, limit, offset);

            try
            {
                var notifications = await _databaseService.GetByFilterAsync<Core.Models.Notification>(
                    CollectionName,
                    n => n.Status == status);

                return notifications.OrderBy(n => n.CreatedAt)
                                   .Skip(offset)
                                   .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications with status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Notification>> GetForRetryAsync(int limit = 100)
        {
            _logger.LogInformation("Getting notifications for retry, limit: {Limit}", limit);

            try
            {
                var now = DateTime.UtcNow;
                var notifications = await _databaseService.GetByFilterAsync<Core.Models.Notification>(
                    CollectionName,
                    n => n.Status == NotificationStatus.Retrying &&
                         n.NextRetryAt.HasValue &&
                         n.NextRetryAt.Value <= now);

                return notifications.OrderBy(n => n.NextRetryAt)
                                   .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for retry");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Notification>> GetDueScheduledNotificationsAsync(int limit = 100)
        {
            _logger.LogInformation("Getting due scheduled notifications, limit: {Limit}", limit);

            try
            {
                var now = DateTime.UtcNow;
                var notifications = await _databaseService.GetByFilterAsync<Core.Models.Notification>(
                    CollectionName,
                    n => n.Status == NotificationStatus.Scheduled &&
                         n.ScheduledAt.HasValue &&
                         n.ScheduledAt.Value <= now);

                return notifications.OrderBy(n => n.ScheduledAt)
                                   .Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting due scheduled notifications");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Notification> UpdateAsync(Core.Models.Notification notification)
        {
            _logger.LogInformation("Updating notification: {Id}", notification.Id);

            try
            {
                return await _databaseService.UpdateAsync<Core.Models.Notification, Guid>(CollectionName, notification.Id, notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification: {Id}", notification.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting notification: {Id}", id);

            try
            {
                return await _databaseService.DeleteAsync<Core.Models.Notification, Guid>(CollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> MarkAsReadAsync(Guid id)
        {
            _logger.LogInformation("Marking notification as read: {Id}", id);

            try
            {
                var notification = await GetByIdAsync(id);
                if (notification == null)
                {
                    return false;
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await UpdateAsync(notification);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> MarkAllAsReadAsync(Guid accountId)
        {
            _logger.LogInformation("Marking all notifications as read for account: {AccountId}", accountId);

            try
            {
                var notifications = await _databaseService.GetByFilterAsync<Core.Models.Notification>(
                    CollectionName,
                    n => n.AccountId == accountId && !n.IsRead);

                var count = 0;
                var now = DateTime.UtcNow;
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = now;
                    await UpdateAsync(notification);
                    count++;
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountByStatusAsync(NotificationStatus status)
        {
            _logger.LogInformation("Getting count of notifications with status: {Status}", status);

            try
            {
                return await _databaseService.CountAsync<Core.Models.Notification>(
                    CollectionName,
                    n => n.Status == status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of notifications with status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting count of notifications for account: {AccountId}", accountId);

            try
            {
                return await _databaseService.CountAsync<Core.Models.Notification>(
                    CollectionName,
                    n => n.AccountId == accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of notifications for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetUnreadCountByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting count of unread notifications for account: {AccountId}", accountId);

            try
            {
                return await _databaseService.CountAsync<Core.Models.Notification>(
                    CollectionName,
                    n => n.AccountId == accountId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of unread notifications for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetSentCountLast24HoursAsync()
        {
            _logger.LogInformation("Getting count of notifications sent in the last 24 hours");

            try
            {
                var cutoff = DateTime.UtcNow.AddHours(-24);
                return await _databaseService.CountAsync<Core.Models.Notification>(
                    CollectionName,
                    n => n.Status == NotificationStatus.Sent &&
                         n.SentAt.HasValue &&
                         n.SentAt.Value >= cutoff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of notifications sent in the last 24 hours");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Core.Models.NotificationType, int>> GetCountByTypeAsync()
        {
            _logger.LogInformation("Getting count of notifications by type");

            try
            {
                var notifications = await _databaseService.GetAllAsync<Core.Models.Notification>(CollectionName);
                return notifications.GroupBy(n => n.Type)
                                   .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of notifications by type");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Core.Enums.NotificationChannel, int>> GetCountByChannelAsync()
        {
            _logger.LogInformation("Getting count of notifications by channel");

            try
            {
                var notifications = await _databaseService.GetAllAsync<Core.Models.Notification>(CollectionName);
                var counts = new Dictionary<Core.Enums.NotificationChannel, int>();

                foreach (var notification in notifications)
                {
                    foreach (var channel in notification.Channels)
                    {
                        var enumChannel = (Core.Enums.NotificationChannel)channel;
                        if (counts.ContainsKey(enumChannel))
                        {
                            counts[enumChannel]++;
                        }
                        else
                        {
                            counts[enumChannel] = 1;
                        }
                    }
                }

                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of notifications by channel");
                throw;
            }
        }
    }
}
