using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Enclave.Enclave.Models
{
    /// <summary>
    /// Event data
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Gets or sets the event ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the event type
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the event name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the event source
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the event data
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the event timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the transaction hash
        /// </summary>
        public string TransactionHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the block number
        /// </summary>
        public long BlockNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract hash
        /// </summary>
        public string ContractHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the function IDs to trigger
        /// </summary>
        public List<Guid> TriggerFunctionIds { get; set; } = new List<Guid>();
    }
}
