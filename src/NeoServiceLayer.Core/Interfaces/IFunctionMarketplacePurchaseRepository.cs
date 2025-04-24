using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function marketplace purchase repository
    /// </summary>
    public interface IFunctionMarketplacePurchaseRepository
    {
        /// <summary>
        /// Creates a new function marketplace purchase
        /// </summary>
        /// <param name="purchase">Function marketplace purchase to create</param>
        /// <returns>The created function marketplace purchase</returns>
        Task<FunctionMarketplacePurchase> CreateAsync(FunctionMarketplacePurchase purchase);

        /// <summary>
        /// Updates a function marketplace purchase
        /// </summary>
        /// <param name="purchase">Function marketplace purchase to update</param>
        /// <returns>The updated function marketplace purchase</returns>
        Task<FunctionMarketplacePurchase> UpdateAsync(FunctionMarketplacePurchase purchase);

        /// <summary>
        /// Gets a function marketplace purchase by ID
        /// </summary>
        /// <param name="id">Purchase ID</param>
        /// <returns>The function marketplace purchase if found, null otherwise</returns>
        Task<FunctionMarketplacePurchase> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function marketplace purchases by item ID
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace purchases</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByItemIdAsync(Guid itemId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace purchases by buyer ID
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace purchases</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByBuyerIdAsync(Guid buyerId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace purchases by transaction ID
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <returns>The function marketplace purchase if found, null otherwise</returns>
        Task<FunctionMarketplacePurchase> GetByTransactionIdAsync(string transactionId);

        /// <summary>
        /// Gets function marketplace purchases by license key
        /// </summary>
        /// <param name="licenseKey">License key</param>
        /// <returns>The function marketplace purchase if found, null otherwise</returns>
        Task<FunctionMarketplacePurchase> GetByLicenseKeyAsync(string licenseKey);

        /// <summary>
        /// Gets function marketplace purchases by refund status
        /// </summary>
        /// <param name="isRefunded">Refund status</param>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace purchases</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByRefundStatusAsync(bool isRefunded, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace purchases by payment status
        /// </summary>
        /// <param name="paymentStatus">Payment status</param>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace purchases</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByPaymentStatusAsync(string paymentStatus, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace purchases by item ID and buyer ID
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of function marketplace purchases</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByItemIdAndBuyerIdAsync(Guid itemId, Guid buyerId);

        /// <summary>
        /// Gets the purchase count for an item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The purchase count</returns>
        Task<int> GetPurchaseCountAsync(Guid itemId);

        /// <summary>
        /// Gets the total revenue for an item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The total revenue</returns>
        Task<decimal> GetTotalRevenueAsync(Guid itemId);

        /// <summary>
        /// Deletes a function marketplace purchase
        /// </summary>
        /// <param name="id">Purchase ID</param>
        /// <returns>True if the purchase was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function marketplace purchases by item ID
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>True if the purchases were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByItemIdAsync(Guid itemId);

        /// <summary>
        /// Deletes function marketplace purchases by buyer ID
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>True if the purchases were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByBuyerIdAsync(Guid buyerId);
    }
}
