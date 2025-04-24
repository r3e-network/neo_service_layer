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
    /// Implementation of the notification template repository
    /// </summary>
    public class NotificationTemplateRepository : INotificationTemplateRepository
    {
        private readonly ILogger<NotificationTemplateRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string CollectionName = "notification_templates";

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationTemplateRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public NotificationTemplateRepository(ILogger<NotificationTemplateRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<NotificationTemplate> CreateAsync(NotificationTemplate template)
        {
            _logger.LogInformation("Creating notification template: {Name} for account: {AccountId}", template.Name, template.AccountId);

            try
            {
                // Set default values
                if (template.Id == Guid.Empty)
                {
                    template.Id = Guid.NewGuid();
                }

                template.CreatedAt = DateTime.UtcNow;
                template.UpdatedAt = DateTime.UtcNow;
                
                if (!template.DefaultChannels.Any())
                {
                    template.DefaultChannels.Add(NotificationChannel.Email);
                }

                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(CollectionName))
                {
                    await _databaseService.CreateCollectionAsync(CollectionName);
                }

                // Create template
                return await _databaseService.CreateAsync(CollectionName, template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification template: {Name} for account: {AccountId}", template.Name, template.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NotificationTemplate> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting notification template by ID: {Id}", id);

            try
            {
                return await _databaseService.GetByIdAsync<NotificationTemplate, Guid>(CollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification template by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<NotificationTemplate>> GetByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting notification templates for account: {AccountId}", accountId);

            try
            {
                return await _databaseService.GetByFilterAsync<NotificationTemplate>(
                    CollectionName,
                    t => t.AccountId == accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification templates for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<NotificationTemplate>> GetByTypeAsync(NotificationType type)
        {
            _logger.LogInformation("Getting notification templates for type: {Type}", type);

            try
            {
                return await _databaseService.GetByFilterAsync<NotificationTemplate>(
                    CollectionName,
                    t => t.Type == type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification templates for type: {Type}", type);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<NotificationTemplate>> GetActiveAsync()
        {
            _logger.LogInformation("Getting active notification templates");

            try
            {
                return await _databaseService.GetByFilterAsync<NotificationTemplate>(
                    CollectionName,
                    t => t.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active notification templates");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<NotificationTemplate> UpdateAsync(NotificationTemplate template)
        {
            _logger.LogInformation("Updating notification template: {Id}", template.Id);

            try
            {
                template.UpdatedAt = DateTime.UtcNow;
                return await _databaseService.UpdateAsync<NotificationTemplate, Guid>(CollectionName, template.Id, template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification template: {Id}", template.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting notification template: {Id}", id);

            try
            {
                return await _databaseService.DeleteAsync<NotificationTemplate, Guid>(CollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification template: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting count of notification templates for account: {AccountId}", accountId);

            try
            {
                return await _databaseService.CountAsync<NotificationTemplate>(
                    CollectionName,
                    t => t.AccountId == accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting count of notification templates for account: {AccountId}", accountId);
                throw;
            }
        }
    }
}
