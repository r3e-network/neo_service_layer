using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using Python.Runtime;

namespace NeoServiceLayer.Services.Function.Runtimes
{
    /// <summary>
    /// Python runtime for executing Python functions
    /// </summary>
    public class PythonRuntime : IFunctionRuntime, IDisposable
    {
        private readonly ILogger<PythonRuntime> _logger;
        private readonly PythonRuntimeOptions _options;
        private readonly Stopwatch _uptime;
        private int _totalExecutions;
        private int _failedExecutions;
        private double _totalExecutionTime;
        private int _activeExecutions;
        private bool _initialized;
        private bool _disposed;

        /// <inheritdoc/>
        public string RuntimeType => "python";

        /// <inheritdoc/>
        public string RuntimeVersion => "3.9";

        /// <inheritdoc/>
        public IEnumerable<string> SupportedFileExtensions => new[] { ".py" };

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRuntime"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="options">Options</param>
        public PythonRuntime(
            ILogger<PythonRuntime> logger,
            IOptions<PythonRuntimeOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _uptime = Stopwatch.StartNew();
        }

        /// <inheritdoc/>
        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing Python runtime");

            try
            {
                // Set Python home
                if (!string.IsNullOrEmpty(_options.PythonHome))
                {
                    // Set Python home path
                    // Note: Runtime.PythonHome is not available, so we'll use environment variables instead
                    Environment.SetEnvironmentVariable("PYTHONHOME", _options.PythonHome);
                }

                // Set Python DLL path
                if (!string.IsNullOrEmpty(_options.PythonDllPath))
                {
                    // Set Python DLL path
                    // Note: Runtime.PythonDLL is not available, so we'll use environment variables instead
                    Environment.SetEnvironmentVariable("PYTHONPATH", _options.PythonDllPath);
                }

                // Initialize Python
                PythonEngine.Initialize();
                _initialized = true;

                // Import standard modules
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    dynamic os = Py.Import("os");
                    dynamic json = Py.Import("json");
                    dynamic datetime = Py.Import("datetime");
                    dynamic math = Py.Import("math");
                    dynamic random = Py.Import("random");
                    dynamic re = Py.Import("re");
                    dynamic collections = Py.Import("collections");
                    dynamic itertools = Py.Import("itertools");
                    dynamic functools = Py.Import("functools");
                    dynamic operatorMod = Py.Import("operator");
                    dynamic base64 = Py.Import("base64");
                    dynamic hashlib = Py.Import("hashlib");
                    dynamic uuid = Py.Import("uuid");
                    dynamic time = Py.Import("time");
                }

                _logger.LogInformation("Python runtime initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Python runtime: {Message}", ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionExecutionResult> ExecuteAsync(Core.Models.Function function, Dictionary<string, object> parameters, FunctionExecutionContext context)
        {
            _logger.LogInformation("Executing Python function: {FunctionId}", function.Id);

            if (!_initialized)
            {
                await InitializeAsync();
            }

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

                // Execute the function
                using (Py.GIL())
                {
                    // Create a new module for this execution
                    var moduleName = $"function_{function.Id}_{Guid.NewGuid():N}";
                    dynamic module = Py.CreateScope(moduleName);

                    // Add logging
                    module.Set("__logs", new List<string>());
                    module.Exec(@"
import sys
class Logger:
    def __init__(self, logs):
        self.logs = logs
    def log(self, message):
        self.logs.append(str(message))
        print(message)
    def error(self, message):
        self.logs.append(f'ERROR: {str(message)}')
        print(f'ERROR: {message}')
    def warn(self, message):
        self.logs.append(f'WARN: {str(message)}')
        print(f'WARN: {message}')
    def info(self, message):
        self.logs.append(f'INFO: {str(message)}')
        print(f'INFO: {message}')

console = Logger(__logs)
");

                    // Add metrics
                    module.Set("__metrics", new Dictionary<string, double>());
                    module.Exec(@"
class Metrics:
    def __init__(self, metrics):
        self.metrics = metrics
    def record(self, name, value):
        self.metrics[name] = float(value)

metrics = Metrics(__metrics)
");

                    // Add environment variables
                    var env = new Dictionary<string, string>();
                    foreach (var variable in context.EnvironmentVariables)
                    {
                        env[variable.Key] = variable.Value;
                    }
                    module.Set("env", env);

                    // Add secrets
                    var secrets = new Dictionary<string, string>();
                    foreach (var secret in context.Secrets)
                    {
                        secrets[secret.Key] = secret.Value;
                    }
                    module.Set("secrets", secrets);

                    // Add services
                    var services = new Dictionary<string, object>();
                    foreach (var service in context.Services)
                    {
                        services[service.Key] = service.Value;
                    }
                    module.Set("services", services);

                    // Add parameters
                    module.Set("params", parameters);

                    // Execute the function
                    var functionCode = function.Code;
                    module.Exec(functionCode);

                    // Get logs
                    var pyLogs = module.Get("__logs");
                    foreach (var log in pyLogs)
                    {
                        logs.Add(log.ToString());
                    }

                    // Get metrics
                    var pyMetrics = module.Get("__metrics");
                    foreach (var metric in pyMetrics)
                    {
                        metrics[metric.Key.ToString()] = Convert.ToDouble(metric.Value);
                    }

                    // Get output
                    object output = null;
                    if (module.Contains("result"))
                    {
                        var pyResult = module.Get("result");
                        output = ConvertPyObjectToNetObject(pyResult);
                    }

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
                }

                _logger.LogInformation("Python function executed successfully: {FunctionId}", function.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Python function {FunctionId}: {Message}", function.Id, ex.Message);

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
            _logger.LogInformation("Validating Python function: {FunctionId}", function.Id);

            if (!_initialized)
            {
                await InitializeAsync();
            }

            var result = new FunctionValidationResult();

            try
            {
                using (Py.GIL())
                {
                    // Create a new module for validation
                    var moduleName = $"validation_{function.Id}_{Guid.NewGuid():N}";
                    dynamic module = Py.CreateScope(moduleName);

                    // Add validation code
                    module.Exec(@"
import ast

def validate_python_code(code):
    try:
        ast.parse(code)
        return True, []
    except SyntaxError as e:
        return False, [f'Syntax error at line {e.lineno}, column {e.offset}: {e.msg}']
    except Exception as e:
        return False, [f'Validation error: {str(e)}']
");

                    // Validate the function code
                    var validationResult = module.Eval($"validate_python_code('''{function.Code}''')");
                    var isValid = validationResult[0].As<bool>();
                    var errors = validationResult[1];

                    result.IsValid = isValid;
                    foreach (var error in errors)
                    {
                        result.Errors.Add(error.ToString());
                    }
                }

                _logger.LogInformation("Python function validation {Result}: {FunctionId}", result.IsValid ? "successful" : "failed", function.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Python function {FunctionId}: {Message}", function.Id, ex.Message);
                result.IsValid = false;
                result.Errors.Add($"Validation error: {ex.Message}");
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<FunctionCompilationResult> CompileAsync(Core.Models.Function function)
        {
            _logger.LogInformation("Compiling Python function: {FunctionId}", function.Id);

            if (!_initialized)
            {
                await InitializeAsync();
            }

            var result = new FunctionCompilationResult();

            try
            {
                // Python doesn't need compilation, but we can still validate it
                var validationResult = await ValidateAsync(function);
                if (!validationResult.IsValid)
                {
                    result.IsSuccess = false;
                    result.Errors = validationResult.Errors;
                    return result;
                }

                using (Py.GIL())
                {
                    // Create a new module for compilation
                    var moduleName = $"compilation_{function.Id}_{Guid.NewGuid():N}";
                    dynamic module = Py.CreateScope(moduleName);

                    // Add compilation code
                    module.Exec(@"
import py_compile
import tempfile
import os

def compile_python_code(code):
    try:
        # Create a temporary file
        fd, path = tempfile.mkstemp(suffix='.py')
        try:
            with os.fdopen(fd, 'w') as f:
                f.write(code)

            # Compile the file
            py_compile.compile(path, doraise=True)

            # Read the compiled file
            compiled_path = path + 'c'
            if os.path.exists(compiled_path):
                with open(compiled_path, 'rb') as f:
                    compiled_code = f.read()
                return True, compiled_code, [], [], compiled_path
            else:
                return False, None, ['Compilation failed: No output file'], [], None
        finally:
            # Clean up
            try:
                os.unlink(path)
            except:
                pass
            try:
                os.unlink(path + 'c')
            except:
                pass
    except py_compile.PyCompileError as e:
        return False, None, [f'Compilation error: {str(e)}'], [], None
    except Exception as e:
        return False, None, [f'Compilation error: {str(e)}'], [], None
");

                    // Compile the function code
                    var compilationResult = module.Eval($"compile_python_code('''{function.Code}''')");
                    var isSuccess = compilationResult[0].As<bool>();
                    var compiledCode = compilationResult[1];
                    var errors = compilationResult[2];
                    var warnings = compilationResult[3];
                    var outputFilePath = compilationResult[4];

                    result.IsSuccess = isSuccess;
                    result.CompiledCode = function.Code; // Use original code since we can't use the bytecode directly

                    foreach (var error in errors)
                    {
                        result.Errors.Add(error.ToString());
                    }

                    foreach (var warning in warnings)
                    {
                        result.Warnings.Add(warning.ToString());
                    }

                    if (outputFilePath != null)
                    {
                        result.OutputFilePath = outputFilePath.ToString();
                    }
                }

                _logger.LogInformation("Python function compilation {Result}: {FunctionId}", result.IsSuccess ? "successful" : "failed", function.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling Python function {FunctionId}: {Message}", function.Id, ex.Message);
                result.IsSuccess = false;
                result.Errors.Add($"Compilation error: {ex.Message}");
            }

            return result;
        }

        /// <inheritdoc/>
        public Task<FunctionRuntimeStatus> GetStatusAsync()
        {
            _logger.LogInformation("Getting Python runtime status");

            var status = new FunctionRuntimeStatus
            {
                IsAvailable = _initialized,
                RuntimeType = RuntimeType,
                RuntimeVersion = RuntimeVersion,
                UptimeSeconds = _uptime.Elapsed.TotalSeconds,
                ActiveExecutions = _activeExecutions,
                TotalExecutions = _totalExecutions,
                FailedExecutions = _failedExecutions,
                AverageExecutionTimeMs = _totalExecutions > 0 ? _totalExecutionTime / _totalExecutions : 0,
                StatusMessage = _initialized ? "Python runtime is available" : "Python runtime is not initialized"
            };

            return Task.FromResult(status);
        }

        /// <inheritdoc/>
        public Task ShutdownAsync()
        {
            _logger.LogInformation("Shutting down Python runtime");

            if (_initialized && !_disposed)
            {
                PythonEngine.Shutdown();
                _initialized = false;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_initialized)
                {
                    PythonEngine.Shutdown();
                    _initialized = false;
                }

                _disposed = true;
            }
        }

        private object ConvertPyObjectToNetObject(dynamic pyObject)
        {
            if (pyObject == null || pyObject.IsNone())
            {
                return null;
            }
            else if (pyObject.GetPythonType().Name == "bool")
            {
                return pyObject.As<bool>();
            }
            else if (pyObject.GetPythonType().Name == "int")
            {
                return pyObject.As<int>();
            }
            else if (pyObject.GetPythonType().Name == "float")
            {
                return pyObject.As<double>();
            }
            else if (pyObject.GetPythonType().Name == "str")
            {
                return pyObject.As<string>();
            }
            else if (pyObject.GetPythonType().Name == "list" || pyObject.GetPythonType().Name == "tuple")
            {
                var list = new List<object>();
                foreach (var item in pyObject)
                {
                    list.Add(ConvertPyObjectToNetObject(item));
                }
                return list;
            }
            else if (pyObject.GetPythonType().Name == "dict")
            {
                var dict = new Dictionary<string, object>();
                foreach (var item in pyObject.items())
                {
                    var key = item[0].ToString();
                    var value = ConvertPyObjectToNetObject(item[1]);
                    dict[key] = value;
                }
                return dict;
            }
            else
            {
                return pyObject.ToString();
            }
        }
    }

    /// <summary>
    /// Options for Python runtime
    /// </summary>
    public class PythonRuntimeOptions
    {
        /// <summary>
        /// Gets or sets the Python home directory
        /// </summary>
        public string PythonHome { get; set; }

        /// <summary>
        /// Gets or sets the Python DLL path
        /// </summary>
        public string PythonDllPath { get; set; }

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
