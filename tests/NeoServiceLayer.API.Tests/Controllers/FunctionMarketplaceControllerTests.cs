using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.API.Controllers;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.API.Tests.Controllers
{
    public class FunctionMarketplaceControllerTests
    {
        private readonly Mock<ILogger<FunctionMarketplaceController>> _loggerMock;
        private readonly Mock<IFunctionMarketplaceService> _marketplaceServiceMock;
        private readonly FunctionMarketplaceController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public FunctionMarketplaceControllerTests()
        {
            _loggerMock = new Mock<ILogger<FunctionMarketplaceController>>();
            _marketplaceServiceMock = new Mock<IFunctionMarketplaceService>();
            _controller = new FunctionMarketplaceController(_loggerMock.Object, _marketplaceServiceMock.Object);

            // Setup controller context with user claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", _userId.ToString()),
                new Claim("name", "Test User")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetItemByIdAsync_ExistingItem_ReturnsOkResult()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Test Item",
                Description = "Test description",
                FunctionId = Guid.NewGuid(),
                PublisherId = _userId
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync(item);

            // Act
            var result = await _controller.GetItemByIdAsync(itemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionMarketplaceItem>(okResult.Value);
            Assert.Equal(itemId, returnValue.Id);
        }

        [Fact]
        public async Task GetItemByIdAsync_NonExistingItem_ReturnsNotFound()
        {
            // Arrange
            var itemId = Guid.NewGuid();

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync((FunctionMarketplaceItem)null);

            // Act
            var result = await _controller.GetItemByIdAsync(itemId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetItemByFunctionIdAsync_ExistingItem_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Id = Guid.NewGuid(),
                Name = "Test Item",
                Description = "Test description",
                FunctionId = functionId,
                PublisherId = _userId
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByFunctionIdAsync(functionId))
                .ReturnsAsync(item);

            // Act
            var result = await _controller.GetItemByFunctionIdAsync(functionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionMarketplaceItem>(okResult.Value);
            Assert.Equal(functionId, returnValue.FunctionId);
        }

        [Fact]
        public async Task GetItemsByPublisherIdAsync_ReturnsOkResult()
        {
            // Arrange
            var publisherId = Guid.NewGuid();
            var items = new List<FunctionMarketplaceItem>
            {
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), PublisherId = publisherId, Name = "Item 1" },
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), PublisherId = publisherId, Name = "Item 2" }
            };

            _marketplaceServiceMock.Setup(service => service.GetItemsByPublisherIdAsync(publisherId))
                .ReturnsAsync(items);

            // Act
            var result = await _controller.GetItemsByPublisherIdAsync(publisherId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionMarketplaceItem>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionMarketplaceItem>)returnValue).Count);
        }

        [Fact]
        public async Task GetItemsByCategoryAsync_ReturnsOkResult()
        {
            // Arrange
            var category = "Finance";
            var items = new List<FunctionMarketplaceItem>
            {
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), Category = category, Name = "Item 1" },
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), Category = category, Name = "Item 2" }
            };

            _marketplaceServiceMock.Setup(service => service.GetItemsByCategoryAsync(category))
                .ReturnsAsync(items);

            // Act
            var result = await _controller.GetItemsByCategoryAsync(category);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionMarketplaceItem>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionMarketplaceItem>)returnValue).Count);
        }

        [Fact]
        public async Task GetItemsByTagsAsync_ReturnsOkResult()
        {
            // Arrange
            var tags = new List<string> { "finance", "blockchain" };
            var items = new List<FunctionMarketplaceItem>
            {
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), Tags = tags, Name = "Item 1" },
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), Tags = tags, Name = "Item 2" }
            };

            _marketplaceServiceMock.Setup(service => service.GetItemsByTagsAsync(tags))
                .ReturnsAsync(items);

            // Act
            var result = await _controller.GetItemsByTagsAsync(tags);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionMarketplaceItem>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionMarketplaceItem>)returnValue).Count);
        }

        [Fact]
        public async Task GetFeaturedItemsAsync_ReturnsOkResult()
        {
            // Arrange
            var items = new List<FunctionMarketplaceItem>
            {
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), IsFeatured = true, Name = "Item 1" },
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), IsFeatured = true, Name = "Item 2" }
            };

            _marketplaceServiceMock.Setup(service => service.GetFeaturedItemsAsync(10))
                .ReturnsAsync(items);

            // Act
            var result = await _controller.GetFeaturedItemsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionMarketplaceItem>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionMarketplaceItem>)returnValue).Count);
        }

        [Fact]
        public async Task GetPopularItemsAsync_ReturnsOkResult()
        {
            // Arrange
            var items = new List<FunctionMarketplaceItem>
            {
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), DownloadCount = 100, Name = "Item 1" },
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), DownloadCount = 50, Name = "Item 2" }
            };

            _marketplaceServiceMock.Setup(service => service.GetPopularItemsAsync(10))
                .ReturnsAsync(items);

            // Act
            var result = await _controller.GetPopularItemsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionMarketplaceItem>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionMarketplaceItem>)returnValue).Count);
        }

        [Fact]
        public async Task GetNewItemsAsync_ReturnsOkResult()
        {
            // Arrange
            var items = new List<FunctionMarketplaceItem>
            {
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, Name = "Item 1" },
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddDays(-1), Name = "Item 2" }
            };

            _marketplaceServiceMock.Setup(service => service.GetNewItemsAsync(10))
                .ReturnsAsync(items);

            // Act
            var result = await _controller.GetNewItemsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionMarketplaceItem>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionMarketplaceItem>)returnValue).Count);
        }

        [Fact]
        public async Task SearchItemsAsync_ReturnsOkResult()
        {
            // Arrange
            var query = "test";
            var items = new List<FunctionMarketplaceItem>
            {
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), Name = "Test Item 1" },
                new FunctionMarketplaceItem { Id = Guid.NewGuid(), Name = "Test Item 2" }
            };

            _marketplaceServiceMock.Setup(service => service.SearchItemsAsync(
                    query, null, null, null, null, null, null, null, null, 10, 0))
                .ReturnsAsync(items);

            // Act
            var result = await _controller.SearchItemsAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionMarketplaceItem>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionMarketplaceItem>)returnValue).Count);
        }

        [Fact]
        public async Task CreateItemAsync_ValidItem_ReturnsCreatedAtAction()
        {
            // Arrange
            var item = new FunctionMarketplaceItem
            {
                Name = "New Item",
                Description = "New item description",
                FunctionId = Guid.NewGuid(),
                Price = 10.0m,
                Currency = "USD"
            };
            var createdItem = new FunctionMarketplaceItem
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Description = item.Description,
                FunctionId = item.FunctionId,
                Price = item.Price,
                Currency = item.Currency,
                PublisherId = _userId,
                PublisherName = "Test User"
            };

            _marketplaceServiceMock.Setup(service => service.CreateItemAsync(It.IsAny<FunctionMarketplaceItem>()))
                .ReturnsAsync(createdItem);

            // Act
            var result = await _controller.CreateItemAsync(item);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<FunctionMarketplaceItem>(createdAtActionResult.Value);
            Assert.Equal(createdItem.Id, returnValue.Id);
            Assert.Equal(_userId, returnValue.PublisherId);
            Assert.Equal("Test User", returnValue.PublisherName);
        }

        [Fact]
        public async Task UpdateItemAsync_ValidItem_ReturnsOkResult()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Updated Item",
                Description = "Updated description",
                FunctionId = Guid.NewGuid(),
                Price = 15.0m,
                Currency = "USD",
                PublisherId = _userId
            };
            var existingItem = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Original Item",
                Description = "Original description",
                FunctionId = item.FunctionId,
                Price = 10.0m,
                Currency = "USD",
                PublisherId = _userId
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync(existingItem);
            _marketplaceServiceMock.Setup(service => service.UpdateItemAsync(It.IsAny<FunctionMarketplaceItem>()))
                .ReturnsAsync(item);

            // Act
            var result = await _controller.UpdateItemAsync(itemId, item);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionMarketplaceItem>(okResult.Value);
            Assert.Equal(itemId, returnValue.Id);
            Assert.Equal(item.Name, returnValue.Name);
            Assert.Equal(item.Price, returnValue.Price);
        }

        [Fact]
        public async Task UpdateItemAsync_NonExistingItem_ReturnsNotFound()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Updated Item",
                Description = "Updated description",
                FunctionId = Guid.NewGuid(),
                PublisherId = _userId
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync((FunctionMarketplaceItem)null);

            // Act
            var result = await _controller.UpdateItemAsync(itemId, item);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateItemAsync_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Id = Guid.NewGuid(), // Different ID
                Name = "Updated Item",
                Description = "Updated description",
                FunctionId = Guid.NewGuid(),
                PublisherId = _userId
            };
            var existingItem = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Original Item",
                Description = "Original description",
                FunctionId = Guid.NewGuid(),
                PublisherId = _userId
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync(existingItem);

            // Act
            var result = await _controller.UpdateItemAsync(itemId, item);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("ID in the path does not match ID in the body", badRequestResult.Value);
        }

        [Fact]
        public async Task PublishItemAsync_ExistingItem_ReturnsOkResult()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var existingItem = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Item to publish",
                FunctionId = Guid.NewGuid(),
                PublisherId = _userId,
                IsPublished = false
            };
            var publishedItem = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = existingItem.Name,
                FunctionId = existingItem.FunctionId,
                PublisherId = _userId,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync(existingItem);
            _marketplaceServiceMock.Setup(service => service.PublishItemAsync(itemId))
                .ReturnsAsync(publishedItem);

            // Act
            var result = await _controller.PublishItemAsync(itemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionMarketplaceItem>(okResult.Value);
            Assert.Equal(itemId, returnValue.Id);
            Assert.True(returnValue.IsPublished);
            Assert.NotNull(returnValue.PublishedAt);
        }

        [Fact]
        public async Task UnpublishItemAsync_ExistingItem_ReturnsOkResult()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var existingItem = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Item to unpublish",
                FunctionId = Guid.NewGuid(),
                PublisherId = _userId,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow.AddDays(-1)
            };
            var unpublishedItem = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = existingItem.Name,
                FunctionId = existingItem.FunctionId,
                PublisherId = _userId,
                IsPublished = false,
                PublishedAt = existingItem.PublishedAt,
                UnpublishedAt = DateTime.UtcNow
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync(existingItem);
            _marketplaceServiceMock.Setup(service => service.UnpublishItemAsync(itemId))
                .ReturnsAsync(unpublishedItem);

            // Act
            var result = await _controller.UnpublishItemAsync(itemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionMarketplaceItem>(okResult.Value);
            Assert.Equal(itemId, returnValue.Id);
            Assert.False(returnValue.IsPublished);
            Assert.NotNull(returnValue.UnpublishedAt);
        }

        [Fact]
        public async Task DeleteItemAsync_ExistingItem_ReturnsNoContent()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var existingItem = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Item to delete",
                FunctionId = Guid.NewGuid(),
                PublisherId = _userId
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync(existingItem);
            _marketplaceServiceMock.Setup(service => service.DeleteItemAsync(itemId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteItemAsync(itemId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CreateReviewAsync_ValidReview_ReturnsCreatedAtAction()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var review = new FunctionMarketplaceReview
            {
                ItemId = itemId,
                Rating = 5,
                Title = "Great function",
                Content = "This function works perfectly"
            };
            var item = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Item",
                FunctionId = Guid.NewGuid(),
                IsFree = true
            };
            var createdReview = new FunctionMarketplaceReview
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                ReviewerId = _userId,
                ReviewerName = "Test User",
                Rating = review.Rating,
                Title = review.Title,
                Content = review.Content,
                CreatedAt = DateTime.UtcNow
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync(item);
            _marketplaceServiceMock.Setup(service => service.HasUserPurchasedItemAsync(itemId, _userId))
                .ReturnsAsync(false);
            _marketplaceServiceMock.Setup(service => service.CreateReviewAsync(It.IsAny<FunctionMarketplaceReview>()))
                .ReturnsAsync(createdReview);

            // Act
            var result = await _controller.CreateReviewAsync(review);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<FunctionMarketplaceReview>(createdAtActionResult.Value);
            Assert.Equal(createdReview.Id, returnValue.Id);
            Assert.Equal(_userId, returnValue.ReviewerId);
            Assert.Equal("Test User", returnValue.ReviewerName);
        }

        [Fact]
        public async Task GetReviewsByItemIdAsync_ReturnsOkResult()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var reviews = new List<FunctionMarketplaceReview>
            {
                new FunctionMarketplaceReview { Id = Guid.NewGuid(), ItemId = itemId, Rating = 5 },
                new FunctionMarketplaceReview { Id = Guid.NewGuid(), ItemId = itemId, Rating = 4 }
            };

            _marketplaceServiceMock.Setup(service => service.GetReviewsByItemIdAsync(itemId, 10, 0))
                .ReturnsAsync(reviews);

            // Act
            var result = await _controller.GetReviewsByItemIdAsync(itemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionMarketplaceReview>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionMarketplaceReview>)returnValue).Count);
        }

        [Fact]
        public async Task CreatePurchaseAsync_ValidPurchase_ReturnsCreatedAtAction()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var purchase = new FunctionMarketplacePurchase
            {
                ItemId = itemId,
                PaymentMethod = "credit_card",
                PaymentReference = "ref123"
            };
            var item = new FunctionMarketplaceItem
            {
                Id = itemId,
                Name = "Item to purchase",
                FunctionId = Guid.NewGuid(),
                Price = 10.0m,
                Currency = "USD"
            };
            var createdPurchase = new FunctionMarketplacePurchase
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                BuyerId = _userId,
                BuyerName = "Test User",
                PricePaid = item.Price,
                Currency = item.Currency,
                PaymentMethod = purchase.PaymentMethod,
                PaymentReference = purchase.PaymentReference,
                PurchasedAt = DateTime.UtcNow
            };

            _marketplaceServiceMock.Setup(service => service.GetItemByIdAsync(itemId))
                .ReturnsAsync(item);
            _marketplaceServiceMock.Setup(service => service.CreatePurchaseAsync(It.IsAny<FunctionMarketplacePurchase>()))
                .ReturnsAsync(createdPurchase);

            // Act
            var result = await _controller.CreatePurchaseAsync(purchase);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<FunctionMarketplacePurchase>(createdAtActionResult.Value);
            Assert.Equal(createdPurchase.Id, returnValue.Id);
            Assert.Equal(_userId, returnValue.BuyerId);
            Assert.Equal("Test User", returnValue.BuyerName);
            Assert.Equal(item.Price, returnValue.PricePaid);
            Assert.Equal(item.Currency, returnValue.Currency);
        }

        [Fact]
        public async Task GetMyPurchasesAsync_ReturnsOkResult()
        {
            // Arrange
            var purchases = new List<FunctionMarketplacePurchase>
            {
                new FunctionMarketplacePurchase { Id = Guid.NewGuid(), BuyerId = _userId, ItemId = Guid.NewGuid() },
                new FunctionMarketplacePurchase { Id = Guid.NewGuid(), BuyerId = _userId, ItemId = Guid.NewGuid() }
            };

            _marketplaceServiceMock.Setup(service => service.GetPurchasesByBuyerIdAsync(_userId, 10, 0))
                .ReturnsAsync(purchases);

            // Act
            var result = await _controller.GetMyPurchasesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionMarketplacePurchase>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionMarketplacePurchase>)returnValue).Count);
        }

        [Fact]
        public async Task HasUserPurchasedItemAsync_ReturnsOkResult()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var hasPurchased = true;

            _marketplaceServiceMock.Setup(service => service.HasUserPurchasedItemAsync(itemId, _userId))
                .ReturnsAsync(hasPurchased);

            // Act
            var result = await _controller.HasUserPurchasedItemAsync(itemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(returnValue);
        }
    }
}
