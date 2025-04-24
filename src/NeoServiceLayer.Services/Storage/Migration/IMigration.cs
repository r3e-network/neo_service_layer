using System.Threading.Tasks;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Storage.Migration
{
    /// <summary>
    /// Interface for database migrations
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// Gets the migration name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the migration version
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Gets the migration description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Applies the migration
        /// </summary>
        /// <param name="provider">Storage provider</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpAsync(IStorageProvider provider);

        /// <summary>
        /// Reverts the migration
        /// </summary>
        /// <param name="provider">Storage provider</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task DownAsync(IStorageProvider provider);
    }
}
