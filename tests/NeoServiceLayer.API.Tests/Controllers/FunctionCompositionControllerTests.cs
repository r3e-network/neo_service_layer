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
    public class FunctionCompositionControllerTests
    {
        private readonly Mock<ILogger<FunctionCompositionController>> _loggerMock;
        private readonly Mock<IFunctionCompositionService> _compositionServiceMock;
        private readonly FunctionCompositionController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public FunctionCompositionControllerTests()
        {
            _loggerMock = new Mock<ILogger<FunctionCompositionController>>();
            _compositionServiceMock = new Mock<IFunctionCompositionService>();
            _controller = new FunctionCompositionController(_loggerMock.Object, _compositionServiceMock.Object);

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
        public async Task GetByIdAsync_ExistingComposition_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Test Composition",
                Description = "Test description",
                AccountId = _userId
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(composition);

            // Act
            var result = await _controller.GetByIdAsync(compositionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionComposition>(okResult.Value);
            Assert.Equal(compositionId, returnValue.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingComposition_ReturnsNotFound()
        {
            // Arrange
            var compositionId = Guid.NewGuid();

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync((FunctionComposition)null);

            // Act
            var result = await _controller.GetByIdAsync(compositionId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetByAccountIdAsync_ReturnsOkResult()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var compositions = new List<FunctionComposition>
            {
                new FunctionComposition { Id = Guid.NewGuid(), AccountId = accountId, Name = "Composition 1" },
                new FunctionComposition { Id = Guid.NewGuid(), AccountId = accountId, Name = "Composition 2" }
            };

            _compositionServiceMock.Setup(service => service.GetByAccountIdAsync(accountId))
                .ReturnsAsync(compositions);

            // Act
            var result = await _controller.GetByAccountIdAsync(accountId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionComposition>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionComposition>)returnValue).Count);
        }

        [Fact]
        public async Task GetByTagsAsync_ReturnsOkResult()
        {
            // Arrange
            var tags = new List<string> { "finance", "blockchain" };
            var compositions = new List<FunctionComposition>
            {
                new FunctionComposition { Id = Guid.NewGuid(), Tags = tags, Name = "Composition 1" },
                new FunctionComposition { Id = Guid.NewGuid(), Tags = tags, Name = "Composition 2" }
            };

            _compositionServiceMock.Setup(service => service.GetByTagsAsync(tags))
                .ReturnsAsync(compositions);

            // Act
            var result = await _controller.GetByTagsAsync(tags);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionComposition>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionComposition>)returnValue).Count);
        }

        [Fact]
        public async Task CreateAsync_ValidComposition_ReturnsCreatedAtAction()
        {
            // Arrange
            var composition = new FunctionComposition
            {
                Name = "New Composition",
                Description = "New composition description",
                ExecutionMode = "sequential",
                MaxExecutionTime = 60000
            };
            var createdComposition = new FunctionComposition
            {
                Id = Guid.NewGuid(),
                Name = composition.Name,
                Description = composition.Description,
                ExecutionMode = composition.ExecutionMode,
                MaxExecutionTime = composition.MaxExecutionTime,
                AccountId = _userId,
                CreatedBy = _userId,
                UpdatedBy = _userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _compositionServiceMock.Setup(service => service.CreateAsync(It.IsAny<FunctionComposition>()))
                .ReturnsAsync(createdComposition);

            // Act
            var result = await _controller.CreateAsync(composition);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<FunctionComposition>(createdAtActionResult.Value);
            Assert.Equal(createdComposition.Id, returnValue.Id);
            Assert.Equal(_userId, returnValue.AccountId);
            Assert.Equal(_userId, returnValue.CreatedBy);
            Assert.Equal(_userId, returnValue.UpdatedBy);
        }

        [Fact]
        public async Task UpdateAsync_ValidComposition_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Updated Composition",
                Description = "Updated description",
                ExecutionMode = "parallel",
                MaxExecutionTime = 120000,
                AccountId = _userId
            };
            var existingComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Original Composition",
                Description = "Original description",
                ExecutionMode = "sequential",
                MaxExecutionTime = 60000,
                AccountId = _userId
            };
            var updatedComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = composition.Name,
                Description = composition.Description,
                ExecutionMode = composition.ExecutionMode,
                MaxExecutionTime = composition.MaxExecutionTime,
                AccountId = _userId,
                UpdatedBy = _userId,
                UpdatedAt = DateTime.UtcNow
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(existingComposition);
            _compositionServiceMock.Setup(service => service.UpdateAsync(It.IsAny<FunctionComposition>()))
                .ReturnsAsync(updatedComposition);

            // Act
            var result = await _controller.UpdateAsync(compositionId, composition);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionComposition>(okResult.Value);
            Assert.Equal(compositionId, returnValue.Id);
            Assert.Equal(composition.Name, returnValue.Name);
            Assert.Equal(composition.ExecutionMode, returnValue.ExecutionMode);
            Assert.Equal(_userId, returnValue.UpdatedBy);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingComposition_ReturnsNotFound()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Updated Composition",
                Description = "Updated description",
                AccountId = _userId
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync((FunctionComposition)null);

            // Act
            var result = await _controller.UpdateAsync(compositionId, composition);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAsync_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var composition = new FunctionComposition
            {
                Id = Guid.NewGuid(), // Different ID
                Name = "Updated Composition",
                Description = "Updated description",
                AccountId = _userId
            };
            var existingComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Original Composition",
                Description = "Original description",
                AccountId = _userId
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(existingComposition);

            // Act
            var result = await _controller.UpdateAsync(compositionId, composition);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("ID in the path does not match ID in the body", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteAsync_ExistingComposition_ReturnsNoContent()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var existingComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Composition to delete",
                AccountId = _userId
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(existingComposition);
            _compositionServiceMock.Setup(service => service.DeleteAsync(compositionId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAsync(compositionId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ExecuteAsync_ExistingComposition_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var inputParameters = new Dictionary<string, object>
            {
                { "param1", "value1" },
                { "param2", 42 }
            };
            var existingComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Composition to execute",
                AccountId = _userId,
                IsEnabled = true
            };
            var execution = new FunctionCompositionExecution
            {
                Id = Guid.NewGuid(),
                CompositionId = compositionId,
                AccountId = _userId,
                Status = "completed",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddSeconds(1),
                ExecutionTimeMs = 1000,
                InputParameters = inputParameters,
                OutputResult = new Dictionary<string, object> { { "result", "success" } }
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(existingComposition);
            _compositionServiceMock.Setup(service => service.ExecuteAsync(compositionId, inputParameters))
                .ReturnsAsync(execution);

            // Act
            var result = await _controller.ExecuteAsync(compositionId, inputParameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionCompositionExecution>(okResult.Value);
            Assert.Equal(execution.Id, returnValue.Id);
            Assert.Equal("completed", returnValue.Status);
            Assert.Equal(1000, returnValue.ExecutionTimeMs);
        }

        [Fact]
        public async Task GetExecutionByIdAsync_ExistingExecution_ReturnsOkResult()
        {
            // Arrange
            var executionId = Guid.NewGuid();
            var execution = new FunctionCompositionExecution
            {
                Id = executionId,
                CompositionId = Guid.NewGuid(),
                AccountId = _userId,
                Status = "completed"
            };

            _compositionServiceMock.Setup(service => service.GetExecutionByIdAsync(executionId))
                .ReturnsAsync(execution);

            // Act
            var result = await _controller.GetExecutionByIdAsync(executionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionCompositionExecution>(okResult.Value);
            Assert.Equal(executionId, returnValue.Id);
        }

        [Fact]
        public async Task GetExecutionsByCompositionIdAsync_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var existingComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Composition",
                AccountId = _userId
            };
            var executions = new List<FunctionCompositionExecution>
            {
                new FunctionCompositionExecution { Id = Guid.NewGuid(), CompositionId = compositionId, Status = "completed" },
                new FunctionCompositionExecution { Id = Guid.NewGuid(), CompositionId = compositionId, Status = "error" }
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(existingComposition);
            _compositionServiceMock.Setup(service => service.GetExecutionsByCompositionIdAsync(compositionId, 10, 0))
                .ReturnsAsync(executions);

            // Act
            var result = await _controller.GetExecutionsByCompositionIdAsync(compositionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionCompositionExecution>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionCompositionExecution>)returnValue).Count);
        }

        [Fact]
        public async Task CancelExecutionAsync_ExistingExecution_ReturnsOkResult()
        {
            // Arrange
            var executionId = Guid.NewGuid();
            var cancelledExecution = new FunctionCompositionExecution
            {
                Id = executionId,
                CompositionId = Guid.NewGuid(),
                AccountId = _userId,
                Status = "cancelled",
                EndTime = DateTime.UtcNow
            };

            _compositionServiceMock.Setup(service => service.CancelExecutionAsync(executionId))
                .ReturnsAsync(cancelledExecution);

            // Act
            var result = await _controller.CancelExecutionAsync(executionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionCompositionExecution>(okResult.Value);
            Assert.Equal(executionId, returnValue.Id);
            Assert.Equal("cancelled", returnValue.Status);
        }

        [Fact]
        public async Task GetExecutionLogsAsync_ExistingExecution_ReturnsOkResult()
        {
            // Arrange
            var executionId = Guid.NewGuid();
            var logs = new List<string>
            {
                "Log entry 1",
                "Log entry 2",
                "Log entry 3"
            };

            _compositionServiceMock.Setup(service => service.GetExecutionLogsAsync(executionId))
                .ReturnsAsync(logs);

            // Act
            var result = await _controller.GetExecutionLogsAsync(executionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<List<string>>(okResult.Value);
            Assert.Equal(3, returnValue.Count);
        }

        [Fact]
        public async Task GetInputSchemaAsync_ExistingComposition_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var schema = "{ \"type\": \"object\", \"properties\": { \"param1\": { \"type\": \"string\" } } }";

            _compositionServiceMock.Setup(service => service.GetInputSchemaAsync(compositionId))
                .ReturnsAsync(schema);

            // Act
            var result = await _controller.GetInputSchemaAsync(compositionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<string>(okResult.Value);
            Assert.Equal(schema, returnValue);
        }

        [Fact]
        public async Task GenerateInputSchemaAsync_ExistingComposition_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var schema = "{ \"type\": \"object\", \"properties\": { \"param1\": { \"type\": \"string\" } } }";

            _compositionServiceMock.Setup(service => service.GenerateInputSchemaAsync(compositionId))
                .ReturnsAsync(schema);

            // Act
            var result = await _controller.GenerateInputSchemaAsync(compositionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<string>(okResult.Value);
            Assert.Equal(schema, returnValue);
        }

        [Fact]
        public async Task AddStepAsync_ExistingComposition_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var step = new FunctionCompositionStep
            {
                Name = "New Step",
                FunctionId = Guid.NewGuid(),
                TimeoutMs = 30000
            };
            var existingComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Composition",
                AccountId = _userId,
                Steps = new List<FunctionCompositionStep>()
            };
            var updatedComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = existingComposition.Name,
                AccountId = _userId,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Id = Guid.NewGuid(),
                        Name = step.Name,
                        FunctionId = step.FunctionId,
                        TimeoutMs = step.TimeoutMs,
                        Order = 0
                    }
                }
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(existingComposition);
            _compositionServiceMock.Setup(service => service.AddStepAsync(compositionId, It.IsAny<FunctionCompositionStep>()))
                .ReturnsAsync(updatedComposition);

            // Act
            var result = await _controller.AddStepAsync(compositionId, step);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionComposition>(okResult.Value);
            Assert.Equal(compositionId, returnValue.Id);
            Assert.Single(returnValue.Steps);
            Assert.Equal(step.Name, returnValue.Steps[0].Name);
        }

        [Fact]
        public async Task UpdateStepAsync_ExistingCompositionAndStep_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var stepId = Guid.NewGuid();
            var step = new FunctionCompositionStep
            {
                Id = stepId,
                Name = "Updated Step",
                FunctionId = Guid.NewGuid(),
                TimeoutMs = 60000
            };
            var existingComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Composition",
                AccountId = _userId,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Id = stepId,
                        Name = "Original Step",
                        FunctionId = Guid.NewGuid(),
                        TimeoutMs = 30000,
                        Order = 0
                    }
                }
            };
            var updatedComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = existingComposition.Name,
                AccountId = _userId,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Id = stepId,
                        Name = step.Name,
                        FunctionId = step.FunctionId,
                        TimeoutMs = step.TimeoutMs,
                        Order = 0
                    }
                }
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(existingComposition);
            _compositionServiceMock.Setup(service => service.UpdateStepAsync(compositionId, It.IsAny<FunctionCompositionStep>()))
                .ReturnsAsync(updatedComposition);

            // Act
            var result = await _controller.UpdateStepAsync(compositionId, stepId, step);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionComposition>(okResult.Value);
            Assert.Equal(compositionId, returnValue.Id);
            Assert.Single(returnValue.Steps);
            Assert.Equal(step.Name, returnValue.Steps[0].Name);
            Assert.Equal(step.TimeoutMs, returnValue.Steps[0].TimeoutMs);
        }

        [Fact]
        public async Task RemoveStepAsync_ExistingCompositionAndStep_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var stepId = Guid.NewGuid();
            var existingComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Composition",
                AccountId = _userId,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Id = stepId,
                        Name = "Step to remove",
                        FunctionId = Guid.NewGuid(),
                        Order = 0
                    }
                }
            };
            var updatedComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = existingComposition.Name,
                AccountId = _userId,
                Steps = new List<FunctionCompositionStep>()
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(existingComposition);
            _compositionServiceMock.Setup(service => service.RemoveStepAsync(compositionId, stepId))
                .ReturnsAsync(updatedComposition);

            // Act
            var result = await _controller.RemoveStepAsync(compositionId, stepId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionComposition>(okResult.Value);
            Assert.Equal(compositionId, returnValue.Id);
            Assert.Empty(returnValue.Steps);
        }

        [Fact]
        public async Task ReorderStepsAsync_ExistingComposition_ReturnsOkResult()
        {
            // Arrange
            var compositionId = Guid.NewGuid();
            var stepId1 = Guid.NewGuid();
            var stepId2 = Guid.NewGuid();
            var stepIds = new List<Guid> { stepId2, stepId1 }; // Reverse order
            var existingComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = "Composition",
                AccountId = _userId,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Id = stepId1,
                        Name = "Step 1",
                        FunctionId = Guid.NewGuid(),
                        Order = 0
                    },
                    new FunctionCompositionStep
                    {
                        Id = stepId2,
                        Name = "Step 2",
                        FunctionId = Guid.NewGuid(),
                        Order = 1
                    }
                }
            };
            var updatedComposition = new FunctionComposition
            {
                Id = compositionId,
                Name = existingComposition.Name,
                AccountId = _userId,
                Steps = new List<FunctionCompositionStep>
                {
                    new FunctionCompositionStep
                    {
                        Id = stepId2,
                        Name = "Step 2",
                        FunctionId = existingComposition.Steps[1].FunctionId,
                        Order = 0
                    },
                    new FunctionCompositionStep
                    {
                        Id = stepId1,
                        Name = "Step 1",
                        FunctionId = existingComposition.Steps[0].FunctionId,
                        Order = 1
                    }
                }
            };

            _compositionServiceMock.Setup(service => service.GetByIdAsync(compositionId))
                .ReturnsAsync(existingComposition);
            _compositionServiceMock.Setup(service => service.ReorderStepsAsync(compositionId, stepIds))
                .ReturnsAsync(updatedComposition);

            // Act
            var result = await _controller.ReorderStepsAsync(compositionId, stepIds);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionComposition>(okResult.Value);
            Assert.Equal(compositionId, returnValue.Id);
            Assert.Equal(2, returnValue.Steps.Count);
            Assert.Equal(stepId2, returnValue.Steps[0].Id);
            Assert.Equal(0, returnValue.Steps[0].Order);
            Assert.Equal(stepId1, returnValue.Steps[1].Id);
            Assert.Equal(1, returnValue.Steps[1].Order);
        }
    }
}
