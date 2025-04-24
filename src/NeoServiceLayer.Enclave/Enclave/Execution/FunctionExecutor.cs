using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Enclave.Enclave.Models;
using NeoServiceLayer.Enclave.Enclave.Services;

namespace NeoServiceLayer.Enclave.Enclave.Execution
{
    /// <summary>
    /// Executes serverless functions in the enclave
    /// </summary>
    public class FunctionExecutor
    {
        private readonly ILogger<FunctionExecutor> _logger;
        private readonly Dictionary<string, IFunctionRuntime> _runtimes;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionExecutor"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="nodeRuntime">Node.js runtime</param>
        /// <param name="dotnetRuntime">.NET runtime</param>
        /// <param name="pythonRuntime">Python runtime</param>
        public FunctionExecutor(
            ILogger<FunctionExecutor> logger,
            NodeJsRuntime nodeRuntime,
            DotNetRuntime dotnetRuntime,
            PythonRuntime pythonRuntime)
        {
            _logger = logger;
            _runtimes = new Dictionary<string, IFunctionRuntime>(StringComparer.OrdinalIgnoreCase)
            {
                { "node", nodeRuntime },
                { "javascript", nodeRuntime },
                { "dotnet", dotnetRuntime },
                { "csharp", dotnetRuntime },
                { "python", pythonRuntime }
            };
        }

        /// <summary>
        /// Validates and compiles a function
        /// </summary>
        /// <param name="metadata">Function metadata</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public virtual async Task ValidateAndCompileAsync(FunctionMetadata metadata)
        {
            _logger.LogInformation("Validating and compiling function: {Id}, Runtime: {Runtime}", metadata.Id, metadata.Runtime);

            if (!_runtimes.TryGetValue(metadata.Runtime, out var runtime))
            {
                throw new Exception($"Unsupported runtime: {metadata.Runtime}");
            }

            try
            {
                // Validate and compile the function
                metadata.CompiledAssembly = (await runtime.CompileAsync(metadata.SourceCode, metadata.EntryPoint)).ToString();
                _logger.LogInformation("Function compiled successfully: {Id}", metadata.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling function: {Id}", metadata.Id);
                throw new Exception($"Error compiling function: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes a function
        /// </summary>
        /// <param name="metadata">Function metadata</param>
        /// <param name="parameters">Function parameters</param>
        /// <returns>Function result</returns>
        public virtual async Task<object> ExecuteAsync(FunctionMetadata metadata, Dictionary<string, object> parameters)
        {
            _logger.LogInformation("Executing function: {Id}, Runtime: {Runtime}", metadata.Id, metadata.Runtime);

            if (!_runtimes.TryGetValue(metadata.Runtime, out var runtime))
            {
                throw new Exception($"Unsupported runtime: {metadata.Runtime}");
            }

            try
            {
                // Create execution context
                var context = new FunctionExecutionContext
                {
                    FunctionId = metadata.Id,
                    AccountId = metadata.AccountId,
                    EnvironmentVariables = metadata.EnvironmentVariables,
                    SecretIds = metadata.SecretIds,
                    MaxExecutionTime = metadata.MaxExecutionTime,
                    MaxMemory = metadata.MaxMemory
                };

                // Execute the function
                object compiledAssembly;

                // Handle different runtime types
                if (metadata.Runtime.Equals("javascript", StringComparison.OrdinalIgnoreCase) ||
                    metadata.Runtime.Equals("node", StringComparison.OrdinalIgnoreCase))
                {
                    // For JavaScript, deserialize the CompiledAssembly string to a CompiledJsFunction object
                    if (metadata.CompiledAssembly is string compiledAssemblyStr)
                    {
                        try
                        {
                            compiledAssembly = new CompiledJsFunction
                            {
                                SourceCode = metadata.SourceCode,
                                EntryPoint = metadata.EntryPoint
                            };
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deserializing compiled assembly for function: {Id}", metadata.Id);
                            throw new Exception($"Error deserializing compiled assembly: {ex.Message}");
                        }
                    }
                    else
                    {
                        compiledAssembly = new CompiledJsFunction
                        {
                            SourceCode = metadata.SourceCode,
                            EntryPoint = metadata.EntryPoint
                        };
                    }
                }
                else
                {
                    // For other runtimes, use the CompiledAssembly as is
                    compiledAssembly = metadata.CompiledAssembly;
                }

                var result = await runtime.ExecuteAsync(compiledAssembly, metadata.EntryPoint, parameters, context);
                _logger.LogInformation("Function executed successfully: {Id}", metadata.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function: {Id}", metadata.Id);
                throw new Exception($"Error executing function: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes a function for an event
        /// </summary>
        /// <param name="metadata">Function metadata</param>
        /// <param name="eventData">Event data</param>
        /// <returns>Function result</returns>
        public virtual async Task<object> ExecuteForEventAsync(FunctionMetadata metadata, NeoServiceLayer.Enclave.Enclave.Models.Event eventData)
        {
            _logger.LogInformation("Executing function for event: {Id}, Runtime: {Runtime}, EventType: {EventType}", metadata.Id, metadata.Runtime, eventData.Type);

            if (!_runtimes.TryGetValue(metadata.Runtime, out var runtime))
            {
                throw new Exception($"Unsupported runtime: {metadata.Runtime}");
            }

            try
            {
                // Create execution context
                var context = new FunctionExecutionContext
                {
                    FunctionId = metadata.Id,
                    AccountId = metadata.AccountId,
                    EnvironmentVariables = metadata.EnvironmentVariables,
                    SecretIds = metadata.SecretIds,
                    MaxExecutionTime = metadata.MaxExecutionTime,
                    MaxMemory = metadata.MaxMemory,
                    Event = eventData
                };

                // Execute the function
                // Convert the Enclave.Models.Event to Core.Models.Event
                var coreEvent = new NeoServiceLayer.Core.Models.Event
                {
                    Id = eventData.Id,
                    Type = eventData.Type,
                    Name = eventData.Name,
                    Source = eventData.Source,
                    Data = eventData.Data,
                    Timestamp = eventData.Timestamp
                };

                // Handle different runtime types for compiled assembly
                object compiledAssembly;

                if (metadata.Runtime.Equals("javascript", StringComparison.OrdinalIgnoreCase) ||
                    metadata.Runtime.Equals("node", StringComparison.OrdinalIgnoreCase))
                {
                    // For JavaScript, deserialize the CompiledAssembly string to a CompiledJsFunction object
                    if (metadata.CompiledAssembly is string compiledAssemblyStr)
                    {
                        try
                        {
                            compiledAssembly = new CompiledJsFunction
                            {
                                SourceCode = metadata.SourceCode,
                                EntryPoint = metadata.EntryPoint
                            };
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deserializing compiled assembly for function: {Id}", metadata.Id);
                            throw new Exception($"Error deserializing compiled assembly: {ex.Message}");
                        }
                    }
                    else
                    {
                        compiledAssembly = new CompiledJsFunction
                        {
                            SourceCode = metadata.SourceCode,
                            EntryPoint = metadata.EntryPoint
                        };
                    }
                }
                else
                {
                    // For other runtimes, use the CompiledAssembly as is
                    compiledAssembly = metadata.CompiledAssembly;
                }

                var result = await runtime.ExecuteForEventAsync(compiledAssembly, metadata.EntryPoint, coreEvent, context);
                _logger.LogInformation("Function executed successfully for event: {Id}, EventType: {EventType}", metadata.Id, eventData.Type);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function for event: {Id}, EventType: {EventType}", metadata.Id, eventData.Type);
                throw new Exception($"Error executing function for event: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Context for function execution
    /// </summary>
    public class FunctionExecutionContext
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the environment variables
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the secret IDs
        /// </summary>
        public List<Guid> SecretIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the maximum execution time
        /// </summary>
        public int MaxExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum memory
        /// </summary>
        public int MaxMemory { get; set; }

        /// <summary>
        /// Gets or sets the event data
        /// </summary>
        public NeoServiceLayer.Enclave.Enclave.Models.Event? Event { get; set; }
    }
}
