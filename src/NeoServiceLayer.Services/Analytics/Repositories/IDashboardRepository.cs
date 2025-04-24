using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Services.Analytics.Repositories
{
    /// <summary>
    /// Interface for dashboard repository
    /// </summary>
    public interface IDashboardRepository
    {
        /// <summary>
        /// Creates a new dashboard
        /// </summary>
        /// <param name="dashboard">Dashboard to create</param>
        /// <returns>The created dashboard</returns>
        Task<Dashboard> CreateAsync(Dashboard dashboard);

        /// <summary>
        /// Gets a dashboard by ID
        /// </summary>
        /// <param name="id">Dashboard ID</param>
        /// <returns>The dashboard if found, null otherwise</returns>
        Task<Dashboard> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets dashboards by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of dashboards for the account</returns>
        Task<IEnumerable<Dashboard>> GetByAccountAsync(Guid accountId);

        /// <summary>
        /// Gets dashboards by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of dashboards created by the user</returns>
        Task<IEnumerable<Dashboard>> GetByUserAsync(Guid userId);

        /// <summary>
        /// Gets public dashboards
        /// </summary>
        /// <returns>List of public dashboards</returns>
        Task<IEnumerable<Dashboard>> GetPublicAsync();

        /// <summary>
        /// Updates a dashboard
        /// </summary>
        /// <param name="dashboard">Dashboard to update</param>
        /// <returns>The updated dashboard</returns>
        Task<Dashboard> UpdateAsync(Dashboard dashboard);

        /// <summary>
        /// Deletes a dashboard
        /// </summary>
        /// <param name="id">Dashboard ID</param>
        /// <returns>True if the dashboard was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets the count of dashboards
        /// </summary>
        /// <returns>Count of dashboards</returns>
        Task<int> GetCountAsync();

        /// <summary>
        /// Gets the count of dashboards by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Count of dashboards for the account</returns>
        Task<int> GetCountByAccountAsync(Guid accountId);
    }
}
