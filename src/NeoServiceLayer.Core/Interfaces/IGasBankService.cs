using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for the GasBank service
    /// </summary>
    public interface IGasBankService
    {
        /// <summary>
        /// Creates a new GasBank account
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="name">The name of the GasBank account</param>
        /// <param name="initialDeposit">The initial deposit amount (optional)</param>
        /// <returns>The created GasBank account</returns>
        Task<GasBankAccount> CreateAccountAsync(Guid accountId, string name, decimal initialDeposit = 0);

        /// <summary>
        /// Gets a GasBank account by ID
        /// </summary>
        /// <param name="id">The GasBank account ID</param>
        /// <returns>The GasBank account</returns>
        Task<GasBankAccount> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all GasBank accounts for an account
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <returns>The GasBank accounts</returns>
        Task<IEnumerable<GasBankAccount>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Deposits GAS into a GasBank account
        /// </summary>
        /// <param name="id">The GasBank account ID</param>
        /// <param name="amount">The amount to deposit</param>
        /// <returns>The updated GasBank account</returns>
        Task<GasBankAccount> DepositAsync(Guid id, decimal amount);

        /// <summary>
        /// Withdraws GAS from a GasBank account
        /// </summary>
        /// <param name="id">The GasBank account ID</param>
        /// <param name="amount">The amount to withdraw</param>
        /// <param name="toAddress">The Neo address to withdraw to</param>
        /// <returns>The transaction hash</returns>
        Task<string> WithdrawAsync(Guid id, decimal amount, string toAddress);

        /// <summary>
        /// Allocates GAS from a GasBank account to a function
        /// </summary>
        /// <param name="id">The GasBank account ID</param>
        /// <param name="functionId">The function ID</param>
        /// <param name="amount">The amount to allocate</param>
        /// <returns>The created GasBank allocation</returns>
        Task<GasBankAllocation> AllocateToFunctionAsync(Guid id, Guid functionId, decimal amount);

        /// <summary>
        /// Gets all allocations for a GasBank account
        /// </summary>
        /// <param name="id">The GasBank account ID</param>
        /// <returns>The GasBank allocations</returns>
        Task<IEnumerable<GasBankAllocation>> GetAllocationsAsync(Guid id);

        /// <summary>
        /// Updates a GasBank allocation
        /// </summary>
        /// <param name="id">The GasBank allocation ID</param>
        /// <param name="amount">The new allocation amount</param>
        /// <returns>The updated GasBank allocation</returns>
        Task<GasBankAllocation> UpdateAllocationAsync(Guid id, decimal amount);

        /// <summary>
        /// Removes a GasBank allocation
        /// </summary>
        /// <param name="id">The GasBank allocation ID</param>
        /// <returns>True if successful</returns>
        Task<bool> RemoveAllocationAsync(Guid id);

        /// <summary>
        /// Gets the transaction history for a GasBank account
        /// </summary>
        /// <param name="id">The GasBank account ID</param>
        /// <param name="startTime">The start time</param>
        /// <param name="endTime">The end time</param>
        /// <returns>The transaction history</returns>
        Task<IEnumerable<GasBankTransaction>> GetTransactionHistoryAsync(Guid id, DateTime startTime, DateTime endTime);
    }
}
