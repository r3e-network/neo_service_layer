using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function monitoring settings repository
    /// </summary>
    public interface IFunctionMonitoringSettingsRepository
    {
        /// <summary>
        /// Creates a new function monitoring settings record
        /// </summary>
        /// <param name="settings">Function monitoring settings to create</param>
        /// <returns>The created function monitoring settings</returns>
        Task<FunctionMonitoringSettings> CreateAsync(FunctionMonitoringSettings settings);

        /// <summary>
        /// Updates a function monitoring settings record
        /// </summary>
        /// <param name="settings">Function monitoring settings to update</param>
        /// <returns>The updated function monitoring settings</returns>
        Task<FunctionMonitoringSettings> UpdateAsync(FunctionMonitoringSettings settings);

        /// <summary>
        /// Gets function monitoring settings by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>The function monitoring settings if found, null otherwise</returns>
        Task<FunctionMonitoringSettings> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function monitoring settings by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of function monitoring settings</returns>
        Task<IEnumerable<FunctionMonitoringSettings>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Deletes function monitoring settings by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the settings were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);
    }
}
