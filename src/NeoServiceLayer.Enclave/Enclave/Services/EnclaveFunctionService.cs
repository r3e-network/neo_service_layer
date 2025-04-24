using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Utilities;
using NeoServiceLayer.Core.Extensions;
using NeoServiceLayer.Enclave.Enclave.Execution;
using NeoServiceLayer.Enclave.Enclave.Models;

namespace NeoServiceLayer.Enclave.Enclave.Services
{
    /// <summary>
    /// Enclave service for function operations
    /// </summary>
    public class EnclaveFunctionService
    {
        private readonly ILogger<EnclaveFunctionService> _logger;
        private readonly FunctionExecutor _functionExecutor;
        private readonly Dictionary<Guid, FunctionMetadata> _functionCache = new Dictionary<Guid, FunctionMetadata>();

        /// <summary>
        /// Initializes a new instance of the <see cref="EnclaveFunctionService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="functionExecutor">Function executor</param>
        public EnclaveFunctionService(ILogger<EnclaveFunctionService> logger, FunctionExecutor functionExecutor)
        {
            _logger = logger;
            _functionExecutor = functionExecutor;
        }

        /// <summary>
        /// Processes a function request
        /// </summary>
        /// <param name="request">Enclave request</param>
        /// <returns>Enclave response</returns>
        public async Task<EnclaveResponse> ProcessRequestAsync(EnclaveRequest request)
        {
            var requestId = request.RequestId ?? Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Operation"] = request.Operation,
                ["PayloadSize"] = request.Payload?.Length ?? 0
            };

            LoggingUtility.LogOperationStart(_logger, "ProcessFunctionRequest", requestId, additionalData);

            try
            {
                var result = await HandleRequestAsync(request.Operation, request.Payload);
                return new EnclaveResponse
                {
                    RequestId = requestId,
                    Success = true,
                    Payload = result
                };
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "ProcessFunctionRequest", requestId, ex, 0, additionalData);

                return new EnclaveResponse
                {
                    RequestId = requestId,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Handles a function request
        /// </summary>
        /// <param name="operation">The operation to perform</param>
        /// <param name="payload">The request payload</param>
        /// <returns>The result of the operation</returns>
        public async Task<byte[]> HandleRequestAsync(string operation, object request)
        {
            byte[] payload = null;
            if (request is byte[] byteArray)
            {
                payload = byteArray;
            }
            else if (request != null)
            {
                payload = JsonUtility.SerializeToUtf8Bytes(request);
            }

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Operation"] = operation,
                ["PayloadSize"] = payload?.Length ?? 0
            };

            LoggingUtility.LogOperationStart(_logger, "HandleFunctionRequest", requestId, additionalData);

            try
            {
                var result = await ExceptionUtility.ExecuteWithExceptionHandlingAsync<EnclaveFunctionService, byte[]>(
                    _logger,
                    async () =>
                    {
                        switch (operation)
                        {
                            case Constants.FunctionOperations.CreateFunction:
                                return await CreateFunctionAsync(payload);
                            case Constants.FunctionOperations.UpdateFunction:
                                return await UpdateFunctionAsync(payload);
                            case Constants.FunctionOperations.UpdateSourceCode:
                                return await UpdateSourceCodeAsync(payload);
                            case Constants.FunctionOperations.ExecuteFunction:
                                return await ExecuteAsync(payload);
                            case "executeFunctionForEvent":
                                return await ExecuteForEventAsync(payload);
                            case "deleteFunction":
                                return await DeleteFunctionAsync(payload);
                            case "getStorageValue":
                                return await GetStorageValueAsync(payload);
                            case "setStorageValue":
                                return await SetStorageValueAsync(payload);
                            case "deleteStorageValue":
                                return await DeleteStorageValueAsync(payload);
                            case "registerBlockchainEvent":
                                return await RegisterBlockchainEventAsync(payload);
                            case "registerTimeEvent":
                                return await RegisterTimeEventAsync(payload);
                            case "triggerCustomEvent":
                                return await TriggerCustomEventAsync(payload);
                            default:
                                throw new InvalidOperationException($"Unknown operation: {operation}");
                        }
                    },
                    operation,
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new InvalidOperationException($"Failed to process function request: {operation}");
                }

                return result.result;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "HandleFunctionRequest", requestId, ex, 0, additionalData);
                throw;
            }
        }

        private async Task<byte[]> ExecuteAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<ExecuteRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.FunctionId, "Function ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = request.FunctionId,
                ["ParameterCount"] = request.Parameters?.Count ?? 0
            };

            LoggingUtility.LogOperationStart(_logger, "ExecuteFunction", requestId, additionalData);

            try
            {
                // Get function metadata from cache or create it
                if (!_functionCache.TryGetValue(request.FunctionId, out var functionMetadata))
                {
                    throw new Exception($"Function with ID {request.FunctionId} not found in cache");
                }

                // Execute the function
                var result = await _functionExecutor.ExecuteAsync(functionMetadata, request.Parameters);

                // Create response
                var response = new
                {
                    FunctionId = request.FunctionId,
                    Result = result,
                    Timestamp = DateTime.UtcNow
                };

                // Log function execution
                LoggingUtility.LogOperationSuccess(_logger, "ExecuteFunction", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "ExecuteFunction", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <summary>
        /// Request model for executing a function
        /// </summary>
        private class ExecuteRequest
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
            /// Gets or sets the function parameters
            /// </summary>
            public Dictionary<string, object> Parameters { get; set; }
        }

        private async Task<byte[]> CreateFunctionAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<CreateFunctionRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.Id, "Function ID");
            ValidationUtility.ValidateNotNullOrEmpty(request.Name, "Function name");
            ValidationUtility.ValidateNotNullOrEmpty(request.SourceCode, "Source code");
            ValidationUtility.ValidateNotNullOrEmpty(request.EntryPoint, "Entry point");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = request.Id,
                ["Name"] = request.Name,
                ["Runtime"] = request.Runtime
            };

            LoggingUtility.LogOperationStart(_logger, "CreateFunction", requestId, additionalData);

            try
            {
                // Create function metadata
                var metadata = new FunctionMetadata
                {
                    Id = request.Id,
                    Name = request.Name,
                    Description = request.Description,
                    Runtime = request.Runtime,
                    SourceCode = request.SourceCode,
                    EntryPoint = request.EntryPoint,
                    AccountId = request.AccountId,
                    MaxExecutionTime = request.MaxExecutionTime,
                    MaxMemory = request.MaxMemory,
                    SecretIds = request.SecretIds ?? new List<Guid>(),
                    EnvironmentVariables = request.EnvironmentVariables ?? new Dictionary<string, string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Validate and compile the function
                await _functionExecutor.ValidateAndCompileAsync(metadata);

                // Add to cache
                _functionCache[metadata.Id] = metadata;

                // Create response
                var response = new
                {
                    Id = metadata.Id,
                    Name = metadata.Name,
                    Status = "Active",
                    Timestamp = DateTime.UtcNow
                };

                LoggingUtility.LogOperationSuccess(_logger, "CreateFunction", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "CreateFunction", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <summary>
        /// Request model for creating a function
        /// </summary>
        private class CreateFunctionRequest
        {
            /// <summary>
            /// Gets or sets the function ID
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the function name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the function description
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the runtime
            /// </summary>
            public string Runtime { get; set; }

            /// <summary>
            /// Gets or sets the source code
            /// </summary>
            public string SourceCode { get; set; }

            /// <summary>
            /// Gets or sets the entry point
            /// </summary>
            public string EntryPoint { get; set; }

            /// <summary>
            /// Gets or sets the account ID
            /// </summary>
            public Guid AccountId { get; set; }

            /// <summary>
            /// Gets or sets the maximum execution time
            /// </summary>
            public int MaxExecutionTime { get; set; }

            /// <summary>
            /// Gets or sets the maximum memory
            /// </summary>
            public int MaxMemory { get; set; }

            /// <summary>
            /// Gets or sets the secret IDs
            /// </summary>
            public List<Guid> SecretIds { get; set; }

            /// <summary>
            /// Gets or sets the environment variables
            /// </summary>
            public Dictionary<string, string> EnvironmentVariables { get; set; }
        }

        private async Task<byte[]> UpdateFunctionAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<UpdateFunctionRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.Id, "Function ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = request.Id
            };

            LoggingUtility.LogOperationStart(_logger, "UpdateFunction", requestId, additionalData);

            try
            {
                // Get function metadata from cache
                if (!_functionCache.TryGetValue(request.Id, out var metadata))
                {
                    throw new Exception($"Function with ID {request.Id} not found in cache");
                }

                additionalData["Name"] = metadata.Name;
                additionalData["Runtime"] = metadata.Runtime;

                // Update metadata
                if (!string.IsNullOrEmpty(request.Name))
                    metadata.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Description))
                    metadata.Description = request.Description;

                if (!string.IsNullOrEmpty(request.EntryPoint))
                    metadata.EntryPoint = request.EntryPoint;

                if (request.MaxExecutionTime > 0)
                    metadata.MaxExecutionTime = request.MaxExecutionTime;

                if (request.MaxMemory > 0)
                    metadata.MaxMemory = request.MaxMemory;

                if (request.EnvironmentVariables != null)
                    metadata.EnvironmentVariables = request.EnvironmentVariables;

                metadata.UpdatedAt = DateTime.UtcNow;

                // Update cache
                _functionCache[metadata.Id] = metadata;

                // Create response
                var response = new
                {
                    Id = metadata.Id,
                    Name = metadata.Name,
                    Status = "Active",
                    Timestamp = DateTime.UtcNow
                };

                LoggingUtility.LogOperationSuccess(_logger, "UpdateFunction", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "UpdateFunction", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <summary>
        /// Request model for updating a function
        /// </summary>
        private class UpdateFunctionRequest
        {
            /// <summary>
            /// Gets or sets the function ID
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the function name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the function description
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the entry point
            /// </summary>
            public string EntryPoint { get; set; }

            /// <summary>
            /// Gets or sets the maximum execution time
            /// </summary>
            public int MaxExecutionTime { get; set; }

            /// <summary>
            /// Gets or sets the maximum memory
            /// </summary>
            public int MaxMemory { get; set; }

            /// <summary>
            /// Gets or sets the environment variables
            /// </summary>
            public Dictionary<string, string> EnvironmentVariables { get; set; }
        }

        private async Task<byte[]> UpdateSourceCodeAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<UpdateSourceCodeRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.Id, "Function ID");
            ValidationUtility.ValidateNotNullOrEmpty(request.SourceCode, "Source code");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = request.Id,
                ["SourceCodeLength"] = request.SourceCode?.Length ?? 0
            };

            LoggingUtility.LogOperationStart(_logger, "UpdateSourceCode", requestId, additionalData);

            try
            {
                // Get function metadata from cache
                if (!_functionCache.TryGetValue(request.Id, out var metadata))
                {
                    throw new Exception($"Function with ID {request.Id} not found in cache");
                }

                additionalData["Name"] = metadata.Name;
                additionalData["Runtime"] = metadata.Runtime;

                // Update source code
                metadata.SourceCode = request.SourceCode;
                metadata.UpdatedAt = DateTime.UtcNow;

                // Recompile the function
                await _functionExecutor.ValidateAndCompileAsync(metadata);

                // Update cache
                _functionCache[metadata.Id] = metadata;

                // Create response
                var response = new
                {
                    Id = metadata.Id,
                    Name = metadata.Name,
                    Status = "Active",
                    Timestamp = DateTime.UtcNow
                };

                LoggingUtility.LogOperationSuccess(_logger, "UpdateSourceCode", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "UpdateSourceCode", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <summary>
        /// Request model for updating source code
        /// </summary>
        private class UpdateSourceCodeRequest
        {
            /// <summary>
            /// Gets or sets the function ID
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the source code
            /// </summary>
            public string SourceCode { get; set; }
        }

        private async Task<byte[]> GetStorageValueAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<StorageRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateNotNullOrEmpty(request.Key, "Key");
            ValidationUtility.ValidateGuid(request.FunctionId, "Function ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = request.FunctionId,
                ["Key"] = request.Key
            };

            LoggingUtility.LogOperationStart(_logger, "GetStorageValue", requestId, additionalData);

            try
            {
                // In a real implementation, this would retrieve the value from a secure storage mechanism
                // For now, we'll return a placeholder value
                var value = $"value-for-{request.Key}";

                // Create response
                var response = new
                {
                    Key = request.Key,
                    Value = value,
                    FunctionId = request.FunctionId
                };

                LoggingUtility.LogOperationSuccess(_logger, "GetStorageValue", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "GetStorageValue", requestId, ex, 0, additionalData);
                throw;
            }
        }

        private async Task<byte[]> SetStorageValueAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<StorageValueRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateNotNullOrEmpty(request.Key, "Key");
            ValidationUtility.ValidateGuid(request.FunctionId, "Function ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = request.FunctionId,
                ["Key"] = request.Key
            };

            LoggingUtility.LogOperationStart(_logger, "SetStorageValue", requestId, additionalData);

            try
            {
                // In a real implementation, this would store the value in a secure storage mechanism
                // For now, we'll just acknowledge the request

                // Create response
                var response = new
                {
                    Key = request.Key,
                    Success = true,
                    FunctionId = request.FunctionId
                };

                LoggingUtility.LogOperationSuccess(_logger, "SetStorageValue", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "SetStorageValue", requestId, ex, 0, additionalData);
                throw;
            }
        }

        private async Task<byte[]> DeleteStorageValueAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<StorageRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateNotNullOrEmpty(request.Key, "Key");
            ValidationUtility.ValidateGuid(request.FunctionId, "Function ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = request.FunctionId,
                ["Key"] = request.Key
            };

            LoggingUtility.LogOperationStart(_logger, "DeleteStorageValue", requestId, additionalData);

            try
            {
                // In a real implementation, this would delete the value from a secure storage mechanism
                // For now, we'll just acknowledge the request

                // Create response
                var response = new
                {
                    Key = request.Key,
                    Success = true,
                    FunctionId = request.FunctionId
                };

                LoggingUtility.LogOperationSuccess(_logger, "DeleteStorageValue", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "DeleteStorageValue", requestId, ex, 0, additionalData);
                throw;
            }
        }

        private async Task<byte[]> RegisterBlockchainEventAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<RegisterBlockchainEventRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateNotNullOrEmpty(request.EventType, "Event type");
            ValidationUtility.ValidateGuid(request.FunctionId, "Function ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = request.FunctionId,
                ["EventType"] = request.EventType,
                ["ContractHash"] = request.ContractHash,
                ["EventName"] = request.EventName
            };

            LoggingUtility.LogOperationStart(_logger, "RegisterBlockchainEvent", requestId, additionalData);

            try
            {
                // In a real implementation, this would register the event with a blockchain event monitoring service
                // For now, we'll just acknowledge the request

                // Create response
                var response = new
                {
                    EventId = Guid.NewGuid(),
                    FunctionId = request.FunctionId,
                    EventType = request.EventType,
                    Success = true
                };

                LoggingUtility.LogOperationSuccess(_logger, "RegisterBlockchainEvent", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "RegisterBlockchainEvent", requestId, ex, 0, additionalData);
                throw;
            }
        }

        private async Task<byte[]> RegisterTimeEventAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<RegisterTimeEventRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateNotNullOrEmpty(request.Schedule, "Schedule");
            ValidationUtility.ValidateGuid(request.FunctionId, "Function ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = request.FunctionId,
                ["Schedule"] = request.Schedule,
                ["Description"] = request.Description
            };

            LoggingUtility.LogOperationStart(_logger, "RegisterTimeEvent", requestId, additionalData);

            try
            {
                // In a real implementation, this would register the event with a time-based scheduler
                // For now, we'll just acknowledge the request

                // Create response
                var response = new
                {
                    EventId = Guid.NewGuid(),
                    FunctionId = request.FunctionId,
                    Schedule = request.Schedule,
                    Success = true
                };

                LoggingUtility.LogOperationSuccess(_logger, "RegisterTimeEvent", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "RegisterTimeEvent", requestId, ex, 0, additionalData);
                throw;
            }
        }

        private async Task<byte[]> TriggerCustomEventAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<TriggerCustomEventRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateNotNullOrEmpty(request.EventName, "Event name");
            ValidationUtility.ValidateGuid(request.FunctionId, "Function ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = request.FunctionId,
                ["EventName"] = request.EventName
            };

            LoggingUtility.LogOperationStart(_logger, "TriggerCustomEvent", requestId, additionalData);

            try
            {
                // In a real implementation, this would trigger a custom event
                // For now, we'll just acknowledge the request

                // Create response
                var response = new
                {
                    EventId = Guid.NewGuid(),
                    FunctionId = request.FunctionId,
                    EventName = request.EventName,
                    Success = true
                };

                LoggingUtility.LogOperationSuccess(_logger, "TriggerCustomEvent", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "TriggerCustomEvent", requestId, ex, 0, additionalData);
                throw;
            }
        }

        private async Task<byte[]> DeleteFunctionAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<DeleteFunctionRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.Id, "Function ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = request.Id
            };

            LoggingUtility.LogOperationStart(_logger, "DeleteFunction", requestId, additionalData);

            try
            {
                // Get function metadata from cache
                if (!_functionCache.TryGetValue(request.Id, out var metadata))
                {
                    throw new Exception($"Function with ID {request.Id} not found in cache");
                }

                additionalData["Name"] = metadata.Name;
                additionalData["Runtime"] = metadata.Runtime;

                // Remove from cache
                _functionCache.Remove(request.Id);

                // Create response
                var response = new
                {
                    Id = request.Id,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                };

                LoggingUtility.LogOperationSuccess(_logger, "DeleteFunction", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "DeleteFunction", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <summary>
        /// Request model for deleting a function
        /// </summary>
        private class DeleteFunctionRequest
        {
            /// <summary>
            /// Gets or sets the function ID
            /// </summary>
            public Guid Id { get; set; }
        }

        private async Task<byte[]> ExecuteForEventAsync(byte[] payload)
        {
            // Parse the request payload
            var request = JsonUtility.Deserialize<ExecuteForEventRequest>(payload);

            // Validate request
            ValidationUtility.ValidateNotNull(request, nameof(request));
            ValidationUtility.ValidateGuid(request.FunctionId, "Function ID");
            ValidationUtility.ValidateGuid(request.EventId, "Event ID");

            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["FunctionId"] = request.FunctionId,
                ["EventId"] = request.EventId,
                ["EventType"] = request.Event?.Type
            };

            LoggingUtility.LogOperationStart(_logger, "ExecuteFunctionForEvent", requestId, additionalData);

            try
            {
                // Get function metadata from cache or create it
                if (!_functionCache.TryGetValue(request.FunctionId, out var functionMetadata))
                {
                    throw new Exception($"Function with ID {request.FunctionId} not found in cache");
                }

                // Create event object
                var eventObj = new Event
                {
                    Id = request.EventId,
                    Type = request.Event.Type,
                    Name = request.Event.Name,
                    Source = request.Event.Source,
                    Data = request.Event.Data,
                    Timestamp = request.Event.Timestamp
                };

                // Execute the function
                var result = await _functionExecutor.ExecuteForEventAsync(functionMetadata, eventObj);

                // Create response
                var response = new
                {
                    FunctionId = request.FunctionId,
                    EventId = request.EventId,
                    Result = result,
                    Timestamp = DateTime.UtcNow
                };

                // Log function execution
                LoggingUtility.LogOperationSuccess(_logger, "ExecuteFunctionForEvent", requestId, 0, additionalData);

                return JsonUtility.SerializeToUtf8Bytes(response);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(_logger, "ExecuteFunctionForEvent", requestId, ex, 0, additionalData);
                throw;
            }
        }

        /// <summary>
        /// Request model for executing a function for an event
        /// </summary>
        private class ExecuteForEventRequest
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
            /// Gets or sets the event ID
            /// </summary>
            public Guid EventId { get; set; }

            /// <summary>
            /// Gets or sets the event
            /// </summary>
            public EventModel Event { get; set; }
        }

        /// <summary>
        /// Event model for function execution
        /// </summary>
        private class EventModel
        {
            /// <summary>
            /// Gets or sets the event ID
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the event type
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the event name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the event source
            /// </summary>
            public string Source { get; set; }

            /// <summary>
            /// Gets or sets the event data
            /// </summary>
            public Dictionary<string, object> Data { get; set; }

            /// <summary>
            /// Gets or sets the event timestamp
            /// </summary>
            public DateTime Timestamp { get; set; }
        }
    }
}
