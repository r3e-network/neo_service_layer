using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function marketplace service
    /// </summary>
    public interface IFunctionMarketplaceService
    {
        /// <summary>
        /// Creates a new marketplace item
        /// </summary>
        /// <param name="item">Marketplace item to create</param>
        /// <returns>The created marketplace item</returns>
        Task<FunctionMarketplaceItem> CreateItemAsync(FunctionMarketplaceItem item);

        /// <summary>
        /// Updates a marketplace item
        /// </summary>
        /// <param name="item">Marketplace item to update</param>
        /// <returns>The updated marketplace item</returns>
        Task<FunctionMarketplaceItem> UpdateItemAsync(FunctionMarketplaceItem item);

        /// <summary>
        /// Gets a marketplace item by ID
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The marketplace item if found, null otherwise</returns>
        Task<FunctionMarketplaceItem> GetItemByIdAsync(Guid id);

        /// <summary>
        /// Gets a marketplace item by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>The marketplace item if found, null otherwise</returns>
        Task<FunctionMarketplaceItem> GetItemByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets marketplace items by publisher ID
        /// </summary>
        /// <param name="publisherId">Publisher ID</param>
        /// <returns>List of marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetItemsByPublisherIdAsync(Guid publisherId);

        /// <summary>
        /// Gets marketplace items by category
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>List of marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetItemsByCategoryAsync(string category);

        /// <summary>
        /// Gets marketplace items by tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>List of marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetItemsByTagsAsync(List<string> tags);

        /// <summary>
        /// Gets featured marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of featured marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetFeaturedItemsAsync(int limit = 10);

        /// <summary>
        /// Gets popular marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of popular marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetPopularItemsAsync(int limit = 10);

        /// <summary>
        /// Gets new marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of new marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetNewItemsAsync(int limit = 10);

        /// <summary>
        /// Searches marketplace items
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
        /// <returns>List of marketplace items matching the search criteria</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> SearchItemsAsync(string query = null, string category = null, List<string> tags = null, decimal? minPrice = null, decimal? maxPrice = null, bool? isFree = null, bool? isVerified = null, string sortBy = null, string sortOrder = null, int limit = 10, int offset = 0);

        /// <summary>
        /// Publishes a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The published marketplace item</returns>
        Task<FunctionMarketplaceItem> PublishItemAsync(Guid id);

        /// <summary>
        /// Unpublishes a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The unpublished marketplace item</returns>
        Task<FunctionMarketplaceItem> UnpublishItemAsync(Guid id);

        /// <summary>
        /// Verifies a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The verified marketplace item</returns>
        Task<FunctionMarketplaceItem> VerifyItemAsync(Guid id);

        /// <summary>
        /// Unverifies a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The unverified marketplace item</returns>
        Task<FunctionMarketplaceItem> UnverifyItemAsync(Guid id);

        /// <summary>
        /// Features a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The featured marketplace item</returns>
        Task<FunctionMarketplaceItem> FeatureItemAsync(Guid id);

        /// <summary>
        /// Unfeatures a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The unfeatured marketplace item</returns>
        Task<FunctionMarketplaceItem> UnfeatureItemAsync(Guid id);

        /// <summary>
        /// Deletes a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>True if the item was deleted successfully, false otherwise</returns>
        Task<bool> DeleteItemAsync(Guid id);

        /// <summary>
        /// Creates a review for a marketplace item
        /// </summary>
        /// <param name="review">Review to create</param>
        /// <returns>The created review</returns>
        Task<FunctionMarketplaceReview> CreateReviewAsync(FunctionMarketplaceReview review);

        /// <summary>
        /// Updates a review
        /// </summary>
        /// <param name="review">Review to update</param>
        /// <returns>The updated review</returns>
        Task<FunctionMarketplaceReview> UpdateReviewAsync(FunctionMarketplaceReview review);

        /// <summary>
        /// Gets a review by ID
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>The review if found, null otherwise</returns>
        Task<FunctionMarketplaceReview> GetReviewByIdAsync(Guid id);

        /// <summary>
        /// Gets reviews for a marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of reviews for the marketplace item</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetReviewsByItemIdAsync(Guid itemId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets reviews by reviewer ID
        /// </summary>
        /// <param name="reviewerId">Reviewer ID</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of reviews by the reviewer</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetReviewsByReviewerIdAsync(Guid reviewerId, int limit = 10, int offset = 0);

        /// <summary>
        /// Deletes a review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>True if the review was deleted successfully, false otherwise</returns>
        Task<bool> DeleteReviewAsync(Guid id);

        /// <summary>
        /// Hides a review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="reason">Reason for hiding the review</param>
        /// <returns>The hidden review</returns>
        Task<FunctionMarketplaceReview> HideReviewAsync(Guid id, string reason);

        /// <summary>
        /// Unhides a review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>The unhidden review</returns>
        Task<FunctionMarketplaceReview> UnhideReviewAsync(Guid id);

        /// <summary>
        /// Votes a review as helpful
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>The updated review</returns>
        Task<FunctionMarketplaceReview> VoteReviewHelpfulAsync(Guid id, Guid userId);

        /// <summary>
        /// Votes a review as unhelpful
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>The updated review</returns>
        Task<FunctionMarketplaceReview> VoteReviewUnhelpfulAsync(Guid id, Guid userId);

        /// <summary>
        /// Creates a purchase for a marketplace item
        /// </summary>
        /// <param name="purchase">Purchase to create</param>
        /// <returns>The created purchase</returns>
        Task<FunctionMarketplacePurchase> CreatePurchaseAsync(FunctionMarketplacePurchase purchase);

        /// <summary>
        /// Updates a purchase
        /// </summary>
        /// <param name="purchase">Purchase to update</param>
        /// <returns>The updated purchase</returns>
        Task<FunctionMarketplacePurchase> UpdatePurchaseAsync(FunctionMarketplacePurchase purchase);

        /// <summary>
        /// Gets a purchase by ID
        /// </summary>
        /// <param name="id">Purchase ID</param>
        /// <returns>The purchase if found, null otherwise</returns>
        Task<FunctionMarketplacePurchase> GetPurchaseByIdAsync(Guid id);

        /// <summary>
        /// Gets purchases for a marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of purchases for the marketplace item</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetPurchasesByItemIdAsync(Guid itemId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets purchases by buyer ID
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of purchases by the buyer</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetPurchasesByBuyerIdAsync(Guid buyerId, int limit = 10, int offset = 0);

        /// <summary>
        /// Refunds a purchase
        /// </summary>
        /// <param name="id">Purchase ID</param>
        /// <param name="reason">Reason for the refund</param>
        /// <param name="refundTransactionId">Refund transaction ID</param>
        /// <returns>The refunded purchase</returns>
        Task<FunctionMarketplacePurchase> RefundPurchaseAsync(Guid id, string reason, string refundTransactionId);

        /// <summary>
        /// Checks if a user has purchased a marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>True if the user has purchased the item, false otherwise</returns>
        Task<bool> HasUserPurchasedItemAsync(Guid itemId, Guid userId);

        /// <summary>
        /// Gets the license key for a purchase
        /// </summary>
        /// <param name="purchaseId">Purchase ID</param>
        /// <returns>The license key if found, null otherwise</returns>
        Task<string> GetLicenseKeyAsync(Guid purchaseId);

        /// <summary>
        /// Validates a license key
        /// </summary>
        /// <param name="licenseKey">License key to validate</param>
        /// <returns>True if the license key is valid, false otherwise</returns>
        Task<bool> ValidateLicenseKeyAsync(string licenseKey);

        /// <summary>
        /// Gets the purchase for a license key
        /// </summary>
        /// <param name="licenseKey">License key</param>
        /// <returns>The purchase if found, null otherwise</returns>
        Task<FunctionMarketplacePurchase> GetPurchaseByLicenseKeyAsync(string licenseKey);

        /// <summary>
        /// Installs a marketplace item for a user
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>The installed function</returns>
        Task<Models.Function> InstallItemAsync(Guid itemId, Guid userId);

        /// <summary>
        /// Uninstalls a marketplace item for a user
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>True if the item was uninstalled successfully, false otherwise</returns>
        Task<bool> UninstallItemAsync(Guid itemId, Guid userId);

        /// <summary>
        /// Gets the installed marketplace items for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of installed marketplace items</returns>
        Task<IEnumerable<FunctionMarketplaceItem>> GetInstalledItemsAsync(Guid userId);

        /// <summary>
        /// Checks if a marketplace item is installed for a user
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>True if the item is installed, false otherwise</returns>
        Task<bool> IsItemInstalledAsync(Guid itemId, Guid userId);
    }
}
