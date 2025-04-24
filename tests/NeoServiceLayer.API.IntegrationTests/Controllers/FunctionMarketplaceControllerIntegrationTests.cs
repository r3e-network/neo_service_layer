using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Function;
using NeoServiceLayer.Services.Function.Repositories;
using NeoServiceLayer.Services.Storage;
using NeoServiceLayer.Services.Storage.Providers;
using Xunit;

namespace NeoServiceLayer.API.IntegrationTests.Controllers
{
    public class FunctionMarketplaceControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _testUserId = Guid.NewGuid().ToString();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public FunctionMarketplaceControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Create a custom factory with in-memory services
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace storage provider with in-memory provider
                    services.AddSingleton<IStorageProvider, InMemoryStorageProvider>();
                    
                    // Register test repositories
                    services.AddSingleton<IFunctionRepository, FunctionRepository>();
                    services.AddSingleton<IFunctionMarketplaceItemRepository, FunctionMarketplaceItemRepository>();
                    services.AddSingleton<IFunctionMarketplaceReviewRepository, FunctionMarketplaceReviewRepository>();
                    services.AddSingleton<IFunctionMarketplacePurchaseRepository, FunctionMarketplacePurchaseRepository>();
                    
                    // Register test services
                    services.AddSingleton<IFunctionService, FunctionService>();
                    services.AddSingleton<IFunctionMarketplaceService, FunctionMarketplaceService>();
                });
            });

            _client = _factory.CreateClient();
            
            // Add authentication header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateTestToken());
        }

        [Fact]
        public async Task CreateItem_ValidItem_ReturnsCreatedItem()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Name = "Test Marketplace Item",
                Description = "Item for integration testing",
                FunctionId = functionId,
                Price = 10.0m,
                Currency = "USD",
                Category = "Finance",
                Tags = new List<string> { "finance", "blockchain" },
                IsFree = false
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Act
            var response = await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(responseContent, _jsonOptions);

            Assert.NotNull(createdItem);
            Assert.NotEqual(Guid.Empty, createdItem.Id);
            Assert.Equal(item.Name, createdItem.Name);
            Assert.Equal(item.FunctionId, createdItem.FunctionId);
            Assert.Equal(Guid.Parse(_testUserId), createdItem.PublisherId);
            Assert.Equal("Test User", createdItem.PublisherName);
            Assert.Equal(item.Price, createdItem.Price);
            Assert.Equal(item.Currency, createdItem.Currency);
            Assert.Equal(item.Category, createdItem.Category);
            Assert.Equal(item.Tags, createdItem.Tags);
            Assert.False(createdItem.IsFree);
            Assert.NotNull(createdItem.CreatedAt);
        }

        [Fact]
        public async Task GetItemById_ExistingItem_ReturnsItem()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Name = "Item to Retrieve",
                Description = "Item for retrieval testing",
                FunctionId = functionId,
                Price = 5.0m,
                Currency = "USD",
                Category = "Utility",
                IsFree = false
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create an item
            var createResponse = await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/functions/marketplace/items/{createdItem.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var retrievedItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(responseContent, _jsonOptions);

            Assert.NotNull(retrievedItem);
            Assert.Equal(createdItem.Id, retrievedItem.Id);
            Assert.Equal(item.Name, retrievedItem.Name);
            Assert.Equal(item.FunctionId, retrievedItem.FunctionId);
            Assert.Equal(Guid.Parse(_testUserId), retrievedItem.PublisherId);
        }

        [Fact]
        public async Task GetItemsByCategory_ExistingItems_ReturnsItems()
        {
            // Arrange
            var functionId1 = Guid.NewGuid();
            var functionId2 = Guid.NewGuid();
            var category = "Gaming";
            var item1 = new FunctionMarketplaceItem
            {
                Name = "Gaming Item 1",
                Description = "First gaming item",
                FunctionId = functionId1,
                Price = 5.0m,
                Currency = "USD",
                Category = category,
                IsFree = false
            };
            var item2 = new FunctionMarketplaceItem
            {
                Name = "Gaming Item 2",
                Description = "Second gaming item",
                FunctionId = functionId2,
                Price = 10.0m,
                Currency = "USD",
                Category = category,
                IsFree = false
            };

            // Create functions
            await CreateTestFunction(functionId1);
            await CreateTestFunction(functionId2);

            // Create items
            await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item1, _jsonOptions), Encoding.UTF8, "application/json"));
            await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item2, _jsonOptions), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.GetAsync($"/api/functions/marketplace/items/category/{category}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<FunctionMarketplaceItem>>(responseContent, _jsonOptions);

            Assert.NotNull(items);
            Assert.Equal(2, items.Count);
            Assert.Contains(items, i => i.Name == item1.Name);
            Assert.Contains(items, i => i.Name == item2.Name);
            Assert.All(items, i => Assert.Equal(category, i.Category));
        }

        [Fact]
        public async Task UpdateItem_ExistingItem_ReturnsUpdatedItem()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Name = "Item to Update",
                Description = "Original description",
                FunctionId = functionId,
                Price = 5.0m,
                Currency = "USD",
                Category = "Utility",
                IsFree = false
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create an item
            var createResponse = await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(createResponseContent, _jsonOptions);

            // Update the item
            createdItem.Description = "Updated description";
            createdItem.Price = 7.5m;
            createdItem.Category = "Finance";

            // Act
            var response = await _client.PutAsync($"/api/functions/marketplace/items/{createdItem.Id}", 
                new StringContent(JsonSerializer.Serialize(createdItem, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(responseContent, _jsonOptions);

            Assert.NotNull(updatedItem);
            Assert.Equal(createdItem.Id, updatedItem.Id);
            Assert.Equal("Updated description", updatedItem.Description);
            Assert.Equal(7.5m, updatedItem.Price);
            Assert.Equal("Finance", updatedItem.Category);
        }

        [Fact]
        public async Task PublishItem_ExistingItem_ReturnsPublishedItem()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Name = "Item to Publish",
                Description = "Item for publishing",
                FunctionId = functionId,
                Price = 5.0m,
                Currency = "USD",
                Category = "Utility",
                IsFree = false,
                IsPublished = false
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create an item
            var createResponse = await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.PostAsync($"/api/functions/marketplace/items/{createdItem.Id}/publish", null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var publishedItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(responseContent, _jsonOptions);

            Assert.NotNull(publishedItem);
            Assert.Equal(createdItem.Id, publishedItem.Id);
            Assert.True(publishedItem.IsPublished);
            Assert.NotNull(publishedItem.PublishedAt);
        }

        [Fact]
        public async Task CreateReview_ValidReview_ReturnsCreatedReview()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Name = "Item for Review",
                Description = "Item for review testing",
                FunctionId = functionId,
                Price = 0.0m,
                Currency = "USD",
                Category = "Utility",
                IsFree = true,
                IsPublished = true
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create an item
            var createItemResponse = await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item, _jsonOptions), Encoding.UTF8, "application/json"));
            createItemResponse.EnsureSuccessStatusCode();
            
            var createItemResponseContent = await createItemResponse.Content.ReadAsStringAsync();
            var createdItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(createItemResponseContent, _jsonOptions);

            // Create a review
            var review = new FunctionMarketplaceReview
            {
                ItemId = createdItem.Id,
                Rating = 5,
                Title = "Great function",
                Content = "This function works perfectly"
            };

            // Act
            var response = await _client.PostAsync("/api/functions/marketplace/reviews", 
                new StringContent(JsonSerializer.Serialize(review, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdReview = JsonSerializer.Deserialize<FunctionMarketplaceReview>(responseContent, _jsonOptions);

            Assert.NotNull(createdReview);
            Assert.NotEqual(Guid.Empty, createdReview.Id);
            Assert.Equal(createdItem.Id, createdReview.ItemId);
            Assert.Equal(Guid.Parse(_testUserId), createdReview.ReviewerId);
            Assert.Equal("Test User", createdReview.ReviewerName);
            Assert.Equal(5, createdReview.Rating);
            Assert.Equal("Great function", createdReview.Title);
            Assert.Equal("This function works perfectly", createdReview.Content);
            Assert.NotNull(createdReview.CreatedAt);
        }

        [Fact]
        public async Task GetReviewsByItemId_ExistingReviews_ReturnsReviews()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Name = "Item with Reviews",
                Description = "Item for reviews testing",
                FunctionId = functionId,
                Price = 0.0m,
                Currency = "USD",
                Category = "Utility",
                IsFree = true,
                IsPublished = true
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create an item
            var createItemResponse = await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item, _jsonOptions), Encoding.UTF8, "application/json"));
            createItemResponse.EnsureSuccessStatusCode();
            
            var createItemResponseContent = await createItemResponse.Content.ReadAsStringAsync();
            var createdItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(createItemResponseContent, _jsonOptions);

            // Create reviews
            var review1 = new FunctionMarketplaceReview
            {
                ItemId = createdItem.Id,
                Rating = 5,
                Title = "Great function",
                Content = "This function works perfectly"
            };
            var review2 = new FunctionMarketplaceReview
            {
                ItemId = createdItem.Id,
                Rating = 4,
                Title = "Good function",
                Content = "This function works well"
            };

            await _client.PostAsync("/api/functions/marketplace/reviews", 
                new StringContent(JsonSerializer.Serialize(review1, _jsonOptions), Encoding.UTF8, "application/json"));
            await _client.PostAsync("/api/functions/marketplace/reviews", 
                new StringContent(JsonSerializer.Serialize(review2, _jsonOptions), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.GetAsync($"/api/functions/marketplace/items/{createdItem.Id}/reviews");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var reviews = JsonSerializer.Deserialize<List<FunctionMarketplaceReview>>(responseContent, _jsonOptions);

            Assert.NotNull(reviews);
            Assert.Equal(2, reviews.Count);
            Assert.Contains(reviews, r => r.Title == review1.Title);
            Assert.Contains(reviews, r => r.Title == review2.Title);
            Assert.All(reviews, r => Assert.Equal(createdItem.Id, r.ItemId));
        }

        [Fact]
        public async Task CreatePurchase_ValidPurchase_ReturnsCreatedPurchase()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var item = new FunctionMarketplaceItem
            {
                Name = "Item to Purchase",
                Description = "Item for purchase testing",
                FunctionId = functionId,
                Price = 10.0m,
                Currency = "USD",
                Category = "Utility",
                IsFree = false,
                IsPublished = true
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create an item
            var createItemResponse = await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item, _jsonOptions), Encoding.UTF8, "application/json"));
            createItemResponse.EnsureSuccessStatusCode();
            
            var createItemResponseContent = await createItemResponse.Content.ReadAsStringAsync();
            var createdItem = JsonSerializer.Deserialize<FunctionMarketplaceItem>(createItemResponseContent, _jsonOptions);

            // Create a purchase
            var purchase = new FunctionMarketplacePurchase
            {
                ItemId = createdItem.Id,
                PaymentMethod = "credit_card",
                PaymentReference = "ref123"
            };

            // Act
            var response = await _client.PostAsync("/api/functions/marketplace/purchases", 
                new StringContent(JsonSerializer.Serialize(purchase, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdPurchase = JsonSerializer.Deserialize<FunctionMarketplacePurchase>(responseContent, _jsonOptions);

            Assert.NotNull(createdPurchase);
            Assert.NotEqual(Guid.Empty, createdPurchase.Id);
            Assert.Equal(createdItem.Id, createdPurchase.ItemId);
            Assert.Equal(Guid.Parse(_testUserId), createdPurchase.BuyerId);
            Assert.Equal("Test User", createdPurchase.BuyerName);
            Assert.Equal(item.Price, createdPurchase.PricePaid);
            Assert.Equal(item.Currency, createdPurchase.Currency);
            Assert.Equal("credit_card", createdPurchase.PaymentMethod);
            Assert.Equal("ref123", createdPurchase.PaymentReference);
            Assert.NotNull(createdPurchase.PurchasedAt);
        }

        [Fact]
        public async Task GetMyPurchases_ExistingPurchases_ReturnsPurchases()
        {
            // Arrange
            var functionId1 = Guid.NewGuid();
            var functionId2 = Guid.NewGuid();
            var item1 = new FunctionMarketplaceItem
            {
                Name = "Item 1",
                Description = "First item",
                FunctionId = functionId1,
                Price = 5.0m,
                Currency = "USD",
                Category = "Utility",
                IsFree = false,
                IsPublished = true
            };
            var item2 = new FunctionMarketplaceItem
            {
                Name = "Item 2",
                Description = "Second item",
                FunctionId = functionId2,
                Price = 10.0m,
                Currency = "USD",
                Category = "Finance",
                IsFree = false,
                IsPublished = true
            };

            // Create functions
            await CreateTestFunction(functionId1);
            await CreateTestFunction(functionId2);

            // Create items
            var createItem1Response = await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item1, _jsonOptions), Encoding.UTF8, "application/json"));
            var createItem2Response = await _client.PostAsync("/api/functions/marketplace/items", 
                new StringContent(JsonSerializer.Serialize(item2, _jsonOptions), Encoding.UTF8, "application/json"));
            
            var createItem1ResponseContent = await createItem1Response.Content.ReadAsStringAsync();
            var createItem2ResponseContent = await createItem2Response.Content.ReadAsStringAsync();
            var createdItem1 = JsonSerializer.Deserialize<FunctionMarketplaceItem>(createItem1ResponseContent, _jsonOptions);
            var createdItem2 = JsonSerializer.Deserialize<FunctionMarketplaceItem>(createItem2ResponseContent, _jsonOptions);

            // Create purchases
            var purchase1 = new FunctionMarketplacePurchase { ItemId = createdItem1.Id, PaymentMethod = "credit_card" };
            var purchase2 = new FunctionMarketplacePurchase { ItemId = createdItem2.Id, PaymentMethod = "paypal" };

            await _client.PostAsync("/api/functions/marketplace/purchases", 
                new StringContent(JsonSerializer.Serialize(purchase1, _jsonOptions), Encoding.UTF8, "application/json"));
            await _client.PostAsync("/api/functions/marketplace/purchases", 
                new StringContent(JsonSerializer.Serialize(purchase2, _jsonOptions), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.GetAsync("/api/functions/marketplace/purchases/my");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var purchases = JsonSerializer.Deserialize<List<FunctionMarketplacePurchase>>(responseContent, _jsonOptions);

            Assert.NotNull(purchases);
            Assert.Equal(2, purchases.Count);
            Assert.Contains(purchases, p => p.ItemId == createdItem1.Id);
            Assert.Contains(purchases, p => p.ItemId == createdItem2.Id);
            Assert.All(purchases, p => Assert.Equal(Guid.Parse(_testUserId), p.BuyerId));
        }

        private async Task CreateTestFunction(Guid functionId, string code = "return {};")
        {
            var function = new Core.Models.Function
            {
                Id = functionId,
                Name = "Test Function",
                Description = "Function for testing",
                Runtime = "javascript",
                Code = code,
                AccountId = Guid.Parse(_testUserId),
                CreatedBy = Guid.Parse(_testUserId),
                UpdatedBy = Guid.Parse(_testUserId),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _client.PostAsync("/api/functions", 
                new StringContent(JsonSerializer.Serialize(function, _jsonOptions), Encoding.UTF8, "application/json"));
        }

        private string GenerateTestToken()
        {
            // In a real scenario, you would generate a proper JWT token
            // For testing purposes, we'll use a simple string that our test authentication handler will accept
            return "test_token_" + _testUserId;
        }
    }
}
