using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function marketplace review repository
    /// </summary>
    public interface IFunctionMarketplaceReviewRepository
    {
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
        /// <returns>List of function marketplace reviews for the specified item</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetByItemIdAsync(Guid itemId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace reviews by reviewer ID
        /// </summary>
        /// <param name="reviewerId">Reviewer ID</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace reviews by the specified reviewer</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetByReviewerIdAsync(Guid reviewerId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace reviews by rating
        /// </summary>
        /// <param name="rating">Rating</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace reviews with the specified rating</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetByRatingAsync(int rating, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace reviews by item ID and rating
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="rating">Rating</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace reviews for the specified item with the specified rating</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetByItemIdAndRatingAsync(Guid itemId, int rating, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace reviews by item ID and reviewer ID
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="reviewerId">Reviewer ID</param>
        /// <returns>The function marketplace review for the specified item by the specified reviewer</returns>
        Task<FunctionMarketplaceReview> GetByItemIdAndReviewerIdAsync(Guid itemId, Guid reviewerId);

        /// <summary>
        /// Gets the average rating for a function marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The average rating for the specified item</returns>
        Task<double> GetAverageRatingAsync(Guid itemId);

        /// <summary>
        /// Gets the rating distribution for a function marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The rating distribution for the specified item</returns>
        Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid itemId);

        /// <summary>
        /// Creates a function marketplace review
        /// </summary>
        /// <param name="review">Review to create</param>
        /// <returns>The created function marketplace review</returns>
        Task<FunctionMarketplaceReview> CreateAsync(FunctionMarketplaceReview review);

        /// <summary>
        /// Updates a function marketplace review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="review">Updated review</param>
        /// <returns>The updated function marketplace review</returns>
        Task<FunctionMarketplaceReview> UpdateAsync(Guid id, FunctionMarketplaceReview review);

        /// <summary>
        /// Deletes a function marketplace review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>True if the review was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function marketplace reviews
        /// </summary>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function marketplace reviews</returns>
        Task<IEnumerable<FunctionMarketplaceReview>> GetAllAsync(int limit = 10, int offset = 0);
    }
}
