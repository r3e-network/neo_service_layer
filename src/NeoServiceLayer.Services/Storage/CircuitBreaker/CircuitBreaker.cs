using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NeoServiceLayer.Services.Storage.CircuitBreaker
{
    /// <summary>
    /// Circuit breaker for database operations
    /// </summary>
    public class CircuitBreaker
    {
        private readonly ILogger<CircuitBreaker> _logger;
        private readonly string _name;
        private readonly int _failureThreshold;
        private readonly TimeSpan _resetTimeout;
        private readonly object _stateLock = new object();
        private CircuitBreakerState _state;
        private int _failureCount;
        private DateTime _lastFailureTime;
        private DateTime _openTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreaker"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="name">Circuit breaker name</param>
        /// <param name="failureThreshold">Failure threshold</param>
        /// <param name="resetTimeoutSeconds">Reset timeout in seconds</param>
        public CircuitBreaker(
            ILogger<CircuitBreaker> logger,
            string name,
            int failureThreshold = 5,
            int resetTimeoutSeconds = 60)
        {
            _logger = logger;
            _name = name;
            _failureThreshold = failureThreshold;
            _resetTimeout = TimeSpan.FromSeconds(resetTimeoutSeconds);
            _state = CircuitBreakerState.Closed;
            _failureCount = 0;
            _lastFailureTime = DateTime.MinValue;
            _openTime = DateTime.MinValue;
        }

        /// <summary>
        /// Gets the circuit breaker name
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets the circuit breaker state
        /// </summary>
        public CircuitBreakerState State
        {
            get
            {
                lock (_stateLock)
                {
                    return _state;
                }
            }
        }

        /// <summary>
        /// Gets the failure count
        /// </summary>
        public int FailureCount
        {
            get
            {
                lock (_stateLock)
                {
                    return _failureCount;
                }
            }
        }

        /// <summary>
        /// Executes an action with circuit breaker protection
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="action">Action to execute</param>
        /// <param name="fallback">Fallback action</param>
        /// <returns>Result of the action or fallback</returns>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<Task<T>> fallback = null)
        {
            // Check if circuit is open
            if (IsOpen())
            {
                _logger.LogWarning("Circuit {Name} is open, skipping action", _name);
                return fallback != null ? await fallback() : default;
            }

            try
            {
                // Execute action
                var result = await action();

                // Reset failure count on success
                lock (_stateLock)
                {
                    if (_state == CircuitBreakerState.HalfOpen)
                    {
                        _logger.LogInformation("Circuit {Name} is now closed", _name);
                        _state = CircuitBreakerState.Closed;
                    }

                    _failureCount = 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                // Record failure
                RecordFailure(ex);

                // Execute fallback if provided
                if (fallback != null)
                {
                    _logger.LogInformation("Executing fallback for circuit {Name}", _name);
                    return await fallback();
                }

                throw;
            }
        }

        /// <summary>
        /// Executes an action with circuit breaker protection
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="fallback">Fallback action</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task ExecuteAsync(Func<Task> action, Func<Task> fallback = null)
        {
            // Check if circuit is open
            if (IsOpen())
            {
                _logger.LogWarning("Circuit {Name} is open, skipping action", _name);
                if (fallback != null)
                {
                    await fallback();
                }
                return;
            }

            try
            {
                // Execute action
                await action();

                // Reset failure count on success
                lock (_stateLock)
                {
                    if (_state == CircuitBreakerState.HalfOpen)
                    {
                        _logger.LogInformation("Circuit {Name} is now closed", _name);
                        _state = CircuitBreakerState.Closed;
                    }

                    _failureCount = 0;
                }
            }
            catch (Exception ex)
            {
                // Record failure
                RecordFailure(ex);

                // Execute fallback if provided
                if (fallback != null)
                {
                    _logger.LogInformation("Executing fallback for circuit {Name}", _name);
                    await fallback();
                    return;
                }

                throw;
            }
        }

        /// <summary>
        /// Resets the circuit breaker
        /// </summary>
        public void Reset()
        {
            lock (_stateLock)
            {
                _logger.LogInformation("Resetting circuit {Name}", _name);
                _state = CircuitBreakerState.Closed;
                _failureCount = 0;
                _lastFailureTime = DateTime.MinValue;
                _openTime = DateTime.MinValue;
            }
        }

        /// <summary>
        /// Checks if the circuit is open
        /// </summary>
        /// <returns>True if the circuit is open, false otherwise</returns>
        private bool IsOpen()
        {
            lock (_stateLock)
            {
                if (_state == CircuitBreakerState.Open)
                {
                    // Check if reset timeout has elapsed
                    if (DateTime.UtcNow - _openTime > _resetTimeout)
                    {
                        _logger.LogInformation("Circuit {Name} is now half-open", _name);
                        _state = CircuitBreakerState.HalfOpen;
                        return false;
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Records a failure
        /// </summary>
        /// <param name="exception">Exception that caused the failure</param>
        private void RecordFailure(Exception exception)
        {
            lock (_stateLock)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                _logger.LogWarning(exception, "Circuit {Name} recorded failure {Count}/{Threshold}", _name, _failureCount, _failureThreshold);

                if (_state == CircuitBreakerState.Closed && _failureCount >= _failureThreshold)
                {
                    _logger.LogWarning("Circuit {Name} is now open", _name);
                    _state = CircuitBreakerState.Open;
                    _openTime = DateTime.UtcNow;
                }
                else if (_state == CircuitBreakerState.HalfOpen)
                {
                    _logger.LogWarning("Circuit {Name} is now open after half-open failure", _name);
                    _state = CircuitBreakerState.Open;
                    _openTime = DateTime.UtcNow;
                }
            }
        }
    }

    /// <summary>
    /// Circuit breaker state
    /// </summary>
    public enum CircuitBreakerState
    {
        /// <summary>
        /// Circuit is closed, all requests are allowed
        /// </summary>
        Closed,

        /// <summary>
        /// Circuit is open, all requests are blocked
        /// </summary>
        Open,

        /// <summary>
        /// Circuit is half-open, allowing a test request
        /// </summary>
        HalfOpen
    }
}
