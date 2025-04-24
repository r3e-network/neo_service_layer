using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Configuration for storage service
    /// </summary>
    public class StorageConfiguration
    {
        /// <summary>
        /// Gets or sets the default provider name
        /// </summary>
        public string DefaultProvider { get; set; }

        /// <summary>
        /// Gets or sets the providers configuration
        /// </summary>
        public List<StorageProviderConfiguration> Providers { get; set; } = new List<StorageProviderConfiguration>();
    }

    /// <summary>
    /// Configuration for a storage provider
    /// </summary>
    public class StorageProviderConfiguration
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the provider type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the container name (for blob storage)
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// Gets or sets the base path (for file storage)
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the maximum file size in bytes (for file storage)
        /// </summary>
        public long MaxFileSize { get; set; } = 104857600; // 100 MB

        /// <summary>
        /// Gets or sets the maximum storage size in bytes per account
        /// </summary>
        public long MaxStorageSize { get; set; } = 1073741824; // 1 GB

        /// <summary>
        /// Gets or sets additional options
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
    }
}
