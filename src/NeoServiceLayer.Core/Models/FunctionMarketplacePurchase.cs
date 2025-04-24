using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a purchase of a function marketplace item
    /// </summary>
    public class FunctionMarketplacePurchase
    {
        /// <summary>
        /// Gets or sets the purchase ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Gets or sets the buyer ID
        /// </summary>
        public Guid BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the buyer name
        /// </summary>
        public string BuyerName { get; set; }

        /// <summary>
        /// Gets or sets the price paid
        /// </summary>
        public decimal PricePaid { get; set; }

        /// <summary>
        /// Gets or sets the currency of the price
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Gets or sets the purchased at timestamp
        /// </summary>
        public DateTime PurchasedAt { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the payment method
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the purchase is refunded
        /// </summary>
        public bool IsRefunded { get; set; } = false;

        /// <summary>
        /// Gets or sets the refunded at timestamp
        /// </summary>
        public DateTime? RefundedAt { get; set; }

        /// <summary>
        /// Gets or sets the refund reason
        /// </summary>
        public string RefundReason { get; set; }

        /// <summary>
        /// Gets or sets the refund transaction ID
        /// </summary>
        public string RefundTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the license key
        /// </summary>
        public string LicenseKey { get; set; }

        /// <summary>
        /// Gets or sets the license expiration date
        /// </summary>
        public DateTime? LicenseExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the license is perpetual
        /// </summary>
        public bool IsPerpetualLicense { get; set; } = true;

        /// <summary>
        /// Gets or sets the version of the item that was purchased
        /// </summary>
        public string ItemVersion { get; set; }
    }
}
