using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Statistics for a database
    /// </summary>
    public class DatabaseStatistics
    {
        /// <summary>
        /// Gets or sets the last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the total number of collections
        /// </summary>
        public int TotalCollections { get; set; }

        /// <summary>
        /// Gets or sets the total number of entities
        /// </summary>
        public int TotalEntities { get; set; }

        /// <summary>
        /// Gets or sets the total size in bytes
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Gets or sets the number of entities by collection
        /// </summary>
        public Dictionary<string, int> EntitiesByCollection { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets or sets the size in bytes by collection
        /// </summary>
        public Dictionary<string, long> SizeByCollection { get; set; } = new Dictionary<string, long>();
    }
}
