using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Enclave.Enclave.Services;
using NeoServiceLayer.Enclave.Enclave.Utilities;

namespace NeoServiceLayer.Enclave.Enclave.Execution
{
    /// <summary>
    /// Node.js runtime for executing JavaScript functions
    /// </summary>
    public class NodeJsRuntime : IFunctionRuntime
    {
        private readonly ILogger<NodeJsRuntime> _logger;
        private readonly EnclavePriceFeedService _priceFeedService;
        private readonly EnclaveSecretsService _secretsService;
        private readonly EnclaveWalletService _walletService;
        private readonly EnclaveFunctionService _functionService;
        private readonly string _sdkScript;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeJsRuntime"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="priceFeedService">Price feed service</param>
        /// <param name="secretsService">Secrets service</param>
        /// <param name="walletService">Wallet service</param>
        /// <param name="functionService">Function service</param>
        public NodeJsRuntime(
            ILogger<NodeJsRuntime> logger,
            EnclavePriceFeedService priceFeedService,
            EnclaveSecretsService secretsService,
            EnclaveWalletService walletService,
            EnclaveFunctionService functionService)
        {
            _logger = logger;
            _priceFeedService = priceFeedService;
            _secretsService = secretsService;
            _walletService = walletService;
            _functionService = functionService;

            // Load the SDK script
            var sdkPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Enclave", "Execution", "JsSdk", "neo-service-sdk.js");
            if (File.Exists(sdkPath))
            {
                _sdkScript = File.ReadAllText(sdkPath);
            }
            else
            {
                _logger.LogWarning("Neo Service SDK script not found at {Path}", sdkPath);
                _sdkScript = "// SDK not available";
            }
        }

        /// <inheritdoc/>
        public async Task<object> CompileAsync(string sourceCode, string entryPoint)
        {
            _logger.LogInformation("Compiling JavaScript function, EntryPoint: {EntryPoint}", entryPoint);

            try
            {
                // For JavaScript, we don't need to compile, just validate the syntax
                // Use Jint to validate the JavaScript syntax
                var engine = new Engine(options => {
                    options.LimitRecursion(100);
                    options.TimeoutInterval(TimeSpan.FromSeconds(5));
                    options.MaxStatements(1000);
                });

                try
                {
                    // Try to parse the code to validate syntax
                    engine.Execute(sourceCode);
                }
                catch (JavaScriptException jsEx)
                {
                    _logger.LogError(jsEx, "JavaScript syntax error: {Message}", jsEx.Message);
                    throw new Exception($"JavaScript syntax error: {jsEx.Message}");
                }

                // Verify that the entry point exists
                if (!sourceCode.Contains(entryPoint))
                {
                    throw new Exception($"Entry point '{entryPoint}' not found in the source code");
                }

                var compiledFunction = new CompiledJsFunction
                {
                    SourceCode = sourceCode,
                    EntryPoint = entryPoint
                };

                _logger.LogInformation("JavaScript function compiled successfully");
                return compiledFunction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling JavaScript function");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(object compiledAssembly, string entryPoint, Dictionary<string, object> parameters, FunctionExecutionContext context)
        {
            _logger.LogInformation("Executing JavaScript function, EntryPoint: {EntryPoint}", entryPoint);

            try
            {
                var compiledFunction = (CompiledJsFunction)compiledAssembly;
                var sourceCode = compiledFunction.SourceCode;

                // Create a new Jint engine with appropriate constraints
                var engine = new Engine(options => {
                    options.LimitRecursion(100);
                    options.TimeoutInterval(TimeSpan.FromSeconds(context.MaxExecutionTime / 1000));
                    options.MaxStatements(10000);
                    options.DebugMode();
                });

                // Add environment variables to the JavaScript context
                if (context.EnvironmentVariables != null)
                {
                    engine.SetValue("env", context.EnvironmentVariables);
                }

                // Add console.log functionality
                var logs = new List<string>();
                engine.SetValue("console", new {
                    log = new Action<object>(message => {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add(logMessage);
                        _logger.LogInformation("[JS Console] {Message}", logMessage);
                    }),
                    error = new Action<object>(message => {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add("ERROR: " + logMessage);
                        _logger.LogError("[JS Console Error] {Message}", logMessage);
                    }),
                    warn = new Action<object>(message => {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add("WARN: " + logMessage);
                        _logger.LogWarning("[JS Console Warn] {Message}", logMessage);
                    }),
                    debug = new Action<object>(message => {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add("DEBUG: " + logMessage);
                        _logger.LogDebug("[JS Console Debug] {Message}", logMessage);
                    })
                });

                // Add native function binding
                engine.SetValue("__callNativeFunction", new Func<string, object, Task<object>>(async (functionName, args) => {
                    return await HandleNativeFunctionCallAsync(functionName, args, context);
                }));

                // Load the Neo Service SDK
                engine.Execute(_sdkScript);

                // Execute the JavaScript code
                engine.Execute(sourceCode);

                // Convert parameters to a JavaScript object
                var parametersJson = JsonConvert.SerializeObject(parameters);
                var jsonString = parametersJson.Replace("'", "\\'");
                engine.Execute($"var __params = JSON.parse('{jsonString}')");

                // Call the entry point function with parameters
                var result = engine.Invoke(entryPoint, engine.GetValue("__params"));

                // Convert the result to a .NET object
                var resultObj = ConvertJsValueToObject(result);

                _logger.LogInformation("JavaScript function executed successfully");
                return new {
                    Result = resultObj,
                    Logs = logs,
                    ExecutionTime = DateTime.UtcNow,
                    MemoryUsage = GC.GetTotalMemory(false) / (1024 * 1024) // Approximate memory usage in MB
                };
            }
            catch (JavaScriptException jsEx)
            {
                _logger.LogError(jsEx, "JavaScript execution error: {Message}", jsEx.Message);
                throw new Exception($"JavaScript execution error: {jsEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing JavaScript function");
                throw;
            }
        }

        /// <summary>
        /// Handles native function calls from JavaScript
        /// </summary>
        /// <param name="functionName">Function name</param>
        /// <param name="args">Function arguments</param>
        /// <param name="context">Execution context</param>
        /// <returns>Function result</returns>
        private async Task<object> HandleNativeFunctionCallAsync(string functionName, object args, FunctionExecutionContext context)
        {
            _logger.LogInformation("Handling native function call: {FunctionName}", functionName);

            try
            {
                // Convert args to a dictionary
                var argsDict = args as Dictionary<string, object>;
                if (argsDict == null && args != null)
                {
                    argsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(args));
                }

                // Handle different function calls
                switch (functionName)
                {
                    // Price Feed functions
                    case "priceFeed.getPrice":
                        return await HandlePriceFeedGetPriceAsync(argsDict, context);
                    case "priceFeed.getAllPrices":
                        return await HandlePriceFeedGetAllPricesAsync(argsDict, context);
                    case "priceFeed.submitToOracle":
                        return await HandlePriceFeedSubmitToOracleAsync(argsDict, context);

                    // Secrets functions
                    case "secrets.getSecret":
                        return await HandleSecretsGetSecretAsync(argsDict, context);
                    case "secrets.getSecretById":
                        return await HandleSecretsGetSecretByIdAsync(argsDict, context);

                    // Blockchain functions
                    case "blockchain.getBlockHeight":
                        return await HandleBlockchainGetBlockHeightAsync(argsDict, context);
                    case "blockchain.getBlock":
                        return await HandleBlockchainGetBlockAsync(argsDict, context);
                    case "blockchain.getTransaction":
                        return await HandleBlockchainGetTransactionAsync(argsDict, context);
                    case "blockchain.getBalance":
                        return await HandleBlockchainGetBalanceAsync(argsDict, context);
                    case "blockchain.invokeRead":
                        return await HandleBlockchainInvokeReadAsync(argsDict, context);
                    case "blockchain.invokeWrite":
                        return await HandleBlockchainInvokeWriteAsync(argsDict, context);

                    // Event functions
                    case "events.registerBlockchainEvent":
                        return await HandleEventsRegisterBlockchainEventAsync(argsDict, context);
                    case "events.registerTimeEvent":
                        return await HandleEventsRegisterTimeEventAsync(argsDict, context);
                    case "events.triggerCustomEvent":
                        return await HandleEventsTriggerCustomEventAsync(argsDict, context);

                    // Logging functions
                    case "log.info":
                        return HandleLogInfoAsync(argsDict, context);
                    case "log.warn":
                        return HandleLogWarnAsync(argsDict, context);
                    case "log.error":
                        return HandleLogErrorAsync(argsDict, context);
                    case "log.debug":
                        return HandleLogDebugAsync(argsDict, context);

                    // Storage functions
                    case "storage.get":
                        return await HandleStorageGetAsync(argsDict, context);
                    case "storage.set":
                        return await HandleStorageSetAsync(argsDict, context);
                    case "storage.delete":
                        return await HandleStorageDeleteAsync(argsDict, context);

                    default:
                        throw new Exception($"Unknown native function: {functionName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling native function call: {FunctionName}", functionName);
                throw;
            }
        }

        #region Price Feed Handlers

        private async Task<object> HandlePriceFeedGetPriceAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var symbol = args["symbol"].ToString();
            var baseCurrency = args.ContainsKey("baseCurrency") ? args["baseCurrency"].ToString() : "USD";

            _logger.LogInformation("Getting price for symbol: {Symbol}, BaseCurrency: {BaseCurrency}", symbol, baseCurrency);

            var request = new
            {
                Symbol = symbol,
                BaseCurrency = baseCurrency
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _priceFeedService.HandleRequestAsync(
                "fetchPriceForSymbol",
                requestBytes);

            return result;
        }

        private async Task<object> HandlePriceFeedGetAllPricesAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var baseCurrency = args.ContainsKey("baseCurrency") ? args["baseCurrency"].ToString() : "USD";

            _logger.LogInformation("Getting all prices, BaseCurrency: {BaseCurrency}", baseCurrency);

            var request = new
            {
                BaseCurrency = baseCurrency
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _priceFeedService.HandleRequestAsync(
                "fetchPrices",
                requestBytes);

            return result;
        }

        private async Task<object> HandlePriceFeedSubmitToOracleAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var price = args["price"];

            _logger.LogInformation("Submitting price to oracle: {Price}", JsonConvert.SerializeObject(price));

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(price);
            var result = await _priceFeedService.HandleRequestAsync(
                "submitToOracle",
                requestBytes);

            return result;
        }

        #endregion

        #region Secrets Handlers

        private async Task<object> HandleSecretsGetSecretAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var name = args["name"].ToString();

            _logger.LogInformation("Getting secret by name: {Name}", name);

            var request = new
            {
                Name = name,
                FunctionId = context.FunctionId
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _secretsService.HandleRequestAsync(
                Constants.SecretsOperations.GetSecret,
                requestBytes);

            return result;
        }

        private async Task<object> HandleSecretsGetSecretByIdAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var id = args["id"].ToString();

            _logger.LogInformation("Getting secret by ID: {Id}", id);

            var request = new
            {
                Id = Guid.Parse(id),
                FunctionId = context.FunctionId
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _secretsService.HandleRequestAsync(
                Constants.SecretsOperations.GetSecret,
                requestBytes);

            return result;
        }

        #endregion

        #region Logging Handlers

        private object HandleLogInfoAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var message = args["message"].ToString();
            _logger.LogInformation("[Function Log] {Message}", message);
            return true;
        }

        private object HandleLogWarnAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var message = args["message"].ToString();
            _logger.LogWarning("[Function Log] {Message}", message);
            return true;
        }

        private object HandleLogErrorAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var message = args["message"].ToString();
            _logger.LogError("[Function Log] {Message}", message);
            return true;
        }

        private object HandleLogDebugAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var message = args["message"].ToString();
            _logger.LogDebug("[Function Log] {Message}", message);
            return true;
        }

        #endregion

        #region Storage Handlers

        private async Task<object> HandleStorageGetAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var key = args["key"].ToString();
            _logger.LogInformation("Getting storage value for key: {Key}", key);

            var request = new
            {
                Key = key,
                FunctionId = context.FunctionId
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _functionService.HandleRequestAsync(
                "getStorageValue",
                requestBytes);

            return result;
        }

        private async Task<object> HandleStorageSetAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var key = args["key"].ToString();
            var value = args["value"];
            _logger.LogInformation("Setting storage value for key: {Key}", key);

            var request = new
            {
                Key = key,
                Value = value,
                FunctionId = context.FunctionId
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _functionService.HandleRequestAsync(
                "setStorageValue",
                requestBytes);

            return result;
        }

        private async Task<object> HandleStorageDeleteAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var key = args["key"].ToString();
            _logger.LogInformation("Deleting storage value for key: {Key}", key);

            var request = new
            {
                Key = key,
                FunctionId = context.FunctionId
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _functionService.HandleRequestAsync(
                "deleteStorageValue",
                requestBytes);

            return result;
        }

        #endregion

        #region Events Handlers

        private async Task<object> HandleEventsRegisterBlockchainEventAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var eventType = args["eventType"].ToString();
            var contractHash = args.ContainsKey("contractHash") ? args["contractHash"].ToString() : null;
            var eventName = args.ContainsKey("eventName") ? args["eventName"].ToString() : null;

            _logger.LogInformation("Registering blockchain event: {EventType}, Contract: {ContractHash}, Event: {EventName}",
                eventType, contractHash, eventName);

            var request = new
            {
                EventType = eventType,
                ContractHash = contractHash,
                EventName = eventName,
                FunctionId = context.FunctionId
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _functionService.HandleRequestAsync(
                "registerBlockchainEvent",
                requestBytes);

            return result;
        }

        private async Task<object> HandleEventsRegisterTimeEventAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var schedule = args["schedule"].ToString();
            var description = args.ContainsKey("description") ? args["description"].ToString() : null;

            _logger.LogInformation("Registering time event: {Schedule}, Description: {Description}",
                schedule, description);

            var request = new
            {
                Schedule = schedule,
                Description = description,
                FunctionId = context.FunctionId
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _functionService.HandleRequestAsync(
                "registerTimeEvent",
                requestBytes);

            return result;
        }

        private async Task<object> HandleEventsTriggerCustomEventAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var eventName = args["eventName"].ToString();
            var eventData = args["eventData"];

            _logger.LogInformation("Triggering custom event: {EventName}", eventName);

            var request = new
            {
                EventName = eventName,
                EventData = eventData,
                FunctionId = context.FunctionId
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _functionService.HandleRequestAsync(
                "triggerCustomEvent",
                requestBytes);

            return result;
        }

        #endregion

        #region Blockchain Handlers

        private async Task<object> HandleBlockchainGetBlockHeightAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            _logger.LogInformation("Getting current block height");

            var request = new { };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _walletService.HandleRequestAsync(
                "getBlockHeight",
                requestBytes);

            return result;
        }

        private async Task<object> HandleBlockchainGetBlockAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var height = Convert.ToInt64(args["height"]);

            _logger.LogInformation("Getting block at height: {Height}", height);

            var request = new
            {
                Height = height
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _walletService.HandleRequestAsync(
                "getBlock",
                requestBytes);

            return result;
        }

        private async Task<object> HandleBlockchainGetTransactionAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var txHash = args["txHash"].ToString();

            _logger.LogInformation("Getting transaction by hash: {TxHash}", txHash);

            var request = new
            {
                TxHash = txHash
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _walletService.HandleRequestAsync(
                "getTransaction",
                requestBytes);

            return result;
        }

        private async Task<object> HandleBlockchainGetBalanceAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var address = args["address"].ToString();
            var assetHash = args["assetHash"].ToString();

            _logger.LogInformation("Getting balance for address: {Address}, AssetHash: {AssetHash}", address, assetHash);

            var request = new
            {
                Address = address,
                AssetHash = assetHash
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _walletService.HandleRequestAsync(
                "getBalance",
                requestBytes);

            return result;
        }

        private async Task<object> HandleBlockchainInvokeReadAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var scriptHash = args["scriptHash"].ToString();
            var operation = args["operation"].ToString();
            var contractArgs = args["args"];

            _logger.LogInformation("Invoking read-only contract: {ScriptHash}, Operation: {Operation}", scriptHash, operation);

            var request = new
            {
                ScriptHash = scriptHash,
                Operation = operation,
                Args = contractArgs
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _walletService.HandleRequestAsync(
                "invokeRead",
                requestBytes);

            return result;
        }

        private async Task<object> HandleBlockchainInvokeWriteAsync(Dictionary<string, object> args, FunctionExecutionContext context)
        {
            var scriptHash = args["scriptHash"].ToString();
            var operation = args["operation"].ToString();
            var contractArgs = args["args"];

            _logger.LogInformation("Invoking contract: {ScriptHash}, Operation: {Operation}", scriptHash, operation);

            var request = new
            {
                ScriptHash = scriptHash,
                Operation = operation,
                Args = contractArgs,
                AccountId = context.AccountId
            };

            var requestBytes = JsonUtility.SerializeToUtf8Bytes(request);
            var result = await _walletService.HandleRequestAsync(
                "invokeWrite",
                requestBytes);

            return result;
        }

        #endregion

        /// <summary>
        /// Converts a Jint JavaScript value to a .NET object
        /// </summary>
        /// <param name="value">JavaScript value</param>
        /// <returns>.NET object</returns>
        private object ConvertJsValueToObject(Jint.Native.JsValue value)
        {
            if (value.IsNull())
                return null;
            if (value.IsUndefined())
                return null;
            if (value.IsBoolean())
                return value.AsBoolean();
            if (value.IsNumber())
                return value.AsNumber();
            if (value.IsString())
                return value.AsString();
            if (value.IsDate())
                return value.AsDate().ToDateTime();
            if (value.IsArray())
            {
                var array = value.AsArray();
                var result = new object[array.Length];
                for (var i = 0; i < array.Length; i++)
                {
                    result[i] = ConvertJsValueToObject(array[i]);
                }
                return result;
            }
            if (value.IsObject())
            {
                var obj = value.AsObject();
                var result = new Dictionary<string, object>();
                foreach (var property in obj.GetOwnProperties())
                {
                    if (property.Value != null && property.Value.Value != null)
                    {
                        result[property.Key.ToString()] = ConvertJsValueToObject(property.Value.Value);
                    }
                }
                return result;
            }

            // For any other type, convert to string
            return value.ToString();
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteForEventAsync(object compiledAssembly, string entryPoint, Event eventData, FunctionExecutionContext context)
        {
            _logger.LogInformation("Executing JavaScript function for event, EntryPoint: {EntryPoint}, EventType: {EventType}", entryPoint, eventData.Type);

            try
            {
                var compiledFunction = (CompiledJsFunction)compiledAssembly;
                var sourceCode = compiledFunction.SourceCode;

                // Create a new Jint engine with appropriate constraints
                var engine = new Engine(options => {
                    options.LimitRecursion(100);
                    options.TimeoutInterval(TimeSpan.FromSeconds(context.MaxExecutionTime / 1000));
                    options.MaxStatements(10000);
                    options.DebugMode();
                });

                // Add environment variables to the JavaScript context
                if (context.EnvironmentVariables != null)
                {
                    engine.SetValue("env", context.EnvironmentVariables);
                }

                // Add native function binding
                engine.SetValue("__callNativeFunction", new Func<string, object, Task<object>>(async (functionName, args) => {
                    return await HandleNativeFunctionCallAsync(functionName, args, context);
                }));

                // Load the Neo Service SDK
                engine.Execute(_sdkScript);

                // Add console.log functionality
                var logs = new List<string>();
                engine.SetValue("console", new {
                    log = new Action<object>(message => {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add(logMessage);
                        _logger.LogInformation("[JS Console] {Message}", logMessage);
                    }),
                    error = new Action<object>(message => {
                        var logMessage = message?.ToString() ?? "null";
                        logs.Add("ERROR: " + logMessage);
                        _logger.LogError("[JS Console Error] {Message}", logMessage);
                    })
                });

                // Execute the JavaScript code
                engine.Execute(sourceCode);

                // Convert event data to a JavaScript object
                var eventJson = JsonConvert.SerializeObject(new {
                    id = eventData.Id,
                    type = eventData.Type,
                    name = eventData.Name,
                    source = eventData.Source,
                    data = eventData.Data,
                    timestamp = eventData.Timestamp
                });
                var jsonString = eventJson.Replace("'", "\\'");
                engine.Execute($"var __event = JSON.parse('{jsonString}')");

                // Call the entry point function with event data
                var result = engine.Invoke(entryPoint, engine.GetValue("__event"));

                // Convert the result to a .NET object
                var resultObj = ConvertJsValueToObject(result);

                _logger.LogInformation("JavaScript function executed successfully for event");
                return new {
                    Result = resultObj,
                    Logs = logs,
                    EventType = eventData.Type,
                    ExecutionTime = DateTime.UtcNow,
                    MemoryUsage = GC.GetTotalMemory(false) / (1024 * 1024) // Approximate memory usage in MB
                };
            }
            catch (JavaScriptException jsEx)
            {
                _logger.LogError(jsEx, "JavaScript execution error for event: {Message}", jsEx.Message);
                throw new Exception($"JavaScript execution error for event: {jsEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing JavaScript function for event");
                throw;
            }
        }
    }

    /// <summary>
    /// Represents a compiled JavaScript function
    /// </summary>
    public class CompiledJsFunction
    {
        /// <summary>
        /// Gets or sets the source code
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Gets or sets the entry point
        /// </summary>
        public string EntryPoint { get; set; }
    }
}
