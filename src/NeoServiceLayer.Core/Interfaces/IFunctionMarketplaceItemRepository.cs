using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function marketplace item repository
    /// </summary>
    public interface IFunctionMarketplaceItemRepository
    {
        /// <summary>
        /// Creates a new function marketplace item
        /// </summary>
        /// <param name="item">Function marketplace item to create</param>
        /// <returns>The created function marketplace item</returns>
        Task<FunctionMarketplaceItem> CreateAsync(FunctionMarketplaceItem item);

        /// <summary>
        /// Updates a function marketplace item
        /// </summary>
        /// <param name="item">Function marketplace item to update</param>
        /// <returns>The updated function marketplace item</returns>
        Task<FunctionMarketplaceItem> UpdateAsync(FunctionMarketplaceItem item);

        /// <summary>
        /// Gets a function marketplace item by ID
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The function marketplace item if found, null otherwise</returns>
        Task<FunctionMarketplaceItem> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a function marketplace item by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>The function marketplace item if found, null otherwise</returns>
        Task<FunctionMarketplaceItem> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function marketplace items by publisher ID
        /// </summary>
        /// <param name="publisherId">Publisher ID</param>
        /// <returns>List of function marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetByPublisherIdAsync(Guid publisherId);

        /// <summary>
        /// Gets function marketplace items by category
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>List of function marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetByCategoryAsync(string category);

        /// <summary>
        /// Gets function marketplace items by tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>List of function marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetByTagsAsync(List<string> tags);

        /// <summary>
        /// Gets featured function marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of featured function marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetFeaturedAsync(int limit = 10);

        /// <summary>
        /// Gets function marketplace items sorted by download count
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of function marketplace items sorted by download count</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetByDownloadCountAsync(int limit = 10);

        /// <summary>
        /// Gets function marketplace items sorted by rating
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of function marketplace items sorted by rating</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetByRatingAsync(int limit = 10);

        /// <summary>
        /// Gets function marketplace items sorted by published date
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of function marketplace items sorted by published date</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetByPublishedDateAsync(int limit = 10);

        /// <summary>
        /// Searches function marketplace items
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="category">Category filter</param>
        /// <param name="tags">Tags filter</param>
        /// <param name="minPrice">Minimum price filter</param>
        /// <param name="maxPrice">Maximum price filter</param>
        /// <param name="isFree">Free items filter</param>
        /// <param name="isVerified">Verified items filter</param>
        /// <param name="sortBy">Sort by field</param>
        /// <param name="sortOrder">Sort order</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace items matching the search criteria</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> SearchAsync(string query = null, string category = null, List<string> tags = null, decimal? minPrice = null, decimal? maxPrice = null, bool? isFree = null, bool? isVerified = null, string sortBy = null, string sortOrder = null, int limit = 10, int offset = 0);

        /// <summary>
        /// Deletes a function marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>True if the item was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function marketplace items by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the items were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Deletes function marketplace items by publisher ID
        /// </summary>
        /// <param name="publisherId">Publisher ID</param>
        /// <returns>True if the items were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByPublisherIdAsync(Guid publisherId);
    }
}
