using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Utilities;

namespace NeoServiceLayer.Core.Repositories
{
    /// <summary>
    /// Generic repository implementation for CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    public class GenericRepository<T, TKey> : IGenericRepository<T, TKey> where T : class
    {
        private readonly ILogger _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{T, TKey}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        /// <param name="collectionName">Collection name</param>
        public GenericRepository(ILogger logger, IStorageProvider storageProvider, string collectionName)
        {
            _logger = logger;
            _storageProvider = storageProvider;
            _collectionName = collectionName;
        }

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync(TKey id)
        {
            ValidationUtility.ValidateNotNull(id, nameof(id));

            try
            {
                return await _storageProvider.GetByIdAsync<T, TKey>(_collectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _storageProvider.GetAllAsync<T>(_collectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all entities");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
        {
            ValidationUtility.ValidateNotNull(filter, nameof(filter));

            try
            {
                // Convert expression to delegate
                var predicate = filter.Compile();
                return await _storageProvider.GetByFilterAsync(_collectionName, predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding entities");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T> CreateAsync(T entity)
        {
            ValidationUtility.ValidateNotNull(entity, nameof(entity));

            try
            {
                return await _storageProvider.CreateAsync(_collectionName, entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T> UpdateAsync(TKey id, T entity)
        {
            ValidationUtility.ValidateNotNull(id, nameof(id));
            ValidationUtility.ValidateNotNull(entity, nameof(entity));

            try
            {
                return await _storageProvider.UpdateAsync(_collectionName, id, entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(TKey id)
        {
            ValidationUtility.ValidateNotNull(id, nameof(id));

            try
            {
                return await _storageProvider.DeleteAsync<T, TKey>(_collectionName, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(TKey id)
        {
            ValidationUtility.ValidateNotNull(id, nameof(id));

            try
            {
                var entity = await _storageProvider.GetByIdAsync<T, TKey>(_collectionName, id);
                return entity != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if entity exists: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync(Expression<Func<T, bool>> filter = null)
        {
            try
            {
                if (filter == null)
                {
                    return await _storageProvider.CountAsync<T>(_collectionName);
                }
                else
                {
                    // Convert expression to delegate
                    var predicate = filter.Compile();
                    return await _storageProvider.CountAsync(_collectionName, predicate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IRepositoryTransaction> BeginTransactionAsync()
        {
            try
            {
                // Create a transaction object
                return new RepositoryTransaction(_logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error beginning transaction");
                throw;
            }
        }

        /// <summary>
        /// Repository transaction implementation
        /// </summary>
        private class RepositoryTransaction : IRepositoryTransaction
        {
            private readonly ILogger _logger;
            private bool _isCommitted;
            private bool _isRolledBack;
            private bool _isDisposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="RepositoryTransaction"/> class
            /// </summary>
            /// <param name="logger">Logger</param>
            public RepositoryTransaction(ILogger logger)
            {
                _logger = logger;
                _isCommitted = false;
                _isRolledBack = false;
                _isDisposed = false;
            }

            /// <inheritdoc/>
            public async Task CommitAsync()
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(RepositoryTransaction));
                }

                if (_isCommitted)
                {
                    throw new InvalidOperationException("Transaction already committed");
                }

                if (_isRolledBack)
                {
                    throw new InvalidOperationException("Transaction already rolled back");
                }

                try
                {
                    // Commit the transaction
                    // Note: This is a placeholder for actual transaction commit logic
                    await Task.CompletedTask;
                    _isCommitted = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error committing transaction");
                    throw;
                }
            }

            /// <inheritdoc/>
            public async Task RollbackAsync()
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(RepositoryTransaction));
                }

                if (_isCommitted)
                {
                    throw new InvalidOperationException("Transaction already committed");
                }

                if (_isRolledBack)
                {
                    throw new InvalidOperationException("Transaction already rolled back");
                }

                try
                {
                    // Roll back the transaction
                    // Note: This is a placeholder for actual transaction rollback logic
                    await Task.CompletedTask;
                    _isRolledBack = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error rolling back transaction");
                    throw;
                }
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                if (!_isCommitted && !_isRolledBack)
                {
                    // Auto-rollback if not committed or rolled back
                    try
                    {
                        RollbackAsync().GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error auto-rolling back transaction during dispose");
                    }
                }

                _isDisposed = true;
            }
        }
    }
}
