using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Notification.Repositories
{
    /// <summary>
    /// Interface for notification template repository
    /// </summary>
    public interface INotificationTemplateRepository
    {
        /// <summary>
        /// Creates a new template
        /// </summary>
        /// <param name="template">Template to create</param>
        /// <returns>The created template</returns>
        Task<NotificationTemplate> CreateAsync(NotificationTemplate template);

        /// <summary>
        /// Gets a template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>The template if found, null otherwise</returns>
        Task<NotificationTemplate> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets templates by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of templates for the account</returns>
        Task<IEnumerable<NotificationTemplate>> GetByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets templates by type
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <returns>List of templates for the type</returns>
        Task<IEnumerable<NotificationTemplate>> GetByTypeAsync(NotificationType type);

        /// <summary>
        /// Gets active templates
        /// </summary>
        /// <returns>List of active templates</returns>
        Task<IEnumerable<NotificationTemplate>> GetActiveAsync();

        /// <summary>
        /// Updates a template
        /// </summary>
        /// <param name="template">Template to update</param>
        /// <returns>The updated template</returns>
        Task<NotificationTemplate> UpdateAsync(NotificationTemplate template);

        /// <summary>
        /// Deletes a template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>True if the template was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets the count of templates by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Count of templates for the account</returns>
        Task<int> GetCountByAccountAsync(Guid accountId);
    }
}
