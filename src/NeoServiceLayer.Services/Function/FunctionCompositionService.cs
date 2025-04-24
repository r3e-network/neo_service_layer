using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function
{
    /// <summary>
    /// Service for managing function compositions
    /// </summary>
    public class FunctionCompositionService : IFunctionCompositionService
    {
        private readonly ILogger<FunctionCompositionService> _logger;
        private readonly IFunctionCompositionRepository _compositionRepository;
        private readonly IFunctionCompositionExecutionRepository _executionRepository;
        private readonly IFunctionRepository _functionRepository;
        private readonly IFunctionService _functionService;
        private readonly IFunctionExecutor _functionExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCompositionService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="compositionRepository">Composition repository</param>
        /// <param name="executionRepository">Execution repository</param>
        /// <param name="functionRepository">Function repository</param>
        /// <param name="functionService">Function service</param>
        /// <param name="functionExecutor">Function executor</param>
        public FunctionCompositionService(
            ILogger<FunctionCompositionService> logger,
            IFunctionCompositionRepository compositionRepository,
            IFunctionCompositionExecutionRepository executionRepository,
            IFunctionRepository functionRepository,
            IFunctionService functionService,
            IFunctionExecutor functionExecutor)
        {
            _logger = logger;
            _compositionRepository = compositionRepository;
            _executionRepository = executionRepository;
            _functionRepository = functionRepository;
            _functionService = functionService;
            _functionExecutor = functionExecutor;
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> CreateAsync(FunctionComposition composition)
        {
            _logger.LogInformation("Creating function composition: {Name} for account {AccountId}", composition.Name, composition.AccountId);

            try
            {
                // Validate the composition
                var validationErrors = await ValidateAsync(composition);
                if (validationErrors.Any())
                {
                    throw new Exception($"Composition validation failed: {string.Join(", ", validationErrors)}");
                }

                // Set default values
                composition.Id = Guid.NewGuid();
                composition.CreatedAt = DateTime.UtcNow;
                composition.UpdatedAt = DateTime.UtcNow;

                // Ensure steps have IDs and are ordered
                for (int i = 0; i < composition.Steps.Count; i++)
                {
                    var step = composition.Steps[i];
                    if (step.Id == Guid.Empty)
                    {
                        step.Id = Guid.NewGuid();
                    }
                    step.Order = i;
                }

                // Generate input and output schemas if not provided
                if (string.IsNullOrWhiteSpace(composition.InputSchema))
                {
                    composition.InputSchema = await GenerateInputSchemaAsync(composition);
                }

                if (string.IsNullOrWhiteSpace(composition.OutputSchema))
                {
                    composition.OutputSchema = await GenerateOutputSchemaAsync(composition);
                }

                return await _compositionRepository.CreateAsync(composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating function composition: {Name} for account {AccountId}", composition.Name, composition.AccountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> UpdateAsync(FunctionComposition composition)
        {
            _logger.LogInformation("Updating function composition: {Id}", composition.Id);

            try
            {
                // Check if the composition exists
                var existingComposition = await _compositionRepository.GetByIdAsync(composition.Id);
                if (existingComposition == null)
                {
                    throw new Exception($"Function composition not found: {composition.Id}");
                }

                // Validate the composition
                var validationErrors = await ValidateAsync(composition);
                if (validationErrors.Any())
                {
                    throw new Exception($"Composition validation failed: {string.Join(", ", validationErrors)}");
                }

                // Preserve certain fields
                composition.CreatedAt = existingComposition.CreatedAt;
                composition.CreatedBy = existingComposition.CreatedBy;

                // Update timestamp
                composition.UpdatedAt = DateTime.UtcNow;

                // Ensure steps have IDs and are ordered
                for (int i = 0; i < composition.Steps.Count; i++)
                {
                    var step = composition.Steps[i];
                    if (step.Id == Guid.Empty)
                    {
                        step.Id = Guid.NewGuid();
                    }
                    step.Order = i;
                }

                // Generate input and output schemas if requested
                if (string.IsNullOrWhiteSpace(composition.InputSchema))
                {
                    composition.InputSchema = await GenerateInputSchemaAsync(composition);
                }

                if (string.IsNullOrWhiteSpace(composition.OutputSchema))
                {
                    composition.OutputSchema = await GenerateOutputSchemaAsync(composition);
                }

                return await _compositionRepository.UpdateAsync(composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating function composition: {Id}", composition.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function composition by ID: {Id}", id);

            try
            {
                return await _compositionRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function composition by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionComposition>> GetByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Getting function compositions by account ID: {AccountId}", accountId);

            try
            {
                return await _compositionRepository.GetByAccountIdAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function compositions by account ID: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionComposition>> GetByTagsAsync(List<string> tags)
        {
            _logger.LogInformation("Getting function compositions by tags: {Tags}", string.Join(", ", tags));

            try
            {
                return await _compositionRepository.GetByTagsAsync(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function compositions by tags: {Tags}", string.Join(", ", tags));
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function composition: {Id}", id);

            try
            {
                // Delete executions first
                await _executionRepository.DeleteByCompositionIdAsync(id);

                // Delete the composition
                return await _compositionRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting function composition: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionCompositionExecution> ExecuteAsync(Guid id, Dictionary<string, object> inputParameters)
        {
            _logger.LogInformation("Executing function composition: {Id}", id);

            try
            {
                // Get the composition
                var composition = await _compositionRepository.GetByIdAsync(id);
                if (composition == null)
                {
                    throw new Exception($"Function composition not found: {id}");
                }

                // Check if the composition is enabled
                if (!composition.IsEnabled)
                {
                    throw new Exception($"Function composition is disabled: {id}");
                }

                // Create an execution record
                var execution = new FunctionCompositionExecution
                {
                    Id = Guid.NewGuid(),
                    CompositionId = id,
                    AccountId = composition.AccountId,
                    Status = "running",
                    StartTime = DateTime.UtcNow,
                    InputParameters = inputParameters ?? new Dictionary<string, object>(),
                    StepExecutions = new List<FunctionCompositionStepExecution>(),
                    Logs = new List<string>()
                };

                // Save the initial execution record
                await _executionRepository.CreateAsync(execution);

                // Add initial log
                execution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Starting execution of composition: {composition.Name}");

                try
                {
                    // Execute the composition based on the execution mode
                    if (composition.ExecutionMode.ToLower() == "parallel")
                    {
                        await ExecuteParallelAsync(composition, execution);
                    }
                    else
                    {
                        await ExecuteSequentialAsync(composition, execution);
                    }

                    // Set the execution status to completed
                    execution.Status = "completed";
                    execution.EndTime = DateTime.UtcNow;
                    execution.ExecutionTimeMs = (execution.EndTime.Value - execution.StartTime).TotalMilliseconds;

                    // Add final log
                    execution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Execution completed successfully in {execution.ExecutionTimeMs} ms");
                }
                catch (Exception ex)
                {
                    // Set the execution status to error
                    execution.Status = "error";
                    execution.EndTime = DateTime.UtcNow;
                    execution.ExecutionTimeMs = (execution.EndTime.Value - execution.StartTime).TotalMilliseconds;
                    execution.ErrorMessage = ex.Message;
                    execution.StackTrace = ex.StackTrace;

                    // Add error log
                    execution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Execution failed: {ex.Message}");
                }

                // Save the final execution record
                return await _executionRepository.UpdateAsync(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function composition: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionCompositionExecution> GetExecutionByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function composition execution by ID: {Id}", id);

            try
            {
                return await _executionRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function composition execution by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetExecutionsByCompositionIdAsync(Guid compositionId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by composition ID: {CompositionId}, limit: {Limit}, offset: {Offset}", compositionId, limit, offset);

            try
            {
                return await _executionRepository.GetByCompositionIdAsync(compositionId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function composition executions by composition ID: {CompositionId}", compositionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetExecutionsByAccountIdAsync(Guid accountId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by account ID: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            try
            {
                return await _executionRepository.GetByAccountIdAsync(accountId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function composition executions by account ID: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionCompositionExecution>> GetExecutionsByStatusAsync(string status, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function composition executions by status: {Status}, limit: {Limit}, offset: {Offset}", status, limit, offset);

            try
            {
                return await _executionRepository.GetByStatusAsync(status, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting function composition executions by status: {Status}", status);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionCompositionExecution> CancelExecutionAsync(Guid executionId)
        {
            _logger.LogInformation("Cancelling function composition execution: {Id}", executionId);

            try
            {
                // Get the execution
                var execution = await _executionRepository.GetByIdAsync(executionId);
                if (execution == null)
                {
                    throw new Exception($"Function composition execution not found: {executionId}");
                }

                // Check if the execution is already completed or cancelled
                if (execution.Status == "completed" || execution.Status == "error" || execution.Status == "cancelled")
                {
                    throw new Exception($"Cannot cancel execution with status: {execution.Status}");
                }

                // Update the execution status
                execution.Status = "cancelled";
                execution.EndTime = DateTime.UtcNow;
                execution.ExecutionTimeMs = (execution.EndTime.Value - execution.StartTime).TotalMilliseconds;

                // Add cancellation log
                execution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Execution cancelled by user");

                // Save the updated execution record
                return await _executionRepository.UpdateAsync(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling function composition execution: {Id}", executionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetExecutionLogsAsync(Guid executionId)
        {
            _logger.LogInformation("Getting logs for function composition execution: {Id}", executionId);

            try
            {
                // Get the execution
                var execution = await _executionRepository.GetByIdAsync(executionId);
                if (execution == null)
                {
                    throw new Exception($"Function composition execution not found: {executionId}");
                }

                return execution.Logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting logs for function composition execution: {Id}", executionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetStepExecutionLogsAsync(Guid executionId, Guid stepId)
        {
            _logger.LogInformation("Getting logs for function composition step execution: {ExecutionId}, step: {StepId}", executionId, stepId);

            try
            {
                // Get the execution
                var execution = await _executionRepository.GetByIdAsync(executionId);
                if (execution == null)
                {
                    throw new Exception($"Function composition execution not found: {executionId}");
                }

                // Find the step execution
                var stepExecution = execution.StepExecutions.FirstOrDefault(s => s.StepId == stepId);
                if (stepExecution == null)
                {
                    throw new Exception($"Function composition step execution not found: {stepId}");
                }

                return stepExecution.Logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting logs for function composition step execution: {ExecutionId}, step: {StepId}", executionId, stepId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> ValidateAsync(FunctionComposition composition)
        {
            _logger.LogInformation("Validating function composition: {Name}", composition.Name);

            var errors = new List<string>();

            try
            {
                // Validate basic properties
                if (string.IsNullOrWhiteSpace(composition.Name))
                {
                    errors.Add("Composition name is required");
                }

                if (composition.AccountId == Guid.Empty)
                {
                    errors.Add("Account ID is required");
                }

                if (composition.MaxExecutionTime <= 0)
                {
                    errors.Add("Maximum execution time must be greater than 0");
                }

                // Validate steps
                if (composition.Steps == null || !composition.Steps.Any())
                {
                    errors.Add("At least one step is required");
                }
                else
                {
                    // Check each step
                    foreach (var step in composition.Steps)
                    {
                        // Validate step properties
                        if (string.IsNullOrWhiteSpace(step.Name))
                        {
                            errors.Add($"Step name is required for step at position {step.Order}");
                        }

                        if (step.FunctionId == Guid.Empty)
                        {
                            errors.Add($"Function ID is required for step '{step.Name}'");
                        }
                        else
                        {
                            // Check if the function exists
                            var function = await _functionRepository.GetByIdAsync(step.FunctionId);
                            if (function == null)
                            {
                                errors.Add($"Function not found for step '{step.Name}': {step.FunctionId}");
                            }
                        }

                        if (step.TimeoutMs <= 0)
                        {
                            errors.Add($"Timeout must be greater than 0 for step '{step.Name}'");
                        }

                        // Validate dependencies
                        if (step.Dependencies != null && step.Dependencies.Any())
                        {
                            foreach (var dependencyId in step.Dependencies)
                            {
                                // Check if the dependency exists in the steps
                                if (!composition.Steps.Any(s => s.Id == dependencyId))
                                {
                                    errors.Add($"Dependency not found for step '{step.Name}': {dependencyId}");
                                }

                                // Check for circular dependencies
                                if (HasCircularDependency(composition.Steps, step.Id, dependencyId))
                                {
                                    errors.Add($"Circular dependency detected for step '{step.Name}'");
                                }
                            }
                        }
                    }
                }

                return errors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating function composition: {Name}", composition.Name);
                errors.Add($"Validation error: {ex.Message}");
                return errors;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> AddStepAsync(Guid compositionId, FunctionCompositionStep step)
        {
            _logger.LogInformation("Adding step to function composition: {CompositionId}", compositionId);

            try
            {
                // Get the composition
                var composition = await _compositionRepository.GetByIdAsync(compositionId);
                if (composition == null)
                {
                    throw new Exception($"Function composition not found: {compositionId}");
                }

                // Set step ID if not provided
                if (step.Id == Guid.Empty)
                {
                    step.Id = Guid.NewGuid();
                }

                // Set step order to the end
                step.Order = composition.Steps.Count;

                // Add the step
                composition.Steps.Add(step);

                // Update timestamp
                composition.UpdatedAt = DateTime.UtcNow;

                // Update the composition
                return await _compositionRepository.UpdateAsync(composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding step to function composition: {CompositionId}", compositionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> UpdateStepAsync(Guid compositionId, FunctionCompositionStep step)
        {
            _logger.LogInformation("Updating step in function composition: {CompositionId}, step: {StepId}", compositionId, step.Id);

            try
            {
                // Get the composition
                var composition = await _compositionRepository.GetByIdAsync(compositionId);
                if (composition == null)
                {
                    throw new Exception($"Function composition not found: {compositionId}");
                }

                // Find the step
                var existingStep = composition.Steps.FirstOrDefault(s => s.Id == step.Id);
                if (existingStep == null)
                {
                    throw new Exception($"Step not found in composition: {step.Id}");
                }

                // Preserve the order
                step.Order = existingStep.Order;

                // Replace the step
                var stepIndex = composition.Steps.IndexOf(existingStep);
                composition.Steps[stepIndex] = step;

                // Update timestamp
                composition.UpdatedAt = DateTime.UtcNow;

                // Update the composition
                return await _compositionRepository.UpdateAsync(composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating step in function composition: {CompositionId}, step: {StepId}", compositionId, step.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> RemoveStepAsync(Guid compositionId, Guid stepId)
        {
            _logger.LogInformation("Removing step from function composition: {CompositionId}, step: {StepId}", compositionId, stepId);

            try
            {
                // Get the composition
                var composition = await _compositionRepository.GetByIdAsync(compositionId);
                if (composition == null)
                {
                    throw new Exception($"Function composition not found: {compositionId}");
                }

                // Find the step
                var step = composition.Steps.FirstOrDefault(s => s.Id == stepId);
                if (step == null)
                {
                    throw new Exception($"Step not found in composition: {stepId}");
                }

                // Remove the step
                composition.Steps.Remove(step);

                // Reorder the steps
                for (int i = 0; i < composition.Steps.Count; i++)
                {
                    composition.Steps[i].Order = i;
                }

                // Update timestamp
                composition.UpdatedAt = DateTime.UtcNow;

                // Update the composition
                return await _compositionRepository.UpdateAsync(composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing step from function composition: {CompositionId}, step: {StepId}", compositionId, stepId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> ReorderStepsAsync(Guid compositionId, List<Guid> stepIds)
        {
            _logger.LogInformation("Reordering steps in function composition: {CompositionId}", compositionId);

            try
            {
                // Get the composition
                var composition = await _compositionRepository.GetByIdAsync(compositionId);
                if (composition == null)
                {
                    throw new Exception($"Function composition not found: {compositionId}");
                }

                // Check if the step IDs match the composition steps
                if (stepIds.Count != composition.Steps.Count || !composition.Steps.All(s => stepIds.Contains(s.Id)))
                {
                    throw new Exception("The provided step IDs do not match the composition steps");
                }

                // Create a new ordered list of steps
                var orderedSteps = new List<FunctionCompositionStep>();
                for (int i = 0; i < stepIds.Count; i++)
                {
                    var step = composition.Steps.First(s => s.Id == stepIds[i]);
                    step.Order = i;
                    orderedSteps.Add(step);
                }

                // Replace the steps
                composition.Steps = orderedSteps;

                // Update timestamp
                composition.UpdatedAt = DateTime.UtcNow;

                // Update the composition
                return await _compositionRepository.UpdateAsync(composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering steps in function composition: {CompositionId}", compositionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetInputSchemaAsync(Guid id)
        {
            _logger.LogInformation("Getting input schema for function composition: {Id}", id);

            try
            {
                // Get the composition
                var composition = await _compositionRepository.GetByIdAsync(id);
                if (composition == null)
                {
                    throw new Exception($"Function composition not found: {id}");
                }

                return composition.InputSchema;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting input schema for function composition: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetOutputSchemaAsync(Guid id)
        {
            _logger.LogInformation("Getting output schema for function composition: {Id}", id);

            try
            {
                // Get the composition
                var composition = await _compositionRepository.GetByIdAsync(id);
                if (composition == null)
                {
                    throw new Exception($"Function composition not found: {id}");
                }

                return composition.OutputSchema;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting output schema for function composition: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GenerateInputSchemaAsync(Guid id)
        {
            _logger.LogInformation("Generating input schema for function composition: {Id}", id);

            try
            {
                // Get the composition
                var composition = await _compositionRepository.GetByIdAsync(id);
                if (composition == null)
                {
                    throw new Exception($"Function composition not found: {id}");
                }

                return await GenerateInputSchemaAsync(composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating input schema for function composition: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GenerateOutputSchemaAsync(Guid id)
        {
            _logger.LogInformation("Generating output schema for function composition: {Id}", id);

            try
            {
                // Get the composition
                var composition = await _compositionRepository.GetByIdAsync(id);
                if (composition == null)
                {
                    throw new Exception($"Function composition not found: {id}");
                }

                return await GenerateOutputSchemaAsync(composition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating output schema for function composition: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Generates the input schema for a function composition
        /// </summary>
        /// <param name="composition">Function composition</param>
        /// <returns>The generated input schema</returns>
        private async Task<string> GenerateInputSchemaAsync(FunctionComposition composition)
        {
            try
            {
                // Find the first steps (steps with no dependencies)
                var firstSteps = composition.Steps.Where(s => s.Dependencies == null || !s.Dependencies.Any()).ToList();

                // Create a schema object
                var schema = new Dictionary<string, object>
                {
                    { "type", "object" },
                    { "properties", new Dictionary<string, object>() }
                };

                var properties = (Dictionary<string, object>)schema["properties"];
                var required = new List<string>();

                // Add properties for each first step
                foreach (var step in firstSteps)
                {
                    // Get the function
                    var function = await _functionRepository.GetByIdAsync(step.FunctionId);
                    if (function == null)
                    {
                        continue;
                    }

                    // Add properties based on the function's input schema
                    if (!string.IsNullOrWhiteSpace(function.InputSchema))
                    {
                        try
                        {
                            var functionSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(function.InputSchema);
                            if (functionSchema.TryGetValue("properties", out var functionProperties) && functionProperties is Dictionary<string, object> props)
                            {
                                foreach (var prop in props)
                                {
                                    // Add the property with the step name as prefix
                                    properties[$"{step.Name}.{prop.Key}"] = prop.Value;
                                }
                            }

                            // Add required properties
                            if (functionSchema.TryGetValue("required", out var functionRequired) && functionRequired is List<object> reqList)
                            {
                                foreach (var req in reqList)
                                {
                                    required.Add($"{step.Name}.{req}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error parsing input schema for function {FunctionId}", function.Id);
                        }
                    }
                }

                // Add required properties to the schema
                if (required.Any())
                {
                    schema["required"] = required;
                }

                // Serialize the schema
                return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating input schema for function composition: {Name}", composition.Name);
                throw;
            }
        }

        /// <summary>
        /// Generates the output schema for a function composition
        /// </summary>
        /// <param name="composition">Function composition</param>
        /// <returns>The generated output schema</returns>
        private async Task<string> GenerateOutputSchemaAsync(FunctionComposition composition)
        {
            try
            {
                // Find the last steps (steps that are not dependencies of any other step)
                var allDependencies = composition.Steps.SelectMany(s => s.Dependencies ?? new List<Guid>()).ToList();
                var lastSteps = composition.Steps.Where(s => !allDependencies.Contains(s.Id)).ToList();

                // Create a schema object
                var schema = new Dictionary<string, object>
                {
                    { "type", "object" },
                    { "properties", new Dictionary<string, object>() }
                };

                var properties = (Dictionary<string, object>)schema["properties"];

                // Add properties for each last step
                foreach (var step in lastSteps)
                {
                    // Get the function
                    var function = await _functionRepository.GetByIdAsync(step.FunctionId);
                    if (function == null)
                    {
                        continue;
                    }

                    // Add properties based on the function's output schema
                    if (!string.IsNullOrWhiteSpace(function.OutputSchema))
                    {
                        try
                        {
                            var functionSchema = JsonSerializer.Deserialize<Dictionary<string, object>>(function.OutputSchema);

                            // Add the property with the step name as key
                            properties[step.Name] = functionSchema;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error parsing output schema for function {FunctionId}", function.Id);
                        }
                    }
                    else
                    {
                        // If no output schema, add a generic property
                        properties[step.Name] = new Dictionary<string, object>
                        {
                            { "type", "object" },
                            { "description", $"Output from step {step.Name}" }
                        };
                    }
                }

                // Serialize the schema
                return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating output schema for function composition: {Name}", composition.Name);
                throw;
            }
        }

        /// <summary>
        /// Executes a function composition sequentially
        /// </summary>
        /// <param name="composition">Function composition</param>
        /// <param name="execution">Execution record</param>
        /// <returns>The execution result</returns>
        private async Task ExecuteSequentialAsync(FunctionComposition composition, FunctionCompositionExecution execution)
        {
            // Get the ordered steps
            var orderedSteps = composition.Steps.OrderBy(s => s.Order).ToList();

            // Create a dictionary to store step results
            var stepResults = new Dictionary<Guid, object>();

            // Execute each step in order
            foreach (var step in orderedSteps)
            {
                // Check if all dependencies are satisfied
                if (step.Dependencies != null && step.Dependencies.Any())
                {
                    var allDependenciesSatisfied = step.Dependencies.All(d => stepResults.ContainsKey(d));
                    if (!allDependenciesSatisfied)
                    {
                        throw new Exception($"Not all dependencies are satisfied for step '{step.Name}'");
                    }
                }

                // Create a step execution record
                var stepExecution = new FunctionCompositionStepExecution
                {
                    Id = Guid.NewGuid(),
                    StepId = step.Id,
                    FunctionId = step.FunctionId,
                    FunctionVersion = step.FunctionVersion,
                    Status = "running",
                    StartTime = DateTime.UtcNow,
                    InputParameters = new Dictionary<string, object>(),
                    Logs = new List<string>()
                };

                // Add the step execution to the execution record
                execution.StepExecutions.Add(stepExecution);

                // Add initial log
                stepExecution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Starting execution of step: {step.Name}");

                try
                {
                    // Prepare input parameters
                    var inputParameters = await PrepareInputParametersAsync(step, execution.InputParameters, stepResults);
                    stepExecution.InputParameters = inputParameters;

                    // Get the function
                    var function = await _functionRepository.GetByIdAsync(step.FunctionId);
                    if (function == null)
                    {
                        throw new Exception($"Function not found: {step.FunctionId}");
                    }

                    // Create execution context
                    var executionContext = new FunctionExecutionContext
                    {
                        FunctionId = step.FunctionId,
                        AccountId = composition.AccountId,
                        MaxExecutionTime = step.TimeoutMs,
                        MaxMemory = function.MaxMemory,
                        EnvironmentVariables = composition.EnvironmentVariables
                    };

                    // Execute the function
                    var startTime = DateTime.UtcNow;
                    var result = await _functionExecutor.ExecuteAsync(step.FunctionId, inputParameters, executionContext);
                    var endTime = DateTime.UtcNow;

                    // Update the step execution record
                    stepExecution.Status = "completed";
                    stepExecution.OutputResult = result;
                    stepExecution.EndTime = endTime;
                    stepExecution.ExecutionTimeMs = (endTime - startTime).TotalMilliseconds;

                    // Add completion log
                    stepExecution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Step completed successfully in {stepExecution.ExecutionTimeMs} ms");

                    // Store the result for use by subsequent steps
                    stepResults[step.Id] = result;

                    // Apply output mappings
                    if (step.OutputMappings != null && step.OutputMappings.Any())
                    {
                        foreach (var mapping in step.OutputMappings)
                        {
                            // TODO: Implement output mappings
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Update the step execution record
                    stepExecution.Status = "error";
                    stepExecution.ErrorMessage = ex.Message;
                    stepExecution.StackTrace = ex.StackTrace;
                    stepExecution.EndTime = DateTime.UtcNow;
                    stepExecution.ExecutionTimeMs = (stepExecution.EndTime.Value - stepExecution.StartTime).TotalMilliseconds;

                    // Add error log
                    stepExecution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Step failed: {ex.Message}");

                    // Handle the error based on the error handling strategy
                    if (step.ErrorHandlingStrategy == "continue" || composition.ErrorHandlingStrategy == "continue")
                    {
                        // Continue with the next step
                        continue;
                    }
                    else
                    {
                        // Stop the execution
                        throw;
                    }
                }
            }

            // Set the execution result
            execution.OutputResult = new Dictionary<string, object>();
            foreach (var step in orderedSteps)
            {
                if (stepResults.TryGetValue(step.Id, out var result))
                {
                    ((Dictionary<string, object>)execution.OutputResult)[step.Name] = result;
                }
            }
        }

        /// <summary>
        /// Executes a function composition in parallel
        /// </summary>
        /// <param name="composition">Function composition</param>
        /// <param name="execution">Execution record</param>
        /// <returns>The execution result</returns>
        private async Task ExecuteParallelAsync(FunctionComposition composition, FunctionCompositionExecution execution)
        {
            // Create a dictionary to store step results
            var stepResults = new Dictionary<Guid, object>();

            // Create a dictionary to track completed steps
            var completedSteps = new Dictionary<Guid, bool>();

            // Create a list of tasks
            var tasks = new List<Task>();

            // Create a cancellation token source
            using var cts = new System.Threading.CancellationTokenSource();

            // Set a timeout for the entire composition
            cts.CancelAfter(composition.MaxExecutionTime);

            // Execute steps in parallel based on dependencies
            foreach (var step in composition.Steps)
            {
                // Create a task for each step
                var task = ExecuteStepAsync(step, composition, execution, stepResults, completedSteps, cts.Token);
                tasks.Add(task);
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Set the execution result
            execution.OutputResult = new Dictionary<string, object>();
            foreach (var step in composition.Steps)
            {
                if (stepResults.TryGetValue(step.Id, out var result))
                {
                    ((Dictionary<string, object>)execution.OutputResult)[step.Name] = result;
                }
            }
        }

        /// <summary>
        /// Executes a single step in a function composition
        /// </summary>
        /// <param name="step">Step to execute</param>
        /// <param name="composition">Function composition</param>
        /// <param name="execution">Execution record</param>
        /// <param name="stepResults">Dictionary of step results</param>
        /// <param name="completedSteps">Dictionary of completed steps</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The step execution task</returns>
        private async Task ExecuteStepAsync(FunctionCompositionStep step, FunctionComposition composition, FunctionCompositionExecution execution, Dictionary<Guid, object> stepResults, Dictionary<Guid, bool> completedSteps, System.Threading.CancellationToken cancellationToken)
        {
            // Wait for dependencies to complete
            if (step.Dependencies != null && step.Dependencies.Any())
            {
                await WaitForDependenciesAsync(step, completedSteps, cancellationToken);
            }

            // Check if the step should be executed based on its condition
            if (!string.IsNullOrWhiteSpace(step.Condition))
            {
                // TODO: Implement condition evaluation
                // For now, always execute the step
            }

            // Create a step execution record
            var stepExecution = new FunctionCompositionStepExecution
            {
                Id = Guid.NewGuid(),
                StepId = step.Id,
                FunctionId = step.FunctionId,
                FunctionVersion = step.FunctionVersion,
                Status = "running",
                StartTime = DateTime.UtcNow,
                InputParameters = new Dictionary<string, object>(),
                Logs = new List<string>()
            };

            // Add the step execution to the execution record
            lock (execution.StepExecutions)
            {
                execution.StepExecutions.Add(stepExecution);
            }

            // Add initial log
            stepExecution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Starting execution of step: {step.Name}");

            try
            {
                // Prepare input parameters
                var inputParameters = await PrepareInputParametersAsync(step, execution.InputParameters, stepResults);
                stepExecution.InputParameters = inputParameters;

                // Get the function
                var function = await _functionRepository.GetByIdAsync(step.FunctionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {step.FunctionId}");
                }

                // Create execution context
                var executionContext = new FunctionExecutionContext
                {
                    FunctionId = step.FunctionId,
                    AccountId = composition.AccountId,
                    MaxExecutionTime = step.TimeoutMs,
                    MaxMemory = function.MaxMemory,
                    EnvironmentVariables = composition.EnvironmentVariables
                };

                // Execute the function with retry policy
                var result = await ExecuteWithRetryAsync(step, executionContext, inputParameters, stepExecution);

                // Update the step execution record
                stepExecution.Status = "completed";
                stepExecution.OutputResult = result;
                stepExecution.EndTime = DateTime.UtcNow;
                stepExecution.ExecutionTimeMs = (stepExecution.EndTime.Value - stepExecution.StartTime).TotalMilliseconds;

                // Add completion log
                stepExecution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Step completed successfully in {stepExecution.ExecutionTimeMs} ms");

                // Store the result for use by subsequent steps
                lock (stepResults)
                {
                    stepResults[step.Id] = result;
                }

                // Mark the step as completed
                lock (completedSteps)
                {
                    completedSteps[step.Id] = true;
                }

                // Apply output mappings
                if (step.OutputMappings != null && step.OutputMappings.Any())
                {
                    foreach (var mapping in step.OutputMappings)
                    {
                        // TODO: Implement output mappings
                    }
                }
            }
            catch (Exception ex)
            {
                // Update the step execution record
                stepExecution.Status = "error";
                stepExecution.ErrorMessage = ex.Message;
                stepExecution.StackTrace = ex.StackTrace;
                stepExecution.EndTime = DateTime.UtcNow;
                stepExecution.ExecutionTimeMs = (stepExecution.EndTime.Value - stepExecution.StartTime).TotalMilliseconds;

                // Add error log
                stepExecution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Step failed: {ex.Message}");

                // Mark the step as completed (with error)
                lock (completedSteps)
                {
                    completedSteps[step.Id] = true;
                }

                // Handle the error based on the error handling strategy
                if (step.ErrorHandlingStrategy != "continue" && composition.ErrorHandlingStrategy != "continue")
                {
                    // Propagate the error
                    throw;
                }
            }
        }

        /// <summary>
        /// Waits for dependencies to complete
        /// </summary>
        /// <param name="step">Step to wait for dependencies</param>
        /// <param name="completedSteps">Dictionary of completed steps</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The wait task</returns>
        private async Task WaitForDependenciesAsync(FunctionCompositionStep step, Dictionary<Guid, bool> completedSteps, System.Threading.CancellationToken cancellationToken)
        {
            while (true)
            {
                // Check if all dependencies are completed
                bool allDependenciesCompleted;
                lock (completedSteps)
                {
                    allDependenciesCompleted = step.Dependencies.All(d => completedSteps.TryGetValue(d, out var completed) && completed);
                }

                if (allDependenciesCompleted)
                {
                    return;
                }

                // Check if the operation is cancelled
                cancellationToken.ThrowIfCancellationRequested();

                // Wait a bit before checking again
                await Task.Delay(100, cancellationToken);
            }
        }

        /// <summary>
        /// Executes a function with retry policy
        /// </summary>
        /// <param name="step">Step to execute</param>
        /// <param name="executionContext">Execution context</param>
        /// <param name="inputParameters">Input parameters</param>
        /// <param name="stepExecution">Step execution record</param>
        /// <returns>The execution result</returns>
        private async Task<object> ExecuteWithRetryAsync(FunctionCompositionStep step, FunctionExecutionContext executionContext, Dictionary<string, object> inputParameters, FunctionCompositionStepExecution stepExecution)
        {
            // If no retry policy, execute once
            if (step.RetryPolicy == null)
            {
                return await _functionExecutor.ExecuteAsync(step.FunctionId, inputParameters, executionContext);
            }

            // Execute with retry
            int retryCount = 0;
            int delay = step.RetryPolicy.InitialDelayMs;
            Exception lastException = null;

            while (retryCount <= step.RetryPolicy.MaxRetries)
            {
                try
                {
                    // Execute the function
                    var startTime = DateTime.UtcNow;
                    var result = await _functionExecutor.ExecuteAsync(step.FunctionId, inputParameters, executionContext);
                    var endTime = DateTime.UtcNow;

                    // Add success log
                    stepExecution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Function executed successfully in {(endTime - startTime).TotalMilliseconds} ms");

                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retryCount++;
                    stepExecution.RetryCount = retryCount;

                    // Add retry log
                    stepExecution.Logs.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Retry {retryCount}/{step.RetryPolicy.MaxRetries}: {ex.Message}");

                    // Check if we've reached the maximum retries
                    if (retryCount > step.RetryPolicy.MaxRetries)
                    {
                        break;
                    }

                    // Calculate the next delay with exponential backoff
                    delay = (int)Math.Min(delay * step.RetryPolicy.BackoffMultiplier, step.RetryPolicy.MaxDelayMs);

                    // Add jitter if enabled
                    if (step.RetryPolicy.UseJitter)
                    {
                        var random = new Random();
                        delay = (int)(delay * (0.5 + random.NextDouble()));
                    }

                    // Wait before retrying
                    await Task.Delay(delay);
                }
            }

            // If we get here, all retries failed
            throw lastException ?? new Exception("Function execution failed after retries");
        }

        /// <summary>
        /// Prepares input parameters for a step
        /// </summary>
        /// <param name="step">Step to prepare parameters for</param>
        /// <param name="compositionInputs">Composition input parameters</param>
        /// <param name="stepResults">Dictionary of step results</param>
        /// <returns>The prepared input parameters</returns>
        private async Task<Dictionary<string, object>> PrepareInputParametersAsync(FunctionCompositionStep step, Dictionary<string, object> compositionInputs, Dictionary<Guid, object> stepResults)
        {
            var inputParameters = new Dictionary<string, object>();

            // Apply input mappings if defined
            if (step.InputMappings != null && step.InputMappings.Any())
            {
                foreach (var mapping in step.InputMappings)
                {
                    // Parse the source path
                    var sourcePath = mapping.Value;
                    if (sourcePath.StartsWith("input."))
                    {
                        // Get from composition inputs
                        var inputPath = sourcePath.Substring(6);
                        if (compositionInputs.TryGetValue(inputPath, out var value))
                        {
                            inputParameters[mapping.Key] = value;
                        }
                    }
                    else if (sourcePath.Contains("."))
                    {
                        // Get from step results
                        var parts = sourcePath.Split('.');
                        var stepName = parts[0];
                        var propertyPath = string.Join(".", parts.Skip(1));

                        // Find the step by name
                        var sourceStep = step.Dependencies
                            .Select(d => step.Dependencies.FirstOrDefault(s => s == d))
                            .FirstOrDefault();

                        if (sourceStep != Guid.Empty && stepResults.TryGetValue(sourceStep, out var stepResult))
                        {
                            // Extract the value from the step result
                            var value = ExtractValueFromPath(stepResult, propertyPath);
                            if (value != null)
                            {
                                inputParameters[mapping.Key] = value;
                            }
                        }
                    }
                    else
                    {
                        // Treat as a literal value
                        inputParameters[mapping.Key] = mapping.Value;
                    }
                }
            }
            else
            {
                // If no mappings, use composition inputs with step name prefix
                foreach (var input in compositionInputs)
                {
                    if (input.Key.StartsWith($"{step.Name}."))
                    {
                        var paramName = input.Key.Substring(step.Name.Length + 1);
                        inputParameters[paramName] = input.Value;
                    }
                }

                // Add results from dependencies
                if (step.Dependencies != null)
                {
                    foreach (var dependencyId in step.Dependencies)
                    {
                        if (stepResults.TryGetValue(dependencyId, out var dependencyResult))
                        {
                            // Find the dependency step
                            var dependencyStep = step.Dependencies
                                .Select(d => step.Dependencies.FirstOrDefault(s => s == d))
                                .FirstOrDefault();

                            if (dependencyStep != Guid.Empty)
                            {
                                inputParameters[$"dependency_{dependencyStep}"] = dependencyResult;
                            }
                        }
                    }
                }
            }

            return inputParameters;
        }

        /// <summary>
        /// Extracts a value from a path in an object
        /// </summary>
        /// <param name="obj">Object to extract from</param>
        /// <param name="path">Path to extract</param>
        /// <returns>The extracted value</returns>
        private object ExtractValueFromPath(object obj, string path)
        {
            if (obj == null || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            // Split the path into parts
            var parts = path.Split('.');
            var current = obj;

            // Navigate through the path
            foreach (var part in parts)
            {
                if (current == null)
                {
                    return null;
                }

                // Handle dictionary
                if (current is IDictionary<string, object> dict)
                {
                    if (dict.TryGetValue(part, out var value))
                    {
                        current = value;
                    }
                    else
                    {
                        return null;
                    }
                }
                // Handle object properties
                else
                {
                    var property = current.GetType().GetProperty(part);
                    if (property != null)
                    {
                        current = property.GetValue(current);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return current;
        }

        /// <summary>
        /// Checks if a step has a circular dependency
        /// </summary>
        /// <param name="steps">List of steps</param>
        /// <param name="stepId">Step ID to check</param>
        /// <param name="dependencyId">Dependency ID to check</param>
        /// <returns>True if there is a circular dependency, false otherwise</returns>
        private bool HasCircularDependency(List<FunctionCompositionStep> steps, Guid stepId, Guid dependencyId)
        {
            // Find the dependency step
            var dependencyStep = steps.FirstOrDefault(s => s.Id == dependencyId);
            if (dependencyStep == null)
            {
                return false;
            }

            // Check if the dependency depends on the step
            if (dependencyStep.Dependencies != null && dependencyStep.Dependencies.Contains(stepId))
            {
                return true;
            }

            // Check if the dependency depends on any step that depends on the step
            if (dependencyStep.Dependencies != null)
            {
                foreach (var transitiveDependencyId in dependencyStep.Dependencies)
                {
                    if (HasCircularDependency(steps, stepId, transitiveDependencyId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
