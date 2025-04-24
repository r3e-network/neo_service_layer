using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for file storage service
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Stores a file
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID (optional)</param>
        /// <param name="fileName">File name</param>
        /// <param name="content">File content</param>
        /// <param name="contentType">Content type</param>
        /// <param name="isPublic">Whether the file is public</param>
        /// <returns>True if the file was stored successfully, false otherwise</returns>
        Task<bool> StoreFileAsync(Guid accountId, Guid? functionId, string fileName, Stream content, string contentType, bool isPublic);

        /// <summary>
        /// Retrieves a file
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID (optional)</param>
        /// <param name="fileName">File name</param>
        /// <returns>File content</returns>
        Task<Stream> RetrieveFileAsync(Guid accountId, Guid? functionId, string fileName);

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID (optional)</param>
        /// <param name="fileName">File name</param>
        /// <returns>True if the file was deleted successfully, false otherwise</returns>
        Task<bool> DeleteFileAsync(Guid accountId, Guid? functionId, string fileName);

        /// <summary>
        /// Lists files for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of file names</returns>
        Task<IEnumerable<string>> ListFilesAsync(Guid accountId);

        /// <summary>
        /// Lists files for a function
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of file names</returns>
        Task<IEnumerable<string>> ListFunctionFilesAsync(Guid accountId, Guid functionId);

        /// <summary>
        /// Gets the URL for a file
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID (optional)</param>
        /// <param name="fileName">File name</param>
        /// <returns>File URL</returns>
        Task<string> GetFileUrlAsync(Guid accountId, Guid? functionId, string fileName);

        /// <summary>
        /// Stores a key-value pair
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID (optional)</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if the key-value pair was stored successfully, false otherwise</returns>
        Task<bool> StoreKeyValueAsync(Guid accountId, Guid? functionId, string key, string value);

        /// <summary>
        /// Retrieves a key-value pair
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID (optional)</param>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        Task<string> RetrieveKeyValueAsync(Guid accountId, Guid? functionId, string key);

        /// <summary>
        /// Deletes a key-value pair
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID (optional)</param>
        /// <param name="key">Key</param>
        /// <returns>True if the key-value pair was deleted successfully, false otherwise</returns>
        Task<bool> DeleteKeyValueAsync(Guid accountId, Guid? functionId, string key);

        /// <summary>
        /// Lists keys for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of keys</returns>
        Task<IEnumerable<string>> ListKeysAsync(Guid accountId);

        /// <summary>
        /// Lists keys for a function
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of keys</returns>
        Task<IEnumerable<string>> ListKeysAsync(Guid accountId, Guid functionId);

        /// <summary>
        /// Gets the storage usage for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Storage usage in bytes</returns>
        Task<long> GetStorageUsageAsync(Guid accountId);

        /// <summary>
        /// Gets the storage usage for a function
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID</param>
        /// <returns>Storage usage in bytes</returns>
        Task<long> GetStorageUsageAsync(Guid accountId, Guid functionId);
    }
}
