using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Utilities;
using NeoServiceLayer.Core.Extensions;
// using NeoServiceLayer.Services.Function.Repositories;

namespace NeoServiceLayer.Services.Function
{
    /// <summary>
    /// Implementation of the function service
    /// </summary>
    public class FunctionService : IFunctionService
    {
        private readonly ILogger<FunctionService> _logger;
        private readonly IFunctionRepository _functionRepository;
        private readonly IFunctionExecutionRepository _executionRepository;
        private readonly IEnclaveService _enclaveService;
        private readonly ISecretsService _secretsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="functionRepository">Function repository</param>
        /// <param name="executionRepository">Function execution repository</param>
        /// <param name="enclaveService">Enclave service</param>
        /// <param name="secretsService">Secrets service</param>
        public FunctionService(
            ILogger<FunctionService> logger,
            IFunctionRepository functionRepository,
            IFunctionExecutionRepository executionRepository,
            IEnclaveService enclaveService,
            ISecretsService secretsService)
        {
            _logger = logger;
            _functionRepository = functionRepository;
            _executionRepository = executionRepository;
            _enclaveService = enclaveService;
            _secretsService = secretsService;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> CreateFunctionAsync(
            string name,
            string description,
            FunctionRuntime runtime,
            string sourceCode,
            string entryPoint,
            Guid accountId,
            int maxExecutionTime = 30000,
            int maxMemory = 128,
            List<Guid> secretIds = null,
            Dictionary<string, string> environmentVariables = null)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Name"] = name,
                ["Runtime"] = runtime.ToString(),
                ["AccountId"] = accountId,
                ["EntryPoint"] = entryPoint,
                ["MaxExecutionTime"] = maxExecutionTime,
                ["MaxMemory"] = maxMemory
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "CreateFunction", requestId, additionalData);

            try
            {
                // Validate input parameters
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(name, "Function name");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(sourceCode, "Source code");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(entryPoint, "Entry point");
                Common.Utilities.ValidationUtility.ValidateGuid(accountId, "Account ID");
                Common.Utilities.ValidationUtility.ValidateGreaterThanZero(maxExecutionTime, "Max execution time");
                Common.Utilities.ValidationUtility.ValidateGreaterThanZero(maxMemory, "Max memory");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, Core.Models.Function>(
                    _logger,
                    async () =>
                    {
                        // Check if function with same name already exists for this account
                        var functions = await _functionRepository.GetByNameAsync(name);
                        var existingFunction = functions.FirstOrDefault(f => f.AccountId == accountId && f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (existingFunction != null)
                        {
                            throw new FunctionException("Function with this name already exists");
                        }

                        // Validate secrets access
                        if (secretIds != null && secretIds.Count > 0)
                        {
                            foreach (var secretId in secretIds)
                            {
                                Common.Utilities.ValidationUtility.ValidateGuid(secretId, "Secret ID");

                                var secret = await _secretsService.GetByIdAsync(secretId);
                                if (secret == null)
                                {
                                    throw new FunctionException($"Secret with ID {secretId} not found");
                                }

                                if (secret.AccountId != accountId)
                                {
                                    throw new FunctionException($"Secret with ID {secretId} does not belong to this account");
                                }
                            }
                        }

                        // Calculate source code hash
                        var sourceHash = ComputeHash(sourceCode);
                        additionalData["SourceHash"] = sourceHash;

                        // Create function object
                        var function = new Core.Models.Function
                        {
                            Id = Guid.NewGuid(),
                            Name = name,
                            Description = description,
                            Runtime = runtime.ToString(),
                            SourceCode = sourceCode,
                            EntryPoint = entryPoint,
                            AccountId = accountId,
                            MaxExecutionTime = maxExecutionTime,
                            MaxMemory = maxMemory,
                            SecretIds = secretIds ?? new List<Guid>(),
                            EnvironmentVariables = environmentVariables ?? new Dictionary<string, string>(),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Status = "Active"
                        };

                        additionalData["FunctionId"] = function.Id;

                        // Send function creation request to enclave
                        var functionRequest = new
                        {
                            Id = function.Id,
                            Name = function.Name,
                            Description = function.Description,
                            Runtime = function.Runtime,
                            SourceCode = function.SourceCode,
                            EntryPoint = function.EntryPoint,
                            AccountId = function.AccountId,
                            MaxExecutionTime = function.MaxExecutionTime,
                            MaxMemory = function.MaxMemory,
                            SecretIds = function.SecretIds,
                            EnvironmentVariables = function.EnvironmentVariables
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.CreateFunction,
                            functionRequest);

                        // Save function to repository
                        await _functionRepository.CreateAsync(function);

                        return function;
                    },
                    "CreateFunction",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to create function");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "CreateFunction", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "CreateFunction", requestId, ex, 0, additionalData);
                throw new FunctionException("Error creating function", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> GetByIdAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetFunctionById", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, Core.Models.Function>(
                    _logger,
                    async () => await _functionRepository.GetByIdAsync(id),
                    "GetFunctionById",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["Name"] = result.result.Name;
                    additionalData["Runtime"] = result.result.Runtime;
                    additionalData["AccountId"] = result.result.AccountId;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetFunctionById", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetFunctionById", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error getting function by ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> GetFunctionAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }

        /// <summary>
        /// Gets a function by ID string
        /// </summary>
        /// <param name="id">Function ID string</param>
        /// <returns>The function if found, null otherwise</returns>
        public async Task<Core.Models.Function> GetFunctionAsync(string id)
        {
            if (Guid.TryParse(id, out Guid guidId))
            {
                return await GetByIdAsync(guidId);
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> GetByNameAsync(string name, Guid accountId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Name"] = name,
                ["AccountId"] = accountId
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetFunctionByName", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(name, "Function name");
                Common.Utilities.ValidationUtility.ValidateGuid(accountId, "Account ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, Core.Models.Function>(
                    _logger,
                    async () => {
                        var functions = await _functionRepository.GetByNameAsync(name);
                        return functions.FirstOrDefault(f => f.AccountId == accountId && f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    },
                    "GetFunctionByName",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["Id"] = result.result.Id;
                    additionalData["Runtime"] = result.result.Runtime;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetFunctionByName", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetFunctionByName", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error getting function by name {name}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Function>> GetByAccountIdAsync(Guid accountId)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["AccountId"] = accountId
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetFunctionsByAccountId", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(accountId, "Account ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, IEnumerable<Core.Models.Function>>(
                    _logger,
                    async () => await _functionRepository.GetByAccountIdAsync(accountId),
                    "GetFunctionsByAccountId",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["FunctionCount"] = result.result.Count();
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetFunctionsByAccountId", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetFunctionsByAccountId", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error getting functions by account ID {accountId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Core.Models.Function>> GetByRuntimeAsync(FunctionRuntime runtime)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Runtime"] = runtime.ToString()
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetFunctionsByRuntime", requestId, additionalData);

            try
            {
                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, IEnumerable<Core.Models.Function>>(
                    _logger,
                    async () =>
                    {
                        var allFunctions = await _functionRepository.GetAllAsync();
                        var filteredFunctions = allFunctions.Where(f => f.Runtime.Equals(runtime.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
                        return filteredFunctions;
                    },
                    "GetFunctionsByRuntime",
                    requestId,
                    additionalData);

                if (result.result != null)
                {
                    additionalData["FunctionCount"] = result.result.Count();
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetFunctionsByRuntime", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetFunctionsByRuntime", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error getting functions by runtime {runtime}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> UpdateAsync(Core.Models.Function function)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = function.Id,
                ["Name"] = function.Name
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "UpdateFunction", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNull(function, nameof(function));
                Common.Utilities.ValidationUtility.ValidateGuid(function.Id, "Function ID");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(function.Name, "Function name");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(function.EntryPoint, "Entry point");
                Common.Utilities.ValidationUtility.ValidateGreaterThanZero(function.MaxExecutionTime, "Max execution time");
                Common.Utilities.ValidationUtility.ValidateGreaterThanZero(function.MaxMemory, "Max memory");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, Core.Models.Function>(
                    _logger,
                    async () =>
                    {
                        var existingFunction = await _functionRepository.GetByIdAsync(function.Id);
                        if (existingFunction == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        // Update function in enclave
                        var updateRequest = new
                        {
                            Id = function.Id,
                            Name = function.Name,
                            Description = function.Description,
                            EntryPoint = function.EntryPoint,
                            MaxExecutionTime = function.MaxExecutionTime,
                            MaxMemory = function.MaxMemory,
                            EnvironmentVariables = function.EnvironmentVariables,
                            Status = function.Status
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.UpdateFunction,
                            updateRequest);

                        function.UpdatedAt = DateTime.UtcNow;
                        return await _functionRepository.UpdateAsync(function.Id, function);
                    },
                    "UpdateFunction",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to update function");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "UpdateFunction", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "UpdateFunction", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error updating function {function.Id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> UpdateSourceCodeAsync(Guid id, string sourceCode)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["SourceCodeLength"] = sourceCode?.Length ?? 0
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "UpdateFunctionSourceCode", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(sourceCode, "Source code");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, Core.Models.Function>(
                    _logger,
                    async () =>
                    {
                        var function = await _functionRepository.GetByIdAsync(id);
                        if (function == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        additionalData["Name"] = function.Name;
                        additionalData["Runtime"] = function.Runtime;

                        // Calculate new source code hash
                        var sourceHash = ComputeHash(sourceCode);
                        additionalData["SourceHash"] = sourceHash;

                        // Update source code in enclave
                        var updateRequest = new
                        {
                            Id = id,
                            SourceCode = sourceCode
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.UpdateSourceCode,
                            updateRequest);

                        function.SourceCode = sourceCode;
                        function.UpdatedAt = DateTime.UtcNow;
                        return await _functionRepository.UpdateAsync(function.Id, function);
                    },
                    "UpdateFunctionSourceCode",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to update function source code");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "UpdateFunctionSourceCode", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "UpdateFunctionSourceCode", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error updating source code for function {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> UpdateEnvironmentVariablesAsync(Guid id, Dictionary<string, string> environmentVariables)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["EnvironmentVariableCount"] = environmentVariables?.Count ?? 0
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "UpdateFunctionEnvironmentVariables", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");
                Common.Utilities.ValidationUtility.ValidateNotNull(environmentVariables, "Environment variables");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, Core.Models.Function>(
                    _logger,
                    async () =>
                    {
                        var function = await _functionRepository.GetByIdAsync(id);
                        if (function == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        additionalData["Name"] = function.Name;
                        additionalData["Runtime"] = function.Runtime;

                        // Update environment variables in enclave
                        var updateRequest = new
                        {
                            Id = id,
                            EnvironmentVariables = environmentVariables
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.UpdateEnvironmentVariables,
                            updateRequest);

                        function.EnvironmentVariables = environmentVariables;
                        function.UpdatedAt = DateTime.UtcNow;
                        return await _functionRepository.UpdateAsync(function.Id, function);
                    },
                    "UpdateFunctionEnvironmentVariables",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to update function environment variables");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "UpdateFunctionEnvironmentVariables", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "UpdateFunctionEnvironmentVariables", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error updating environment variables for function {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> UpdateSecretAccessAsync(Guid id, List<Guid> secretIds)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["SecretCount"] = secretIds?.Count ?? 0
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "UpdateFunctionSecretAccess", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");
                Common.Utilities.ValidationUtility.ValidateNotNull(secretIds, "Secret IDs");

                // Validate each secret ID
                if (secretIds.Count > 0)
                {
                    foreach (var secretId in secretIds)
                    {
                        Common.Utilities.ValidationUtility.ValidateGuid(secretId, "Secret ID");
                    }
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, Core.Models.Function>(
                    _logger,
                    async () =>
                    {
                        var function = await _functionRepository.GetByIdAsync(id);
                        if (function == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        additionalData["Name"] = function.Name;
                        additionalData["Runtime"] = function.Runtime;
                        additionalData["AccountId"] = function.AccountId;

                        // Validate secrets access
                        foreach (var secretId in secretIds)
                        {
                            var secret = await _secretsService.GetByIdAsync(secretId);
                            if (secret == null)
                            {
                                throw new FunctionException($"Secret with ID {secretId} not found");
                            }

                            if (secret.AccountId != function.AccountId)
                            {
                                throw new FunctionException($"Secret with ID {secretId} does not belong to this account");
                            }
                        }

                        // Update secret access in enclave
                        var updateRequest = new
                        {
                            Id = id,
                            SecretIds = secretIds
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.UpdateSecretAccess,
                            updateRequest);

                        function.SecretIds = secretIds;
                        function.UpdatedAt = DateTime.UtcNow;
                        return await _functionRepository.UpdateAsync(function.Id, function);
                    },
                    "UpdateFunctionSecretAccess",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to update function secret access");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "UpdateFunctionSecretAccess", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "UpdateFunctionSecretAccess", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error updating secret access for function {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(Guid id, Dictionary<string, object> parameters = null)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["ParameterCount"] = parameters?.Count ?? 0
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "ExecuteFunction", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, object>(
                    _logger,
                    async () =>
                    {
                        var function = await _functionRepository.GetByIdAsync(id);
                        if (function == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        additionalData["Name"] = function.Name;
                        additionalData["Runtime"] = function.Runtime;
                        additionalData["AccountId"] = function.AccountId;

                        if (function.Status != "Active")
                        {
                            throw new FunctionException("Function is not active");
                        }

                        // Create execution record
                        var execution = new FunctionExecutionResult
                        {
                            Id = Guid.NewGuid(),
                            ExecutionId = Guid.NewGuid(),
                            FunctionId = function.Id,
                            Input = parameters,
                            Status = "Running",
                            StartTime = DateTime.UtcNow
                        };

                        additionalData["ExecutionId"] = execution.Id;

                        await _executionRepository.CreateAsync(execution);

                        // Execute function in enclave
                        var executeRequest = new
                        {
                            ExecutionId = execution.Id,
                            FunctionId = function.Id,
                            Parameters = parameters
                        };

                        var functionResult = await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.ExecuteFunction,
                            executeRequest);

                        // Update execution record
                        execution.Status = "Completed";
                        execution.Output = functionResult;

                        additionalData["DurationMs"] = (long)(DateTime.UtcNow - execution.StartTime).TotalMilliseconds;

                        await _executionRepository.UpdateAsync(execution.Id, execution);

                        // Update function's last executed timestamp
                        function.LastExecutedAt = DateTime.UtcNow;
                        await _functionRepository.UpdateAsync(function.Id, function);

                        return functionResult;
                    },
                    "ExecuteFunction",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to execute function");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "ExecuteFunction", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "ExecuteFunction", requestId, ex, 0, additionalData);
                throw new FunctionException("Error executing function", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteForEventAsync(Guid id, Event eventData)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["EventType"] = eventData?.Type
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "ExecuteFunctionForEvent", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");
                Common.Utilities.ValidationUtility.ValidateNotNull(eventData, "Event data");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, object>(
                    _logger,
                    async () =>
                    {
                        var function = await _functionRepository.GetByIdAsync(id);
                        if (function == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        additionalData["Name"] = function.Name;
                        additionalData["Runtime"] = function.Runtime;
                        additionalData["AccountId"] = function.AccountId;

                        if (function.Status != "Active")
                        {
                            throw new FunctionException("Function is not active");
                        }

                        // Create execution record
                        var execution = new FunctionExecutionResult
                        {
                            Id = Guid.NewGuid(),
                            ExecutionId = Guid.NewGuid(),
                            FunctionId = function.Id,
                            Status = "Running",
                            StartTime = DateTime.UtcNow
                        };

                        additionalData["ExecutionId"] = execution.Id;

                        await _executionRepository.CreateAsync(execution);

                        // Execute function in enclave
                        var executeRequest = new
                        {
                            ExecutionId = execution.Id,
                            FunctionId = function.Id,
                            Event = eventData
                        };

                        var functionResult = await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.ExecuteFunctionForEvent,
                            executeRequest);

                        // Update execution record
                        execution.Status = "Completed";
                        execution.Output = functionResult;

                        additionalData["DurationMs"] = (long)(DateTime.UtcNow - execution.StartTime).TotalMilliseconds;

                        await _executionRepository.UpdateAsync(execution.Id, execution);

                        // Update function's last executed timestamp
                        function.LastExecutedAt = DateTime.UtcNow;
                        await _functionRepository.UpdateAsync(function.Id, function);

                        return functionResult;
                    },
                    "ExecuteFunctionForEvent",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to execute function for event");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "ExecuteFunctionForEvent", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "ExecuteFunctionForEvent", requestId, ex, 0, additionalData);
                throw new FunctionException("Error executing function for event", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<object>> GetExecutionHistoryAsync(Guid id, DateTime startTime, DateTime endTime)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id,
                ["StartTime"] = startTime,
                ["EndTime"] = endTime
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetFunctionExecutionHistory", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");

                if (endTime < startTime)
                {
                    throw new ArgumentException("End time must be greater than or equal to start time");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, IEnumerable<object>>(
                    _logger,
                    async () =>
                    {
                        var function = await _functionRepository.GetByIdAsync(id);
                        if (function == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        additionalData["Name"] = function.Name;
                        additionalData["Runtime"] = function.Runtime;

                        var executions = await _executionRepository.GetByFunctionIdAsync(id);
                        var filteredExecutions = executions
                            .Where(e => e.StartTime >= startTime && (e.EndTime == null || e.EndTime <= endTime))
                            .OrderByDescending(e => e.StartTime)
                            .Select(e => new
                            {
                                Id = e.Id,
                                FunctionId = e.FunctionId,
                                Status = e.Status,
                                StartTime = e.StartTime,
                                EndTime = e.EndTime,
                                DurationMs = e.DurationMs,
                                MemoryUsageMb = e.MemoryUsageMb,
                                CpuUsagePercent = e.CpuUsagePercent,
                                BillingAmount = e.BillingAmount
                            })
                            .ToList();

                        additionalData["ExecutionCount"] = filteredExecutions.Count;

                        return filteredExecutions;
                    },
                    "GetFunctionExecutionHistory",
                    requestId,
                    additionalData);

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetFunctionExecutionHistory", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetFunctionExecutionHistory", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error getting execution history for function {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> ActivateAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "ActivateFunction", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, Core.Models.Function>(
                    _logger,
                    async () =>
                    {
                        var function = await _functionRepository.GetByIdAsync(id);
                        if (function == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        additionalData["Name"] = function.Name;
                        additionalData["Runtime"] = function.Runtime;

                        if (function.Status == "Active")
                        {
                            Common.Utilities.LoggingUtility.LogWarning(_logger, "Function is already active", requestId, additionalData);
                            return function;
                        }

                        // Activate function in enclave
                        var activateRequest = new
                        {
                            Id = id
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.ActivateFunction,
                            activateRequest);

                        function.Status = "Active";
                        function.UpdatedAt = DateTime.UtcNow;
                        return await _functionRepository.UpdateAsync(function.Id, function);
                    },
                    "ActivateFunction",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to activate function");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "ActivateFunction", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "ActivateFunction", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error activating function {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Core.Models.Function> DeactivateAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "DeactivateFunction", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, Core.Models.Function>(
                    _logger,
                    async () =>
                    {
                        var function = await _functionRepository.GetByIdAsync(id);
                        if (function == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        additionalData["Name"] = function.Name;
                        additionalData["Runtime"] = function.Runtime;

                        if (function.Status == "Inactive")
                        {
                            Common.Utilities.LoggingUtility.LogWarning(_logger, "Function is already inactive", requestId, additionalData);
                            return function;
                        }

                        // Deactivate function in enclave
                        var deactivateRequest = new
                        {
                            Id = id
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.DeactivateFunction,
                            deactivateRequest);

                        function.Status = "Inactive";
                        function.UpdatedAt = DateTime.UtcNow;
                        return await _functionRepository.UpdateAsync(function.Id, function);
                    },
                    "DeactivateFunction",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to deactivate function");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "DeactivateFunction", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "DeactivateFunction", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error deactivating function {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "DeleteFunction", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Function ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<FunctionService, bool>(
                    _logger,
                    async () =>
                    {
                        var function = await _functionRepository.GetByIdAsync(id);
                        if (function == null)
                        {
                            throw new FunctionException("Function not found");
                        }

                        additionalData["Name"] = function.Name;
                        additionalData["Runtime"] = function.Runtime;

                        // Delete function in enclave
                        var deleteRequest = new
                        {
                            Id = id
                        };

                        await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.Function,
                            Constants.FunctionOperations.DeleteFunction,
                            deleteRequest);

                        return await _functionRepository.DeleteAsync(id);
                    },
                    "DeleteFunction",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new FunctionException("Failed to delete function");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "DeleteFunction", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "DeleteFunction", requestId, ex, 0, additionalData);
                throw new FunctionException($"Error deleting function {id}", ex);
            }
        }

        private string ComputeHash(string input)
        {
            return NeoServiceLayer.Core.Utilities.HashUtility.ComputeSha256Hash(input);
        }



        /// <inheritdoc/>
        public Task<IEnumerable<FunctionTemplate>> GetTemplatesAsync()
        {
            throw new NotImplementedException("Template functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<FunctionTemplate> GetTemplateByIdAsync(Guid id)
        {
            throw new NotImplementedException("Template functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FunctionTemplate>> GetTemplatesByCategoryAsync(string category)
        {
            throw new NotImplementedException("Template functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FunctionTemplate>> GetTemplatesByRuntimeAsync(string runtime)
        {
            throw new NotImplementedException("Template functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<IEnumerable<FunctionTemplate>> GetTemplatesByTagsAsync(List<string> tags)
        {
            throw new NotImplementedException("Template functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<Core.Models.Function> CreateFromTemplateAsync(Guid templateId, string name, string description, Guid accountId, Dictionary<string, string> environmentVariables = null, List<Guid> secretIds = null, int maxExecutionTime = 30000, int maxMemory = 128)
        {
            throw new NotImplementedException("Template functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<FunctionTemplate> CreateTemplateAsync(FunctionTemplate template)
        {
            throw new NotImplementedException("Template functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<FunctionTemplate> UpdateTemplateAsync(FunctionTemplate template)
        {
            throw new NotImplementedException("Template functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<bool> DeleteTemplateAsync(Guid id)
        {
            throw new NotImplementedException("Template functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Core.Models.Function>> ProcessUploadAsync(string filePath, string fileName, Guid accountId)
        {
            throw new NotImplementedException("Upload functionality not implemented yet");
        }

        /// <inheritdoc/>
        public Task<Core.Models.Function> CreateFromZipAsync(Stream zipStream, string name, string description, FunctionRuntime runtime, string entryPoint, Guid accountId, Dictionary<string, string> environmentVariables = null, List<Guid> secretIds = null, int maxExecutionTime = 30000, int maxMemory = 128)
        {
            throw new NotImplementedException("ZIP functionality not implemented yet");
        }
    }
}
