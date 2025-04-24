using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Runtimes
{
    /// <summary>
    /// Factory for creating function runtimes
    /// </summary>
    public class FunctionRuntimeFactory : IFunctionRuntimeFactory
    {
        private readonly ILogger<FunctionRuntimeFactory> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, IFunctionRuntime> _runtimes;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionRuntimeFactory"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="serviceProvider">Service provider</param>
        public FunctionRuntimeFactory(
            ILogger<FunctionRuntimeFactory> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _runtimes = new Dictionary<string, IFunctionRuntime>(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public IFunctionRuntime GetRuntime(string runtimeType)
        {
            _logger.LogInformation("Getting runtime: {RuntimeType}", runtimeType);

            if (string.IsNullOrEmpty(runtimeType))
            {
                throw new ArgumentException("Runtime type cannot be null or empty", nameof(runtimeType));
            }

            // Check if runtime is already created
            if (_runtimes.TryGetValue(runtimeType, out var runtime))
            {
                return runtime;
            }

            // Create runtime based on type
            switch (runtimeType.ToLowerInvariant())
            {
                case "javascript":
                case "js":
                    runtime = _serviceProvider.GetRequiredService<JavaScriptRuntime>();
                    break;

                case "python":
                case "py":
                    runtime = _serviceProvider.GetRequiredService<PythonRuntime>();
                    break;

                case "csharp":
                case "cs":
                    runtime = _serviceProvider.GetRequiredService<CSharpRuntime>();
                    break;

                default:
                    throw new ArgumentException($"Unsupported runtime type: {runtimeType}", nameof(runtimeType));
            }

            // Initialize runtime
            runtime.InitializeAsync().GetAwaiter().GetResult();

            // Cache runtime
            _runtimes[runtimeType] = runtime;

            return runtime;
        }

        /// <inheritdoc/>
        public IFunctionRuntime GetRuntimeForFunction(Core.Models.Function function)
        {
            _logger.LogInformation("Getting runtime for function: {FunctionId}", function.Id);

            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            // If runtime is specified, use it
            if (!string.IsNullOrEmpty(function.Runtime))
            {
                return GetRuntime(function.Runtime);
            }

            // Try to determine runtime from file extension
            var fileExtension = System.IO.Path.GetExtension(function.EntryPoint);
            if (!string.IsNullOrEmpty(fileExtension))
            {
                // Get all runtimes
                var runtimes = new List<IFunctionRuntime>
                {
                    _serviceProvider.GetRequiredService<JavaScriptRuntime>(),
                    _serviceProvider.GetRequiredService<PythonRuntime>(),
                    _serviceProvider.GetRequiredService<CSharpRuntime>()
                };

                // Find runtime that supports the file extension
                foreach (var runtime in runtimes)
                {
                    if (runtime.SupportedFileExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                    {
                        // Initialize runtime
                        runtime.InitializeAsync().GetAwaiter().GetResult();

                        // Cache runtime
                        _runtimes[runtime.RuntimeType] = runtime;

                        return runtime;
                    }
                }
            }

            // Default to JavaScript
            return GetRuntime("javascript");
        }

        /// <inheritdoc/>
        public IEnumerable<IFunctionRuntime> GetAllRuntimes()
        {
            _logger.LogInformation("Getting all runtimes");

            // Create all runtimes
            var runtimes = new List<IFunctionRuntime>
            {
                GetRuntime("javascript"),
                GetRuntime("python"),
                GetRuntime("csharp")
            };

            return runtimes;
        }

        /// <inheritdoc/>
        public void ShutdownAllRuntimes()
        {
            _logger.LogInformation("Shutting down all runtimes");

            foreach (var runtime in _runtimes.Values)
            {
                try
                {
                    runtime.ShutdownAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error shutting down runtime {RuntimeType}: {Message}", runtime.RuntimeType, ex.Message);
                }
            }

            _runtimes.Clear();
        }
    }
}
