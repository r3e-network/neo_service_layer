using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an event in a blockchain block
    /// </summary>
    public class BlockEvent
    {
        /// <summary>
        /// Gets or sets the transaction hash that contained the event
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Gets or sets the block hash that contained the event
        /// </summary>
        public string BlockHash { get; set; }

        /// <summary>
        /// Gets or sets the block height that contained the event
        /// </summary>
        public long BlockHeight { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the block that contained the event
        /// </summary>
        public DateTime BlockTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the contract hash that emitted the event
        /// </summary>
        public string ContractHash { get; set; }

        /// <summary>
        /// Gets or sets the event name
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the event data
        /// </summary>
        public Dictionary<string, object> EventData { get; set; } = new Dictionary<string, object>();
    }
}
