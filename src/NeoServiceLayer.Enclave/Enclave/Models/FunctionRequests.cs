using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Enclave.Enclave.Models
{
    /// <summary>
    /// Request to delete a function
    /// </summary>
    public class DeleteFunctionRequest
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }
    }

    /// <summary>
    /// Request to get a storage value
    /// </summary>
    public class StorageRequest
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the key
        /// </summary>
        public string Key { get; set; }
    }

    /// <summary>
    /// Request to set a storage value
    /// </summary>
    public class StorageValueRequest
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Request to register a blockchain event
    /// </summary>
    public class RegisterBlockchainEventRequest
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the event type
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the contract hash
        /// </summary>
        public string ContractHash { get; set; }

        /// <summary>
        /// Gets or sets the event name
        /// </summary>
        public string EventName { get; set; }
    }

    /// <summary>
    /// Request to register a time event
    /// </summary>
    public class RegisterTimeEventRequest
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the schedule
        /// </summary>
        public string Schedule { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Request to trigger a custom event
    /// </summary>
    public class TriggerCustomEventRequest
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the event name
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the event data
        /// </summary>
        public object EventData { get; set; }
    }
}
