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
    /// Repository for function marketplace reviews
    /// </summary>
    public class FunctionMarketplaceReviewRepository : IFunctionMarketplaceReviewRepository
    {
        private readonly ILogger<FunctionMarketplaceReviewRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_marketplace_reviews";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionMarketplaceReviewRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionMarketplaceReviewRepository(ILogger<FunctionMarketplaceReviewRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> CreateAsync(FunctionMarketplaceReview review)
        {
            _logger.LogInformation("Creating function marketplace review for item {ItemId} by reviewer {ReviewerId}", review.ItemId, review.ReviewerId);

            // Ensure ID is set
            if (review.Id == Guid.Empty)
            {
                review.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, review);

            return review;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> UpdateAsync(Guid id, FunctionMarketplaceReview review)
        {
            _logger.LogInformation("Updating function marketplace review: {Id}", id);

            // Ensure the ID matches
            review.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionMarketplaceReview, Guid>(_collectionName, id, review);

            return review;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> UpdateAsync(FunctionMarketplaceReview review)
        {
            return await UpdateAsync(review.Id, review);
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function marketplace review by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionMarketplaceReview, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceReview>> GetByItemIdAsync(Guid itemId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace reviews by item ID: {ItemId}, limit: {Limit}, offset: {Offset}", itemId, limit, offset);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by item ID and sort by created date
            return reviews
                .Where(r => r.ItemId == itemId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceReview>> GetByReviewerIdAsync(Guid reviewerId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace reviews by reviewer ID: {ReviewerId}, limit: {Limit}, offset: {Offset}", reviewerId, limit, offset);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by reviewer ID and sort by created date
            return reviews
                .Where(r => r.ReviewerId == reviewerId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceReview>> GetByRatingAsync(int rating, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace reviews by rating: {Rating}, limit: {Limit}, offset: {Offset}", rating, limit, offset);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by rating and sort by created date
            return reviews
                .Where(r => r.Rating == rating)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceReview>> GetByItemIdAndRatingAsync(Guid itemId, int rating, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace reviews by item ID: {ItemId} and rating: {Rating}, limit: {Limit}, offset: {Offset}", itemId, rating, limit, offset);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by item ID and rating, and sort by created date
            return reviews
                .Where(r => r.ItemId == itemId && r.Rating == rating)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceReview>> GetByHiddenStatusAsync(Guid itemId, bool isHidden, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace reviews by item ID: {ItemId} and hidden status: {IsHidden}, limit: {Limit}, offset: {Offset}", itemId, isHidden, limit, offset);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by item ID and hidden status, and sort by created date
            return reviews
                .Where(r => r.ItemId == itemId && r.IsHidden == isHidden)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<double> GetAverageRatingAsync(Guid itemId)
        {
            _logger.LogInformation("Getting average rating for function marketplace item: {ItemId}", itemId);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by item ID and not hidden
            var filteredReviews = reviews.Where(r => r.ItemId == itemId && !r.IsHidden);

            // Calculate average rating
            if (!filteredReviews.Any())
            {
                return 0;
            }

            return filteredReviews.Average(r => r.Rating);
        }

        /// <inheritdoc/>
        public async Task<int> GetRatingCountAsync(Guid itemId)
        {
            _logger.LogInformation("Getting rating count for function marketplace item: {ItemId}", itemId);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by item ID and not hidden
            var filteredReviews = reviews.Where(r => r.ItemId == itemId && !r.IsHidden);

            // Return count
            return filteredReviews.Count();
        }

        /// <inheritdoc/>
        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid itemId)
        {
            _logger.LogInformation("Getting rating distribution for function marketplace item: {ItemId}", itemId);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by item ID and not hidden
            var filteredReviews = reviews.Where(r => r.ItemId == itemId && !r.IsHidden);

            // Calculate rating distribution
            var distribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                distribution[i] = filteredReviews.Count(r => r.Rating == i);
            }

            return distribution;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function marketplace review: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionMarketplaceReview, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByItemIdAsync(Guid itemId)
        {
            _logger.LogInformation("Deleting function marketplace reviews by item ID: {ItemId}", itemId);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by item ID
            var filteredReviews = reviews.Where(r => r.ItemId == itemId);

            // Delete each review
            var success = true;
            foreach (var review in filteredReviews)
            {
                var result = await DeleteAsync(review.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByReviewerIdAsync(Guid reviewerId)
        {
            _logger.LogInformation("Deleting function marketplace reviews by reviewer ID: {ReviewerId}", reviewerId);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by reviewer ID
            var filteredReviews = reviews.Where(r => r.ReviewerId == reviewerId);

            // Delete each review
            var success = true;
            foreach (var review in filteredReviews)
            {
                var result = await DeleteAsync(review.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplaceReview> GetByItemIdAndReviewerIdAsync(Guid itemId, Guid reviewerId)
        {
            _logger.LogInformation("Getting function marketplace review by item ID: {ItemId} and reviewer ID: {ReviewerId}", itemId, reviewerId);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Filter by item ID and reviewer ID
            return reviews.FirstOrDefault(r => r.ItemId == itemId && r.ReviewerId == reviewerId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplaceReview>> GetAllAsync(int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting all function marketplace reviews, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all reviews
            var reviews = await _storageProvider.GetAllAsync<FunctionMarketplaceReview>(_collectionName);

            // Sort by created date and apply pagination
            return reviews
                .OrderByDescending(r => r.CreatedAt)
                .Skip(offset)
                .Take(limit);
        }
    }
}
