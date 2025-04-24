using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function marketplace purchase repository
    /// </summary>
    public interface IFunctionMarketplacePurchaseRepository
    {
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
        /// <returns>List of function marketplace purchases for the specified item</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByItemIdAsync(Guid itemId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace purchases by buyer ID
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace purchases by the specified buyer</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByBuyerIdAsync(Guid buyerId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets a function marketplace purchase by transaction ID
        /// </summary>
        /// <param name="transactionId">Transaction ID</param>
        /// <returns>The function marketplace purchase with the specified transaction ID</returns>
        Task<FunctionMarketplacePurchase> GetByTransactionIdAsync(string transactionId);

        /// <summary>
        /// Gets a function marketplace purchase by license key
        /// </summary>
        /// <param name="licenseKey">License key</param>
        /// <returns>The function marketplace purchase with the specified license key</returns>
        Task<FunctionMarketplacePurchase> GetByLicenseKeyAsync(string licenseKey);

        /// <summary>
        /// Gets function marketplace purchases by refund status
        /// </summary>
        /// <param name="isRefunded">Refund status</param>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace purchases with the specified refund status</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByRefundStatusAsync(bool isRefunded, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace purchases by payment status
        /// </summary>
        /// <param name="paymentStatus">Payment status</param>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function marketplace purchases with the specified payment status</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByPaymentStatusAsync(string paymentStatus, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function marketplace purchases by item ID and buyer ID
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of function marketplace purchases for the specified item by the specified buyer</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetByItemIdAndBuyerIdAsync(Guid itemId, Guid buyerId);

        /// <summary>
        /// Gets the purchase count for a function marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The number of purchases for the specified item</returns>
        Task<int> GetPurchaseCountAsync(Guid itemId);

        /// <summary>
        /// Gets the total revenue for a function marketplace item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>The total revenue for the specified item</returns>
        Task<decimal> GetTotalRevenueAsync(Guid itemId);

        /// <summary>
        /// Creates a function marketplace purchase
        /// </summary>
        /// <param name="purchase">Purchase to create</param>
        /// <returns>The created function marketplace purchase</returns>
        Task<FunctionMarketplacePurchase> CreateAsync(FunctionMarketplacePurchase purchase);

        /// <summary>
        /// Updates a function marketplace purchase
        /// </summary>
        /// <param name="purchase">Updated purchase</param>
        /// <returns>The updated function marketplace purchase</returns>
        Task<FunctionMarketplacePurchase> UpdateAsync(FunctionMarketplacePurchase purchase);

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

        /// <summary>
        /// Gets all function marketplace purchases
        /// </summary>
        /// <param name="limit">Maximum number of purchases to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function marketplace purchases</returns>
        Task<IEnumerable<FunctionMarketplacePurchase>> GetAllAsync(int limit = 10, int offset = 0);
    }
}
