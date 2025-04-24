using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an event in the Neo Service Layer
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Unique identifier for the event
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the event
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the event (Blockchain, Time, Custom)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Source of the event
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Data associated with the event
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        /// <summary>
        /// Timestamp when the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Transaction hash if the event is from the blockchain
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Block index if the event is from the blockchain
        /// </summary>
        public long? BlockIndex { get; set; }

        /// <summary>
        /// Contract hash if the event is from the blockchain
        /// </summary>
        public string ContractHash { get; set; }

        /// <summary>
        /// Function IDs that should be triggered by this event
        /// </summary>
        public List<Guid> TriggerFunctionIds { get; set; }

        /// <summary>
        /// Status of the event processing (Pending, Processing, Completed, Failed)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Date and time when the event was processed
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Error message if the event processing failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
