using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Storage.Providers
{
    /// <summary>
    /// Interface for file storage providers
    /// </summary>
    public interface IFileStorageProvider
    {
        /// <summary>
        /// Stores data
        /// </summary>
        /// <param name="path">Path to store the data</param>
        /// <param name="content">Content to store</param>
        /// <param name="metadata">Metadata for the content</param>
        /// <returns>Task</returns>
        Task StoreAsync(string path, Stream content, Dictionary<string, string> metadata = null);

        /// <summary>
        /// Retrieves data
        /// </summary>
        /// <param name="path">Path to retrieve the data from</param>
        /// <returns>Content stream</returns>
        Task<Stream> RetrieveAsync(string path);

        /// <summary>
        /// Deletes data
        /// </summary>
        /// <param name="path">Path to delete the data from</param>
        /// <returns>True if the data was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(string path);

        /// <summary>
        /// Lists data
        /// </summary>
        /// <param name="prefix">Prefix to filter the data</param>
        /// <returns>List of paths</returns>
        Task<IEnumerable<string>> ListAsync(string prefix);

        /// <summary>
        /// Gets the URL for a path
        /// </summary>
        /// <param name="path">Path to get the URL for</param>
        /// <returns>URL</returns>
        Task<string> GetUrlAsync(string path);

        /// <summary>
        /// Gets the storage usage for a prefix
        /// </summary>
        /// <param name="prefix">Prefix to get the storage usage for</param>
        /// <returns>Storage usage in bytes</returns>
        Task<long> GetStorageUsageAsync(string prefix);
    }

    /// <summary>
    /// Extended storage provider interface that combines database and file storage capabilities
    /// </summary>
    public interface IExtendedStorageProvider : IStorageProvider, IFileStorageProvider
    {
    }
}
