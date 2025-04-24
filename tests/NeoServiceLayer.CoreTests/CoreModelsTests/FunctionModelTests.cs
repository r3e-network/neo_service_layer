using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreTests.CoreModelsTests
{
    public class FunctionModelTests
    {
        [Fact]
        public void Function_Properties_Work()
        {
            // Arrange
            var function = new Function
            {
                Id = Guid.NewGuid(),
                Name = "TestFunction",
                Description = "A test function",
                Runtime = "dotnet",
                SourceCode = "public class Test { public static void Main() { } }",
                EntryPoint = "Test.Main",
                AccountId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("TestFunction", function.Name);
            Assert.Equal("A test function", function.Description);
            Assert.Equal("dotnet", function.Runtime);
            Assert.Equal("public class Test { public static void Main() { } }", function.SourceCode);
            Assert.Equal("Test.Main", function.EntryPoint);
        }

        [Fact]
        public void FunctionExecution_Properties_Work()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var endTime = startTime.AddSeconds(5);
            var execution = new FunctionExecution
            {
                Id = Guid.NewGuid(),
                FunctionId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                Input = "{\"param1\": \"value1\"}",
                Output = "{\"result\": \"success\"}",
                Status = "Completed",
                ErrorMessage = string.Empty,
                StartTime = startTime,
                EndTime = endTime,
                DurationMs = 5000,
                MemoryUsageMb = 128.5,
                CpuUsagePercent = 25.0,
                BillingAmount = 0.05m,
                Logs = new List<FunctionLog>(),
                Transactions = new List<string>()
            };

            // Act & Assert
            Assert.Equal("Completed", execution.Status);
            Assert.Equal("{\"param1\": \"value1\"}", execution.Input);
            Assert.Equal("{\"result\": \"success\"}", execution.Output);
            Assert.Equal(string.Empty, execution.ErrorMessage);
            Assert.Equal(endTime, execution.EndTime);
            Assert.Equal(5000, execution.DurationMs);
            Assert.Equal(128.5, execution.MemoryUsageMb);
            Assert.Equal(25.0, execution.CpuUsagePercent);
            Assert.Equal(0.05m, execution.BillingAmount);
            Assert.Empty(execution.Logs);
            Assert.Empty(execution.Transactions);
        }

        [Fact]
        public void FunctionLog_Properties_Work()
        {
            // Arrange
            var log = new FunctionLog
            {
                Id = Guid.NewGuid(),
                ExecutionId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Level = "INFO",
                Message = "Function executed successfully"
            };

            // Act & Assert
            Assert.Equal("INFO", log.Level);
            Assert.Equal("Function executed successfully", log.Message);
        }
    }
}
