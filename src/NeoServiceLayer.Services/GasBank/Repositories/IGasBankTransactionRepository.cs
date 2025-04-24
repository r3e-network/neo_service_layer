using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.GasBank.Repositories
{
    /// <summary>
    /// Interface for the GasBank transaction repository
    /// </summary>
    public interface IGasBankTransactionRepository
    {
        /// <summary>
        /// Creates a new GasBank transaction
        /// </summary>
        /// <param name="transaction">The GasBank transaction to create</param>
        /// <returns>The created GasBank transaction</returns>
        Task<GasBankTransaction> CreateAsync(GasBankTransaction transaction);

        /// <summary>
        /// Gets a GasBank transaction by ID
        /// </summary>
        /// <param name="id">The GasBank transaction ID</param>
        /// <returns>The GasBank transaction</returns>
        Task<GasBankTransaction> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all GasBank transactions for a GasBank account
        /// </summary>
        /// <param name="gasBankAccountId">The GasBank account ID</param>
        /// <returns>The GasBank transactions</returns>
        Task<IEnumerable<GasBankTransaction>> GetByGasBankAccountIdAsync(Guid gasBankAccountId);

        /// <summary>
        /// Gets all GasBank transactions for a GasBank account within a time range
        /// </summary>
        /// <param name="gasBankAccountId">The GasBank account ID</param>
        /// <param name="startTime">The start time</param>
        /// <param name="endTime">The end time</param>
        /// <returns>The GasBank transactions</returns>
        Task<IEnumerable<GasBankTransaction>> GetByGasBankAccountIdAndTimeRangeAsync(Guid gasBankAccountId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets all GasBank transactions for a function
        /// </summary>
        /// <param name="functionId">The function ID</param>
        /// <returns>The GasBank transactions</returns>
        Task<IEnumerable<GasBankTransaction>> GetByFunctionIdAsync(Guid functionId);
    }
}
