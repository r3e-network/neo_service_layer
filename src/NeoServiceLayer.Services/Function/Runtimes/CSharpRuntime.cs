using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Runtimes
{
    /// <summary>
    /// C# runtime for executing C# functions
    /// </summary>
    public class CSharpRuntime : IFunctionRuntime
    {
        private readonly ILogger<CSharpRuntime> _logger;
        private readonly CSharpRuntimeOptions _options;
        private readonly Stopwatch _uptime;
        private int _totalExecutions;
        private int _failedExecutions;
        private double _totalExecutionTime;
        private int _activeExecutions;
        private readonly Dictionary<Guid, Assembly> _compiledAssemblies;

        /// <inheritdoc/>
        public string RuntimeType => "csharp";

        /// <inheritdoc/>
        public string RuntimeVersion => "C# 10.0";

        /// <inheritdoc/>
        public IEnumerable<string> SupportedFileExtensions => new[] { ".cs" };

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpRuntime"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="options">Options</param>
        public CSharpRuntime(
            ILogger<CSharpRuntime> logger,
            IOptions<CSharpRuntimeOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _uptime = Stopwatch.StartNew();
            _compiledAssemblies = new Dictionary<Guid, Assembly>();
        }

        /// <inheritdoc/>
        public Task InitializeAsync()
        {
            _logger.LogInformation("Initializing C# runtime");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<FunctionExecutionResult> ExecuteAsync(Core.Models.Function function, Dictionary<string, object> parameters, FunctionExecutionContext context)
        {
            _logger.LogInformation("Executing C# function: {FunctionId}", function.Id);

            var result = new FunctionExecutionResult
            {
                ExecutionId = context.ExecutionId,
                FunctionId = function.Id,
                StartTime = DateTime.UtcNow,
                TraceId = context.TraceId
            };

            var logs = new List<string>();
            var metrics = new Dictionary<string, double>();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Increment active executions
                _activeExecutions++;

                // Get or compile the assembly
                Assembly assembly;
                if (!_compiledAssemblies.TryGetValue(function.Id, out assembly))
                {
                    var compilationResult = await CompileAsync(function);
                    if (!compilationResult.IsSuccess)
                    {
                        throw new Exception($"Compilation failed: {string.Join(", ", compilationResult.Errors)}");
                    }

                    // Load the assembly
                    var assemblyBytes = File.ReadAllBytes(compilationResult.OutputFilePath);
                    assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(assemblyBytes));
                    _compiledAssemblies[function.Id] = assembly;
                }

                // Create a logger
                var logger = new FunctionLogger(logs);

                // Create a metrics recorder
                var metricsRecorder = new FunctionMetricsRecorder(metrics);

                // Create the execution context
                var executionContext = new FunctionExecutionContextWrapper
                {
                    ExecutionId = context.ExecutionId,
                    AccountId = context.AccountId,
                    UserId = context.UserId,
                    TimeoutMs = context.TimeoutMs,
                    MaxMemoryMb = context.MaxMemoryMb,
                    EnvironmentVariables = context.EnvironmentVariables,
                    Secrets = context.Secrets,
                    Services = context.Services,
                    TraceId = context.TraceId,
                    ParentSpanId = context.ParentSpanId,
                    ExecutionMode = context.ExecutionMode,
                    Tags = context.Tags,
                    Logger = logger,
                    Metrics = metricsRecorder
                };

                // Find the function class
                var functionClass = assembly.GetType("NeoFunction.Function");
                if (functionClass == null)
                {
                    throw new Exception("Function class not found");
                }

                // Create an instance of the function class
                var functionInstance = Activator.CreateInstance(functionClass);

                // Find the execute method
                var executeMethod = functionClass.GetMethod("Execute");
                if (executeMethod == null)
                {
                    throw new Exception("Execute method not found");
                }

                // Execute the function
                var output = executeMethod.Invoke(functionInstance, new object[] { parameters, executionContext });

                // Set result
                result.Status = "success";
                result.Output = output;
                result.EndTime = DateTime.UtcNow;
                result.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                result.Logs = logs;
                result.Metrics = metrics;

                // Update statistics
                _totalExecutions++;
                _totalExecutionTime += result.ExecutionTimeMs;

                _logger.LogInformation("C# function executed successfully: {FunctionId}", function.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing C# function {FunctionId}: {Message}", function.Id, ex.Message);

                // Set result
                result.Status = "error";
                result.Error = ex.Message;
                result.EndTime = DateTime.UtcNow;
                result.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                result.Logs = logs;
                result.Metrics = metrics;

                // Update statistics
                _totalExecutions++;
                _failedExecutions++;
                _totalExecutionTime += result.ExecutionTimeMs;
            }
            finally
            {
                // Decrement active executions
                _activeExecutions--;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<FunctionValidationResult> ValidateAsync(Core.Models.Function function)
        {
            _logger.LogInformation("Validating C# function: {FunctionId}", function.Id);

            var result = new FunctionValidationResult();

            try
            {
                // Parse the function code
                var syntaxTree = CSharpSyntaxTree.ParseText(function.Code);
                var errors = syntaxTree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
                var warnings = syntaxTree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Warning).ToList();

                // Check for errors
                if (errors.Any())
                {
                    result.IsValid = false;
                    foreach (var error in errors)
                    {
                        result.Errors.Add($"Error {error.Id}: {error.GetMessage()} at {error.Location}");
                    }
                }
                else
                {
                    result.IsValid = true;
                }

                // Add warnings
                foreach (var warning in warnings)
                {
                    result.Warnings.Add($"Warning {warning.Id}: {warning.GetMessage()} at {warning.Location}");
                }

                _logger.LogInformation("C# function validation {Result}: {FunctionId}", result.IsValid ? "successful" : "failed", function.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating C# function {FunctionId}: {Message}", function.Id, ex.Message);
                result.IsValid = false;
                result.Errors.Add($"Validation error: {ex.Message}");
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<FunctionCompilationResult> CompileAsync(Core.Models.Function function)
        {
            _logger.LogInformation("Compiling C# function: {FunctionId}", function.Id);

            var result = new FunctionCompilationResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Create a unique output file path
                var outputFilePath = Path.Combine(Path.GetTempPath(), $"function_{function.Id}_{Guid.NewGuid():N}.dll");

                // Parse the function code
                var syntaxTree = CSharpSyntaxTree.ParseText(function.Code);

                // Create compilation
                var references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Dictionary<,>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ILogger).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IFunctionRuntime).Assembly.Location)
                };

                // Add additional references
                foreach (var reference in _options.AdditionalReferences)
                {
                    references.Add(MetadataReference.CreateFromFile(reference));
                }

                var compilation = CSharpCompilation.Create(
                    $"function_{function.Id}",
                    new[] { syntaxTree },
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                // Emit the assembly
                using (var ms = new MemoryStream())
                {
                    var emitResult = compilation.Emit(ms);

                    if (emitResult.Success)
                    {
                        // Write the assembly to disk
                        ms.Seek(0, SeekOrigin.Begin);
                        using (var fs = new FileStream(outputFilePath, FileMode.Create))
                        {
                            ms.CopyTo(fs);
                        }

                        // Set result
                        result.IsSuccess = true;
                        result.CompiledCode = function.Code;
                        result.CompilationTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                        result.OutputFilePath = outputFilePath;

                        _logger.LogInformation("C# function compilation successful: {FunctionId}", function.Id);
                    }
                    else
                    {
                        // Set result
                        result.IsSuccess = false;
                        result.CompilationTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                        // Add errors
                        foreach (var diagnostic in emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                        {
                            result.Errors.Add($"Error {diagnostic.Id}: {diagnostic.GetMessage()} at {diagnostic.Location}");
                        }

                        // Add warnings
                        foreach (var diagnostic in emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning))
                        {
                            result.Warnings.Add($"Warning {diagnostic.Id}: {diagnostic.GetMessage()} at {diagnostic.Location}");
                        }

                        _logger.LogWarning("C# function compilation failed: {FunctionId}", function.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling C# function {FunctionId}: {Message}", function.Id, ex.Message);
                result.IsSuccess = false;
                result.Errors.Add($"Compilation error: {ex.Message}");
            }

            return result;
        }

        /// <inheritdoc/>
        public Task<FunctionRuntimeStatus> GetStatusAsync()
        {
            _logger.LogInformation("Getting C# runtime status");

            var status = new FunctionRuntimeStatus
            {
                IsAvailable = true,
                RuntimeType = RuntimeType,
                RuntimeVersion = RuntimeVersion,
                UptimeSeconds = _uptime.Elapsed.TotalSeconds,
                ActiveExecutions = _activeExecutions,
                TotalExecutions = _totalExecutions,
                FailedExecutions = _failedExecutions,
                AverageExecutionTimeMs = _totalExecutions > 0 ? _totalExecutionTime / _totalExecutions : 0,
                StatusMessage = "C# runtime is available"
            };

            return Task.FromResult(status);
        }

        /// <inheritdoc/>
        public Task ShutdownAsync()
        {
            _logger.LogInformation("Shutting down C# runtime");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Logger for functions
        /// </summary>
        public class FunctionLogger
        {
            private readonly List<string> _logs;

            /// <summary>
            /// Initializes a new instance of the <see cref="FunctionLogger"/> class
            /// </summary>
            /// <param name="logs">Logs</param>
            public FunctionLogger(List<string> logs)
            {
                _logs = logs;
            }

            /// <summary>
            /// Logs a message
            /// </summary>
            /// <param name="message">Message</param>
            public void Log(string message)
            {
                _logs.Add(message);
                Console.WriteLine(message);
            }

            /// <summary>
            /// Logs an error
            /// </summary>
            /// <param name="message">Message</param>
            public void Error(string message)
            {
                _logs.Add($"ERROR: {message}");
                Console.Error.WriteLine($"ERROR: {message}");
            }

            /// <summary>
            /// Logs a warning
            /// </summary>
            /// <param name="message">Message</param>
            public void Warn(string message)
            {
                _logs.Add($"WARN: {message}");
                Console.WriteLine($"WARN: {message}");
            }

            /// <summary>
            /// Logs an info message
            /// </summary>
            /// <param name="message">Message</param>
            public void Info(string message)
            {
                _logs.Add($"INFO: {message}");
                Console.WriteLine($"INFO: {message}");
            }
        }

        /// <summary>
        /// Metrics recorder for functions
        /// </summary>
        public class FunctionMetricsRecorder
        {
            private readonly Dictionary<string, double> _metrics;

            /// <summary>
            /// Initializes a new instance of the <see cref="FunctionMetricsRecorder"/> class
            /// </summary>
            /// <param name="metrics">Metrics</param>
            public FunctionMetricsRecorder(Dictionary<string, double> metrics)
            {
                _metrics = metrics;
            }

            /// <summary>
            /// Records a metric
            /// </summary>
            /// <param name="name">Name</param>
            /// <param name="value">Value</param>
            public void Record(string name, double value)
            {
                _metrics[name] = value;
            }
        }

        /// <summary>
        /// Wrapper for function execution context
        /// </summary>
        public class FunctionExecutionContextWrapper
        {
            /// <summary>
            /// Gets or sets the execution ID
            /// </summary>
            public Guid ExecutionId { get; set; }

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
            public int TimeoutMs { get; set; }

            /// <summary>
            /// Gets or sets the maximum memory in MB
            /// </summary>
            public int MaxMemoryMb { get; set; }

            /// <summary>
            /// Gets or sets the environment variables
            /// </summary>
            public Dictionary<string, string> EnvironmentVariables { get; set; }

            /// <summary>
            /// Gets or sets the secrets
            /// </summary>
            public Dictionary<string, string> Secrets { get; set; }

            /// <summary>
            /// Gets or sets the services
            /// </summary>
            public Dictionary<string, object> Services { get; set; }

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
            public string ExecutionMode { get; set; }

            /// <summary>
            /// Gets or sets the execution tags
            /// </summary>
            public Dictionary<string, string> Tags { get; set; }

            /// <summary>
            /// Gets or sets the logger
            /// </summary>
            public FunctionLogger Logger { get; set; }

            /// <summary>
            /// Gets or sets the metrics recorder
            /// </summary>
            public FunctionMetricsRecorder Metrics { get; set; }
        }
    }

    /// <summary>
    /// Options for C# runtime
    /// </summary>
    public class CSharpRuntimeOptions
    {
        /// <summary>
        /// Gets or sets the additional references
        /// </summary>
        public List<string> AdditionalReferences { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the default timeout in milliseconds
        /// </summary>
        public int DefaultTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Gets or sets whether debug mode is enabled
        /// </summary>
        public bool DebugMode { get; set; } = false;
    }
}
