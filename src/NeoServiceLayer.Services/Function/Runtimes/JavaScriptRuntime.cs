using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Runtimes
{
    /// <summary>
    /// JavaScript runtime for executing JavaScript functions
    /// </summary>
    public class JavaScriptRuntime : IFunctionRuntime
    {
        private readonly ILogger<JavaScriptRuntime> _logger;
        private readonly JavaScriptRuntimeOptions _options;
        private readonly Engine _engine;
        private readonly Stopwatch _uptime;
        private int _totalExecutions;
        private int _failedExecutions;
        private double _totalExecutionTime;
        private int _activeExecutions;

        /// <inheritdoc/>
        public string RuntimeType => "javascript";

        /// <inheritdoc/>
        public string RuntimeVersion => "ES2020";

        /// <inheritdoc/>
        public IEnumerable<string> SupportedFileExtensions => new[] { ".js", ".mjs" };

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptRuntime"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="options">Options</param>
        public JavaScriptRuntime(
            ILogger<JavaScriptRuntime> logger,
            IOptions<JavaScriptRuntimeOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _uptime = Stopwatch.StartNew();

            // Create engine with constraints
            _engine = new Engine(cfg =>
            {
                cfg.LimitRecursion(_options.MaxRecursionDepth);
                cfg.MaxStatements(_options.MaxStatements);
                cfg.TimeoutInterval(TimeSpan.FromMilliseconds(_options.DefaultTimeoutMs));
                cfg.DebugMode(_options.DebugMode);
                // AllowClr takes an assembly parameter, not a boolean
                if (_options.AllowClr)
                {
                    cfg.AllowClr();
                }
            });

            // Register global objects
            RegisterGlobalObjects();
        }

        /// <inheritdoc/>
        public Task InitializeAsync()
        {
            _logger.LogInformation("Initializing JavaScript runtime");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<FunctionExecutionResult> ExecuteAsync(Core.Models.Function function, Dictionary<string, object> parameters, FunctionExecutionContext context)
        {
            _logger.LogInformation("Executing JavaScript function: {FunctionId}", function.Id);

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

                // Create a new engine instance for this execution
                var engine = new Engine(cfg =>
                {
                    cfg.LimitRecursion(_options.MaxRecursionDepth);
                    cfg.MaxStatements(_options.MaxStatements);
                    cfg.TimeoutInterval(TimeSpan.FromMilliseconds(context.TimeoutMs));
                    cfg.DebugMode(_options.DebugMode);
                    // AllowClr takes an assembly parameter, not a boolean
                    if (_options.AllowClr)
                    {
                        cfg.AllowClr();
                    }
                });

                // Register global objects
                RegisterGlobalObjects(engine);

                // Register console.log
                engine.SetValue("console", new
                {
                    log = new Action<object>(message =>
                    {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add(logMessage);
                        _logger.LogInformation("Function {FunctionId} log: {Message}", function.Id, logMessage);
                    }),
                    error = new Action<object>(message =>
                    {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add($"ERROR: {logMessage}");
                        _logger.LogError("Function {FunctionId} error: {Message}", function.Id, logMessage);
                    }),
                    warn = new Action<object>(message =>
                    {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add($"WARN: {logMessage}");
                        _logger.LogWarning("Function {FunctionId} warning: {Message}", function.Id, logMessage);
                    }),
                    info = new Action<object>(message =>
                    {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add($"INFO: {logMessage}");
                        _logger.LogInformation("Function {FunctionId} info: {Message}", function.Id, logMessage);
                    })
                });

                // Register metrics
                engine.SetValue("metrics", new
                {
                    record = new Action<string, double>((name, value) =>
                    {
                        metrics[name] = value;
                        _logger.LogInformation("Function {FunctionId} metric: {Name}={Value}", function.Id, name, value);
                    })
                });

                // Register environment variables
                var env = new Dictionary<string, string>();
                foreach (var variable in context.EnvironmentVariables)
                {
                    env[variable.Key] = variable.Value;
                }
                engine.SetValue("env", env);

                // Register secrets
                var secrets = new Dictionary<string, string>();
                foreach (var secret in context.Secrets)
                {
                    secrets[secret.Key] = secret.Value;
                }
                engine.SetValue("secrets", secrets);

                // Register services
                var services = new Dictionary<string, object>();
                foreach (var service in context.Services)
                {
                    services[service.Key] = service.Value;
                }
                engine.SetValue("services", services);

                // Register parameters
                engine.SetValue("params", parameters);

                // Execute the function
                var functionCode = function.Code;
                var output = engine.Evaluate(functionCode);

                // Convert output to .NET object
                var netOutput = ConvertJsValueToNetObject(output);

                // Set result
                result.Status = "success";
                result.Output = netOutput;
                result.EndTime = DateTime.UtcNow;
                result.ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                result.Logs = logs;
                result.Metrics = metrics;

                // Update statistics
                _totalExecutions++;
                _totalExecutionTime += result.ExecutionTimeMs;

                _logger.LogInformation("JavaScript function executed successfully: {FunctionId}", function.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing JavaScript function {FunctionId}: {Message}", function.Id, ex.Message);

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
            _logger.LogInformation("Validating JavaScript function: {FunctionId}", function.Id);

            var result = new FunctionValidationResult();

            try
            {
                // Create a new engine instance for validation
                var engine = new Engine(cfg =>
                {
                    cfg.LimitRecursion(_options.MaxRecursionDepth);
                    cfg.MaxStatements(_options.MaxStatements);
                    cfg.TimeoutInterval(TimeSpan.FromMilliseconds(_options.DefaultTimeoutMs));
                    cfg.DebugMode(_options.DebugMode);
                    // AllowClr takes an assembly parameter, not a boolean
                    if (_options.AllowClr)
                    {
                        cfg.AllowClr();
                    }
                });

                // Parse the function code
                engine.Execute(function.Code);

                // If no exception was thrown, the function is valid
                result.IsValid = true;
                _logger.LogInformation("JavaScript function validation successful: {FunctionId}", function.Id);
            }
            catch (JavaScriptException ex)
            {
                _logger.LogError(ex, "JavaScript error in function {FunctionId}: {Message}", function.Id, ex.Message);
                result.IsValid = false;
                result.Errors.Add($"JavaScript error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JavaScript function {FunctionId}: {Message}", function.Id, ex.Message);
                result.IsValid = false;
                result.Errors.Add($"Validation error: {ex.Message}");
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<FunctionCompilationResult> CompileAsync(Core.Models.Function function)
        {
            _logger.LogInformation("Compiling JavaScript function: {FunctionId}", function.Id);

            var result = new FunctionCompilationResult();

            try
            {
                // JavaScript doesn't need compilation, but we can still validate it
                var validationResult = await ValidateAsync(function);
                if (!validationResult.IsValid)
                {
                    result.IsSuccess = false;
                    result.Errors = validationResult.Errors;
                    return result;
                }

                // Set result
                result.IsSuccess = true;
                result.CompiledCode = function.Code;
                _logger.LogInformation("JavaScript function compilation successful: {FunctionId}", function.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling JavaScript function {FunctionId}: {Message}", function.Id, ex.Message);
                result.IsSuccess = false;
                result.Errors.Add($"Compilation error: {ex.Message}");
            }

            return result;
        }

        /// <inheritdoc/>
        public Task<FunctionRuntimeStatus> GetStatusAsync()
        {
            _logger.LogInformation("Getting JavaScript runtime status");

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
                StatusMessage = "JavaScript runtime is available"
            };

            return Task.FromResult(status);
        }

        /// <inheritdoc/>
        public Task ShutdownAsync()
        {
            _logger.LogInformation("Shutting down JavaScript runtime");
            return Task.CompletedTask;
        }

        private void RegisterGlobalObjects(Engine engine = null)
        {
            engine = engine ?? _engine;

            // Register global objects
            engine.SetValue("setTimeout", new Action<object, int>((callback, timeout) =>
            {
                // No-op for now
            }));

            engine.SetValue("clearTimeout", new Action<object>(id =>
            {
                // No-op for now
            }));

            engine.SetValue("setInterval", new Action<object, int>((callback, interval) =>
            {
                // No-op for now
            }));

            engine.SetValue("clearInterval", new Action<object>(id =>
            {
                // No-op for now
            }));

            engine.SetValue("JSON", new
            {
                parse = new Func<string, object>(json => Newtonsoft.Json.JsonConvert.DeserializeObject(json)),
                stringify = new Func<object, string>(obj => Newtonsoft.Json.JsonConvert.SerializeObject(obj))
            });
        }

        private object ConvertJsValueToNetObject(JsValue value)
        {
            if (value.IsNull())
            {
                return null;
            }
            else if (value.IsUndefined())
            {
                return null;
            }
            else if (value.IsBoolean())
            {
                return value.AsBoolean();
            }
            else if (value.IsNumber())
            {
                return value.AsNumber();
            }
            else if (value.IsString())
            {
                return value.AsString();
            }
            else if (value.IsDate())
            {
                return value.AsDate().ToDateTime();
            }
            else if (value.IsArray())
            {
                var array = value.AsArray();
                var result = new object[array.Length];
                for (var i = 0; i < array.Length; i++)
                {
                    result[i] = ConvertJsValueToNetObject(array[i]);
                }
                return result;
            }
            else if (value.IsObject())
            {
                var obj = value.AsObject();
                var result = new Dictionary<string, object>();
                foreach (var property in obj.GetOwnProperties())
                {
                    var propertyValue = obj.Get(property.Key);
                    // Convert JsValue to string before using as key
                    result[property.Key.ToString()] = ConvertJsValueToNetObject(propertyValue);
                }
                return result;
            }
            else
            {
                return value.ToString();
            }
        }
    }

    /// <summary>
    /// Options for JavaScript runtime
    /// </summary>
    public class JavaScriptRuntimeOptions
    {
        /// <summary>
        /// Gets or sets the maximum recursion depth
        /// </summary>
        public int MaxRecursionDepth { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum statements
        /// </summary>
        public int MaxStatements { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the default timeout in milliseconds
        /// </summary>
        public int DefaultTimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Gets or sets whether debug mode is enabled
        /// </summary>
        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// Gets or sets whether CLR is allowed
        /// </summary>
        public bool AllowClr { get; set; } = false;
    }
}
