using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.GasBank.Repositories
{
    /// <summary>
    /// Interface for the GasBank allocation repository
    /// </summary>
    public interface IGasBankAllocationRepository
    {
        /// <summary>
        /// Creates a new GasBank allocation
        /// </summary>
        /// <param name="allocation">The GasBank allocation to create</param>
        /// <returns>The created GasBank allocation</returns>
        Task<GasBankAllocation> CreateAsync(GasBankAllocation allocation);

        /// <summary>
        /// Gets a GasBank allocation by ID
        /// </summary>
        /// <param name="id">The GasBank allocation ID</param>
        /// <returns>The GasBank allocation</returns>
        Task<GasBankAllocation> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all GasBank allocations for a GasBank account
        /// </summary>
        /// <param name="gasBankAccountId">The GasBank account ID</param>
        /// <returns>The GasBank allocations</returns>
        Task<IEnumerable<GasBankAllocation>> GetByGasBankAccountIdAsync(Guid gasBankAccountId);

        /// <summary>
        /// Gets all GasBank allocations for a function
        /// </summary>
        /// <param name="functionId">The function ID</param>
        /// <returns>The GasBank allocations</returns>
        Task<IEnumerable<GasBankAllocation>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Updates a GasBank allocation
        /// </summary>
        /// <param name="allocation">The GasBank allocation to update</param>
        /// <returns>The updated GasBank allocation</returns>
        Task<GasBankAllocation> UpdateAsync(GasBankAllocation allocation);

        /// <summary>
        /// Deletes a GasBank allocation
        /// </summary>
        /// <param name="id">The GasBank allocation ID</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
