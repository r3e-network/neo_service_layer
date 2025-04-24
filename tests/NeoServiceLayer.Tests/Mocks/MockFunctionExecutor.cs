using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Enclave.Enclave.Execution;
using NeoServiceLayer.Enclave.Enclave.Models;

namespace NeoServiceLayer.Tests.Mocks
{
    public class MockFunctionExecutor : FunctionExecutor
    {
        public MockFunctionExecutor(ILogger<NeoServiceLayer.Enclave.Enclave.Execution.FunctionExecutor> logger)
            : base(logger, null!, null!, null!)
        {
        }

        public override Task ValidateAndCompileAsync(FunctionMetadata metadata)
        {
            return Task.CompletedTask;
        }

        public override Task<object> ExecuteAsync(FunctionMetadata metadata, Dictionary<string, object> parameters)
        {
            return Task.FromResult<object>(new { Result = "Success" });
        }

        public override Task<object> ExecuteForEventAsync(FunctionMetadata metadata, Event eventData)
        {
            return Task.FromResult<object>(new { Result = "Success" });
        }
    }
}
