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
    public class FunctionTestSuiteControllerTests
    {
        private readonly Mock<ILogger<FunctionTestSuiteController>> _loggerMock;
        private readonly Mock<IFunctionTestService> _testServiceMock;
        private readonly FunctionTestSuiteController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public FunctionTestSuiteControllerTests()
        {
            _loggerMock = new Mock<ILogger<FunctionTestSuiteController>>();
            _testServiceMock = new Mock<IFunctionTestService>();
            _controller = new FunctionTestSuiteController(_loggerMock.Object, _testServiceMock.Object);

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
        public async Task GetByIdAsync_ExistingSuite_ReturnsOkResult()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var suite = new FunctionTestSuite { Id = suiteId, Name = "Test Suite 1" };
            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync(suite);

            // Act
            var result = await _controller.GetByIdAsync(suiteId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTestSuite>(okResult.Value);
            Assert.Equal(suiteId, returnValue.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingSuite_ReturnsNotFound()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync((FunctionTestSuite)null);

            // Act
            var result = await _controller.GetByIdAsync(suiteId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetByFunctionIdAsync_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var suites = new List<FunctionTestSuite>
            {
                new FunctionTestSuite { Id = Guid.NewGuid(), FunctionId = functionId, Name = "Test Suite 1" },
                new FunctionTestSuite { Id = Guid.NewGuid(), FunctionId = functionId, Name = "Test Suite 2" }
            };
            _testServiceMock.Setup(service => service.GetTestSuitesByFunctionIdAsync(functionId)).ReturnsAsync(suites);

            // Act
            var result = await _controller.GetByFunctionIdAsync(functionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionTestSuite>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionTestSuite>)returnValue).Count);
        }

        [Fact]
        public async Task CreateAsync_ValidSuite_ReturnsCreatedAtAction()
        {
            // Arrange
            var suite = new FunctionTestSuite
            {
                Name = "New Test Suite",
                FunctionId = Guid.NewGuid(),
                Description = "Test suite description"
            };
            var createdSuite = new FunctionTestSuite
            {
                Id = Guid.NewGuid(),
                Name = suite.Name,
                FunctionId = suite.FunctionId,
                Description = suite.Description,
                CreatedBy = _userId,
                UpdatedBy = _userId
            };

            _testServiceMock.Setup(service => service.CreateTestSuiteAsync(It.IsAny<FunctionTestSuite>())).ReturnsAsync(createdSuite);

            // Act
            var result = await _controller.CreateAsync(suite);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTestSuite>(createdAtActionResult.Value);
            Assert.Equal(createdSuite.Id, returnValue.Id);
            Assert.Equal(_userId, returnValue.CreatedBy);
            Assert.Equal(_userId, returnValue.UpdatedBy);
        }

        [Fact]
        public async Task UpdateAsync_ValidSuite_ReturnsOkResult()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var suite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = "Updated Test Suite",
                FunctionId = Guid.NewGuid(),
                Description = "Updated description"
            };
            var existingSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = "Original Test Suite",
                FunctionId = suite.FunctionId,
                Description = "Original description"
            };
            var updatedSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = suite.Name,
                FunctionId = suite.FunctionId,
                Description = suite.Description,
                UpdatedBy = _userId
            };

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync(existingSuite);
            _testServiceMock.Setup(service => service.UpdateTestSuiteAsync(It.IsAny<FunctionTestSuite>())).ReturnsAsync(updatedSuite);

            // Act
            var result = await _controller.UpdateAsync(suiteId, suite);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTestSuite>(okResult.Value);
            Assert.Equal(suiteId, returnValue.Id);
            Assert.Equal(suite.Name, returnValue.Name);
            Assert.Equal(_userId, returnValue.UpdatedBy);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingSuite_ReturnsNotFound()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var suite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = "Updated Test Suite",
                FunctionId = Guid.NewGuid(),
                Description = "Updated description"
            };

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync((FunctionTestSuite)null);

            // Act
            var result = await _controller.UpdateAsync(suiteId, suite);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAsync_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var suite = new FunctionTestSuite
            {
                Id = Guid.NewGuid(), // Different ID
                Name = "Updated Test Suite",
                FunctionId = Guid.NewGuid(),
                Description = "Updated description"
            };
            var existingSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = "Original Test Suite",
                FunctionId = Guid.NewGuid(),
                Description = "Original description"
            };

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync(existingSuite);

            // Act
            var result = await _controller.UpdateAsync(suiteId, suite);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("ID in the path does not match ID in the body", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteAsync_ExistingSuite_ReturnsNoContent()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var existingSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = "Test Suite to delete",
                FunctionId = Guid.NewGuid()
            };

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync(existingSuite);
            _testServiceMock.Setup(service => service.DeleteTestSuiteAsync(suiteId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAsync(suiteId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingSuite_ReturnsNotFound()
        {
            // Arrange
            var suiteId = Guid.NewGuid();

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync((FunctionTestSuite)null);

            // Act
            var result = await _controller.DeleteAsync(suiteId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddTestAsync_ExistingSuiteAndTest_ReturnsOkResult()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var testId = Guid.NewGuid();
            var existingSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = "Test Suite",
                FunctionId = Guid.NewGuid(),
                Tests = new List<Guid>()
            };
            var existingTest = new FunctionTest
            {
                Id = testId,
                Name = "Test to add",
                FunctionId = existingSuite.FunctionId
            };
            var updatedSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = existingSuite.Name,
                FunctionId = existingSuite.FunctionId,
                Tests = new List<Guid> { testId }
            };

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync(existingSuite);
            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync(existingTest);
            _testServiceMock.Setup(service => service.AddTestToSuiteAsync(suiteId, testId)).ReturnsAsync(updatedSuite);

            // Act
            var result = await _controller.AddTestAsync(suiteId, testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTestSuite>(okResult.Value);
            Assert.Equal(suiteId, returnValue.Id);
            Assert.Contains(testId, returnValue.Tests);
        }

        [Fact]
        public async Task AddTestAsync_NonExistingSuite_ReturnsNotFound()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var testId = Guid.NewGuid();

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync((FunctionTestSuite)null);

            // Act
            var result = await _controller.AddTestAsync(suiteId, testId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Test suite not found", notFoundResult.Value);
        }

        [Fact]
        public async Task AddTestAsync_NonExistingTest_ReturnsNotFound()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var testId = Guid.NewGuid();
            var existingSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = "Test Suite",
                FunctionId = Guid.NewGuid(),
                Tests = new List<Guid>()
            };

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync(existingSuite);
            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync((FunctionTest)null);

            // Act
            var result = await _controller.AddTestAsync(suiteId, testId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Test not found", notFoundResult.Value);
        }

        [Fact]
        public async Task RemoveTestAsync_ExistingSuiteAndTest_ReturnsOkResult()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var testId = Guid.NewGuid();
            var existingSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = "Test Suite",
                FunctionId = Guid.NewGuid(),
                Tests = new List<Guid> { testId }
            };
            var updatedSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = existingSuite.Name,
                FunctionId = existingSuite.FunctionId,
                Tests = new List<Guid>()
            };

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync(existingSuite);
            _testServiceMock.Setup(service => service.RemoveTestFromSuiteAsync(suiteId, testId)).ReturnsAsync(updatedSuite);

            // Act
            var result = await _controller.RemoveTestAsync(suiteId, testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTestSuite>(okResult.Value);
            Assert.Equal(suiteId, returnValue.Id);
            Assert.DoesNotContain(testId, returnValue.Tests);
        }

        [Fact]
        public async Task RemoveTestAsync_NonExistingSuite_ReturnsNotFound()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var testId = Guid.NewGuid();

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync((FunctionTestSuite)null);

            // Act
            var result = await _controller.RemoveTestAsync(suiteId, testId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Test suite not found", notFoundResult.Value);
        }

        [Fact]
        public async Task RunSuiteAsync_ExistingSuite_ReturnsOkResult()
        {
            // Arrange
            var suiteId = Guid.NewGuid();
            var testId1 = Guid.NewGuid();
            var testId2 = Guid.NewGuid();
            var existingSuite = new FunctionTestSuite
            {
                Id = suiteId,
                Name = "Test Suite to run",
                FunctionId = Guid.NewGuid(),
                Tests = new List<Guid> { testId1, testId2 }
            };
            var testResults = new List<FunctionTestResult>
            {
                new FunctionTestResult { Id = Guid.NewGuid(), TestId = testId1, Status = "passed" },
                new FunctionTestResult { Id = Guid.NewGuid(), TestId = testId2, Status = "failed" }
            };

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync(existingSuite);
            _testServiceMock.Setup(service => service.RunTestSuiteAsync(suiteId, null)).ReturnsAsync(testResults);

            // Act
            var result = await _controller.RunSuiteAsync(suiteId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionTestResult>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionTestResult>)returnValue).Count);
        }

        [Fact]
        public async Task RunSuiteAsync_NonExistingSuite_ReturnsNotFound()
        {
            // Arrange
            var suiteId = Guid.NewGuid();

            _testServiceMock.Setup(service => service.GetTestSuiteByIdAsync(suiteId)).ReturnsAsync((FunctionTestSuite)null);

            // Act
            var result = await _controller.RunSuiteAsync(suiteId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
