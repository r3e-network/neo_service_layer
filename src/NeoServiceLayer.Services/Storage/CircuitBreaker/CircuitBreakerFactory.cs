using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Services.Storage.Configuration;

namespace NeoServiceLayer.Services.Storage.CircuitBreaker
{
    /// <summary>
    /// Factory for creating circuit breakers
    /// </summary>
    public class CircuitBreakerFactory
    {
        private readonly ILogger<CircuitBreaker> _logger;
        private readonly CircuitBreakerConfiguration _configuration;
        private readonly ConcurrentDictionary<string, CircuitBreaker> _circuitBreakers = new ConcurrentDictionary<string, CircuitBreaker>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerFactory"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Circuit breaker configuration</param>
        public CircuitBreakerFactory(
            ILogger<CircuitBreaker> logger,
            IOptions<CircuitBreakerConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
        }

        /// <summary>
        /// Creates a circuit breaker
        /// </summary>
        /// <param name="name">Circuit breaker name</param>
        /// <returns>Circuit breaker</returns>
        public CircuitBreaker Create(string name)
        {
            return _circuitBreakers.GetOrAdd(name, key => new CircuitBreaker(
                _logger,
                key,
                _configuration.FailureThreshold,
                _configuration.ResetTimeoutSeconds));
        }

        /// <summary>
        /// Gets a circuit breaker
        /// </summary>
        /// <param name="name">Circuit breaker name</param>
        /// <returns>Circuit breaker if found, null otherwise</returns>
        public CircuitBreaker Get(string name)
        {
            _circuitBreakers.TryGetValue(name, out var circuitBreaker);
            return circuitBreaker;
        }

        /// <summary>
        /// Gets all circuit breakers
        /// </summary>
        /// <returns>List of circuit breakers</returns>
        public IEnumerable<CircuitBreaker> GetAll()
        {
            return _circuitBreakers.Values;
        }

        /// <summary>
        /// Resets all circuit breakers
        /// </summary>
        public void ResetAll()
        {
            foreach (var circuitBreaker in _circuitBreakers.Values)
            {
                circuitBreaker.Reset();
            }
        }
    }
}
