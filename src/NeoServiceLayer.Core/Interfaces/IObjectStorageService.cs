using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for object storage service
    /// </summary>
    public interface IObjectStorageService
    {
        /// <summary>
        /// Gets an object from a container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="objectName">Object name</param>
        /// <returns>Object content</returns>
        Task<string> GetObjectAsync(string containerName, string objectName);

        /// <summary>
        /// Puts an object in a container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="objectName">Object name</param>
        /// <param name="content">Object content</param>
        /// <returns>True if the object was stored successfully, false otherwise</returns>
        Task<bool> PutObjectAsync(string containerName, string objectName, string content);

        /// <summary>
        /// Deletes an object from a container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="objectName">Object name</param>
        /// <returns>True if the object was deleted successfully, false otherwise</returns>
        Task<bool> DeleteObjectAsync(string containerName, string objectName);

        /// <summary>
        /// Lists objects in a container
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <returns>List of object names</returns>
        Task<IEnumerable<string>> ListObjectsAsync(string containerName);
    }
}
