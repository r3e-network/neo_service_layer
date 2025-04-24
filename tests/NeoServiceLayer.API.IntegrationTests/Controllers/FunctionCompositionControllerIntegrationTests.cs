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
    public class FunctionCompositionControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _testUserId = Guid.NewGuid().ToString();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public FunctionCompositionControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
                    services.AddSingleton<IFunctionCompositionRepository, FunctionCompositionRepository>();
                    services.AddSingleton<IFunctionCompositionExecutionRepository, FunctionCompositionExecutionRepository>();
                    
                    // Register test services
                    services.AddSingleton<IFunctionService, FunctionService>();
                    services.AddSingleton<IFunctionCompositionService, FunctionCompositionService>();
                    services.AddSingleton<IFunctionExecutor, TestFunctionExecutor>();
                });
            });

            _client = _factory.CreateClient();
            
            // Add authentication header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateTestToken());
        }

        [Fact]
        public async Task CreateComposition_ValidComposition_ReturnsCreatedComposition()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Name = "Test Composition",
                Description = "Composition for integration testing",
                ExecutionMode = "sequential",
                MaxExecutionTime = 60000,
                ErrorHandlingStrategy = "stop",
                IsEnabled = true,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Name = "Step 1",
                        FunctionId = functionId,
                        TimeoutMs = 30000,
                        RetryPolicy = new FunctionRetryPolicy
                        {
                            MaxRetries = 3,
                            InitialDelayMs = 1000,
                            BackoffMultiplier = 2.0,
                            MaxDelayMs = 10000
                        }
                    }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Act
            var response = await _client.PostAsync("/api/functions/compositions", 
                new StringContent(JsonSerializer.Serialize(composition, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdComposition = JsonSerializer.Deserialize<FunctionComposition>(responseContent, _jsonOptions);

            Assert.NotNull(createdComposition);
            Assert.NotEqual(Guid.Empty, createdComposition.Id);
            Assert.Equal(composition.Name, createdComposition.Name);
            Assert.Equal(Guid.Parse(_testUserId), createdComposition.AccountId);
            Assert.NotNull(createdComposition.CreatedAt);
            Assert.NotNull(createdComposition.UpdatedAt);
            Assert.Single(createdComposition.Steps);
            Assert.Equal(composition.Steps[0].Name, createdComposition.Steps[0].Name);
            Assert.Equal(composition.Steps[0].FunctionId, createdComposition.Steps[0].FunctionId);
        }

        [Fact]
        public async Task GetCompositionById_ExistingComposition_ReturnsComposition()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Name = "Composition to Retrieve",
                Description = "Composition for retrieval testing",
                ExecutionMode = "sequential",
                MaxExecutionTime = 60000,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Name = "Step 1",
                        FunctionId = functionId,
                        TimeoutMs = 30000
                    }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create a composition
            var createResponse = await _client.PostAsync("/api/functions/compositions", 
                new StringContent(JsonSerializer.Serialize(composition, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdComposition = JsonSerializer.Deserialize<FunctionComposition>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/functions/compositions/{createdComposition.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var retrievedComposition = JsonSerializer.Deserialize<FunctionComposition>(responseContent, _jsonOptions);

            Assert.NotNull(retrievedComposition);
            Assert.Equal(createdComposition.Id, retrievedComposition.Id);
            Assert.Equal(composition.Name, retrievedComposition.Name);
            Assert.Equal(Guid.Parse(_testUserId), retrievedComposition.AccountId);
        }

        [Fact]
        public async Task GetCompositionsByAccountId_ExistingCompositions_ReturnsCompositions()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var composition1 = new FunctionComposition
            {
                Name = "Composition 1",
                Description = "First composition",
                ExecutionMode = "sequential",
                MaxExecutionTime = 60000,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Name = "Step 1",
                        FunctionId = functionId,
                        TimeoutMs = 30000
                    }
                }
            };
            var composition2 = new FunctionComposition
            {
                Name = "Composition 2",
                Description = "Second composition",
                ExecutionMode = "parallel",
                MaxExecutionTime = 120000,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Name = "Step 1",
                        FunctionId = functionId,
                        TimeoutMs = 30000
                    }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create compositions
            await _client.PostAsync("/api/functions/compositions", 
                new StringContent(JsonSerializer.Serialize(composition1, _jsonOptions), Encoding.UTF8, "application/json"));
            await _client.PostAsync("/api/functions/compositions", 
                new StringContent(JsonSerializer.Serialize(composition2, _jsonOptions), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.GetAsync($"/api/functions/compositions/account/{_testUserId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var compositions = JsonSerializer.Deserialize<List<FunctionComposition>>(responseContent, _jsonOptions);

            Assert.NotNull(compositions);
            Assert.Equal(2, compositions.Count);
            Assert.Contains(compositions, c => c.Name == composition1.Name);
            Assert.Contains(compositions, c => c.Name == composition2.Name);
        }

        [Fact]
        public async Task UpdateComposition_ExistingComposition_ReturnsUpdatedComposition()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Name = "Composition to Update",
                Description = "Original description",
                ExecutionMode = "sequential",
                MaxExecutionTime = 60000,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Name = "Step 1",
                        FunctionId = functionId,
                        TimeoutMs = 30000
                    }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create a composition
            var createResponse = await _client.PostAsync("/api/functions/compositions", 
                new StringContent(JsonSerializer.Serialize(composition, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdComposition = JsonSerializer.Deserialize<FunctionComposition>(createResponseContent, _jsonOptions);

            // Update the composition
            createdComposition.Description = "Updated description";
            createdComposition.ExecutionMode = "parallel";
            createdComposition.MaxExecutionTime = 120000;

            // Act
            var response = await _client.PutAsync($"/api/functions/compositions/{createdComposition.Id}", 
                new StringContent(JsonSerializer.Serialize(createdComposition, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedComposition = JsonSerializer.Deserialize<FunctionComposition>(responseContent, _jsonOptions);

            Assert.NotNull(updatedComposition);
            Assert.Equal(createdComposition.Id, updatedComposition.Id);
            Assert.Equal("Updated description", updatedComposition.Description);
            Assert.Equal("parallel", updatedComposition.ExecutionMode);
            Assert.Equal(120000, updatedComposition.MaxExecutionTime);
        }

        [Fact]
        public async Task DeleteComposition_ExistingComposition_ReturnsNoContent()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Name = "Composition to Delete",
                Description = "Composition for deletion",
                ExecutionMode = "sequential",
                MaxExecutionTime = 60000,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Name = "Step 1",
                        FunctionId = functionId,
                        TimeoutMs = 30000
                    }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create a composition
            var createResponse = await _client.PostAsync("/api/functions/compositions", 
                new StringContent(JsonSerializer.Serialize(composition, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdComposition = JsonSerializer.Deserialize<FunctionComposition>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.DeleteAsync($"/api/functions/compositions/{createdComposition.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the composition is deleted
            var getResponse = await _client.GetAsync($"/api/functions/compositions/{createdComposition.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task ExecuteComposition_ExistingComposition_ReturnsExecution()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Name = "Composition to Execute",
                Description = "Composition for execution",
                ExecutionMode = "sequential",
                MaxExecutionTime = 60000,
                IsEnabled = true,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Name = "Step 1",
                        FunctionId = functionId,
                        TimeoutMs = 30000
                    }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create a composition
            var createResponse = await _client.PostAsync("/api/functions/compositions", 
                new StringContent(JsonSerializer.Serialize(composition, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdComposition = JsonSerializer.Deserialize<FunctionComposition>(createResponseContent, _jsonOptions);

            // Prepare input parameters
            var inputParameters = new Dictionary<string, object>
            {
                { "param1", "value1" },
                { "param2", 42 }
            };

            // Act
            var response = await _client.PostAsync($"/api/functions/compositions/{createdComposition.Id}/execute", 
                new StringContent(JsonSerializer.Serialize(inputParameters, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var execution = JsonSerializer.Deserialize<FunctionCompositionExecution>(responseContent, _jsonOptions);

            Assert.NotNull(execution);
            Assert.Equal(createdComposition.Id, execution.CompositionId);
            Assert.Equal(Guid.Parse(_testUserId), execution.AccountId);
            Assert.Equal("completed", execution.Status);
            Assert.NotNull(execution.StartTime);
            Assert.NotNull(execution.EndTime);
            Assert.NotNull(execution.ExecutionTimeMs);
            Assert.NotEmpty(execution.StepExecutions);
            Assert.Equal(functionId, execution.StepExecutions[0].FunctionId);
        }

        [Fact]
        public async Task AddStep_ExistingComposition_ReturnsUpdatedComposition()
        {
            // Arrange
            var functionId1 = Guid.NewGuid();
            var functionId2 = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Name = "Composition for Step Addition",
                Description = "Composition for step addition",
                ExecutionMode = "sequential",
                MaxExecutionTime = 60000,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Name = "Step 1",
                        FunctionId = functionId1,
                        TimeoutMs = 30000
                    }
                }
            };

            // Create functions
            await CreateTestFunction(functionId1);
            await CreateTestFunction(functionId2);

            // Create a composition
            var createResponse = await _client.PostAsync("/api/functions/compositions", 
                new StringContent(JsonSerializer.Serialize(composition, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdComposition = JsonSerializer.Deserialize<FunctionComposition>(createResponseContent, _jsonOptions);

            // Create a new step
            var newStep = new FunctionCompositionStep
            {
                Name = "Step 2",
                FunctionId = functionId2,
                TimeoutMs = 45000,
                Dependencies = new List<Guid> { createdComposition.Steps[0].Id }
            };

            // Act
            var response = await _client.PostAsync($"/api/functions/compositions/{createdComposition.Id}/steps", 
                new StringContent(JsonSerializer.Serialize(newStep, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedComposition = JsonSerializer.Deserialize<FunctionComposition>(responseContent, _jsonOptions);

            Assert.NotNull(updatedComposition);
            Assert.Equal(2, updatedComposition.Steps.Count);
            Assert.Equal("Step 1", updatedComposition.Steps[0].Name);
            Assert.Equal("Step 2", updatedComposition.Steps[1].Name);
            Assert.Equal(functionId2, updatedComposition.Steps[1].FunctionId);
            Assert.Equal(45000, updatedComposition.Steps[1].TimeoutMs);
            Assert.Contains(updatedComposition.Steps[0].Id, updatedComposition.Steps[1].Dependencies);
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

    /// <summary>
    /// Test function executor for integration tests
    /// </summary>
    public class TestFunctionExecutor : IFunctionExecutor
    {
        public Task<object> ExecuteAsync(Guid functionId, Dictionary<string, object> parameters, FunctionExecutionContext context)
        {
            // Return a mock result
            return Task.FromResult<object>(new Dictionary<string, object>
            {
                { "result", "success" },
                { "executedAt", DateTime.UtcNow },
                { "parameters", parameters }
            });
        }
    }
}
