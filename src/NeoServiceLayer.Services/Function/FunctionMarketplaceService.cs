using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Enums;

namespace NeoServiceLayer.Services.Function
{
    /// <summary>
    /// Service for managing function marketplace
    /// </summary>
    public class FunctionMarketplaceService : IFunctionMarketplaceService
    {
        private readonly ILogger<FunctionMarketplaceService> _logger;
        private readonly IFunctionMarketplaceItemRepository _itemRepository;
        private readonly IFunctionMarketplaceReviewRepository _reviewRepository;
        private readonly IFunctionMarketplacePurchaseRepository _purchaseRepository;
        private readonly IFunctionRepository _functionRepository;
        private readonly IFunctionService _functionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionMarketplaceService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="itemRepository">Item repository</param>
        /// <param name="reviewRepository">Review repository</param>
        /// <param name="purchaseRepository">Purchase repository</param>
        /// <param name="functionRepository">Function repository</param>
        /// <param name="functionService">Function service</param>
        public FunctionMarketplaceService(
            ILogger<FunctionMarketplaceService> logger,
            IFunctionMarketplaceItemRepository itemRepository,
            IFunctionMarketplaceReviewRepository reviewRepository,
            IFunctionMarketplacePurchaseRepository purchaseRepository,
            IFunctionRepository functionRepository,
            IFunctionService functionService)
        {
            _logger = logger;
            _itemRepository = itemRepository;
            _reviewRepository = reviewRepository;
            _purchaseRepository = purchaseRepository;
            _functionRepository = functionRepository;
            _functionService = functionService;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> CreateItemAsync(FunctionMarketplaceItem item)
        {
            _logger.LogInformation("Creating marketplace item for function {FunctionId}", item.FunctionId);

            try
            {
                // Check if the function exists
                var function = await _functionRepository.GetByIdAsync(item.FunctionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {item.FunctionId}");
                }

                // Check if an item already exists for this function
                var existingItem = await _itemRepository.GetByFunctionIdAsync(item.FunctionId);
                if (existingItem != null)
                {
                    throw new Exception($"Marketplace item already exists for function: {item.FunctionId}");
                }

                // Set default values
                item.Id = Guid.NewGuid();
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
                item.IsPublished = false;
                item.IsFeatured = false;
                item.IsVerified = false;
                item.Rating = 0;
                item.RatingCount = 0;
                item.DownloadCount = 0;

                return await _itemRepository.CreateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating marketplace item for function {FunctionId}", item.FunctionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> UpdateItemAsync(FunctionMarketplaceItem item)
        {
            _logger.LogInformation("Updating marketplace item {ItemId}", item.Id);

            try
            {
                // Check if the item exists
                var existingItem = await _itemRepository.GetByIdAsync(item.Id);
                if (existingItem == null)
                {
                    throw new Exception($"Marketplace item not found: {item.Id}");
                }

                // Preserve certain fields that should not be updated directly
                item.FunctionId = existingItem.FunctionId;
                item.PublisherId = existingItem.PublisherId;
                item.CreatedAt = existingItem.CreatedAt;
                item.PublishedAt = existingItem.PublishedAt;
                item.IsPublished = existingItem.IsPublished;
                item.IsFeatured = existingItem.IsFeatured;
                item.IsVerified = existingItem.IsVerified;
                item.Rating = existingItem.Rating;
                item.RatingCount = existingItem.RatingCount;
                item.DownloadCount = existingItem.DownloadCount;

                // Update timestamp
                item.UpdatedAt = DateTime.UtcNow;

                return await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating marketplace item {ItemId}", item.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> GetItemByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting marketplace item {ItemId}", id);

            try
            {
                return await _itemRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace item {ItemId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> GetItemByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Getting marketplace item by function ID {FunctionId}", functionId);

            try
            {
                return await _itemRepository.GetByFunctionIdAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace item by function ID {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetItemsByPublisherIdAsync(Guid publisherId)
        {
            _logger.LogInformation("Getting marketplace items by publisher ID {PublisherId}", publisherId);

            try
            {
                return await _itemRepository.GetByPublisherIdAsync(publisherId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace items by publisher ID {PublisherId}", publisherId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetItemsByCategoryAsync(string category)
        {
            _logger.LogInformation("Getting marketplace items by category {Category}", category);

            try
            {
                return await _itemRepository.GetByCategoryAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace items by category {Category}", category);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetItemsByTagsAsync(List<string> tags)
        {
            _logger.LogInformation("Getting marketplace items by tags {Tags}", string.Join(", ", tags));

            try
            {
                return await _itemRepository.GetByTagsAsync(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace items by tags {Tags}", string.Join(", ", tags));
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetFeaturedItemsAsync(int limit = 10)
        {
            _logger.LogInformation("Getting featured marketplace items, limit: {Limit}", limit);

            try
            {
                return await _itemRepository.GetFeaturedAsync(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured marketplace items");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetPopularItemsAsync(int limit = 10)
        {
            _logger.LogInformation("Getting popular marketplace items, limit: {Limit}", limit);

            try
            {
                return await _itemRepository.GetByDownloadCountAsync(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular marketplace items");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetNewItemsAsync(int limit = 10)
        {
            _logger.LogInformation("Getting new marketplace items, limit: {Limit}", limit);

            try
            {
                return await _itemRepository.GetByPublishedDateAsync(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new marketplace items");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> SearchItemsAsync(string query = null, string category = null, List<string> tags = null, decimal? minPrice = null, decimal? maxPrice = null, bool? isFree = null, bool? isVerified = null, string sortBy = null, string sortOrder = null, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Searching marketplace items, query: {Query}, category: {Category}, tags: {Tags}, limit: {Limit}, offset: {Offset}", query, category, tags != null ? string.Join(", ", tags) : null, limit, offset);

            try
            {
                return await _itemRepository.SearchAsync(query, category, tags, minPrice, maxPrice, isFree, isVerified, sortBy, sortOrder, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching marketplace items");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> PublishItemAsync(Guid id)
        {
            _logger.LogInformation("Publishing marketplace item {ItemId}", id);

            try
            {
                // Get the item
                var item = await _itemRepository.GetByIdAsync(id);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {id}");
                }

                // Update the item
                item.IsPublished = true;
                item.PublishedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;

                return await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing marketplace item {ItemId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> UnpublishItemAsync(Guid id)
        {
            _logger.LogInformation("Unpublishing marketplace item {ItemId}", id);

            try
            {
                // Get the item
                var item = await _itemRepository.GetByIdAsync(id);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {id}");
                }

                // Update the item
                item.IsPublished = false;
                item.UpdatedAt = DateTime.UtcNow;

                return await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing marketplace item {ItemId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> VerifyItemAsync(Guid id)
        {
            _logger.LogInformation("Verifying marketplace item {ItemId}", id);

            try
            {
                // Get the item
                var item = await _itemRepository.GetByIdAsync(id);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {id}");
                }

                // Update the item
                item.IsVerified = true;
                item.UpdatedAt = DateTime.UtcNow;

                return await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying marketplace item {ItemId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> UnverifyItemAsync(Guid id)
        {
            _logger.LogInformation("Unverifying marketplace item {ItemId}", id);

            try
            {
                // Get the item
                var item = await _itemRepository.GetByIdAsync(id);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {id}");
                }

                // Update the item
                item.IsVerified = false;
                item.UpdatedAt = DateTime.UtcNow;

                return await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unverifying marketplace item {ItemId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> FeatureItemAsync(Guid id)
        {
            _logger.LogInformation("Featuring marketplace item {ItemId}", id);

            try
            {
                // Get the item
                var item = await _itemRepository.GetByIdAsync(id);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {id}");
                }

                // Update the item
                item.IsFeatured = true;
                item.UpdatedAt = DateTime.UtcNow;

                return await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error featuring marketplace item {ItemId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceItem> UnfeatureItemAsync(Guid id)
        {
            _logger.LogInformation("Unfeaturing marketplace item {ItemId}", id);

            try
            {
                // Get the item
                var item = await _itemRepository.GetByIdAsync(id);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {id}");
                }

                // Update the item
                item.IsFeatured = false;
                item.UpdatedAt = DateTime.UtcNow;

                return await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfeaturing marketplace item {ItemId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteItemAsync(Guid id)
        {
            _logger.LogInformation("Deleting marketplace item {ItemId}", id);

            try
            {
                // Delete reviews first
                await _reviewRepository.DeleteByItemIdAsync(id);

                // Delete purchases
                await _purchaseRepository.DeleteByItemIdAsync(id);

                // Delete the item
                return await _itemRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting marketplace item {ItemId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> CreateReviewAsync(FunctionMarketplaceReview review)
        {
            _logger.LogInformation("Creating review for marketplace item {ItemId} by reviewer {ReviewerId}", review.ItemId, review.ReviewerId);

            try
            {
                // Check if the item exists
                var item = await _itemRepository.GetByIdAsync(review.ItemId);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {review.ItemId}");
                }

                // Check if the reviewer has already reviewed this item
                var existingReviews = await _reviewRepository.GetByItemIdAsync(review.ItemId);
                var existingReview = existingReviews.FirstOrDefault(r => r.ReviewerId == review.ReviewerId);
                if (existingReview != null)
                {
                    throw new Exception($"Reviewer {review.ReviewerId} has already reviewed item {review.ItemId}");
                }

                // Set default values
                review.Id = Guid.NewGuid();
                review.CreatedAt = DateTime.UtcNow;
                review.UpdatedAt = DateTime.UtcNow;
                review.IsVerified = false;
                review.HelpfulCount = 0;
                review.UnhelpfulCount = 0;
                review.IsHidden = false;

                // Create the review
                var createdReview = await _reviewRepository.CreateAsync(review);

                // Update the item's rating
                await UpdateItemRatingAsync(review.ItemId);

                return createdReview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for marketplace item {ItemId} by reviewer {ReviewerId}", review.ItemId, review.ReviewerId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> UpdateReviewAsync(FunctionMarketplaceReview review)
        {
            _logger.LogInformation("Updating review {ReviewId}", review.Id);

            try
            {
                // Check if the review exists
                var existingReview = await _reviewRepository.GetByIdAsync(review.Id);
                if (existingReview == null)
                {
                    throw new Exception($"Review not found: {review.Id}");
                }

                // Preserve certain fields that should not be updated directly
                review.ItemId = existingReview.ItemId;
                review.ReviewerId = existingReview.ReviewerId;
                review.CreatedAt = existingReview.CreatedAt;
                review.IsVerified = existingReview.IsVerified;
                review.HelpfulCount = existingReview.HelpfulCount;
                review.UnhelpfulCount = existingReview.UnhelpfulCount;

                // Update timestamp
                review.UpdatedAt = DateTime.UtcNow;

                // Update the review
                var updatedReview = await _reviewRepository.UpdateAsync(review);

                // Update the item's rating
                await UpdateItemRatingAsync(review.ItemId);

                return updatedReview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", review.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> GetReviewByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting review {ReviewId}", id);

            try
            {
                return await _reviewRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review {ReviewId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceReview>> GetReviewsByItemIdAsync(Guid itemId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting reviews for marketplace item {ItemId}, limit: {Limit}, offset: {Offset}", itemId, limit, offset);

            try
            {
                return await _reviewRepository.GetByItemIdAsync(itemId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for marketplace item {ItemId}", itemId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceReview>> GetReviewsByReviewerIdAsync(Guid reviewerId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting reviews by reviewer {ReviewerId}, limit: {Limit}, offset: {Offset}", reviewerId, limit, offset);

            try
            {
                return await _reviewRepository.GetByReviewerIdAsync(reviewerId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews by reviewer {ReviewerId}", reviewerId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteReviewAsync(Guid id)
        {
            _logger.LogInformation("Deleting review {ReviewId}", id);

            try
            {
                // Get the review to get the item ID
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                {
                    throw new Exception($"Review not found: {id}");
                }

                // Delete the review
                var result = await _reviewRepository.DeleteAsync(id);

                // Update the item's rating
                await UpdateItemRatingAsync(review.ItemId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> HideReviewAsync(Guid id, string reason)
        {
            _logger.LogInformation("Hiding review {ReviewId}, reason: {Reason}", id, reason);

            try
            {
                // Get the review
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                {
                    throw new Exception($"Review not found: {id}");
                }

                // Update the review
                review.IsHidden = true;
                review.HiddenReason = reason;
                review.UpdatedAt = DateTime.UtcNow;

                // Update the review
                var updatedReview = await _reviewRepository.UpdateAsync(review);

                // Update the item's rating
                await UpdateItemRatingAsync(review.ItemId);

                return updatedReview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hiding review {ReviewId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> UnhideReviewAsync(Guid id)
        {
            _logger.LogInformation("Unhiding review {ReviewId}", id);

            try
            {
                // Get the review
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                {
                    throw new Exception($"Review not found: {id}");
                }

                // Update the review
                review.IsHidden = false;
                review.HiddenReason = null;
                review.UpdatedAt = DateTime.UtcNow;

                // Update the review
                var updatedReview = await _reviewRepository.UpdateAsync(review);

                // Update the item's rating
                await UpdateItemRatingAsync(review.ItemId);

                return updatedReview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unhiding review {ReviewId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> VoteReviewHelpfulAsync(Guid id, Guid userId)
        {
            _logger.LogInformation("Voting review {ReviewId} as helpful by user {UserId}", id, userId);

            try
            {
                // Get the review
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                {
                    throw new Exception($"Review not found: {id}");
                }

                // Update the review
                review.HelpfulCount++;
                review.UpdatedAt = DateTime.UtcNow;

                return await _reviewRepository.UpdateAsync(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting review {ReviewId} as helpful by user {UserId}", id, userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> VoteReviewUnhelpfulAsync(Guid id, Guid userId)
        {
            _logger.LogInformation("Voting review {ReviewId} as unhelpful by user {UserId}", id, userId);

            try
            {
                // Get the review
                var review = await _reviewRepository.GetByIdAsync(id);
                if (review == null)
                {
                    throw new Exception($"Review not found: {id}");
                }

                // Update the review
                review.UnhelpfulCount++;
                review.UpdatedAt = DateTime.UtcNow;

                return await _reviewRepository.UpdateAsync(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting review {ReviewId} as unhelpful by user {UserId}", id, userId);
                throw;
            }
        }

        /// <summary>
        /// Updates the rating of a marketplace item based on its reviews
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The updated item</returns>
        private async Task<FunctionMarketplaceItem> UpdateItemRatingAsync(Guid itemId)
        {
            try
            {
                // Get the item
                var item = await _itemRepository.GetByIdAsync(itemId);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {itemId}");
                }

                // Get the average rating and rating count
                var averageRating = await _reviewRepository.GetAverageRatingAsync(itemId);
                var ratingCount = await _reviewRepository.GetRatingCountAsync(itemId);

                // Update the item
                item.Rating = averageRating;
                item.RatingCount = ratingCount;
                item.UpdatedAt = DateTime.UtcNow;

                return await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating for marketplace item {ItemId}", itemId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> CreatePurchaseAsync(FunctionMarketplacePurchase purchase)
        {
            _logger.LogInformation("Creating purchase for marketplace item {ItemId} by buyer {BuyerId}", purchase.ItemId, purchase.BuyerId);

            try
            {
                // Check if the item exists
                var item = await _itemRepository.GetByIdAsync(purchase.ItemId);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {purchase.ItemId}");
                }

                // Set default values
                purchase.Id = Guid.NewGuid();
                purchase.PurchasedAt = DateTime.UtcNow;
                purchase.IsRefunded = false;
                purchase.RefundedAt = null;
                purchase.RefundReason = null;
                purchase.RefundTransactionId = null;
                purchase.LicenseKey = GenerateLicenseKey();
                purchase.IsPerpetualLicense = true;
                purchase.ItemVersion = item.Version;

                // Create the purchase
                var createdPurchase = await _purchaseRepository.CreateAsync(purchase);

                // Update the item's download count
                await UpdateItemDownloadCountAsync(purchase.ItemId);

                return createdPurchase;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase for marketplace item {ItemId} by buyer {BuyerId}", purchase.ItemId, purchase.BuyerId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> UpdatePurchaseAsync(FunctionMarketplacePurchase purchase)
        {
            _logger.LogInformation("Updating purchase {PurchaseId}", purchase.Id);

            try
            {
                // Check if the purchase exists
                var existingPurchase = await _purchaseRepository.GetByIdAsync(purchase.Id);
                if (existingPurchase == null)
                {
                    throw new Exception($"Purchase not found: {purchase.Id}");
                }

                // Preserve certain fields that should not be updated directly
                purchase.ItemId = existingPurchase.ItemId;
                purchase.BuyerId = existingPurchase.BuyerId;
                purchase.PurchasedAt = existingPurchase.PurchasedAt;
                purchase.TransactionId = existingPurchase.TransactionId;
                purchase.LicenseKey = existingPurchase.LicenseKey;
                purchase.ItemVersion = existingPurchase.ItemVersion;

                return await _purchaseRepository.UpdateAsync(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase {PurchaseId}", purchase.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> GetPurchaseByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting purchase {PurchaseId}", id);

            try
            {
                return await _purchaseRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase {PurchaseId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplacePurchase>> GetPurchasesByItemIdAsync(Guid itemId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting purchases for marketplace item {ItemId}, limit: {Limit}, offset: {Offset}", itemId, limit, offset);

            try
            {
                return await _purchaseRepository.GetByItemIdAsync(itemId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchases for marketplace item {ItemId}", itemId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplacePurchase>> GetPurchasesByBuyerIdAsync(Guid buyerId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting purchases by buyer {BuyerId}, limit: {Limit}, offset: {Offset}", buyerId, limit, offset);

            try
            {
                return await _purchaseRepository.GetByBuyerIdAsync(buyerId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchases by buyer {BuyerId}", buyerId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> RefundPurchaseAsync(Guid id, string reason, string refundTransactionId)
        {
            _logger.LogInformation("Refunding purchase {PurchaseId}, reason: {Reason}, refund transaction ID: {RefundTransactionId}", id, reason, refundTransactionId);

            try
            {
                // Get the purchase
                var purchase = await _purchaseRepository.GetByIdAsync(id);
                if (purchase == null)
                {
                    throw new Exception($"Purchase not found: {id}");
                }

                // Check if the purchase is already refunded
                if (purchase.IsRefunded)
                {
                    throw new Exception($"Purchase {id} is already refunded");
                }

                // Update the purchase
                purchase.IsRefunded = true;
                purchase.RefundedAt = DateTime.UtcNow;
                purchase.RefundReason = reason;
                purchase.RefundTransactionId = refundTransactionId;

                return await _purchaseRepository.UpdateAsync(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding purchase {PurchaseId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HasUserPurchasedItemAsync(Guid itemId, Guid userId)
        {
            _logger.LogInformation("Checking if user {UserId} has purchased item {ItemId}", userId, itemId);

            try
            {
                // Get purchases by item ID and buyer ID
                var purchases = await _purchaseRepository.GetByItemIdAndBuyerIdAsync(itemId, userId);

                // Check if there are any non-refunded purchases
                return purchases.Any(p => !p.IsRefunded);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} has purchased item {ItemId}", userId, itemId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetLicenseKeyAsync(Guid purchaseId)
        {
            _logger.LogInformation("Getting license key for purchase {PurchaseId}", purchaseId);

            try
            {
                // Get the purchase
                var purchase = await _purchaseRepository.GetByIdAsync(purchaseId);
                if (purchase == null)
                {
                    throw new Exception($"Purchase not found: {purchaseId}");
                }

                // Check if the purchase is refunded
                if (purchase.IsRefunded)
                {
                    throw new Exception($"Purchase {purchaseId} is refunded");
                }

                // Check if the license is expired
                if (purchase.LicenseExpiresAt.HasValue && purchase.LicenseExpiresAt.Value < DateTime.UtcNow)
                {
                    throw new Exception($"License for purchase {purchaseId} is expired");
                }

                return purchase.LicenseKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting license key for purchase {PurchaseId}", purchaseId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateLicenseKeyAsync(string licenseKey)
        {
            _logger.LogInformation("Validating license key {LicenseKey}", licenseKey);

            try
            {
                // Get the purchase by license key
                var purchase = await _purchaseRepository.GetByLicenseKeyAsync(licenseKey);
                if (purchase == null)
                {
                    return false;
                }

                // Check if the purchase is refunded
                if (purchase.IsRefunded)
                {
                    return false;
                }

                // Check if the license is expired
                if (purchase.LicenseExpiresAt.HasValue && purchase.LicenseExpiresAt.Value < DateTime.UtcNow)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating license key {LicenseKey}", licenseKey);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> GetPurchaseByLicenseKeyAsync(string licenseKey)
        {
            _logger.LogInformation("Getting purchase by license key {LicenseKey}", licenseKey);

            try
            {
                return await _purchaseRepository.GetByLicenseKeyAsync(licenseKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase by license key {LicenseKey}", licenseKey);
                throw;
            }
        }

        /// <summary>
        /// Updates the download count of a marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The updated item</returns>
        private async Task<FunctionMarketplaceItem> UpdateItemDownloadCountAsync(Guid itemId)
        {
            try
            {
                // Get the item
                var item = await _itemRepository.GetByIdAsync(itemId);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {itemId}");
                }

                // Get the purchase count
                var purchaseCount = await _purchaseRepository.GetPurchaseCountAsync(itemId);

                // Update the item
                item.DownloadCount = purchaseCount;
                item.UpdatedAt = DateTime.UtcNow;

                return await _itemRepository.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating download count for marketplace item {ItemId}", itemId);
                throw;
            }
        }

        /// <summary>
        /// Generates a license key
        /// </summary>
        /// <returns>The generated license key</returns>
        private string GenerateLicenseKey()
        {
            // Generate a random license key
            var key = Guid.NewGuid().ToString("N").ToUpper();
            var parts = new List<string>();
            for (var i = 0; i < key.Length; i += 5)
            {
                parts.Add(key.Substring(i, Math.Min(5, key.Length - i)));
            }
            return string.Join("-", parts);
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> InstallItemAsync(Guid itemId, Guid userId)
        {
            _logger.LogInformation("Installing marketplace item {ItemId} for user {UserId}", itemId, userId);

            try
            {
                // Check if the item exists
                var item = await _itemRepository.GetByIdAsync(itemId);
                if (item == null)
                {
                    throw new Exception($"Marketplace item not found: {itemId}");
                }

                // Check if the user has purchased the item
                var hasPurchased = await HasUserPurchasedItemAsync(itemId, userId);
                if (!item.IsFree && !hasPurchased)
                {
                    throw new Exception($"User {userId} has not purchased item {itemId}");
                }

                // Get the function
                var function = await _functionRepository.GetByIdAsync(item.FunctionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {item.FunctionId}");
                }

                // Create a copy of the function for the user
                var newFunction = new Core.Models.Function
                {
                    Id = Guid.NewGuid(),
                    Name = function.Name,
                    Description = function.Description,
                    AccountId = userId,
                    Runtime = function.Runtime,
                    Source = function.Source,
                    Version = function.Version,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Tags = function.Tags,
                    MaxMemory = function.MaxMemory,
                    MaxExecutionTime = function.MaxExecutionTime,
                    EnvironmentVariables = function.EnvironmentVariables,
                    Dependencies = function.Dependencies,
                    Metadata = new Dictionary<string, object>
                    {
                        { "InstalledFrom", itemId.ToString() },
                        { "OriginalFunctionId", function.Id.ToString() },
                        { "InstalledAt", DateTime.UtcNow.ToString("o") }
                    }
                };

                // Create the function
                return await _functionService.CreateFunctionAsync(
                    newFunction.Name,
                    newFunction.Description,
                    (FunctionRuntime)Enum.Parse(typeof(FunctionRuntime), newFunction.Runtime),
                    newFunction.SourceCode,
                    newFunction.EntryPoint,
                    newFunction.AccountId,
                    newFunction.MaxExecutionTime,
                    (int)newFunction.MaxMemory,
                    newFunction.SecretIds,
                    newFunction.EnvironmentVariables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing marketplace item {ItemId} for user {UserId}", itemId, userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UninstallItemAsync(Guid itemId, Guid userId)
        {
            _logger.LogInformation("Uninstalling marketplace item {ItemId} for user {UserId}", itemId, userId);

            try
            {
                // Get all functions for the user
                var functions = await _functionService.GetByAccountIdAsync(userId);

                // Find functions installed from this item
                var installedFunctions = functions.Where(f =>
                    f.Metadata != null &&
                    f.Metadata.TryGetValue("InstalledFrom", out var installedFrom) &&
                    installedFrom.ToString() == itemId.ToString());

                // Delete each function
                var success = true;
                foreach (var function in installedFunctions)
                {
                    var result = await _functionService.DeleteAsync(function.Id);
                    if (!result)
                    {
                        success = false;
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uninstalling marketplace item {ItemId} for user {UserId}", itemId, userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceItem>> GetInstalledItemsAsync(Guid userId)
        {
            _logger.LogInformation("Getting installed marketplace items for user {UserId}", userId);

            try
            {
                // Get all functions for the user
                var functions = await _functionService.GetByAccountIdAsync(userId);

                // Find functions installed from marketplace items
                var installedFunctionsByItem = functions
                    .Where(f => f.Metadata != null && f.Metadata.ContainsKey("InstalledFrom"))
                    .GroupBy(f => Guid.Parse(f.Metadata["InstalledFrom"].ToString()));

                // Get the items
                var items = new List<FunctionMarketplaceItem>();
                foreach (var group in installedFunctionsByItem)
                {
                    var item = await _itemRepository.GetByIdAsync(group.Key);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting installed marketplace items for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsItemInstalledAsync(Guid itemId, Guid userId)
        {
            _logger.LogInformation("Checking if marketplace item {ItemId} is installed for user {UserId}", itemId, userId);

            try
            {
                // Get all functions for the user
                var functions = await _functionService.GetByAccountIdAsync(userId);

                // Check if any function is installed from this item
                return functions.Any(f =>
                    f.Metadata != null &&
                    f.Metadata.TryGetValue("InstalledFrom", out var installedFrom) &&
                    installedFrom.ToString() == itemId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if marketplace item {ItemId} is installed for user {UserId}", itemId, userId);
                throw;
            }
        }
    }
}
