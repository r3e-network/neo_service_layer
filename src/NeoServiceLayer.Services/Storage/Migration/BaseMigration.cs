using System;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Storage.Migration
{
    /// <summary>
    /// Base class for database migrations
    /// </summary>
    public abstract class BaseMigration : IMigration
    {
        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract int Version { get; }

        /// <inheritdoc/>
        public abstract string Description { get; }

        /// <inheritdoc/>
        public abstract Task UpAsync(IStorageProvider provider);

        /// <inheritdoc/>
        public abstract Task DownAsync(IStorageProvider provider);

        /// <summary>
        /// Creates a collection if it doesn't exist
        /// </summary>
        /// <param name="provider">Storage provider</param>
        /// <param name="collectionName">Collection name</param>
        /// <returns>Task representing the asynchronous operation</returns>
        protected async Task CreateCollectionIfNotExistsAsync(IStorageProvider provider, string collectionName)
        {
            if (!await provider.CollectionExistsAsync(collectionName))
            {
                await provider.CreateCollectionAsync(collectionName);
            }
        }

        /// <summary>
        /// Deletes a collection if it exists
        /// </summary>
        /// <param name="provider">Storage provider</param>
        /// <param name="collectionName">Collection name</param>
        /// <returns>Task representing the asynchronous operation</returns>
        protected async Task DeleteCollectionIfExistsAsync(IStorageProvider provider, string collectionName)
        {
            if (await provider.CollectionExistsAsync(collectionName))
            {
                await provider.DeleteCollectionAsync(collectionName);
            }
        }

        /// <summary>
        /// Renames a collection
        /// </summary>
        /// <param name="provider">Storage provider</param>
        /// <param name="oldName">Old collection name</param>
        /// <param name="newName">New collection name</param>
        /// <returns>Task representing the asynchronous operation</returns>
        protected async Task RenameCollectionAsync(IStorageProvider provider, string oldName, string newName)
        {
            if (!await provider.CollectionExistsAsync(oldName))
            {
                throw new InvalidOperationException($"Collection {oldName} does not exist");
            }

            if (await provider.CollectionExistsAsync(newName))
            {
                throw new InvalidOperationException($"Collection {newName} already exists");
            }

            // Get all entities from old collection
            var entities = await provider.GetAllAsync<object>(oldName);

            // Create new collection
            await provider.CreateCollectionAsync(newName);

            // Copy entities to new collection
            foreach (var entity in entities)
            {
                await provider.CreateAsync(newName, entity);
            }

            // Delete old collection
            await provider.DeleteCollectionAsync(oldName);
        }
    }
}
