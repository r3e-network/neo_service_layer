using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models.Analytics;

namespace NeoServiceLayer.Services.Analytics.Repositories
{
    /// <summary>
    /// Implementation of the dashboard repository
    /// </summary>
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ILogger<DashboardRepository> _logger;
        private readonly IDatabaseService _databaseService;
        private const string DashboardsCollectionName = "analytics_dashboards";

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        public DashboardRepository(ILogger<DashboardRepository> logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
        }

        /// <inheritdoc/>
        public async Task<Dashboard> CreateAsync(Dashboard dashboard)
        {
            _logger.LogInformation("Creating dashboard: {Name} for account: {AccountId}", dashboard.Name, dashboard.AccountId);

            try
            {
                // Create collection if it doesn't exist
                if (!await _databaseService.CollectionExistsAsync(DashboardsCollectionName))
                {
                    await _databaseService.CreateCollectionAsync(DashboardsCollectionName);
                }

                // Set default values
                if (dashboard.Id == Guid.Empty)
                {
                    dashboard.Id = Guid.NewGuid();
                }

                dashboard.CreatedAt = DateTime.UtcNow;
                dashboard.UpdatedAt = DateTime.UtcNow;

                // Create dashboard
                return await _databaseService.CreateAsync(DashboardsCollectionName, dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dashboard: {Name} for account: {AccountId}", dashboard.Name, dashboard.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dashboard> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting dashboard: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DashboardsCollectionName))
                {
                    return null;
                }

                // Get dashboard
                return await _databaseService.GetByIdAsync<Dashboard, Guid>(DashboardsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Dashboard>> GetByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting dashboards for account: {AccountId}", accountId);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DashboardsCollectionName))
                {
                    return Enumerable.Empty<Dashboard>();
                }

                // Get dashboards
                return await _databaseService.GetByFilterAsync<Dashboard>(
                    DashboardsCollectionName,
                    d => d.AccountId == accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboards for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Dashboard>> GetByUserAsync(Guid userId)
        {
            _logger.LogInformation("Getting dashboards for user: {UserId}", userId);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DashboardsCollectionName))
                {
                    return Enumerable.Empty<Dashboard>();
                }

                // Get dashboards
                return await _databaseService.GetByFilterAsync<Dashboard>(
                    DashboardsCollectionName,
                    d => d.CreatedBy == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboards for user: {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Dashboard>> GetPublicAsync()
        {
            _logger.LogInformation("Getting public dashboards");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DashboardsCollectionName))
                {
                    return Enumerable.Empty<Dashboard>();
                }

                // Get dashboards
                return await _databaseService.GetByFilterAsync<Dashboard>(
                    DashboardsCollectionName,
                    d => d.IsPublic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public dashboards");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dashboard> UpdateAsync(Dashboard dashboard)
        {
            _logger.LogInformation("Updating dashboard: {Id}", dashboard.Id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DashboardsCollectionName))
                {
                    throw new InvalidOperationException("Dashboards collection does not exist");
                }

                // Update timestamp
                dashboard.UpdatedAt = DateTime.UtcNow;

                // Update dashboard
                return await _databaseService.UpdateAsync<Dashboard, Guid>(DashboardsCollectionName, dashboard.Id, dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard: {Id}", dashboard.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting dashboard: {Id}", id);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DashboardsCollectionName))
                {
                    return false;
                }

                // Delete dashboard
                return await _databaseService.DeleteAsync<Dashboard, Guid>(DashboardsCollectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting dashboard: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync()
        {
            _logger.LogInformation("Getting dashboard count");

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DashboardsCollectionName))
                {
                    return 0;
                }

                // Get count
                var dashboards = await _databaseService.GetAllAsync<Dashboard>(DashboardsCollectionName);
                return dashboards.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard count");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetCountByAccountAsync(Guid accountId)
        {
            _logger.LogInformation("Getting dashboard count for account: {AccountId}", accountId);

            try
            {
                // Check if collection exists
                if (!await _databaseService.CollectionExistsAsync(DashboardsCollectionName))
                {
                    return 0;
                }

                // Get count
                var dashboards = await _databaseService.GetByFilterAsync<Dashboard>(
                    DashboardsCollectionName,
                    d => d.AccountId == accountId);

                return dashboards.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard count for account: {AccountId}", accountId);
                throw;
            }
        }
    }
}
