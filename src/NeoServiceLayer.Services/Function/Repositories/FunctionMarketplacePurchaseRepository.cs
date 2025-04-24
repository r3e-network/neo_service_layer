using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Repository for function marketplace purchases
    /// </summary>
    public class FunctionMarketplacePurchaseRepository : IFunctionMarketplacePurchaseRepository
    {
        private readonly ILogger<FunctionMarketplacePurchaseRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_marketplace_purchases";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionMarketplacePurchaseRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionMarketplacePurchaseRepository(ILogger<FunctionMarketplacePurchaseRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> CreateAsync(FunctionMarketplacePurchase purchase)
        {
            _logger.LogInformation("Creating function marketplace purchase for item {ItemId} by buyer {BuyerId}", purchase.ItemId, purchase.BuyerId);

            // Ensure ID is set
            if (purchase.Id == Guid.Empty)
            {
                purchase.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, purchase);

            return purchase;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> UpdateAsync(FunctionMarketplacePurchase purchase)
        {
            _logger.LogInformation("Updating function marketplace purchase: {Id}", purchase.Id);

            // Update in store
            await _storageProvider.UpdateAsync<FunctionMarketplacePurchase, Guid>(_collectionName, purchase.Id, purchase);

            return purchase;
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function marketplace purchase by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionMarketplacePurchase, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplacePurchase>> GetByItemIdAsync(Guid itemId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace purchases by item ID: {ItemId}, limit: {Limit}, offset: {Offset}", itemId, limit, offset);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by item ID and sort by purchased date
            return purchases
                .Where(p => p.ItemId == itemId)
                .OrderByDescending(p => p.PurchasedAt)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplacePurchase>> GetByBuyerIdAsync(Guid buyerId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace purchases by buyer ID: {BuyerId}, limit: {Limit}, offset: {Offset}", buyerId, limit, offset);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by buyer ID and sort by purchased date
            return purchases
                .Where(p => p.BuyerId == buyerId)
                .OrderByDescending(p => p.PurchasedAt)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> GetByTransactionIdAsync(string transactionId)
        {
            _logger.LogInformation("Getting function marketplace purchase by transaction ID: {TransactionId}", transactionId);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by transaction ID
            return purchases.FirstOrDefault(p => p.TransactionId == transactionId);
        }

        /// <inheritdoc/>
        public async Task<FunctionMarketplacePurchase> GetByLicenseKeyAsync(string licenseKey)
        {
            _logger.LogInformation("Getting function marketplace purchase by license key: {LicenseKey}", licenseKey);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by license key
            return purchases.FirstOrDefault(p => p.LicenseKey == licenseKey);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplacePurchase>> GetByRefundStatusAsync(bool isRefunded, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace purchases by refund status: {IsRefunded}, limit: {Limit}, offset: {Offset}", isRefunded, limit, offset);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by refund status and sort by purchased date
            return purchases
                .Where(p => p.IsRefunded == isRefunded)
                .OrderByDescending(p => p.PurchasedAt)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplacePurchase>> GetByPaymentStatusAsync(string paymentStatus, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function marketplace purchases by payment status: {PaymentStatus}, limit: {Limit}, offset: {Offset}", paymentStatus, limit, offset);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by payment status and sort by purchased date
            return purchases
                .Where(p => p.PaymentStatus == paymentStatus)
                .OrderByDescending(p => p.PurchasedAt)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplacePurchase>> GetByItemIdAndBuyerIdAsync(Guid itemId, Guid buyerId)
        {
            _logger.LogInformation("Getting function marketplace purchases by item ID: {ItemId} and buyer ID: {BuyerId}", itemId, buyerId);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by item ID and buyer ID
            return purchases.Where(p => p.ItemId == itemId && p.BuyerId == buyerId);
        }

        /// <inheritdoc/>
        public async Task<int> GetPurchaseCountAsync(Guid itemId)
        {
            _logger.LogInformation("Getting purchase count for function marketplace item: {ItemId}", itemId);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by item ID and not refunded
            var filteredPurchases = purchases.Where(p => p.ItemId == itemId && !p.IsRefunded);

            // Return count
            return filteredPurchases.Count();
        }

        /// <inheritdoc/>
        public async Task<decimal> GetTotalRevenueAsync(Guid itemId)
        {
            _logger.LogInformation("Getting total revenue for function marketplace item: {ItemId}", itemId);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by item ID and not refunded
            var filteredPurchases = purchases.Where(p => p.ItemId == itemId && !p.IsRefunded);

            // Calculate total revenue
            return filteredPurchases.Sum(p => p.PricePaid);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function marketplace purchase: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionMarketplacePurchase, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByItemIdAsync(Guid itemId)
        {
            _logger.LogInformation("Deleting function marketplace purchases by item ID: {ItemId}", itemId);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by item ID
            var filteredPurchases = purchases.Where(p => p.ItemId == itemId);

            // Delete each purchase
            var success = true;
            foreach (var purchase in filteredPurchases)
            {
                var result = await DeleteAsync(purchase.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByBuyerIdAsync(Guid buyerId)
        {
            _logger.LogInformation("Deleting function marketplace purchases by buyer ID: {BuyerId}", buyerId);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Filter by buyer ID
            var filteredPurchases = purchases.Where(p => p.BuyerId == buyerId);

            // Delete each purchase
            var success = true;
            foreach (var purchase in filteredPurchases)
            {
                var result = await DeleteAsync(purchase.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionMarketplacePurchase>> GetAllAsync(int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting all function marketplace purchases, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all purchases
            var purchases = await _storageProvider.GetAllAsync<FunctionMarketplacePurchase>(_collectionName);

            // Sort by purchased date and apply pagination
            return purchases
                .OrderByDescending(p => p.PurchasedAt)
                .Skip(offset)
                .Take(limit);
        }
    }
}
