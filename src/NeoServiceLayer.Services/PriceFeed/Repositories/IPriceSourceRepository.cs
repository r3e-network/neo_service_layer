using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.PriceFeed.Repositories
{
    /// <summary>
    /// Interface for price source repository
    /// </summary>
    public interface IPriceSourceRepository
    {
        /// <summary>
        /// Creates a new price source
        /// </summary>
        /// <param name="source">Price source to create</param>
        /// <returns>The created price source</returns>
        Task<PriceSource> CreateAsync(PriceSource source);

        /// <summary>
        /// Gets a price source by ID
        /// </summary>
        /// <param name="id">Price source ID</param>
        /// <returns>The price source if found, null otherwise</returns>
        Task<PriceSource> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a price source by name
        /// </summary>
        /// <param name="name">Price source name</param>
        /// <returns>The price source if found, null otherwise</returns>
        Task<PriceSource> GetByNameAsync(string name);

        /// <summary>
        /// Gets all price sources
        /// </summary>
        /// <returns>List of all price sources</returns>
        Task<IEnumerable<PriceSource>> GetAllAsync();

        /// <summary>
        /// Gets active price sources
        /// </summary>
        /// <returns>List of active price sources</returns>
        Task<IEnumerable<PriceSource>> GetActiveSourcesAsync();

        /// <summary>
        /// Gets price sources by type
        /// </summary>
        /// <param name="type">Price source type</param>
        /// <returns>List of price sources of the specified type</returns>
        Task<IEnumerable<PriceSource>> GetByTypeAsync(PriceSourceType type);

        /// <summary>
        /// Gets price sources that support a specific asset
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <returns>List of price sources that support the asset</returns>
        Task<IEnumerable<PriceSource>> GetByAssetAsync(string symbol);

        /// <summary>
        /// Updates a price source
        /// </summary>
        /// <param name="source">Price source to update</param>
        /// <returns>The updated price source</returns>
        Task<PriceSource> UpdateAsync(PriceSource source);

        /// <summary>
        /// Deletes a price source
        /// </summary>
        /// <param name="id">Price source ID</param>
        /// <returns>True if price source was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
