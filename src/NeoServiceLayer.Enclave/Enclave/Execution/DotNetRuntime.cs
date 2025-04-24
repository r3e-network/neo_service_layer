using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;
using System.Linq;
using System.Text.Json;

namespace NeoServiceLayer.Enclave.Enclave.Execution
{
    /// <summary>
    /// .NET runtime for executing C# functions
    /// </summary>
    public class DotNetRuntime : IFunctionRuntime
    {
        private readonly ILogger<DotNetRuntime> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetRuntime"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public DotNetRuntime(ILogger<DotNetRuntime> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<object> CompileAsync(string sourceCode, string entryPoint)
        {
            _logger.LogInformation("Compiling C# function, EntryPoint: {EntryPoint}", entryPoint);

            try
            {
                // Parse the entry point
                var entryPointParts = entryPoint.Split('.');
                if (entryPointParts.Length < 2)
                {
                    throw new Exception($"Invalid entry point format: {entryPoint}. Expected format: Namespace.Class.Method");
                }

                // Add necessary references and imports
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
                var references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Dictionary<,>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(JsonSerializer).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).Assembly.Location)
                };

                // Compile the code
                var compilation = CSharpCompilation.Create(
                    $"DynamicFunction_{Guid.NewGuid():N}",
                    new[] { syntaxTree },
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using var ms = new MemoryStream();
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var errors = new StringBuilder();
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            errors.AppendLine(diagnostic.ToString());
                        }
                    }
                    throw new Exception($"Compilation failed: {errors}");
                }

                ms.Seek(0, SeekOrigin.Begin);
                var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                _logger.LogInformation("C# function compiled successfully");
                return assembly;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling C# function");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(object compiledAssembly, string entryPoint, Dictionary<string, object> parameters, FunctionExecutionContext context)
        {
            _logger.LogInformation("Executing C# function, EntryPoint: {EntryPoint}", entryPoint);

            try
            {
                var assembly = (Assembly)compiledAssembly;

                // Parse the entry point
                var entryPointParts = entryPoint.Split('.');
                var methodName = entryPointParts[entryPointParts.Length - 1];
                var className = entryPointParts[entryPointParts.Length - 2];
                var namespaceName = string.Join(".", entryPointParts, 0, entryPointParts.Length - 2);

                // Find the type and method
                var type = assembly.GetType($"{namespaceName}.{className}");
                if (type == null)
                {
                    throw new Exception($"Type not found: {namespaceName}.{className}");
                }

                var method = type.GetMethod(methodName);
                if (method == null)
                {
                    throw new Exception($"Method not found: {methodName}");
                }

                // Create an instance of the class
                var instance = Activator.CreateInstance(type);

                // Set up environment variables
                foreach (var kvp in context.EnvironmentVariables)
                {
                    Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
                }

                // Set up cancellation token for timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(context.MaxExecutionTime));

                // Execute the method
                var task = (Task)method.Invoke(instance, new object[] { parameters, cts.Token });
                await task;

                // Get the result
                var resultProperty = task.GetType().GetProperty("Result");
                var result = resultProperty?.GetValue(task);

                _logger.LogInformation("C# function executed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing C# function");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteForEventAsync(object compiledAssembly, string entryPoint, Event eventData, FunctionExecutionContext context)
        {
            _logger.LogInformation("Executing C# function for event, EntryPoint: {EntryPoint}, EventType: {EventType}", entryPoint, eventData.Type);

            try
            {
                var assembly = (Assembly)compiledAssembly;

                // Parse the entry point
                var entryPointParts = entryPoint.Split('.');
                var methodName = entryPointParts[entryPointParts.Length - 1];
                var className = entryPointParts[entryPointParts.Length - 2];
                var namespaceName = string.Join(".", entryPointParts, 0, entryPointParts.Length - 2);

                // Find the type and method
                var type = assembly.GetType($"{namespaceName}.{className}");
                if (type == null)
                {
                    throw new Exception($"Type not found: {namespaceName}.{className}");
                }

                var method = type.GetMethod(methodName);
                if (method == null)
                {
                    throw new Exception($"Method not found: {methodName}");
                }

                // Create an instance of the class
                var instance = Activator.CreateInstance(type);

                // Set up environment variables
                foreach (var kvp in context.EnvironmentVariables)
                {
                    Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
                }

                // Set up cancellation token for timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(context.MaxExecutionTime));

                // Execute the method
                var task = (Task)method.Invoke(instance, new object[] { eventData, cts.Token });
                await task;

                // Get the result
                var resultProperty = task.GetType().GetProperty("Result");
                var result = resultProperty?.GetValue(task);

                _logger.LogInformation("C# function executed successfully for event");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing C# function for event");
                throw;
            }
        }
    }
}
