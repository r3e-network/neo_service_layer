using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Repository for function marketplace items
    /// </summary>
    public class FunctionMarketplaceItemRepository : IFunctionMarketplaceItemRepository
    {
        private readonly ILogger<FunctionMarketplaceItemRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_marketplace_items";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionMarketplaceItemRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionMarketplaceItemRepository(ILogger<FunctionMarketplaceItemRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> CreateAsync(FunctionMarketplaceItem item)
        {
            _logger.LogInformation("Creating function marketplace item for function {FunctionId}", item.FunctionId);

            // Ensure ID is set
            if (item.Id == Guid.Empty)
            {
                item.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, item);

            return item;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> UpdateAsync(FunctionMarketplaceItem item)
        {
            _logger.LogInformation("Updating function marketplace item: {Id}", item.Id);

            // Update in store
            await _storageProvider.UpdateAsync<FunctionMarketplaceItem, Guid>(_collectionName, item.Id, item);

            return item;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> UpdateAsync(Guid id, FunctionMarketplaceItem item)
        {
            _logger.LogInformation("Updating function marketplace item: {Id}", id);

            // Ensure the ID matches
            item.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionMarketplaceItem, Guid>(_collectionName, id, item);

            return item;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function marketplace item by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionMarketplaceItem, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> GetByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Getting function marketplace item by function ID: {FunctionId}", functionId);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by function ID
            return items.FirstOrDefault(i => i.FunctionId == functionId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetBySellerIdAsync(Guid sellerId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace items by seller ID: {SellerId}, limit: {Limit}, offset: {Offset}", sellerId, limit, offset);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by seller ID and apply pagination
            return items
                .Where(i => i.PublisherId == sellerId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetByPublisherIdAsync(Guid publisherId)
        {
            _logger.LogInformation("Getting function marketplace items by publisher ID: {PublisherId}", publisherId);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by publisher ID
            return items.Where(i => i.PublisherId == publisherId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetByCategoryAsync(string category, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace items by category: {Category}, limit: {Limit}, offset: {Offset}", category, limit, offset);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by category and apply pagination
            return items
                .Where(i => i.Category == category)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetByTagsAsync(List<string> tags, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace items by tags: {Tags}, limit: {Limit}, offset: {Offset}", string.Join(", ", tags), limit, offset);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by tags and apply pagination
            return items
                .Where(i => i.Tags.Any(t => tags.Contains(t, StringComparer.OrdinalIgnoreCase)))
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetFeaturedAsync(int limit = 10)
        {
            _logger.LogInformation("Getting featured function marketplace items, limit: {Limit}", limit);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by featured and published
            return items.Where(i => i.IsFeatured && i.IsPublished).Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetByDownloadCountAsync(int limit = 10)
        {
            _logger.LogInformation("Getting function marketplace items by download count, limit: {Limit}", limit);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by published and sort by download count
            return items.Where(i => i.IsPublished).OrderByDescending(i => i.DownloadCount).Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetByRatingAsync(int limit = 10)
        {
            _logger.LogInformation("Getting function marketplace items by rating, limit: {Limit}", limit);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by published and sort by rating
            return items.Where(i => i.IsPublished).OrderByDescending(i => i.Rating).Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetByPublishedDateAsync(int limit = 10)
        {
            _logger.LogInformation("Getting function marketplace items by published date, limit: {Limit}", limit);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by published and sort by published date
            return items.Where(i => i.IsPublished).OrderByDescending(i => i.PublishedAt).Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> SearchAsync(string query = null, string category = null, List<string> tags = null, decimal? minPrice = null, decimal? maxPrice = null, bool? isFree = null, bool? isVerified = null, string sortBy = null, string sortOrder = null, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Searching function marketplace items, query: {Query}, category: {Category}, tags: {Tags}, limit: {Limit}, offset: {Offset}", query, category, tags != null ? string.Join(", ", tags) : null, limit, offset);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by published
            var filteredItems = items.Where(i => i.IsPublished);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query))
            {
                filteredItems = filteredItems.Where(i =>
                    i.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    i.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    i.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                filteredItems = filteredItems.Where(i => i.Category == category);
            }

            if (tags != null && tags.Any())
            {
                filteredItems = filteredItems.Where(i => i.Tags.Any(t => tags.Contains(t, StringComparer.OrdinalIgnoreCase)));
            }

            if (minPrice.HasValue)
            {
                filteredItems = filteredItems.Where(i => i.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                filteredItems = filteredItems.Where(i => i.Price <= maxPrice.Value);
            }

            if (isFree.HasValue)
            {
                filteredItems = filteredItems.Where(i => i.IsFree == isFree.Value);
            }

            if (isVerified.HasValue)
            {
                filteredItems = filteredItems.Where(i => i.IsVerified == isVerified.Value);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var isDescending = sortOrder?.ToLower() == "desc";

                switch (sortBy.ToLower())
                {
                    case "name":
                        filteredItems = isDescending
                            ? filteredItems.OrderByDescending(i => i.Name)
                            : filteredItems.OrderBy(i => i.Name);
                        break;

                    case "price":
                        filteredItems = isDescending
                            ? filteredItems.OrderByDescending(i => i.Price)
                            : filteredItems.OrderBy(i => i.Price);
                        break;

                    case "rating":
                        filteredItems = isDescending
                            ? filteredItems.OrderByDescending(i => i.Rating)
                            : filteredItems.OrderBy(i => i.Rating);
                        break;

                    case "downloads":
                        filteredItems = isDescending
                            ? filteredItems.OrderByDescending(i => i.DownloadCount)
                            : filteredItems.OrderBy(i => i.DownloadCount);
                        break;

                    case "published":
                        filteredItems = isDescending
                            ? filteredItems.OrderByDescending(i => i.PublishedAt)
                            : filteredItems.OrderBy(i => i.PublishedAt);
                        break;

                    default:
                        filteredItems = isDescending
                            ? filteredItems.OrderByDescending(i => i.PublishedAt)
                            : filteredItems.OrderBy(i => i.PublishedAt);
                        break;
                }
            }
            else
            {
                // Default sorting by published date
                filteredItems = filteredItems.OrderByDescending(i => i.PublishedAt);
            }

            // Apply pagination
            return filteredItems.Skip(offset).Take(limit);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function marketplace item: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionMarketplaceItem, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Deleting function marketplace items by function ID: {FunctionId}", functionId);

            // Get item by function ID
            var item = await GetByFunctionIdAsync(functionId);
            if (item == null)
            {
                return true;
            }

            // Delete the item
            return await DeleteAsync(item.Id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByPublisherIdAsync(Guid publisherId)
        {
            _logger.LogInformation("Deleting function marketplace items by publisher ID: {PublisherId}", publisherId);

            // Get items by publisher ID
            var items = await GetByPublisherIdAsync(publisherId);

            // Delete each item
            var success = true;
            foreach (var item in items)
            {
                var result = await DeleteAsync(item.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace items by price range: {MinPrice} to {MaxPrice}, limit: {Limit}, offset: {Offset}", minPrice, maxPrice, limit, offset);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by price range and apply pagination
            return items
                .Where(i => i.Price >= minPrice && i.Price <= maxPrice)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetFreeItemsAsync(int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting free function marketplace items, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by free and apply pagination
            return items
                .Where(i => i.IsFree)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetVerifiedItemsAsync(int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting verified function marketplace items, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by verified and apply pagination
            return items
                .Where(i => i.IsVerified)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetFeaturedItemsAsync(int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting featured function marketplace items, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Filter by featured and apply pagination
            return items
                .Where(i => i.IsFeatured)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetAllAsync(int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting all function marketplace items, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all items
            var items = await _storageProvider.GetAllAsync<FunctionMarketplaceItem>(_collectionName);

            // Apply pagination
            return items
                .Skip(offset)
                .Take(limit);
        }
    }
}
