using System.Collections.Generic;

namespace NeoServiceLayer.Services.Storage.Configuration
{
    /// <summary>
    /// Configuration for MongoDB sharding
    /// </summary>
    public class MongoDbShardingConfiguration
    {
        /// <summary>
        /// Gets or sets whether sharding is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the MongoDB connection string for the config server
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the database name
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the collections to shard
        /// </summary>
        public List<ShardedCollection> ShardedCollections { get; set; } = new List<ShardedCollection>();

        /// <summary>
        /// Gets or sets the shards to add
        /// </summary>
        public List<string> Shards { get; set; } = new List<string>();
    }

    /// <summary>
    /// Configuration for a sharded collection
    /// </summary>
    public class ShardedCollection
    {
        /// <summary>
        /// Gets or sets the collection name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the shard key
        /// </summary>
        public string ShardKey { get; set; }
    }
}
