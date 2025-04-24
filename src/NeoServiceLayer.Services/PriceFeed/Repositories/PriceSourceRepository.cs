using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Storage.Repositories;

namespace NeoServiceLayer.Services.PriceFeed.Repositories
{
    /// <summary>
    /// Implementation of the price source repository
    /// </summary>
    public class PriceSourceRepository : BaseRepository<PriceSource, Guid>, IPriceSourceRepository
    {
        private readonly ILogger<PriceSourceRepository> _logger;
        private readonly IStorageService _storageService;
        private readonly Guid _systemAccountId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceSourceRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageService">Storage service</param>
        public PriceSourceRepository(
            ILogger<PriceSourceRepository> logger,
            IStorageService storageService)
            : base(logger, storageService, Guid.Parse("00000000-0000-0000-0000-000000000001"), "priceSources")
        {
            _logger = logger;
            _storageService = storageService;
            _systemAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        /// <inheritdoc/>
        protected override Guid GetId(PriceSource entity)
        {
            return entity.Id;
        }

        /// <inheritdoc/>
        protected override void SetId(PriceSource entity, Guid id)
        {
            entity.Id = id;
        }

        /// <inheritdoc/>
        public override async Task<PriceSource> CreateAsync(PriceSource source)
        {
            _logger.LogInformation("Creating price source: {Id}, Name: {Name}, Type: {Type}", source.Id, source.Name, source.Type);

            if (source.Id == Guid.Empty)
            {
                source.Id = Guid.NewGuid();
            }

            source.CreatedAt = DateTime.UtcNow;
            source.UpdatedAt = DateTime.UtcNow;

            return await base.CreateAsync(source);
        }

        /// <inheritdoc/>
        public async Task<PriceSource> GetByNameAsync(string name)
        {
            _logger.LogInformation("Getting price source by name: {Name}", name);

            try
            {
                var sources = await GetAllAsync();
                return sources.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price source by name: {Name}", name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PriceSource>> GetActiveSourcesAsync()
        {
            _logger.LogInformation("Getting active price sources");

            try
            {
                var sources = await GetAllAsync();
                return sources.Where(s => s.Status == Core.Models.PriceSourceStatus.Active).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active price sources");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PriceSource>> GetByTypeAsync(PriceSourceType type)
        {
            _logger.LogInformation("Getting price sources by type: {Type}", type);

            try
            {
                var sources = await GetAllAsync();
                return sources.Where(s => s.Type == type).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price sources by type: {Type}", type);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PriceSource>> GetByAssetAsync(string symbol)
        {
            _logger.LogInformation("Getting price sources by asset: {Symbol}", symbol);

            try
            {
                var sources = await GetAllAsync();
                return sources.Where(s => s.SupportedAssets.Contains(symbol, StringComparer.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price sources by asset: {Symbol}", symbol);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<PriceSource> UpdateAsync(PriceSource source)
        {
            _logger.LogInformation("Updating price source: {Id}, Name: {Name}", source.Id, source.Name);

            try
            {
                // Check if source exists
                var exists = await ExistsAsync(source.Id);
                if (!exists)
                {
                    return null;
                }

                source.UpdatedAt = DateTime.UtcNow;
                return await base.UpdateAsync(source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating price source: {Id}", source.Id);
                throw;
            }
        }
    }
}
