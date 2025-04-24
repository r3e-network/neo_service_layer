using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreModelsTests
{
    public class FunctionModelsTests
    {
        [Fact]
        public void Function_Properties_Work()
        {
            // Arrange
            var function = new Function
            {
                Id = Guid.NewGuid(),
                Name = "PriceAggregator",
                Description = "Aggregates price data from multiple sources",
                Runtime = "dotnet",
                Handler = "PriceAggregator::Handler.Process",
                SourceCode = "public class Handler { public static object Process(object input) { return input; } }",
                EntryPoint = "Handler.Process",
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 30000,
                MaxMemory = 256,
                SourceCodeUrl = "https://example.com/code.zip",
                SourceCodeHash = "abc123",
                MemoryRequirementMb = 256,
                CpuRequirement = 1,
                TimeoutSeconds = 30,
                CreatedBy = "admin",
                SecretIds = new List<Guid> { Guid.NewGuid() },
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "LOG_LEVEL", "INFO" },
                    { "PRICE_SOURCES", "Binance,CoinGecko" }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastExecutedAt = DateTime.UtcNow,
                Status = "Active",
                RequiresTee = true,
                RequiresVpc = false
            };

            // Act & Assert
            Assert.Equal("PriceAggregator", function.Name);
            Assert.Equal("Aggregates price data from multiple sources", function.Description);
            Assert.Equal("dotnet", function.Runtime);
            Assert.Equal("PriceAggregator::Handler.Process", function.Handler);
            Assert.Contains("public class Handler", function.SourceCode);
            Assert.Equal("Handler.Process", function.EntryPoint);
            Assert.Equal(30000, function.MaxExecutionTime);
            Assert.Equal(256, function.MaxMemory);
            Assert.Equal("https://example.com/code.zip", function.SourceCodeUrl);
            Assert.Equal("abc123", function.SourceCodeHash);
            Assert.Equal(256, function.MemoryRequirementMb);
            Assert.Equal(1, function.CpuRequirement);
            Assert.Equal(30, function.TimeoutSeconds);
            Assert.Equal("admin", function.CreatedBy);
            Assert.Single(function.SecretIds);
            Assert.Equal("INFO", function.EnvironmentVariables["LOG_LEVEL"]);
            Assert.Equal("Binance,CoinGecko", function.EnvironmentVariables["PRICE_SOURCES"]);
            Assert.Equal("Active", function.Status);
            Assert.True(function.RequiresTee);
            Assert.False(function.RequiresVpc);
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
