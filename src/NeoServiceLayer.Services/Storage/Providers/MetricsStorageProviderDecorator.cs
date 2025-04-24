using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage.Monitoring;

namespace NeoServiceLayer.Services.Storage.Providers
{
    /// <summary>
    /// Decorator for storage providers that collects metrics
    /// </summary>
    public class MetricsStorageProviderDecorator : Core.Interfaces.IStorageProvider
    {
        private readonly Core.Interfaces.IStorageProvider _innerProvider;
        private readonly DatabaseMetricsCollector _metricsCollector;
        private readonly ILogger<MetricsStorageProviderDecorator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsStorageProviderDecorator"/> class
        /// </summary>
        /// <param name="innerProvider">Inner storage provider</param>
        /// <param name="metricsCollector">Metrics collector</param>
        /// <param name="logger">Logger</param>
        public MetricsStorageProviderDecorator(
            Core.Interfaces.IStorageProvider innerProvider,
            DatabaseMetricsCollector metricsCollector,
            ILogger<MetricsStorageProviderDecorator> logger)
        {
            _innerProvider = innerProvider;
            _metricsCollector = metricsCollector;
            _logger = logger;
        }

        /// <inheritdoc/>
        public string Name => _innerProvider.Name;

        /// <inheritdoc/>
        public string Type => _innerProvider.Type;

        /// <inheritdoc/>
        public async Task<bool> InitializeAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                success = await _innerProvider.InitializeAsync();
                return success;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "Initialize", "N/A", stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HealthCheckAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                success = await _innerProvider.HealthCheckAsync();
                return success;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "HealthCheck", "N/A", stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<T> CreateAsync<T>(string collection, T entity) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.CreateAsync(collection, entity);
                success = true;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "Create", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync<T, TKey>(string collection, TKey id) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.GetByIdAsync<T, TKey>(collection, id);
                success = true;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "GetById", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.GetAllAsync<T>(collection);
                success = true;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "GetAll", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetByFilterAsync<T>(string collection, Func<T, bool> filter) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.GetByFilterAsync(collection, filter);
                success = true;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "Query", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<T> UpdateAsync<T, TKey>(string collection, TKey id, T entity) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.UpdateAsync<T, TKey>(collection, id, entity);
                success = true;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "Update", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync<T, TKey>(string collection, TKey id) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.DeleteAsync<T, TKey>(collection, id);
                success = result;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "Delete", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.CountAsync(collection, filter);
                success = true;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "Count", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CollectionExistsAsync(string collection)
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.CollectionExistsAsync(collection);
                success = true;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "CollectionExists", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CreateCollectionAsync(string collection)
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.CreateCollectionAsync(collection);
                success = result;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "CreateCollection", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCollectionAsync(string collection)
        {
            var stopwatch = Stopwatch.StartNew();
            bool success = false;

            try
            {
                var result = await _innerProvider.DeleteCollectionAsync(collection);
                success = result;
                return result;
            }
            catch (Exception)
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsCollector.RecordOperation(Name, "DeleteCollection", collection, stopwatch.ElapsedMilliseconds, success);
            }
        }








    }
}
