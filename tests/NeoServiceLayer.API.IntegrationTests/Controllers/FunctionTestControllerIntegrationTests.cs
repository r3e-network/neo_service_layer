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
    public class FunctionTestControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _testUserId = Guid.NewGuid().ToString();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public FunctionTestControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
                    services.AddSingleton<IFunctionTestRepository, FunctionTestRepository>();
                    services.AddSingleton<IFunctionTestResultRepository, FunctionTestResultRepository>();
                    services.AddSingleton<IFunctionTestSuiteRepository, FunctionTestSuiteRepository>();
                    
                    // Register test services
                    services.AddSingleton<IFunctionService, FunctionService>();
                    services.AddSingleton<IFunctionTestService, FunctionTestService>();
                });
            });

            _client = _factory.CreateClient();
            
            // Add authentication header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateTestToken());
        }

        [Fact]
        public async Task CreateTest_ValidTest_ReturnsCreatedTest()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var test = new FunctionTest
            {
                Name = "Integration Test",
                Description = "Test for integration testing",
                FunctionId = functionId,
                InputParameters = new Dictionary<string, object>
                {
                    { "param1", "value1" },
                    { "param2", 42 }
                },
                ExpectedOutput = new Dictionary<string, object>
                {
                    { "result", "success" }
                },
                Tags = new List<string> { "integration", "test" }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Act
            var response = await _client.PostAsync("/api/functions/tests", 
                new StringContent(JsonSerializer.Serialize(test, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdTest = JsonSerializer.Deserialize<FunctionTest>(responseContent, _jsonOptions);

            Assert.NotNull(createdTest);
            Assert.NotEqual(Guid.Empty, createdTest.Id);
            Assert.Equal(test.Name, createdTest.Name);
            Assert.Equal(test.FunctionId, createdTest.FunctionId);
            Assert.NotNull(createdTest.CreatedAt);
            Assert.NotNull(createdTest.UpdatedAt);
        }

        [Fact]
        public async Task GetTestById_ExistingTest_ReturnsTest()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var test = new FunctionTest
            {
                Name = "Test to Retrieve",
                Description = "Test for retrieval testing",
                FunctionId = functionId,
                InputParameters = new Dictionary<string, object>
                {
                    { "param1", "value1" }
                },
                ExpectedOutput = new Dictionary<string, object>
                {
                    { "result", "success" }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create a test
            var createResponse = await _client.PostAsync("/api/functions/tests", 
                new StringContent(JsonSerializer.Serialize(test, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdTest = JsonSerializer.Deserialize<FunctionTest>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.GetAsync($"/api/functions/tests/{createdTest.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var retrievedTest = JsonSerializer.Deserialize<FunctionTest>(responseContent, _jsonOptions);

            Assert.NotNull(retrievedTest);
            Assert.Equal(createdTest.Id, retrievedTest.Id);
            Assert.Equal(test.Name, retrievedTest.Name);
            Assert.Equal(test.FunctionId, retrievedTest.FunctionId);
        }

        [Fact]
        public async Task GetTestsByFunctionId_ExistingTests_ReturnsTests()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var test1 = new FunctionTest
            {
                Name = "Test 1",
                Description = "First test",
                FunctionId = functionId,
                InputParameters = new Dictionary<string, object>
                {
                    { "param1", "value1" }
                }
            };
            var test2 = new FunctionTest
            {
                Name = "Test 2",
                Description = "Second test",
                FunctionId = functionId,
                InputParameters = new Dictionary<string, object>
                {
                    { "param1", "value2" }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create tests
            await _client.PostAsync("/api/functions/tests", 
                new StringContent(JsonSerializer.Serialize(test1, _jsonOptions), Encoding.UTF8, "application/json"));
            await _client.PostAsync("/api/functions/tests", 
                new StringContent(JsonSerializer.Serialize(test2, _jsonOptions), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.GetAsync($"/api/functions/tests/function/{functionId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var tests = JsonSerializer.Deserialize<List<FunctionTest>>(responseContent, _jsonOptions);

            Assert.NotNull(tests);
            Assert.Equal(2, tests.Count);
            Assert.Contains(tests, t => t.Name == test1.Name);
            Assert.Contains(tests, t => t.Name == test2.Name);
        }

        [Fact]
        public async Task UpdateTest_ExistingTest_ReturnsUpdatedTest()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var test = new FunctionTest
            {
                Name = "Test to Update",
                Description = "Original description",
                FunctionId = functionId,
                InputParameters = new Dictionary<string, object>
                {
                    { "param1", "value1" }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create a test
            var createResponse = await _client.PostAsync("/api/functions/tests", 
                new StringContent(JsonSerializer.Serialize(test, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdTest = JsonSerializer.Deserialize<FunctionTest>(createResponseContent, _jsonOptions);

            // Update the test
            createdTest.Description = "Updated description";
            createdTest.InputParameters = new Dictionary<string, object>
            {
                { "param1", "updated_value" }
            };

            // Act
            var response = await _client.PutAsync($"/api/functions/tests/{createdTest.Id}", 
                new StringContent(JsonSerializer.Serialize(createdTest, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedTest = JsonSerializer.Deserialize<FunctionTest>(responseContent, _jsonOptions);

            Assert.NotNull(updatedTest);
            Assert.Equal(createdTest.Id, updatedTest.Id);
            Assert.Equal("Updated description", updatedTest.Description);
            Assert.Equal("updated_value", updatedTest.InputParameters["param1"]);
        }

        [Fact]
        public async Task DeleteTest_ExistingTest_ReturnsNoContent()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var test = new FunctionTest
            {
                Name = "Test to Delete",
                Description = "Test for deletion",
                FunctionId = functionId
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create a test
            var createResponse = await _client.PostAsync("/api/functions/tests", 
                new StringContent(JsonSerializer.Serialize(test, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdTest = JsonSerializer.Deserialize<FunctionTest>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.DeleteAsync($"/api/functions/tests/{createdTest.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the test is deleted
            var getResponse = await _client.GetAsync($"/api/functions/tests/{createdTest.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task RunTest_ExistingTest_ReturnsTestResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var test = new FunctionTest
            {
                Name = "Test to Run",
                Description = "Test for running",
                FunctionId = functionId,
                InputParameters = new Dictionary<string, object>
                {
                    { "param1", "value1" }
                },
                ExpectedOutput = new Dictionary<string, object>
                {
                    { "result", "success" }
                }
            };

            // Create a function first with mock implementation
            await CreateTestFunction(functionId, "return { result: 'success' };");

            // Create a test
            var createResponse = await _client.PostAsync("/api/functions/tests", 
                new StringContent(JsonSerializer.Serialize(test, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdTest = JsonSerializer.Deserialize<FunctionTest>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.PostAsync($"/api/functions/tests/{createdTest.Id}/run", null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var testResult = JsonSerializer.Deserialize<FunctionTestResult>(responseContent, _jsonOptions);

            Assert.NotNull(testResult);
            Assert.Equal(createdTest.Id, testResult.TestId);
            Assert.Equal("passed", testResult.Status);
            Assert.NotNull(testResult.ExecutionTime);
            Assert.NotNull(testResult.StartTime);
            Assert.NotNull(testResult.EndTime);
        }

        [Fact]
        public async Task GetTestResults_ExistingTest_ReturnsResults()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var test = new FunctionTest
            {
                Name = "Test with Results",
                Description = "Test for results",
                FunctionId = functionId,
                InputParameters = new Dictionary<string, object>
                {
                    { "param1", "value1" }
                },
                ExpectedOutput = new Dictionary<string, object>
                {
                    { "result", "success" }
                }
            };

            // Create a function first with mock implementation
            await CreateTestFunction(functionId, "return { result: 'success' };");

            // Create a test
            var createResponse = await _client.PostAsync("/api/functions/tests", 
                new StringContent(JsonSerializer.Serialize(test, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdTest = JsonSerializer.Deserialize<FunctionTest>(createResponseContent, _jsonOptions);

            // Run the test twice
            await _client.PostAsync($"/api/functions/tests/{createdTest.Id}/run", null);
            await _client.PostAsync($"/api/functions/tests/{createdTest.Id}/run", null);

            // Act
            var response = await _client.GetAsync($"/api/functions/tests/{createdTest.Id}/results");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var results = JsonSerializer.Deserialize<List<FunctionTestResult>>(responseContent, _jsonOptions);

            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.All(results, r => Assert.Equal(createdTest.Id, r.TestId));
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
