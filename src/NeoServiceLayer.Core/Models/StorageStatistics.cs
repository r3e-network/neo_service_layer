using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Statistics for storage service
    /// </summary>
    public class StorageStatistics
    {
        /// <summary>
        /// Gets or sets the total storage size in bytes
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of files
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Gets or sets the total number of key-value pairs
        /// </summary>
        public int TotalKeyValues { get; set; }

        /// <summary>
        /// Gets or sets the storage usage by account
        /// </summary>
        public Dictionary<Guid, long> UsageByAccount { get; set; } = new Dictionary<Guid, long>();

        /// <summary>
        /// Gets or sets the storage usage by function
        /// </summary>
        public Dictionary<Guid, long> UsageByFunction { get; set; } = new Dictionary<Guid, long>();

        /// <summary>
        /// Gets or sets the last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }


}
