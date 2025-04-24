using System;
using System.Collections.Generic;

namespace NeoServiceLayer.ServiceTests.Mocks
{
    public class FunctionExecutionResult
    {
        public bool Success { get; set; }
        public object Result { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> Logs { get; set; } = new List<string>();
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
        public TimeSpan ExecutionTime { get; set; }
        public long MemoryUsage { get; set; }
    }

    public class FunctionValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }

    public class FunctionRuntimeDetails
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string[] SupportedLanguages { get; set; }
    }

    public class FunctionExecutionContext
    {
        public Guid ExecutionId { get; set; } = Guid.NewGuid();
        public Guid FunctionId { get; set; }
        public Guid AccountId { get; set; }
        public int MaxExecutionTime { get; set; } = 30000;
        public int MaxMemory { get; set; } = 128;
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
        public List<Guid> SecretIds { get; set; } = new List<Guid>();
    }
}
