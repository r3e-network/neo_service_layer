using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Function execution context
    /// </summary>
    public class FunctionExecutionContext
    {
        /// <summary>
        /// Gets or sets the execution ID
        /// </summary>
        public Guid ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the timeout in milliseconds
        /// </summary>
        public int TimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the maximum execution time
        /// </summary>
        public int MaxExecutionTime { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the maximum memory in MB
        /// </summary>
        public int MaxMemoryMb { get; set; } = 256;

        /// <summary>
        /// Gets or sets the maximum memory
        /// </summary>
        public int MaxMemory { get; set; } = 256;

        /// <summary>
        /// Gets or sets the environment variables
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the secrets
        /// </summary>
        public Dictionary<string, string> Secrets { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the secret IDs
        /// </summary>
        public List<Guid> SecretIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the services
        /// </summary>
        public Dictionary<string, object> Services { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the trace ID
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Gets or sets the parent span ID
        /// </summary>
        public string ParentSpanId { get; set; }

        /// <summary>
        /// Gets or sets the execution mode
        /// </summary>
        public string ExecutionMode { get; set; } = "normal";

        /// <summary>
        /// Gets or sets the execution tags
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the event data
        /// </summary>
        public Event Event { get; set; }
    }

    /// <summary>
    /// Function execution result
    /// </summary>
    public class FunctionExecutionResult
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the execution ID
        /// </summary>
        public Guid ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the input parameters
        /// </summary>
        public object Input { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the output
        /// </summary>
        public object Output { get; set; }

        /// <summary>
        /// Gets or sets the error
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the execution time in milliseconds
        /// </summary>
        public double ExecutionTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the memory usage in MB
        /// </summary>
        public double MemoryUsageMb { get; set; }

        /// <summary>
        /// Gets or sets the CPU usage in percentage
        /// </summary>
        public double CpuUsagePercentage { get; set; }

        /// <summary>
        /// Gets or sets the CPU usage percentage
        /// </summary>
        public double CpuUsagePercent { get; set; }

        /// <summary>
        /// Gets or sets the duration in milliseconds
        /// </summary>
        public double DurationMs { get; set; }

        /// <summary>
        /// Gets or sets the billing amount
        /// </summary>
        public decimal BillingAmount { get; set; }

        /// <summary>
        /// Gets or sets the logs
        /// </summary>
        public List<string> Logs { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the metrics
        /// </summary>
        public Dictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets or sets the trace ID
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Gets or sets the span ID
        /// </summary>
        public string SpanId { get; set; }
    }

    /// <summary>
    /// Function validation result
    /// </summary>
    public class FunctionValidationResult
    {
        /// <summary>
        /// Gets or sets whether the function is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the errors
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the warnings
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the information messages
        /// </summary>
        public List<string> Info { get; set; } = new List<string>();
    }

    /// <summary>
    /// Function compilation result
    /// </summary>
    public class FunctionCompilationResult
    {
        /// <summary>
        /// Gets or sets whether the compilation was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the compiled code
        /// </summary>
        public string CompiledCode { get; set; }

        /// <summary>
        /// Gets or sets the errors
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the warnings
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the compilation time in milliseconds
        /// </summary>
        public double CompilationTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the output file path
        /// </summary>
        public string OutputFilePath { get; set; }
    }

    /// <summary>
    /// Function runtime status
    /// </summary>
    public class FunctionRuntimeStatus
    {
        /// <summary>
        /// Gets or sets whether the runtime is available
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Gets or sets the runtime type
        /// </summary>
        public string RuntimeType { get; set; }

        /// <summary>
        /// Gets or sets the runtime version
        /// </summary>
        public string RuntimeVersion { get; set; }

        /// <summary>
        /// Gets or sets the uptime in seconds
        /// </summary>
        public double UptimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the memory usage in MB
        /// </summary>
        public double MemoryUsageMb { get; set; }

        /// <summary>
        /// Gets or sets the CPU usage in percentage
        /// </summary>
        public double CpuUsagePercentage { get; set; }

        /// <summary>
        /// Gets or sets the number of active executions
        /// </summary>
        public int ActiveExecutions { get; set; }

        /// <summary>
        /// Gets or sets the total executions
        /// </summary>
        public int TotalExecutions { get; set; }

        /// <summary>
        /// Gets or sets the failed executions
        /// </summary>
        public int FailedExecutions { get; set; }

        /// <summary>
        /// Gets or sets the average execution time in milliseconds
        /// </summary>
        public double AverageExecutionTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the status message
        /// </summary>
        public string StatusMessage { get; set; }
    }
}
