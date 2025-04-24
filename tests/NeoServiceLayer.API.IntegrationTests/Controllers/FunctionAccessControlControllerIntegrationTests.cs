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
    public class FunctionAccessControlControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _testUserId = Guid.NewGuid().ToString();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public FunctionAccessControlControllerIntegrationTests(WebApplicationFactory<Program> factory)
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
                    services.AddSingleton<IFunctionPermissionRepository, FunctionPermissionRepository>();
                    services.AddSingleton<IFunctionAccessPolicyRepository, FunctionAccessPolicyRepository>();
                    services.AddSingleton<IFunctionAccessRequestRepository, FunctionAccessRequestRepository>();
                    
                    // Register test services
                    services.AddSingleton<IFunctionService, FunctionService>();
                    services.AddSingleton<IFunctionAccessControlService, FunctionAccessControlService>();
                });
            });

            _client = _factory.CreateClient();
            
            // Add authentication header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateTestToken());
        }

        [Fact]
        public async Task GrantPermission_ValidPermission_ReturnsCreatedPermission()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var principalId = "user123";
            var principalType = "user";
            var permissionLevel = "read";
            var allowedOperations = new List<string> { "read", "execute" };
            var deniedOperations = new List<string> { "write", "delete" };
            var expiresAt = DateTime.UtcNow.AddDays(30);

            // Create a function first
            await CreateTestFunction(functionId);

            // Act
            var response = await _client.PostAsync(
                $"/api/functions/access/permissions?functionId={functionId}&principalId={principalId}&principalType={principalType}&permissionLevel={permissionLevel}&allowedOperations={string.Join("&allowedOperations=", allowedOperations)}&deniedOperations={string.Join("&deniedOperations=", deniedOperations)}&expiresAt={expiresAt:o}",
                null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var permission = JsonSerializer.Deserialize<FunctionPermission>(responseContent, _jsonOptions);

            Assert.NotNull(permission);
            Assert.NotEqual(Guid.Empty, permission.Id);
            Assert.Equal(functionId, permission.FunctionId);
            Assert.Equal(principalId, permission.PrincipalId);
            Assert.Equal(principalType, permission.PrincipalType);
            Assert.Equal(permissionLevel, permission.PermissionLevel);
            Assert.Equal(allowedOperations, permission.AllowedOperations);
            Assert.Equal(deniedOperations, permission.DeniedOperations);
            Assert.Equal(expiresAt.ToUniversalTime().ToString("o"), permission.ExpiresAt?.ToUniversalTime().ToString("o"));
            Assert.NotNull(permission.CreatedAt);
        }

        [Fact]
        public async Task GetPermissions_ExistingPermissions_ReturnsPermissions()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var principalId1 = "user123";
            var principalId2 = "group456";
            var principalType1 = "user";
            var principalType2 = "group";
            var permissionLevel = "read";

            // Create a function first
            await CreateTestFunction(functionId);

            // Create permissions
            await _client.PostAsync(
                $"/api/functions/access/permissions?functionId={functionId}&principalId={principalId1}&principalType={principalType1}&permissionLevel={permissionLevel}",
                null);
            await _client.PostAsync(
                $"/api/functions/access/permissions?functionId={functionId}&principalId={principalId2}&principalType={principalType2}&permissionLevel={permissionLevel}",
                null);

            // Act
            var response = await _client.GetAsync($"/api/functions/access/permissions/function/{functionId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var permissions = JsonSerializer.Deserialize<List<FunctionPermission>>(responseContent, _jsonOptions);

            Assert.NotNull(permissions);
            Assert.Equal(2, permissions.Count);
            Assert.Contains(permissions, p => p.PrincipalId == principalId1 && p.PrincipalType == principalType1);
            Assert.Contains(permissions, p => p.PrincipalId == principalId2 && p.PrincipalType == principalType2);
            Assert.All(permissions, p => Assert.Equal(functionId, p.FunctionId));
            Assert.All(permissions, p => Assert.Equal(permissionLevel, p.PermissionLevel));
        }

        [Fact]
        public async Task GetPermissionsByPrincipal_ExistingPermissions_ReturnsPermissions()
        {
            // Arrange
            var functionId1 = Guid.NewGuid();
            var functionId2 = Guid.NewGuid();
            var principalId = "user123";
            var principalType = "user";
            var permissionLevel = "read";

            // Create functions
            await CreateTestFunction(functionId1);
            await CreateTestFunction(functionId2);

            // Create permissions
            await _client.PostAsync(
                $"/api/functions/access/permissions?functionId={functionId1}&principalId={principalId}&principalType={principalType}&permissionLevel={permissionLevel}",
                null);
            await _client.PostAsync(
                $"/api/functions/access/permissions?functionId={functionId2}&principalId={principalId}&principalType={principalType}&permissionLevel={permissionLevel}",
                null);

            // Act
            var response = await _client.GetAsync($"/api/functions/access/permissions/principal?principalId={principalId}&principalType={principalType}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var permissions = JsonSerializer.Deserialize<List<FunctionPermission>>(responseContent, _jsonOptions);

            Assert.NotNull(permissions);
            Assert.Equal(2, permissions.Count);
            Assert.Contains(permissions, p => p.FunctionId == functionId1);
            Assert.Contains(permissions, p => p.FunctionId == functionId2);
            Assert.All(permissions, p => Assert.Equal(principalId, p.PrincipalId));
            Assert.All(permissions, p => Assert.Equal(principalType, p.PrincipalType));
            Assert.All(permissions, p => Assert.Equal(permissionLevel, p.PermissionLevel));
        }

        [Fact]
        public async Task HasPermission_ExistingPermission_ReturnsTrue()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var principalId = "user123";
            var principalType = "user";
            var permissionLevel = "read";
            var operation = "read";

            // Create a function first
            await CreateTestFunction(functionId);

            // Create permission
            await _client.PostAsync(
                $"/api/functions/access/permissions?functionId={functionId}&principalId={principalId}&principalType={principalType}&permissionLevel={permissionLevel}",
                null);

            // Act
            var response = await _client.GetAsync($"/api/functions/access/check?functionId={functionId}&principalId={principalId}&principalType={principalType}&operation={operation}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var hasPermission = JsonSerializer.Deserialize<bool>(responseContent, _jsonOptions);

            Assert.True(hasPermission);
        }

        [Fact]
        public async Task RevokePermission_ExistingPermission_ReturnsNoContent()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var principalId = "user123";
            var principalType = "user";
            var permissionLevel = "read";

            // Create a function first
            await CreateTestFunction(functionId);

            // Create permission
            var createResponse = await _client.PostAsync(
                $"/api/functions/access/permissions?functionId={functionId}&principalId={principalId}&principalType={principalType}&permissionLevel={permissionLevel}",
                null);
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var permission = JsonSerializer.Deserialize<FunctionPermission>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.DeleteAsync($"/api/functions/access/permissions/{permission.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the permission is revoked
            var checkResponse = await _client.GetAsync($"/api/functions/access/check?functionId={functionId}&principalId={principalId}&principalType={principalType}&operation=read");
            checkResponse.EnsureSuccessStatusCode();
            
            var checkResponseContent = await checkResponse.Content.ReadAsStringAsync();
            var hasPermission = JsonSerializer.Deserialize<bool>(checkResponseContent, _jsonOptions);

            Assert.False(hasPermission);
        }

        [Fact]
        public async Task CreateAccessPolicy_ValidPolicy_ReturnsCreatedPolicy()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var policy = new FunctionAccessPolicy
            {
                Name = "Test Policy",
                Description = "Policy for integration testing",
                FunctionId = functionId,
                IsEnabled = true,
                Conditions = new Dictionary<string, object>
                {
                    { "ip", "192.168.1.1" },
                    { "time", "business_hours" }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Act
            var response = await _client.PostAsync("/api/functions/access/policies", 
                new StringContent(JsonSerializer.Serialize(policy, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdPolicy = JsonSerializer.Deserialize<FunctionAccessPolicy>(responseContent, _jsonOptions);

            Assert.NotNull(createdPolicy);
            Assert.NotEqual(Guid.Empty, createdPolicy.Id);
            Assert.Equal(policy.Name, createdPolicy.Name);
            Assert.Equal(policy.Description, createdPolicy.Description);
            Assert.Equal(policy.FunctionId, createdPolicy.FunctionId);
            Assert.True(createdPolicy.IsEnabled);
            Assert.Equal(policy.Conditions.Count, createdPolicy.Conditions.Count);
            Assert.Equal("192.168.1.1", createdPolicy.Conditions["ip"]);
            Assert.Equal("business_hours", createdPolicy.Conditions["time"]);
            Assert.NotNull(createdPolicy.CreatedAt);
        }

        [Fact]
        public async Task GetAccessPolicies_ExistingPolicies_ReturnsPolicies()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var policy1 = new FunctionAccessPolicy
            {
                Name = "Policy 1",
                Description = "First policy",
                FunctionId = functionId,
                IsEnabled = true,
                Conditions = new Dictionary<string, object>
                {
                    { "ip", "192.168.1.1" }
                }
            };
            var policy2 = new FunctionAccessPolicy
            {
                Name = "Policy 2",
                Description = "Second policy",
                FunctionId = functionId,
                IsEnabled = true,
                Conditions = new Dictionary<string, object>
                {
                    { "time", "business_hours" }
                }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create policies
            await _client.PostAsync("/api/functions/access/policies", 
                new StringContent(JsonSerializer.Serialize(policy1, _jsonOptions), Encoding.UTF8, "application/json"));
            await _client.PostAsync("/api/functions/access/policies", 
                new StringContent(JsonSerializer.Serialize(policy2, _jsonOptions), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.GetAsync($"/api/functions/access/policies/function/{functionId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var policies = JsonSerializer.Deserialize<List<FunctionAccessPolicy>>(responseContent, _jsonOptions);

            Assert.NotNull(policies);
            Assert.Equal(2, policies.Count);
            Assert.Contains(policies, p => p.Name == policy1.Name);
            Assert.Contains(policies, p => p.Name == policy2.Name);
            Assert.All(policies, p => Assert.Equal(functionId, p.FunctionId));
            Assert.All(policies, p => Assert.True(p.IsEnabled));
        }

        [Fact]
        public async Task EvaluateAccessPolicies_ValidContext_ReturnsResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var policy = new FunctionAccessPolicy
            {
                Name = "IP Policy",
                Description = "Policy for IP address",
                FunctionId = functionId,
                IsEnabled = true,
                Conditions = new Dictionary<string, object>
                {
                    { "ip", "192.168.1.1" }
                }
            };
            var context = new Dictionary<string, object>
            {
                { "ip", "192.168.1.1" }
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create policy
            await _client.PostAsync("/api/functions/access/policies", 
                new StringContent(JsonSerializer.Serialize(policy, _jsonOptions), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.PostAsync($"/api/functions/access/policies/evaluate?functionId={functionId}", 
                new StringContent(JsonSerializer.Serialize(context, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<bool>(responseContent, _jsonOptions);

            Assert.True(result);
        }

        [Fact]
        public async Task CreateAccessRequest_ValidRequest_ReturnsCreatedRequest()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var request = new FunctionAccessRequest
            {
                FunctionId = functionId,
                PrincipalId = "user123",
                PrincipalType = "user",
                RequestedPermissionLevel = "read",
                RequestedOperations = new List<string> { "read", "execute" },
                Reason = "Need to read and execute this function"
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Act
            var response = await _client.PostAsync("/api/functions/access/requests", 
                new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdRequest = JsonSerializer.Deserialize<FunctionAccessRequest>(responseContent, _jsonOptions);

            Assert.NotNull(createdRequest);
            Assert.NotEqual(Guid.Empty, createdRequest.Id);
            Assert.Equal(request.FunctionId, createdRequest.FunctionId);
            Assert.Equal(request.PrincipalId, createdRequest.PrincipalId);
            Assert.Equal(request.PrincipalType, createdRequest.PrincipalType);
            Assert.Equal(request.RequestedPermissionLevel, createdRequest.RequestedPermissionLevel);
            Assert.Equal(request.RequestedOperations, createdRequest.RequestedOperations);
            Assert.Equal(request.Reason, createdRequest.Reason);
            Assert.Equal("pending", createdRequest.Status);
            Assert.NotNull(createdRequest.RequestedAt);
        }

        [Fact]
        public async Task GetAccessRequests_ExistingRequests_ReturnsRequests()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var request1 = new FunctionAccessRequest
            {
                FunctionId = functionId,
                PrincipalId = "user123",
                PrincipalType = "user",
                RequestedPermissionLevel = "read",
                Reason = "Need to read this function"
            };
            var request2 = new FunctionAccessRequest
            {
                FunctionId = functionId,
                PrincipalId = "user456",
                PrincipalType = "user",
                RequestedPermissionLevel = "write",
                Reason = "Need to write to this function"
            };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create requests
            await _client.PostAsync("/api/functions/access/requests", 
                new StringContent(JsonSerializer.Serialize(request1, _jsonOptions), Encoding.UTF8, "application/json"));
            await _client.PostAsync("/api/functions/access/requests", 
                new StringContent(JsonSerializer.Serialize(request2, _jsonOptions), Encoding.UTF8, "application/json"));

            // Act
            var response = await _client.GetAsync($"/api/functions/access/requests/function/{functionId}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var requests = JsonSerializer.Deserialize<List<FunctionAccessRequest>>(responseContent, _jsonOptions);

            Assert.NotNull(requests);
            Assert.Equal(2, requests.Count);
            Assert.Contains(requests, r => r.PrincipalId == request1.PrincipalId && r.RequestedPermissionLevel == request1.RequestedPermissionLevel);
            Assert.Contains(requests, r => r.PrincipalId == request2.PrincipalId && r.RequestedPermissionLevel == request2.RequestedPermissionLevel);
            Assert.All(requests, r => Assert.Equal(functionId, r.FunctionId));
            Assert.All(requests, r => Assert.Equal("pending", r.Status));
        }

        [Fact]
        public async Task ApproveAccessRequest_PendingRequest_ReturnsApprovedRequest()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var request = new FunctionAccessRequest
            {
                FunctionId = functionId,
                PrincipalId = "user123",
                PrincipalType = "user",
                RequestedPermissionLevel = "read",
                RequestedOperations = new List<string> { "read", "execute" },
                Reason = "Need to read and execute this function"
            };
            var approverId = "approver456";
            var reason = "Approved for business needs";
            var expiresAt = DateTime.UtcNow.AddDays(30);
            var grantedOperations = new List<string> { "read" };

            // Create a function first
            await CreateTestFunction(functionId);

            // Create request
            var createResponse = await _client.PostAsync("/api/functions/access/requests", 
                new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdRequest = JsonSerializer.Deserialize<FunctionAccessRequest>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.PostAsync(
                $"/api/functions/access/requests/{createdRequest.Id}/approve?approverId={approverId}&reason={reason}&expiresAt={expiresAt:o}&grantedOperations={string.Join("&grantedOperations=", grantedOperations)}",
                null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var approvedRequest = JsonSerializer.Deserialize<FunctionAccessRequest>(responseContent, _jsonOptions);

            Assert.NotNull(approvedRequest);
            Assert.Equal(createdRequest.Id, approvedRequest.Id);
            Assert.Equal("approved", approvedRequest.Status);
            Assert.Equal(approverId, approvedRequest.ApproverId);
            Assert.Equal(reason, approvedRequest.ApprovalReason);
            Assert.Equal(expiresAt.ToUniversalTime().ToString("o"), approvedRequest.ExpiresAt?.ToUniversalTime().ToString("o"));
            Assert.Equal(grantedOperations, approvedRequest.GrantedOperations);
            Assert.NotNull(approvedRequest.ApprovedAt);

            // Verify that a permission was created
            var checkResponse = await _client.GetAsync($"/api/functions/access/check?functionId={functionId}&principalId={request.PrincipalId}&principalType={request.PrincipalType}&operation=read");
            checkResponse.EnsureSuccessStatusCode();
            
            var checkResponseContent = await checkResponse.Content.ReadAsStringAsync();
            var hasPermission = JsonSerializer.Deserialize<bool>(checkResponseContent, _jsonOptions);

            Assert.True(hasPermission);
        }

        [Fact]
        public async Task RejectAccessRequest_PendingRequest_ReturnsRejectedRequest()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var request = new FunctionAccessRequest
            {
                FunctionId = functionId,
                PrincipalId = "user123",
                PrincipalType = "user",
                RequestedPermissionLevel = "read",
                Reason = "Need to read this function"
            };
            var approverId = "approver456";
            var reason = "Rejected due to security concerns";

            // Create a function first
            await CreateTestFunction(functionId);

            // Create request
            var createResponse = await _client.PostAsync("/api/functions/access/requests", 
                new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json"));
            createResponse.EnsureSuccessStatusCode();
            
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createdRequest = JsonSerializer.Deserialize<FunctionAccessRequest>(createResponseContent, _jsonOptions);

            // Act
            var response = await _client.PostAsync(
                $"/api/functions/access/requests/{createdRequest.Id}/reject?approverId={approverId}&reason={reason}",
                null);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var rejectedRequest = JsonSerializer.Deserialize<FunctionAccessRequest>(responseContent, _jsonOptions);

            Assert.NotNull(rejectedRequest);
            Assert.Equal(createdRequest.Id, rejectedRequest.Id);
            Assert.Equal("rejected", rejectedRequest.Status);
            Assert.Equal(approverId, rejectedRequest.ApproverId);
            Assert.Equal(reason, rejectedRequest.RejectionReason);
            Assert.NotNull(rejectedRequest.RejectedAt);

            // Verify that no permission was created
            var checkResponse = await _client.GetAsync($"/api/functions/access/check?functionId={functionId}&principalId={request.PrincipalId}&principalType={request.PrincipalType}&operation=read");
            checkResponse.EnsureSuccessStatusCode();
            
            var checkResponseContent = await checkResponse.Content.ReadAsStringAsync();
            var hasPermission = JsonSerializer.Deserialize<bool>(checkResponseContent, _jsonOptions);

            Assert.False(hasPermission);
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
