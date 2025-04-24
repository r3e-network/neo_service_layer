using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function marketplace review repository
    /// </summary>
    public interface IFunctionMarketplaceReviewRepository
    {
        /// <summary>
        /// Creates a new function marketplace review
        /// </summary>
        /// <param name="review">Function marketplace review to create</param>
        /// <returns>The created function marketplace review</returns>
        Task<FunctionMarketplaceReview> CreateAsync(FunctionMarketplaceReview review);

        /// <summary>
        /// Updates a function marketplace review
        /// </summary>
        /// <param name="review">Function marketplace review to update</param>
        /// <returns>The updated function marketplace review</returns>
        Task<FunctionMarketplaceReview> UpdateAsync(FunctionMarketplaceReview review);

        /// <summary>
        /// Gets a function marketplace review by ID
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>The function marketplace review if found, null otherwise</returns>
        Task<FunctionMarketplaceReview> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function marketplace reviews by item ID
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace reviews</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetByItemIdAsync(Guid itemId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace reviews by reviewer ID
        /// </summary>
        /// <param name="reviewerId">Reviewer ID</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace reviews</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetByReviewerIdAsync(Guid reviewerId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace reviews by rating
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="rating">Rating</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace reviews</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetByRatingAsync(Guid itemId, int rating, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace reviews by hidden status
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="isHidden">Hidden status</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace reviews</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetByHiddenStatusAsync(Guid itemId, bool isHidden, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets the average rating for an item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The average rating</returns>
        Task<double> GetAverageRatingAsync(Guid itemId);

        /// <summary>
        /// Gets the rating count for an item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The rating count</returns>
        Task<int> GetRatingCountAsync(Guid itemId);

        /// <summary>
        /// Gets the rating distribution for an item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>Dictionary with rating as key and count as value</returns>
        Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid itemId);

        /// <summary>
        /// Deletes a function marketplace review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>True if the review was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function marketplace reviews by item ID
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>True if the reviews were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByItemIdAsync(Guid itemId);

        /// <summary>
        /// Deletes function marketplace reviews by reviewer ID
        /// </summary>
        /// <param name="reviewerId">Reviewer ID</param>
        /// <returns>True if the reviews were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByReviewerIdAsync(Guid reviewerId);
    }
}
