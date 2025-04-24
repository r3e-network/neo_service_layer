using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for store provider
    /// </summary>
    public interface IStoreProvider
    {
        /// <summary>
        /// Gets an object from the store
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="id">Object ID</param>
        /// <returns>The object if found, null otherwise</returns>
        Task<T?> GetAsync<T>(Guid id) where T : class;

        /// <summary>
        /// Gets objects from the store by a filter
        /// </summary>
        /// <typeparam name="T">Type of the objects</typeparam>
        /// <param name="filter">Filter function</param>
        /// <param name="limit">Maximum number of objects to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of objects matching the filter</returns>
        Task<IEnumerable<T>> GetByFilterAsync<T>(Func<T, bool> filter, int limit = 100, int offset = 0) where T : class;

        /// <summary>
        /// Creates an object in the store
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="obj">Object to create</param>
        /// <returns>The created object</returns>
        Task<T> CreateAsync<T>(T obj) where T : class;

        /// <summary>
        /// Updates an object in the store
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="id">Object ID</param>
        /// <param name="obj">Updated object</param>
        /// <returns>The updated object</returns>
        Task<T> UpdateAsync<T>(Guid id, T obj) where T : class;

        /// <summary>
        /// Deletes an object from the store
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="id">Object ID</param>
        /// <returns>True if the object was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync<T>(Guid id) where T : class;

        /// <summary>
        /// Checks if an object exists in the store
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="id">Object ID</param>
        /// <returns>True if the object exists, false otherwise</returns>
        Task<bool> ExistsAsync<T>(Guid id) where T : class;

        /// <summary>
        /// Gets all objects of a type from the store
        /// </summary>
        /// <typeparam name="T">Type of the objects</typeparam>
        /// <param name="limit">Maximum number of objects to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all objects of the specified type</returns>
        Task<IEnumerable<T>> GetAllAsync<T>(int limit = 100, int offset = 0) where T : class;

        /// <summary>
        /// Counts objects of a type in the store
        /// </summary>
        /// <typeparam name="T">Type of the objects</typeparam>
        /// <returns>Number of objects of the specified type</returns>
        Task<int> CountAsync<T>() where T : class;

        /// <summary>
        /// Counts objects of a type in the store by a filter
        /// </summary>
        /// <typeparam name="T">Type of the objects</typeparam>
        /// <param name="filter">Filter function</param>
        /// <returns>Number of objects matching the filter</returns>
        Task<int> CountByFilterAsync<T>(Func<T, bool> filter) where T : class;

        /// <summary>
        /// Uploads a file to the store
        /// </summary>
        /// <param name="path">Path to store the file</param>
        /// <param name="content">File content</param>
        /// <param name="metadata">File metadata</param>
        /// <returns>The URL of the uploaded file</returns>
        Task<string> UploadFileAsync(string path, Stream content, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Downloads a file from the store
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>The file content</returns>
        Task<Stream> DownloadFileAsync(string path);

        /// <summary>
        /// Deletes a file from the store
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>True if the file was deleted successfully, false otherwise</returns>
        Task<bool> DeleteFileAsync(string path);

        /// <summary>
        /// Checks if a file exists in the store
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>True if the file exists, false otherwise</returns>
        Task<bool> FileExistsAsync(string path);

        /// <summary>
        /// Gets the metadata of a file
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>The file metadata</returns>
        Task<Dictionary<string, string>> GetFileMetadataAsync(string path);

        /// <summary>
        /// Updates the metadata of a file
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <param name="metadata">New metadata</param>
        /// <returns>True if the metadata was updated successfully, false otherwise</returns>
        Task<bool> UpdateFileMetadataAsync(string path, Dictionary<string, string> metadata);

        /// <summary>
        /// Lists files in a directory
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="recursive">Whether to list files recursively</param>
        /// <returns>List of file paths</returns>
        Task<IEnumerable<string>> ListFilesAsync(string path, bool recursive = false);

        /// <summary>
        /// Creates a directory in the store
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <returns>True if the directory was created successfully, false otherwise</returns>
        Task<bool> CreateDirectoryAsync(string path);

        /// <summary>
        /// Deletes a directory from the store
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="recursive">Whether to delete the directory recursively</param>
        /// <returns>True if the directory was deleted successfully, false otherwise</returns>
        Task<bool> DeleteDirectoryAsync(string path, bool recursive = false);

        /// <summary>
        /// Checks if a directory exists in the store
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <returns>True if the directory exists, false otherwise</returns>
        Task<bool> DirectoryExistsAsync(string path);
    }
}
