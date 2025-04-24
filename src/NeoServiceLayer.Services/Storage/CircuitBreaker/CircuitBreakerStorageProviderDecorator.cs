using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Storage.Configuration;

namespace NeoServiceLayer.Services.Storage.CircuitBreaker
{
    /// <summary>
    /// Decorator for storage providers that adds circuit breaker protection
    /// </summary>
    public class CircuitBreakerStorageProviderDecorator : Core.Interfaces.IStorageProvider
    {
        private readonly Core.Interfaces.IStorageProvider _innerProvider;
        private readonly CircuitBreakerFactory _circuitBreakerFactory;
        private readonly ILogger<CircuitBreakerStorageProviderDecorator> _logger;
        private readonly CircuitBreakerConfiguration _configuration;
        private readonly CircuitBreaker _circuitBreaker;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerStorageProviderDecorator"/> class
        /// </summary>
        /// <param name="innerProvider">Inner storage provider</param>
        /// <param name="circuitBreakerFactory">Circuit breaker factory</param>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Circuit breaker configuration</param>
        public CircuitBreakerStorageProviderDecorator(
            Core.Interfaces.IStorageProvider innerProvider,
            CircuitBreakerFactory circuitBreakerFactory,
            ILogger<CircuitBreakerStorageProviderDecorator> logger,
            IOptions<CircuitBreakerConfiguration> configuration)
        {
            _innerProvider = innerProvider;
            _circuitBreakerFactory = circuitBreakerFactory;
            _logger = logger;
            _configuration = configuration.Value;
            _circuitBreaker = _circuitBreakerFactory.Create($"StorageProvider:{innerProvider.Name}");
        }

        /// <inheritdoc/>
        public string Name => _innerProvider.Name;

        /// <inheritdoc/>
        public string Type => _innerProvider.Type;

        /// <inheritdoc/>
        public async Task<bool> InitializeAsync()
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.InitializeAsync();
            }

            return await _circuitBreaker.ExecuteAsync(
                async () => await _innerProvider.InitializeAsync(),
                async () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} initialization, returning false", Name);
                    return await Task.FromResult(false);
                });
        }

        /// <inheritdoc/>
        public async Task<bool> HealthCheckAsync()
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.HealthCheckAsync();
            }

            return await _circuitBreaker.ExecuteAsync(
                async () => await _innerProvider.HealthCheckAsync(),
                async () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} health check, returning false", Name);
                    return await Task.FromResult(false);
                });
        }

        /// <inheritdoc/>
        public async Task<T> CreateAsync<T>(string collection, T entity) where T : class
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.CreateAsync(collection, entity);
            }

            return await _circuitBreaker.ExecuteAsync<T>(
                () => _innerProvider.CreateAsync(collection, entity),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} create operation, throwing exception", Name);
                    throw new CircuitBreakerOpenException($"Circuit breaker open for {Name} create operation");
                });
        }

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync<T, TKey>(string collection, TKey id) where T : class
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.GetByIdAsync<T, TKey>(collection, id);
            }

            return await _circuitBreaker.ExecuteAsync<T>(
                () => _innerProvider.GetByIdAsync<T, TKey>(collection, id),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} get by ID operation, returning null", Name);
                    return Task.FromResult<T>(null);
                });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.GetAllAsync<T>(collection);
            }

            return await _circuitBreaker.ExecuteAsync<IEnumerable<T>>(
                () => _innerProvider.GetAllAsync<T>(collection),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} get all operation, returning empty list", Name);
                    return Task.FromResult<IEnumerable<T>>(new List<T>());
                });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetByFilterAsync<T>(string collection, Func<T, bool> filter) where T : class
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.GetByFilterAsync(collection, filter);
            }

            return await _circuitBreaker.ExecuteAsync<IEnumerable<T>>(
                () => _innerProvider.GetByFilterAsync(collection, filter),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} query operation, returning empty list", Name);
                    return Task.FromResult<IEnumerable<T>>(new List<T>());
                });
        }

        /// <inheritdoc/>
        public async Task<T> UpdateAsync<T, TKey>(string collection, TKey id, T entity) where T : class
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.UpdateAsync<T, TKey>(collection, id, entity);
            }

            return await _circuitBreaker.ExecuteAsync<T>(
                () => _innerProvider.UpdateAsync<T, TKey>(collection, id, entity),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} update operation, throwing exception", Name);
                    throw new CircuitBreakerOpenException($"Circuit breaker open for {Name} update operation");
                });
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync<T, TKey>(string collection, TKey id) where T : class
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.DeleteAsync<T, TKey>(collection, id);
            }

            return await _circuitBreaker.ExecuteAsync<bool>(
                () => _innerProvider.DeleteAsync<T, TKey>(collection, id),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} delete operation, returning false", Name);
                    return Task.FromResult(false);
                });
        }

        /// <inheritdoc/>
        public async Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.CountAsync(collection, filter);
            }

            return await _circuitBreaker.ExecuteAsync<int>(
                () => _innerProvider.CountAsync(collection, filter),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} count operation, returning 0", Name);
                    return Task.FromResult(0);
                });
        }

        /// <inheritdoc/>
        public async Task<bool> CollectionExistsAsync(string collection)
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.CollectionExistsAsync(collection);
            }

            return await _circuitBreaker.ExecuteAsync<bool>(
                () => _innerProvider.CollectionExistsAsync(collection),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} collection exists operation, returning false", Name);
                    return Task.FromResult(false);
                });
        }

        /// <inheritdoc/>
        public async Task<bool> CreateCollectionAsync(string collection)
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.CreateCollectionAsync(collection);
            }

            return await _circuitBreaker.ExecuteAsync<bool>(
                () => _innerProvider.CreateCollectionAsync(collection),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} create collection operation, returning false", Name);
                    return Task.FromResult(false);
                });
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCollectionAsync(string collection)
        {
            if (!_configuration.Enabled)
            {
                return await _innerProvider.DeleteCollectionAsync(collection);
            }

            return await _circuitBreaker.ExecuteAsync<bool>(
                () => _innerProvider.DeleteCollectionAsync(collection),
                () =>
                {
                    _logger.LogWarning("Circuit breaker open for {Provider} delete collection operation, returning false", Name);
                    return Task.FromResult(false);
                });
        }








    }

    /// <summary>
    /// Exception thrown when a circuit breaker is open
    /// </summary>
    public class CircuitBreakerOpenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerOpenException"/> class
        /// </summary>
        /// <param name="message">Exception message</param>
        public CircuitBreakerOpenException(string message) : base(message)
        {
        }
    }
}
