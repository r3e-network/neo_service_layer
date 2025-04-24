using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a GasBank transaction
    /// </summary>
    public class GasBankTransaction
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the GasBank account ID
        /// </summary>
        public Guid GasBankAccountId { get; set; }

        /// <summary>
        /// Gets or sets the transaction type
        /// </summary>
        public GasBankTransactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the balance after transaction
        /// </summary>
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// Gets or sets the transaction hash
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Gets or sets the related entity ID (e.g., function ID for allocations)
        /// </summary>
        public Guid? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the Neo address (for withdrawals)
        /// </summary>
        public string NeoAddress { get; set; }

        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Represents the type of GasBank transaction
    /// </summary>
    public enum GasBankTransactionType
    {
        /// <summary>
        /// Deposit
        /// </summary>
        Deposit,

        /// <summary>
        /// Withdrawal
        /// </summary>
        Withdrawal,

        /// <summary>
        /// Allocation
        /// </summary>
        Allocation,

        /// <summary>
        /// Deallocation
        /// </summary>
        Deallocation,

        /// <summary>
        /// Function execution
        /// </summary>
        FunctionExecution
    }
}
