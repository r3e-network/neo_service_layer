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
    public class FunctionTestControllerTests
    {
        private readonly Mock<ILogger<FunctionTestController>> _loggerMock;
        private readonly Mock<IFunctionTestService> _testServiceMock;
        private readonly FunctionTestController _controller;
        private readonly Guid _userId = Guid.NewGuid();

        public FunctionTestControllerTests()
        {
            _loggerMock = new Mock<ILogger<FunctionTestController>>();
            _testServiceMock = new Mock<IFunctionTestService>();
            _controller = new FunctionTestController(_loggerMock.Object, _testServiceMock.Object);

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
        public async Task GetByIdAsync_ExistingTest_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var test = new FunctionTest { Id = testId, Name = "Test 1" };
            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync(test);

            // Act
            var result = await _controller.GetByIdAsync(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTest>(okResult.Value);
            Assert.Equal(testId, returnValue.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingTest_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync((FunctionTest)null);

            // Act
            var result = await _controller.GetByIdAsync(testId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetByFunctionIdAsync_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var tests = new List<FunctionTest>
            {
                new FunctionTest { Id = Guid.NewGuid(), FunctionId = functionId, Name = "Test 1" },
                new FunctionTest { Id = Guid.NewGuid(), FunctionId = functionId, Name = "Test 2" }
            };
            _testServiceMock.Setup(service => service.GetByFunctionIdAsync(functionId)).ReturnsAsync(tests);

            // Act
            var result = await _controller.GetByFunctionIdAsync(functionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionTest>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionTest>)returnValue).Count);
        }

        [Fact]
        public async Task CreateAsync_ValidTest_ReturnsCreatedAtAction()
        {
            // Arrange
            var test = new FunctionTest
            {
                Name = "New Test",
                FunctionId = Guid.NewGuid(),
                Description = "Test description"
            };
            var createdTest = new FunctionTest
            {
                Id = Guid.NewGuid(),
                Name = test.Name,
                FunctionId = test.FunctionId,
                Description = test.Description,
                CreatedBy = _userId,
                UpdatedBy = _userId
            };

            _testServiceMock.Setup(service => service.ValidateTestAsync(It.IsAny<FunctionTest>())).ReturnsAsync(new List<string>());
            _testServiceMock.Setup(service => service.CreateAsync(It.IsAny<FunctionTest>())).ReturnsAsync(createdTest);

            // Act
            var result = await _controller.CreateAsync(test);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTest>(createdAtActionResult.Value);
            Assert.Equal(createdTest.Id, returnValue.Id);
            Assert.Equal(_userId, returnValue.CreatedBy);
            Assert.Equal(_userId, returnValue.UpdatedBy);
        }

        [Fact]
        public async Task CreateAsync_InvalidTest_ReturnsBadRequest()
        {
            // Arrange
            var test = new FunctionTest
            {
                Name = "New Test",
                FunctionId = Guid.NewGuid(),
                Description = "Test description"
            };
            var validationErrors = new List<string> { "Error 1", "Error 2" };

            _testServiceMock.Setup(service => service.ValidateTestAsync(It.IsAny<FunctionTest>())).ReturnsAsync(validationErrors);

            // Act
            var result = await _controller.CreateAsync(test);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<object>(badRequestResult.Value);
            Assert.Equal(validationErrors, ((dynamic)returnValue).Errors);
        }

        [Fact]
        public async Task UpdateAsync_ValidTest_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var test = new FunctionTest
            {
                Id = testId,
                Name = "Updated Test",
                FunctionId = Guid.NewGuid(),
                Description = "Updated description"
            };
            var existingTest = new FunctionTest
            {
                Id = testId,
                Name = "Original Test",
                FunctionId = test.FunctionId,
                Description = "Original description"
            };
            var updatedTest = new FunctionTest
            {
                Id = testId,
                Name = test.Name,
                FunctionId = test.FunctionId,
                Description = test.Description,
                UpdatedBy = _userId
            };

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync(existingTest);
            _testServiceMock.Setup(service => service.ValidateTestAsync(It.IsAny<FunctionTest>())).ReturnsAsync(new List<string>());
            _testServiceMock.Setup(service => service.UpdateAsync(It.IsAny<FunctionTest>())).ReturnsAsync(updatedTest);

            // Act
            var result = await _controller.UpdateAsync(testId, test);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTest>(okResult.Value);
            Assert.Equal(testId, returnValue.Id);
            Assert.Equal(test.Name, returnValue.Name);
            Assert.Equal(_userId, returnValue.UpdatedBy);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingTest_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var test = new FunctionTest
            {
                Id = testId,
                Name = "Updated Test",
                FunctionId = Guid.NewGuid(),
                Description = "Updated description"
            };

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync((FunctionTest)null);

            // Act
            var result = await _controller.UpdateAsync(testId, test);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateAsync_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var test = new FunctionTest
            {
                Id = Guid.NewGuid(), // Different ID
                Name = "Updated Test",
                FunctionId = Guid.NewGuid(),
                Description = "Updated description"
            };
            var existingTest = new FunctionTest
            {
                Id = testId,
                Name = "Original Test",
                FunctionId = Guid.NewGuid(),
                Description = "Original description"
            };

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync(existingTest);

            // Act
            var result = await _controller.UpdateAsync(testId, test);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("ID in the path does not match ID in the body", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteAsync_ExistingTest_ReturnsNoContent()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingTest = new FunctionTest
            {
                Id = testId,
                Name = "Test to delete",
                FunctionId = Guid.NewGuid()
            };

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync(existingTest);
            _testServiceMock.Setup(service => service.DeleteAsync(testId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAsync(testId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingTest_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync((FunctionTest)null);

            // Act
            var result = await _controller.DeleteAsync(testId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RunTestAsync_ExistingTest_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingTest = new FunctionTest
            {
                Id = testId,
                Name = "Test to run",
                FunctionId = Guid.NewGuid()
            };
            var testResult = new FunctionTestResult
            {
                Id = Guid.NewGuid(),
                TestId = testId,
                Status = "passed",
                ExecutionTime = 100
            };

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync(existingTest);
            _testServiceMock.Setup(service => service.RunTestAsync(testId, null)).ReturnsAsync(testResult);

            // Act
            var result = await _controller.RunTestAsync(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTestResult>(okResult.Value);
            Assert.Equal(testId, returnValue.TestId);
            Assert.Equal("passed", returnValue.Status);
        }

        [Fact]
        public async Task RunTestAsync_NonExistingTest_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync((FunctionTest)null);

            // Act
            var result = await _controller.RunTestAsync(testId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetResultsAsync_ExistingTest_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingTest = new FunctionTest
            {
                Id = testId,
                Name = "Test with results",
                FunctionId = Guid.NewGuid()
            };
            var testResults = new List<FunctionTestResult>
            {
                new FunctionTestResult { Id = Guid.NewGuid(), TestId = testId, Status = "passed" },
                new FunctionTestResult { Id = Guid.NewGuid(), TestId = testId, Status = "failed" }
            };

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync(existingTest);
            _testServiceMock.Setup(service => service.GetTestResultsAsync(testId, 10, 0)).ReturnsAsync(testResults);

            // Act
            var result = await _controller.GetResultsAsync(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionTestResult>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionTestResult>)returnValue).Count);
        }

        [Fact]
        public async Task GetLatestResultAsync_ExistingTestWithResults_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingTest = new FunctionTest
            {
                Id = testId,
                Name = "Test with results",
                FunctionId = Guid.NewGuid()
            };
            var latestResult = new FunctionTestResult
            {
                Id = Guid.NewGuid(),
                TestId = testId,
                Status = "passed",
                ExecutionTime = 100
            };

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync(existingTest);
            _testServiceMock.Setup(service => service.GetLatestTestResultAsync(testId)).ReturnsAsync(latestResult);

            // Act
            var result = await _controller.GetLatestResultAsync(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<FunctionTestResult>(okResult.Value);
            Assert.Equal(testId, returnValue.TestId);
            Assert.Equal("passed", returnValue.Status);
        }

        [Fact]
        public async Task GetLatestResultAsync_ExistingTestWithNoResults_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingTest = new FunctionTest
            {
                Id = testId,
                Name = "Test with no results",
                FunctionId = Guid.NewGuid()
            };

            _testServiceMock.Setup(service => service.GetByIdAsync(testId)).ReturnsAsync(existingTest);
            _testServiceMock.Setup(service => service.GetLatestTestResultAsync(testId)).ReturnsAsync((FunctionTestResult)null);

            // Act
            var result = await _controller.GetLatestResultAsync(testId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("No test results found", notFoundResult.Value);
        }

        [Fact]
        public async Task GenerateTestsAsync_ValidFunctionId_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var generatedTests = new List<FunctionTest>
            {
                new FunctionTest { Id = Guid.NewGuid(), FunctionId = functionId, Name = "Generated Test 1" },
                new FunctionTest { Id = Guid.NewGuid(), FunctionId = functionId, Name = "Generated Test 2" }
            };

            _testServiceMock.Setup(service => service.GenerateTestsAsync(functionId)).ReturnsAsync(generatedTests);

            // Act
            var result = await _controller.GenerateTestsAsync(functionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<FunctionTest>>(okResult.Value);
            Assert.Equal(2, ((List<FunctionTest>)returnValue).Count);
        }

        [Fact]
        public async Task GetCoverageAsync_ValidFunctionId_ReturnsOkResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var coverage = new
            {
                TotalLines = 100,
                CoveredLines = 80,
                CoveragePercentage = 80.0,
                UncoveredLines = new List<int> { 10, 20, 30 }
            };

            _testServiceMock.Setup(service => service.GetTestCoverageAsync(functionId)).ReturnsAsync(coverage);

            // Act
            var result = await _controller.GetCoverageAsync(functionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            dynamic returnValue = okResult.Value;
            Assert.Equal(100, returnValue.TotalLines);
            Assert.Equal(80, returnValue.CoveredLines);
            Assert.Equal(80.0, returnValue.CoveragePercentage);
        }
    }
}
