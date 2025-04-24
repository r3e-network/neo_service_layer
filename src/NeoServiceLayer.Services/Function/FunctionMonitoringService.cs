using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function
{
    /// <summary>
    /// Service for monitoring functions
    /// </summary>
    public class FunctionMonitoringService : IFunctionMonitoringService
    {
        private readonly ILogger<FunctionMonitoringService> _logger;
        private readonly IFunctionExecutionRepository _executionRepository;
        private readonly IFunctionLogRepository _logRepository;
        private readonly IFunctionMetricsRepository _metricsRepository;
        private readonly IFunctionMonitoringSettingsRepository _monitoringSettingsRepository;
        private readonly IFunctionRepository _functionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionMonitoringService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="executionRepository">Execution repository</param>
        /// <param name="logRepository">Log repository</param>
        /// <param name="metricsRepository">Metrics repository</param>
        /// <param name="monitoringSettingsRepository">Monitoring settings repository</param>
        /// <param name="functionRepository">Function repository</param>
        public FunctionMonitoringService(
            ILogger<FunctionMonitoringService> logger,
            IFunctionExecutionRepository executionRepository,
            IFunctionLogRepository logRepository,
            IFunctionMetricsRepository metricsRepository,
            IFunctionMonitoringSettingsRepository monitoringSettingsRepository,
            IFunctionRepository functionRepository)
        {
            _logger = logger;
            _executionRepository = executionRepository;
            _logRepository = logRepository;
            _metricsRepository = metricsRepository;
            _monitoringSettingsRepository = monitoringSettingsRepository;
            _functionRepository = functionRepository;
        }

        /// <inheritdoc/>
        public async Task<FunctionExecution> RecordExecutionAsync(FunctionExecution execution)
        {
            _logger.LogInformation("Recording function execution: {ExecutionId} for function: {FunctionId}", execution.Id, execution.FunctionId);

            try
            {
                // Convert FunctionExecution to FunctionExecutionResult
                var executionResult = new FunctionExecutionResult
                {
                    Id = execution.Id,
                    ExecutionId = execution.Id,
                    FunctionId = execution.FunctionId,
                    Status = execution.Status,
                    Output = execution.Output,
                    Error = execution.ErrorMessage,
                    StartTime = execution.StartTime,
                    EndTime = execution.EndTime,
                    DurationMs = execution.DurationMs,
                    MemoryUsageMb = execution.MemoryUsageMb,
                    CpuUsagePercent = execution.CpuUsagePercent,
                    BillingAmount = execution.BillingAmount,
                    // Convert logs if needed
                    Logs = execution.Logs != null ? execution.Logs.Select(l => l.Message).ToList() : new List<string>(),
                    TraceId = execution.EventType // Use EventType as TraceId for now
                };

                // Save the execution
                var savedExecution = await _executionRepository.CreateAsync(executionResult);

                // Calculate execution time and memory usage
                double executionTime = 0;
                double memoryUsage = 0;

                if (execution.EndTime.HasValue)
                {
                    executionTime = (execution.EndTime.Value - execution.StartTime).TotalMilliseconds;
                }

                // Update function metrics
                await UpdateMetricsAsync(execution.FunctionId, executionTime, memoryUsage);

                // Update function statistics
                await UpdateFunctionStatisticsAsync(execution.FunctionId, executionTime, memoryUsage);

                // Convert back to FunctionExecution
                return execution;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording function execution: {ExecutionId} for function: {FunctionId}", execution.Id, execution.FunctionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionLog> RecordLogAsync(Guid functionId, Guid executionId, string message, string level, DateTime timestamp)
        {
            _logger.LogDebug("Recording function log for function: {FunctionId}, execution: {ExecutionId}", functionId, executionId);

            try
            {
                var log = new FunctionLog
                {
                    Id = Guid.NewGuid(),
                    ExecutionId = executionId,
                    Message = message,
                    Level = level,
                    Timestamp = timestamp
                };

                return await _logRepository.CreateAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording function log for function: {FunctionId}, execution: {ExecutionId}", functionId, executionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionExecution>> GetExecutionsByFunctionIdAsync(Guid functionId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting executions for function: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            try
            {
                var results = await _executionRepository.GetByFunctionIdAsync(functionId, limit, offset);

                // Convert FunctionExecutionResult to FunctionExecution
                return results.Select(r => new FunctionExecution
                {
                    Id = r.Id,
                    FunctionId = r.FunctionId,
                    Status = r.Status,
                    Output = r.Output?.ToString(),
                    ErrorMessage = r.Error,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    DurationMs = (long)r.DurationMs,
                    MemoryUsageMb = r.MemoryUsageMb,
                    CpuUsagePercent = r.CpuUsagePercent,
                    BillingAmount = r.BillingAmount,
                    EventType = r.TraceId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionExecution>> GetExecutionsByAccountIdAsync(Guid accountId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting executions for account: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            try
            {
                var results = await _executionRepository.GetByAccountIdAsync(accountId, limit, offset);

                // Convert FunctionExecutionResult to FunctionExecution
                return results.Select(r => new FunctionExecution
                {
                    Id = r.Id,
                    FunctionId = r.FunctionId,
                    Status = r.Status,
                    Output = r.Output?.ToString(),
                    ErrorMessage = r.Error,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    DurationMs = (long)r.DurationMs,
                    MemoryUsageMb = r.MemoryUsageMb,
                    CpuUsagePercent = r.CpuUsagePercent,
                    BillingAmount = r.BillingAmount,
                    EventType = r.TraceId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting executions for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionExecution> GetExecutionByIdAsync(Guid executionId)
        {
            _logger.LogInformation("Getting execution by ID: {ExecutionId}", executionId);

            try
            {
                var result = await _executionRepository.GetByIdAsync(executionId);
                if (result == null)
                {
                    return null;
                }

                // Convert FunctionExecutionResult to FunctionExecution
                return new FunctionExecution
                {
                    Id = result.Id,
                    FunctionId = result.FunctionId,
                    Status = result.Status,
                    Output = result.Output?.ToString(),
                    ErrorMessage = result.Error,
                    StartTime = result.StartTime,
                    EndTime = result.EndTime,
                    DurationMs = (long)result.DurationMs,
                    MemoryUsageMb = result.MemoryUsageMb,
                    CpuUsagePercent = result.CpuUsagePercent,
                    BillingAmount = result.BillingAmount,
                    EventType = result.TraceId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting execution by ID: {ExecutionId}", executionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionLog>> GetLogsByExecutionIdAsync(Guid executionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting logs for execution: {ExecutionId}, limit: {Limit}, offset: {Offset}", executionId, limit, offset);

            try
            {
                return await _logRepository.GetByExecutionIdAsync(executionId, limit, offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting logs for execution: {ExecutionId}", executionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMetrics> GetMetricsByFunctionIdAsync(Guid functionId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting metrics for function: {FunctionId}, startTime: {StartTime}, endTime: {EndTime}", functionId, startTime, endTime);

            try
            {
                return await _metricsRepository.GetByFunctionIdAsync(functionId, startTime, endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMetrics>> GetMetricsByAccountIdAsync(Guid accountId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting metrics for account: {AccountId}, startTime: {StartTime}, endTime: {EndTime}", accountId, startTime, endTime);

            try
            {
                return await _metricsRepository.GetByAccountIdAsync(accountId, startTime, endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for account: {AccountId}", accountId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMetrics> UpdateMetricsAsync(Guid functionId, double executionTime, double memoryUsage)
        {
            _logger.LogInformation("Updating metrics for function: {FunctionId}, executionTime: {ExecutionTime}, memoryUsage: {MemoryUsage}", functionId, executionTime, memoryUsage);

            try
            {
                // Get the function to get the account ID
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Get the current metrics for today
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);
                var metrics = await _metricsRepository.GetByFunctionIdAsync(functionId, today, tomorrow);

                if (metrics == null)
                {
                    // Create new metrics
                    metrics = new FunctionMetrics
                    {
                        FunctionId = functionId,
                        AccountId = function.AccountId,
                        TimePeriod = "daily",
                        StartTime = today,
                        EndTime = tomorrow,
                        Invocations = 1,
                        SuccessfulInvocations = 1,
                        FailedInvocations = 0,
                        ThrottledInvocations = 0,
                        AverageExecutionTime = executionTime,
                        MinExecutionTime = executionTime,
                        MaxExecutionTime = executionTime,
                        AverageMemoryUsage = memoryUsage,
                        MinMemoryUsage = memoryUsage,
                        MaxMemoryUsage = memoryUsage,
                        ExecutionTimePercentiles = new Dictionary<string, double>
                        {
                            { "p50", executionTime },
                            { "p90", executionTime },
                            { "p95", executionTime },
                            { "p99", executionTime }
                        },
                        MemoryUsagePercentiles = new Dictionary<string, double>
                        {
                            { "p50", memoryUsage },
                            { "p90", memoryUsage },
                            { "p95", memoryUsage },
                            { "p99", memoryUsage }
                        },
                        ErrorCounts = new Dictionary<string, int>(),
                        InvocationCountsByStatus = new Dictionary<string, int>
                        {
                            { "Success", 1 }
                        },
                        InvocationCountsByHour = new Dictionary<int, int>
                        {
                            { DateTime.UtcNow.Hour, 1 }
                        },
                        InvocationCountsByDay = new Dictionary<string, int>
                        {
                            { today.ToString("yyyy-MM-dd"), 1 }
                        },
                        TotalExecutionTime = executionTime,
                        TotalMemoryUsage = memoryUsage,
                        TotalCost = 0,
                        CostPerInvocation = 0,
                        LastUpdated = DateTime.UtcNow
                    };

                    return await _metricsRepository.CreateAsync(metrics);
                }
                else
                {
                    // Update existing metrics
                    metrics.Invocations++;
                    metrics.SuccessfulInvocations++;

                    // Update execution time metrics
                    metrics.TotalExecutionTime += executionTime;
                    metrics.AverageExecutionTime = metrics.TotalExecutionTime / metrics.Invocations;
                    metrics.MinExecutionTime = Math.Min(metrics.MinExecutionTime, executionTime);
                    metrics.MaxExecutionTime = Math.Max(metrics.MaxExecutionTime, executionTime);

                    // Update memory usage metrics
                    metrics.TotalMemoryUsage += memoryUsage;
                    metrics.AverageMemoryUsage = metrics.TotalMemoryUsage / metrics.Invocations;
                    metrics.MinMemoryUsage = Math.Min(metrics.MinMemoryUsage, memoryUsage);
                    metrics.MaxMemoryUsage = Math.Max(metrics.MaxMemoryUsage, memoryUsage);

                    // Update invocation counts by hour
                    var hour = DateTime.UtcNow.Hour;
                    if (metrics.InvocationCountsByHour.ContainsKey(hour))
                    {
                        metrics.InvocationCountsByHour[hour]++;
                    }
                    else
                    {
                        metrics.InvocationCountsByHour[hour] = 1;
                    }

                    // Update invocation counts by status
                    if (metrics.InvocationCountsByStatus.ContainsKey("Success"))
                    {
                        metrics.InvocationCountsByStatus["Success"]++;
                    }
                    else
                    {
                        metrics.InvocationCountsByStatus["Success"] = 1;
                    }

                    // Update last updated timestamp
                    metrics.LastUpdated = DateTime.UtcNow;

                    return await _metricsRepository.UpdateAsync(metrics);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metrics for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetupMonitoringAsync(Guid functionId, FunctionMonitoringSettings settings)
        {
            _logger.LogInformation("Setting up monitoring for function: {FunctionId}", functionId);

            try
            {
                // Check if the function exists
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Set default values
                settings.FunctionId = functionId;
                settings.CreatedAt = DateTime.UtcNow;
                settings.UpdatedAt = DateTime.UtcNow;

                // Save the settings
                await _monitoringSettingsRepository.CreateAsync(settings);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up monitoring for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMonitoringSettings> GetMonitoringSettingsAsync(Guid functionId)
        {
            _logger.LogInformation("Getting monitoring settings for function: {FunctionId}", functionId);

            try
            {
                return await _monitoringSettingsRepository.GetByFunctionIdAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monitoring settings for function: {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionMonitoringSettings> UpdateMonitoringSettingsAsync(Guid functionId, FunctionMonitoringSettings settings)
        {
            _logger.LogInformation("Updating monitoring settings for function: {FunctionId}", functionId);

            try
            {
                // Check if the function exists
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Get the existing settings
                var existingSettings = await _monitoringSettingsRepository.GetByFunctionIdAsync(functionId);
                if (existingSettings == null)
                {
                    throw new Exception($"Monitoring settings not found for function: {functionId}");
                }

                // Update the settings
                settings.FunctionId = functionId;
                settings.CreatedAt = existingSettings.CreatedAt;
                settings.UpdatedAt = DateTime.UtcNow;

                return await _monitoringSettingsRepository.UpdateAsync(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating monitoring settings for function: {FunctionId}", functionId);
                throw;
            }
        }

        private async Task UpdateFunctionStatisticsAsync(Guid functionId, double executionTime, double memoryUsage)
        {
            _logger.LogDebug("Updating function statistics for function: {FunctionId}", functionId);

            try
            {
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Update function statistics
                function.ExecutionCount++;
                function.LastExecutionTime = executionTime;

                // Update average execution time
                var totalExecutionTime = function.AverageExecutionTime * (function.ExecutionCount - 1) + executionTime;
                function.AverageExecutionTime = totalExecutionTime / function.ExecutionCount;

                // Update max memory usage
                function.MaxMemoryUsage = Math.Max(function.MaxMemoryUsage, memoryUsage);

                // Update last executed at
                function.LastExecutedAt = DateTime.UtcNow;

                await _functionRepository.UpdateAsync(function.Id, function);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating function statistics for function: {FunctionId}", functionId);
                throw;
            }
        }
    }
}
