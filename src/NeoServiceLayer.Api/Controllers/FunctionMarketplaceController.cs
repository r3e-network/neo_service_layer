using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.API.Controllers
{
    /// <summary>
    /// Controller for function marketplace
    /// </summary>
    [ApiController]
    [Route("api/functions/marketplace")]
    public class FunctionMarketplaceController : ControllerBase
    {
        private readonly ILogger<FunctionMarketplaceController> _logger;
        private readonly IFunctionMarketplaceService _marketplaceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionMarketplaceController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="marketplaceService">Function marketplace service</param>
        public FunctionMarketplaceController(ILogger<FunctionMarketplaceController> logger, IFunctionMarketplaceService marketplaceService)
        {
            _logger = logger;
            _marketplaceService = marketplaceService;
        }

        /// <summary>
        /// Gets a marketplace item by ID
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The marketplace item</returns>
        [HttpGet("items/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionMarketplaceItem>> GetItemByIdAsync(Guid id)
        {
            var item = await _marketplaceService.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        /// <summary>
        /// Gets a marketplace item by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>The marketplace item</returns>
        [HttpGet("items/function/{functionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionMarketplaceItem>> GetItemByFunctionIdAsync(Guid functionId)
        {
            var item = await _marketplaceService.GetItemByFunctionIdAsync(functionId);
            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        /// <summary>
        /// Gets marketplace items by publisher ID
        /// </summary>
        /// <param name="publisherId">Publisher ID</param>
        /// <returns>List of marketplace items</returns>
        [HttpGet("items/publisher/{publisherId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionMarketplaceItem>>> GetItemsByPublisherIdAsync(Guid publisherId)
        {
            var items = await _marketplaceService.GetItemsByPublisherIdAsync(publisherId);
            return Ok(items);
        }

        /// <summary>
        /// Gets marketplace items by category
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>List of marketplace items</returns>
        [HttpGet("items/category/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionMarketplaceItem>>> GetItemsByCategoryAsync(string category)
        {
            var items = await _marketplaceService.GetItemsByCategoryAsync(category);
            return Ok(items);
        }

        /// <summary>
        /// Gets marketplace items by tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>List of marketplace items</returns>
        [HttpGet("items/tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionMarketplaceItem>>> GetItemsByTagsAsync([FromQuery] List<string> tags)
        {
            var items = await _marketplaceService.GetItemsByTagsAsync(tags);
            return Ok(items);
        }

        /// <summary>
        /// Gets featured marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of featured marketplace items</returns>
        [HttpGet("items/featured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionMarketplaceItem>>> GetFeaturedItemsAsync([FromQuery] int limit = 10)
        {
            var items = await _marketplaceService.GetFeaturedItemsAsync(limit);
            return Ok(items);
        }

        /// <summary>
        /// Gets popular marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of popular marketplace items</returns>
        [HttpGet("items/popular")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionMarketplaceItem>>> GetPopularItemsAsync([FromQuery] int limit = 10)
        {
            var items = await _marketplaceService.GetPopularItemsAsync(limit);
            return Ok(items);
        }

        /// <summary>
        /// Gets new marketplace items
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>List of new marketplace items</returns>
        [HttpGet("items/new")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionMarketplaceItem>>> GetNewItemsAsync([FromQuery] int limit = 10)
        {
            var items = await _marketplaceService.GetNewItemsAsync(limit);
            return Ok(items);
        }

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
        [HttpGet("items/search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionMarketplaceItem>>> SearchItemsAsync(
            [FromQuery] string query = null,
            [FromQuery] string category = null,
            [FromQuery] List<string> tags = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? isFree = null,
            [FromQuery] bool? isVerified = null,
            [FromQuery] string sortBy = null,
            [FromQuery] string sortOrder = null,
            [FromQuery] int limit = 10,
            [FromQuery] int offset = 0)
        {
            var items = await _marketplaceService.SearchItemsAsync(query, category, tags, minPrice, maxPrice, isFree, isVerified, sortBy, sortOrder, limit, offset);
            return Ok(items);
        }

        /// <summary>
        /// Creates a new marketplace item
        /// </summary>
        /// <param name="item">Marketplace item to create</param>
        /// <returns>The created marketplace item</returns>
        [HttpPost("items")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionMarketplaceItem>> CreateItemAsync([FromBody] FunctionMarketplaceItem item)
        {
            // Set the publisher ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                item.PublisherId = userGuid;
                item.PublisherName = User.FindFirst("name")?.Value ?? "Unknown";
            }

            var createdItem = await _marketplaceService.CreateItemAsync(item);
            return CreatedAtAction(nameof(GetItemByIdAsync), new { id = createdItem.Id }, createdItem);
        }

        /// <summary>
        /// Updates a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <param name="item">Marketplace item to update</param>
        /// <returns>The updated marketplace item</returns>
        [HttpPut("items/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionMarketplaceItem>> UpdateItemAsync(Guid id, [FromBody] FunctionMarketplaceItem item)
        {
            // Check if the item exists
            var existingItem = await _marketplaceService.GetItemByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            // Ensure the ID in the path matches the ID in the body
            if (id != item.Id)
            {
                return BadRequest("ID in the path does not match ID in the body");
            }

            // Check if the user is the publisher
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingItem.PublisherId != userGuid)
                {
                    return Forbid("You are not the publisher of this item");
                }
            }

            var updatedItem = await _marketplaceService.UpdateItemAsync(item);
            return Ok(updatedItem);
        }

        /// <summary>
        /// Publishes a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The published marketplace item</returns>
        [HttpPost("items/{id}/publish")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionMarketplaceItem>> PublishItemAsync(Guid id)
        {
            // Check if the item exists
            var existingItem = await _marketplaceService.GetItemByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            // Check if the user is the publisher
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingItem.PublisherId != userGuid)
                {
                    return Forbid("You are not the publisher of this item");
                }
            }

            var publishedItem = await _marketplaceService.PublishItemAsync(id);
            return Ok(publishedItem);
        }

        /// <summary>
        /// Unpublishes a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The unpublished marketplace item</returns>
        [HttpPost("items/{id}/unpublish")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionMarketplaceItem>> UnpublishItemAsync(Guid id)
        {
            // Check if the item exists
            var existingItem = await _marketplaceService.GetItemByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            // Check if the user is the publisher
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingItem.PublisherId != userGuid)
                {
                    return Forbid("You are not the publisher of this item");
                }
            }

            var unpublishedItem = await _marketplaceService.UnpublishItemAsync(id);
            return Ok(unpublishedItem);
        }

        /// <summary>
        /// Deletes a marketplace item
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>No content</returns>
        [HttpDelete("items/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteItemAsync(Guid id)
        {
            // Check if the item exists
            var existingItem = await _marketplaceService.GetItemByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            // Check if the user is the publisher
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingItem.PublisherId != userGuid)
                {
                    return Forbid("You are not the publisher of this item");
                }
            }

            var result = await _marketplaceService.DeleteItemAsync(id);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the item");
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a review for a marketplace item
        /// </summary>
        /// <param name="review">Review to create</param>
        /// <returns>The created review</returns>
        [HttpPost("reviews")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionMarketplaceReview>> CreateReviewAsync([FromBody] FunctionMarketplaceReview review)
        {
            // Set the reviewer ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                review.ReviewerId = userGuid;
                review.ReviewerName = User.FindFirst("name")?.Value ?? "Unknown";
            }

            // Check if the item exists
            var item = await _marketplaceService.GetItemByIdAsync(review.ItemId);
            if (item == null)
            {
                return BadRequest("Item not found");
            }

            // Check if the user has purchased the item
            var hasPurchased = await _marketplaceService.HasUserPurchasedItemAsync(review.ItemId, review.ReviewerId);
            if (!hasPurchased && !item.IsFree)
            {
                return BadRequest("You must purchase the item before reviewing it");
            }

            var createdReview = await _marketplaceService.CreateReviewAsync(review);
            return CreatedAtAction(nameof(GetReviewByIdAsync), new { id = createdReview.Id }, createdReview);
        }

        /// <summary>
        /// Gets a review by ID
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>The review</returns>
        [HttpGet("reviews/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionMarketplaceReview>> GetReviewByIdAsync(Guid id)
        {
            var review = await _marketplaceService.GetReviewByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            return Ok(review);
        }

        /// <summary>
        /// Gets reviews for a marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="limit">Maximum number of reviews to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of reviews</returns>
        [HttpGet("items/{itemId}/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionMarketplaceReview>>> GetReviewsByItemIdAsync(Guid itemId, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            var reviews = await _marketplaceService.GetReviewsByItemIdAsync(itemId, limit, offset);
            return Ok(reviews);
        }

        /// <summary>
        /// Updates a review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="review">Review to update</param>
        /// <returns>The updated review</returns>
        [HttpPut("reviews/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionMarketplaceReview>> UpdateReviewAsync(Guid id, [FromBody] FunctionMarketplaceReview review)
        {
            // Check if the review exists
            var existingReview = await _marketplaceService.GetReviewByIdAsync(id);
            if (existingReview == null)
            {
                return NotFound();
            }

            // Ensure the ID in the path matches the ID in the body
            if (id != review.Id)
            {
                return BadRequest("ID in the path does not match ID in the body");
            }

            // Check if the user is the reviewer
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingReview.ReviewerId != userGuid)
                {
                    return Forbid("You are not the reviewer of this review");
                }
            }

            var updatedReview = await _marketplaceService.UpdateReviewAsync(review);
            return Ok(updatedReview);
        }

        /// <summary>
        /// Deletes a review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>No content</returns>
        [HttpDelete("reviews/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteReviewAsync(Guid id)
        {
            // Check if the review exists
            var existingReview = await _marketplaceService.GetReviewByIdAsync(id);
            if (existingReview == null)
            {
                return NotFound();
            }

            // Check if the user is the reviewer
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (existingReview.ReviewerId != userGuid)
                {
                    return Forbid("You are not the reviewer of this review");
                }
            }

            var result = await _marketplaceService.DeleteReviewAsync(id);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the review");
            }

            return NoContent();
        }

        /// <summary>
        /// Creates a purchase for a marketplace item
        /// </summary>
        /// <param name="purchase">Purchase to create</param>
        /// <returns>The created purchase</returns>
        [HttpPost("purchases")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FunctionMarketplacePurchase>> CreatePurchaseAsync([FromBody] FunctionMarketplacePurchase purchase)
        {
            // Set the buyer ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                purchase.BuyerId = userGuid;
                purchase.BuyerName = User.FindFirst("name")?.Value ?? "Unknown";
            }

            // Check if the item exists
            var item = await _marketplaceService.GetItemByIdAsync(purchase.ItemId);
            if (item == null)
            {
                return BadRequest("Item not found");
            }

            // Set the price from the item
            purchase.PricePaid = item.Price;
            purchase.Currency = item.Currency;

            var createdPurchase = await _marketplaceService.CreatePurchaseAsync(purchase);
            return CreatedAtAction(nameof(GetPurchaseByIdAsync), new { id = createdPurchase.Id }, createdPurchase);
        }

        /// <summary>
        /// Gets a purchase by ID
        /// </summary>
        /// <param name="id">Purchase ID</param>
        /// <returns>The purchase</returns>
        [HttpGet("purchases/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionMarketplacePurchase>> GetPurchaseByIdAsync(Guid id)
        {
            var purchase = await _marketplaceService.GetPurchaseByIdAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }

            // Check if the user is the buyer
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                if (purchase.BuyerId != userGuid)
                {
                    return Forbid("You are not the buyer of this purchase");
                }
            }

            return Ok(purchase);
        }

        /// <summary>
        /// Gets purchases for a user
        /// </summary>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of purchases</returns>
        [HttpGet("purchases/my")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionMarketplacePurchase>>> GetMyPurchasesAsync([FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            // Get the user ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var purchases = await _marketplaceService.GetPurchasesByBuyerIdAsync(userGuid, limit, offset);
            return Ok(purchases);
        }

        /// <summary>
        /// Checks if a user has purchased a marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>True if the user has purchased the item, false otherwise</returns>
        [HttpGet("purchases/check/{itemId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> HasUserPurchasedItemAsync(Guid itemId)
        {
            // Get the user ID from the claims
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var hasPurchased = await _marketplaceService.HasUserPurchasedItemAsync(itemId, userGuid);
            return Ok(hasPurchased);
        }
    }
}
