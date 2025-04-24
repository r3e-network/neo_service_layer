using System.Collections.Generic;

namespace NeoServiceLayer.Services.Storage.Configuration
{
    /// <summary>
    /// Configuration for a storage provider
    /// </summary>
    public class StorageProviderConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the storage provider
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the storage provider
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the base path for file-based storage providers
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the options for the storage provider
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
    }
}
