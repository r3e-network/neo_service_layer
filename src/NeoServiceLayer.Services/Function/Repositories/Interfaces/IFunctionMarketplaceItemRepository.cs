using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function marketplace item repository
    /// </summary>
    public interface IFunctionMarketplaceItemRepository
    {
        /// <summary>
        /// Gets a function marketplace item by ID
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The function marketplace item if found, null otherwise</returns>
        Task<FunctionMarketplaceItem> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function marketplace items by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>The function marketplace item for the specified function</returns>
        Task<FunctionMarketplaceItem> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function marketplace items by seller ID
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace items for the specified seller</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetBySellerIdAsync(Guid sellerId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace items by category
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace items in the specified category</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetByCategoryAsync(string category, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace items by tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace items with the specified tags</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetByTagsAsync(List<string> tags, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace items by price range
        /// </summary>
        /// <param name="minPrice">Minimum price</param>
        /// <param name="maxPrice">Maximum price</param>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace items in the specified price range</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets free function marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of free function marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetFreeItemsAsync(int limit = 10, int offset = 0);

        /// <summary>
        /// Gets verified function marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of verified function marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetVerifiedItemsAsync(int limit = 10, int offset = 0);

        /// <summary>
        /// Gets featured function marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of featured function marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetFeaturedItemsAsync(int limit = 10, int offset = 0);

        /// <summary>
        /// Searches for function marketplace items
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
        /// Creates a function marketplace item
        /// </summary>
        /// <param name="item">Item to create</param>
        /// <returns>The created function marketplace item</returns>
        Task<FunctionMarketplaceItem> CreateAsync(FunctionMarketplaceItem item);

        /// <summary>
        /// Updates a function marketplace item
        /// </summary>
        /// <param name="item">Updated item</param>
        /// <returns>The updated function marketplace item</returns>
        Task<FunctionMarketplaceItem> UpdateAsync(FunctionMarketplaceItem item);

        /// <summary>
        /// Deletes a function marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>True if the item was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetAllAsync(int limit = 10, int offset = 0);
    }
}
