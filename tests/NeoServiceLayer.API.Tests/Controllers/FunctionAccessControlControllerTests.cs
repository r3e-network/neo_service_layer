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
    public class FunctionAccessControlControllerTests
    {
        private readonly Mock<ILogger<FunctionAccessControlController>> _loggerMock;
        private readonly Mock<IFunctionAccessControlService> _accessControlServiceMock;
        private readonly FunctionAccessControlController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public FunctionAccessControlControllerTests()
        {
            _loggerMock = new Mock<ILogger<FunctionAccessControlController>>();
            _accessControlServiceMock = new Mock<IFunctionAccessControlService>();
            _controller = new FunctionAccessControlController(_loggerMock.Object, _accessControlServiceMock.Object);

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
        public async Task HasPermissionAsync_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var principalId = "user123";
            var principalType = "user";
            var operation = "read";
            var hasPermission = true;

            _accessControlServiceMock.Setup(service => service.HasPermissionAsync(functionId, principalId, principalType, operation))
                .ReturnsAsync(hasPermission);

            // Act
            var result = await _controller.HasPermissionAsync(functionId, principalId, principalType, operation);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(returnValue);
        }

        [Fact]
        public async Task GetPermissionsAsync_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var permissions = new List<FunctionPermission>
            {
                new FunctionPermission { Id = Guid.NewGuid(), FunctionId = functionId, PrincipalId = "user1", PrincipalType = "user" },
                new FunctionPermission { Id = Guid.NewGuid(), FunctionId = functionId, PrincipalId = "group1", PrincipalType = "group" }
            };

            _accessControlServiceMock.Setup(service => service.GetPermissionsAsync(functionId))
                .ReturnsAsync(permissions);

            // Act
            var result = await _controller.GetPermissionsAsync(functionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionPermission>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionPermission>)returnValue).Count);
        }

        [Fact]
        public async Task GetPermissionsByPrincipalAsync_ReturnsOkResult()
        {
            // Arrange
            var principalId = "user123";
            var principalType = "user";
            var permissions = new List<FunctionPermission>
            {
                new FunctionPermission { Id = Guid.NewGuid(), FunctionId = Guid.NewGuid(), PrincipalId = principalId, PrincipalType = principalType },
                new FunctionPermission { Id = Guid.NewGuid(), FunctionId = Guid.NewGuid(), PrincipalId = principalId, PrincipalType = principalType }
            };

            _accessControlServiceMock.Setup(service => service.GetPermissionsByPrincipalAsync(principalId, principalType))
                .ReturnsAsync(permissions);

            // Act
            var result = await _controller.GetPermissionsByPrincipalAsync(principalId, principalType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionPermission>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionPermission>)returnValue).Count);
        }

        [Fact]
        public async Task GrantPermissionAsync_ReturnsCreatedAtAction()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var principalId = "user123";
            var principalType = "user";
            var permissionLevel = "read";
            var allowedOperations = new List<string> { "read", "execute" };
            var deniedOperations = new List<string> { "write", "delete" };
            var expiresAt = DateTime.UtcNow.AddDays(30);

            var permission = new FunctionPermission
            {
                Id = Guid.NewGuid(),
                FunctionId = functionId,
                PrincipalId = principalId,
                PrincipalType = principalType,
                PermissionLevel = permissionLevel,
                AllowedOperations = allowedOperations,
                DeniedOperations = deniedOperations,
                ExpiresAt = expiresAt
            };

            _accessControlServiceMock.Setup(service => service.GrantPermissionAsync(
                    functionId, principalId, principalType, permissionLevel, allowedOperations, deniedOperations, expiresAt))
                .ReturnsAsync(permission);

            // Act
            var result = await _controller.GrantPermissionAsync(
                functionId, principalId, principalType, permissionLevel, allowedOperations, deniedOperations, expiresAt);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<FunctionPermission>(createdAtActionResult.Value);
            Assert.Equal(functionId, returnValue.FunctionId);
            Assert.Equal(principalId, returnValue.PrincipalId);
            Assert.Equal(principalType, returnValue.PrincipalType);
            Assert.Equal(permissionLevel, returnValue.PermissionLevel);
        }

        [Fact]
        public async Task RevokePermissionAsync_ExistingPermission_ReturnsNoContent()
        {
            // Arrange
            var permissionId = Guid.NewGuid();

            _accessControlServiceMock.Setup(service => service.RevokePermissionAsync(permissionId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RevokePermissionAsync(permissionId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RevokePermissionAsync_NonExistingPermission_ReturnsNotFound()
        {
            // Arrange
            var permissionId = Guid.NewGuid();

            _accessControlServiceMock.Setup(service => service.RevokePermissionAsync(permissionId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.RevokePermissionAsync(permissionId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RevokeAllPermissionsAsync_ReturnsNoContent()
        {
            // Arrange
            var functionId = Guid.NewGuid();

            _accessControlServiceMock.Setup(service => service.RevokeAllPermissionsAsync(functionId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RevokeAllPermissionsAsync(functionId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RevokeAllPermissionsByPrincipalAsync_ReturnsNoContent()
        {
            // Arrange
            var principalId = "user123";
            var principalType = "user";

            _accessControlServiceMock.Setup(service => service.RevokeAllPermissionsByPrincipalAsync(principalId, principalType))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RevokeAllPermissionsByPrincipalAsync(principalId, principalType);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdatePermissionAsync_ValidPermission_ReturnsOkResult()
        {
            // Arrange
            var permissionId = Guid.NewGuid();
            var permission = new FunctionPermission
            {
                Id = permissionId,
                FunctionId = Guid.NewGuid(),
                PrincipalId = "user123",
                PrincipalType = "user",
                PermissionLevel = "read"
            };

            _accessControlServiceMock.Setup(service => service.UpdatePermissionAsync(permission))
                .ReturnsAsync(permission);

            // Act
            var result = await _controller.UpdatePermissionAsync(permissionId, permission);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionPermission>(okResult.Value);
            Assert.Equal(permissionId, returnValue.Id);
        }

        [Fact]
        public async Task UpdatePermissionAsync_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var permissionId = Guid.NewGuid();
            var permission = new FunctionPermission
            {
                Id = Guid.NewGuid(), // Different ID
                FunctionId = Guid.NewGuid(),
                PrincipalId = "user123",
                PrincipalType = "user",
                PermissionLevel = "read"
            };

            // Act
            var result = await _controller.UpdatePermissionAsync(permissionId, permission);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("ID in the path does not match ID in the body", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateAccessPolicyAsync_ValidPolicy_ReturnsCreatedAtAction()
        {
            // Arrange
            var policy = new FunctionAccessPolicy
            {
                Name = "Test Policy",
                FunctionId = Guid.NewGuid(),
                Description = "Test policy description",
                Conditions = new Dictionary<string, object>
                {
                    { "ip", "192.168.1.1" },
                    { "time", "business_hours" }
                }
            };
            var createdPolicy = new FunctionAccessPolicy
            {
                Id = Guid.NewGuid(),
                Name = policy.Name,
                FunctionId = policy.FunctionId,
                Description = policy.Description,
                Conditions = policy.Conditions
            };

            _accessControlServiceMock.Setup(service => service.CreateAccessPolicyAsync(It.IsAny<FunctionAccessPolicy>()))
                .ReturnsAsync(createdPolicy);

            // Act
            var result = await _controller.CreateAccessPolicyAsync(policy);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<FunctionAccessPolicy>(createdAtActionResult.Value);
            Assert.Equal(createdPolicy.Id, returnValue.Id);
            Assert.Equal(policy.Name, returnValue.Name);
        }

        [Fact]
        public async Task UpdateAccessPolicyAsync_ValidPolicy_ReturnsOkResult()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy = new FunctionAccessPolicy
            {
                Id = policyId,
                Name = "Updated Policy",
                FunctionId = Guid.NewGuid(),
                Description = "Updated policy description",
                Conditions = new Dictionary<string, object>
                {
                    { "ip", "192.168.1.1" },
                    { "time", "business_hours" }
                }
            };

            _accessControlServiceMock.Setup(service => service.UpdateAccessPolicyAsync(policy))
                .ReturnsAsync(policy);

            // Act
            var result = await _controller.UpdateAccessPolicyAsync(policyId, policy);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionAccessPolicy>(okResult.Value);
            Assert.Equal(policyId, returnValue.Id);
            Assert.Equal(policy.Name, returnValue.Name);
        }

        [Fact]
        public async Task UpdateAccessPolicyAsync_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy = new FunctionAccessPolicy
            {
                Id = Guid.NewGuid(), // Different ID
                Name = "Updated Policy",
                FunctionId = Guid.NewGuid(),
                Description = "Updated policy description"
            };

            // Act
            var result = await _controller.UpdateAccessPolicyAsync(policyId, policy);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("ID in the path does not match ID in the body", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAccessPolicyByIdAsync_ExistingPolicy_ReturnsOkResult()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var policy = new FunctionAccessPolicy
            {
                Id = policyId,
                Name = "Test Policy",
                FunctionId = Guid.NewGuid(),
                Description = "Test policy description"
            };

            _accessControlServiceMock.Setup(service => service.GetAccessPolicyByIdAsync(policyId))
                .ReturnsAsync(policy);

            // Act
            var result = await _controller.GetAccessPolicyByIdAsync(policyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionAccessPolicy>(okResult.Value);
            Assert.Equal(policyId, returnValue.Id);
        }

        [Fact]
        public async Task GetAccessPolicyByIdAsync_NonExistingPolicy_ReturnsNotFound()
        {
            // Arrange
            var policyId = Guid.NewGuid();

            _accessControlServiceMock.Setup(service => service.GetAccessPolicyByIdAsync(policyId))
                .ReturnsAsync((FunctionAccessPolicy)null);

            // Act
            var result = await _controller.GetAccessPolicyByIdAsync(policyId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAccessPoliciesAsync_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var policies = new List<FunctionAccessPolicy>
            {
                new FunctionAccessPolicy { Id = Guid.NewGuid(), FunctionId = functionId, Name = "Policy 1" },
                new FunctionAccessPolicy { Id = Guid.NewGuid(), FunctionId = functionId, Name = "Policy 2" }
            };

            _accessControlServiceMock.Setup(service => service.GetAccessPoliciesAsync(functionId))
                .ReturnsAsync(policies);

            // Act
            var result = await _controller.GetAccessPoliciesAsync(functionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionAccessPolicy>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionAccessPolicy>)returnValue).Count);
        }

        [Fact]
        public async Task DeleteAccessPolicyAsync_ExistingPolicy_ReturnsNoContent()
        {
            // Arrange
            var policyId = Guid.NewGuid();

            _accessControlServiceMock.Setup(service => service.DeleteAccessPolicyAsync(policyId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAccessPolicyAsync(policyId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAccessPolicyAsync_NonExistingPolicy_ReturnsNotFound()
        {
            // Arrange
            var policyId = Guid.NewGuid();

            _accessControlServiceMock.Setup(service => service.DeleteAccessPolicyAsync(policyId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteAccessPolicyAsync(policyId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EvaluateAccessPoliciesAsync_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var context = new Dictionary<string, object>
            {
                { "ip", "192.168.1.1" },
                { "time", "business_hours" }
            };
            var accessAllowed = true;

            _accessControlServiceMock.Setup(service => service.EvaluateAccessPoliciesAsync(functionId, context))
                .ReturnsAsync(accessAllowed);

            // Act
            var result = await _controller.EvaluateAccessPoliciesAsync(functionId, context);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(returnValue);
        }

        [Fact]
        public async Task CreateAccessRequestAsync_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var request = new FunctionAccessRequest
            {
                FunctionId = Guid.NewGuid(),
                PrincipalId = "user123",
                PrincipalType = "user",
                RequestedPermissionLevel = "read",
                Reason = "Need to read function data"
            };
            var createdRequest = new FunctionAccessRequest
            {
                Id = Guid.NewGuid(),
                FunctionId = request.FunctionId,
                PrincipalId = request.PrincipalId,
                PrincipalType = request.PrincipalType,
                RequestedPermissionLevel = request.RequestedPermissionLevel,
                Reason = request.Reason,
                Status = "pending"
            };

            _accessControlServiceMock.Setup(service => service.CreateAccessRequestAsync(It.IsAny<FunctionAccessRequest>()))
                .ReturnsAsync(createdRequest);

            // Act
            var result = await _controller.CreateAccessRequestAsync(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<FunctionAccessRequest>(createdAtActionResult.Value);
            Assert.Equal(createdRequest.Id, returnValue.Id);
            Assert.Equal(request.FunctionId, returnValue.FunctionId);
            Assert.Equal("pending", returnValue.Status);
        }

        [Fact]
        public async Task GetAccessRequestByIdAsync_ExistingRequest_ReturnsOkResult()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var request = new FunctionAccessRequest
            {
                Id = requestId,
                FunctionId = Guid.NewGuid(),
                PrincipalId = "user123",
                PrincipalType = "user",
                Status = "pending"
            };

            _accessControlServiceMock.Setup(service => service.GetAccessRequestByIdAsync(requestId))
                .ReturnsAsync(request);

            // Act
            var result = await _controller.GetAccessRequestByIdAsync(requestId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionAccessRequest>(okResult.Value);
            Assert.Equal(requestId, returnValue.Id);
        }

        [Fact]
        public async Task GetAccessRequestByIdAsync_NonExistingRequest_ReturnsNotFound()
        {
            // Arrange
            var requestId = Guid.NewGuid();

            _accessControlServiceMock.Setup(service => service.GetAccessRequestByIdAsync(requestId))
                .ReturnsAsync((FunctionAccessRequest)null);

            // Act
            var result = await _controller.GetAccessRequestByIdAsync(requestId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAccessRequestsAsync_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var requests = new List<FunctionAccessRequest>
            {
                new FunctionAccessRequest { Id = Guid.NewGuid(), FunctionId = functionId, Status = "pending" },
                new FunctionAccessRequest { Id = Guid.NewGuid(), FunctionId = functionId, Status = "approved" }
            };

            _accessControlServiceMock.Setup(service => service.GetAccessRequestsAsync(functionId))
                .ReturnsAsync(requests);

            // Act
            var result = await _controller.GetAccessRequestsAsync(functionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionAccessRequest>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionAccessRequest>)returnValue).Count);
        }

        [Fact]
        public async Task ApproveAccessRequestAsync_ExistingRequest_ReturnsOkResult()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var approverId = "approver123";
            var reason = "Approved for business needs";
            var expiresAt = DateTime.UtcNow.AddDays(30);
            var grantedOperations = new List<string> { "read", "execute" };

            var approvedRequest = new FunctionAccessRequest
            {
                Id = requestId,
                Status = "approved",
                ApproverId = approverId,
                ApprovalReason = reason,
                ApprovedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                GrantedOperations = grantedOperations
            };

            _accessControlServiceMock.Setup(service => service.ApproveAccessRequestAsync(
                    requestId, approverId, reason, expiresAt, grantedOperations))
                .ReturnsAsync(approvedRequest);

            // Act
            var result = await _controller.ApproveAccessRequestAsync(
                requestId, approverId, reason, expiresAt, grantedOperations);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionAccessRequest>(okResult.Value);
            Assert.Equal(requestId, returnValue.Id);
            Assert.Equal("approved", returnValue.Status);
            Assert.Equal(approverId, returnValue.ApproverId);
        }

        [Fact]
        public async Task RejectAccessRequestAsync_ExistingRequest_ReturnsOkResult()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var approverId = "approver123";
            var reason = "Rejected due to security concerns";

            var rejectedRequest = new FunctionAccessRequest
            {
                Id = requestId,
                Status = "rejected",
                ApproverId = approverId,
                RejectionReason = reason,
                RejectedAt = DateTime.UtcNow
            };

            _accessControlServiceMock.Setup(service => service.RejectAccessRequestAsync(
                    requestId, approverId, reason))
                .ReturnsAsync(rejectedRequest);

            // Act
            var result = await _controller.RejectAccessRequestAsync(
                requestId, approverId, reason);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionAccessRequest>(okResult.Value);
            Assert.Equal(requestId, returnValue.Id);
            Assert.Equal("rejected", returnValue.Status);
            Assert.Equal(approverId, returnValue.ApproverId);
            Assert.Equal(reason, returnValue.RejectionReason);
        }
    }
}
