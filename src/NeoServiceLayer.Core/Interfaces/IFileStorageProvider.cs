using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for file storage providers
    /// </summary>
    public interface IFileStorageProvider
    {
        /// <summary>
        /// Stores a file
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="content">File content</param>
        /// <param name="metadata">File metadata</param>
        /// <returns>True if the file was stored successfully, false otherwise</returns>
        Task<bool> StoreAsync(string path, Stream content, Dictionary<string, string> metadata = null);

        /// <summary>
        /// Retrieves a file
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>File content</returns>
        Task<Stream> RetrieveAsync(string path);

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>True if the file was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(string path);

        /// <summary>
        /// Lists files in a directory
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <returns>List of file names</returns>
        Task<IEnumerable<string>> ListAsync(string path);

        /// <summary>
        /// Gets the URL for a file
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="expirationMinutes">URL expiration in minutes</param>
        /// <returns>File URL</returns>
        Task<string> GetUrlAsync(string path, int expirationMinutes = 60);

        /// <summary>
        /// Gets file metadata
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>File metadata</returns>
        Task<Dictionary<string, string>> GetMetadataAsync(string path);

        /// <summary>
        /// Updates file metadata
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="metadata">File metadata</param>
        /// <returns>True if the metadata was updated successfully, false otherwise</returns>
        Task<bool> UpdateMetadataAsync(string path, Dictionary<string, string> metadata);

        /// <summary>
        /// Checks if a file exists
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>True if the file exists, false otherwise</returns>
        Task<bool> ExistsAsync(string path);

        /// <summary>
        /// Gets the size of a file
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>File size in bytes</returns>
        Task<long> GetSizeAsync(string path);

        /// <summary>
        /// Copies a file
        /// </summary>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="destinationPath">Destination file path</param>
        /// <returns>True if the file was copied successfully, false otherwise</returns>
        Task<bool> CopyAsync(string sourcePath, string destinationPath);

        /// <summary>
        /// Moves a file
        /// </summary>
        /// <param name="sourcePath">Source file path</param>
        /// <param name="destinationPath">Destination file path</param>
        /// <returns>True if the file was moved successfully, false otherwise</returns>
        Task<bool> MoveAsync(string sourcePath, string destinationPath);
    }
}
