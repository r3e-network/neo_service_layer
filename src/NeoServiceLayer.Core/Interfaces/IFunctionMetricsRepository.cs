using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function metrics repository
    /// </summary>
    public interface IFunctionMetricsRepository
    {
        /// <summary>
        /// Creates a new function metrics record
        /// </summary>
        /// <param name="metrics">Function metrics to create</param>
        /// <returns>The created function metrics</returns>
        Task<FunctionMetrics> CreateAsync(FunctionMetrics metrics);

        /// <summary>
        /// Updates a function metrics record
        /// </summary>
        /// <param name="metrics">Function metrics to update</param>
        /// <returns>The updated function metrics</returns>
        Task<FunctionMetrics> UpdateAsync(FunctionMetrics metrics);

        /// <summary>
        /// Gets function metrics by function ID and time range
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>The function metrics if found, null otherwise</returns>
        Task<FunctionMetrics> GetByFunctionIdAsync(Guid functionId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets function metrics by account ID and time range
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>List of function metrics</returns>
        Task<IEnumerable<FunctionMetrics>> GetByAccountIdAsync(Guid accountId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets function metrics by time period
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="timePeriod">Time period for the metrics (e.g., "daily", "weekly", "monthly")</param>
        /// <returns>List of function metrics</returns>
        Task<IEnumerable<FunctionMetrics>> GetByTimePeriodAsync(Guid functionId, string timePeriod);

        /// <summary>
        /// Deletes function metrics by function ID and time range
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>True if the metrics were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Deletes function metrics by account ID and time range
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="startTime">Start time for the metrics</param>
        /// <param name="endTime">End time for the metrics</param>
        /// <returns>True if the metrics were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByAccountIdAsync(Guid accountId, DateTime startTime, DateTime endTime);
    }
}
