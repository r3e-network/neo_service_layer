using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace NeoServiceLayer.Core.Utilities
{
    /// <summary>
    /// Utility class for metrics collection
    /// </summary>
    public static class MetricsUtility
    {
        /// <summary>
        /// Measure the execution time of an action
        /// </summary>
        /// <param name="action">The action to measure</param>
        /// <returns>The execution time in milliseconds</returns>
        public static long MeasureExecutionTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Measure the execution time of a function
        /// </summary>
        /// <param name="func">The function to measure</param>
        /// <typeparam name="T">The return type of the function</typeparam>
        /// <returns>A tuple containing the result of the function and the execution time in milliseconds</returns>
        public static (T result, long executionTimeMs) MeasureExecutionTime<T>(Func<T> func)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = func();
            stopwatch.Stop();
            return (result, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Log the execution time of an action
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="action">The action to measure</param>
        /// <param name="operationName">The name of the operation</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <typeparam name="T">The logger type</typeparam>
        /// <returns>The execution time in milliseconds</returns>
        public static long LogExecutionTime<T>(ILogger<T> logger, Action action, string operationName, Dictionary<string, object> additionalData = null)
        {
            var executionTimeMs = MeasureExecutionTime(action);
            
            LoggingUtility.LogMetric(logger, $"{operationName}_duration", executionTimeMs, "ms", additionalData);
            
            return executionTimeMs;
        }

        /// <summary>
        /// Log the execution time of a function
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="func">The function to measure</param>
        /// <param name="operationName">The name of the operation</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <typeparam name="T">The logger type</typeparam>
        /// <typeparam name="TResult">The return type of the function</typeparam>
        /// <returns>A tuple containing the result of the function and the execution time in milliseconds</returns>
        public static (TResult result, long executionTimeMs) LogExecutionTime<T, TResult>(ILogger<T> logger, Func<TResult> func, string operationName, Dictionary<string, object> additionalData = null)
        {
            var (result, executionTimeMs) = MeasureExecutionTime(func);
            
            LoggingUtility.LogMetric(logger, $"{operationName}_duration", executionTimeMs, "ms", additionalData);
            
            return (result, executionTimeMs);
        }

        /// <summary>
        /// Create a metrics context for measuring and logging execution time
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operationName">The name of the operation</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <typeparam name="T">The logger type</typeparam>
        /// <returns>A disposable metrics context</returns>
        public static IDisposable CreateMetricsContext<T>(ILogger<T> logger, string operationName, Dictionary<string, object> additionalData = null)
        {
            return new MetricsContext<T>(logger, operationName, additionalData);
        }

        /// <summary>
        /// Metrics context for measuring and logging execution time
        /// </summary>
        /// <typeparam name="T">The logger type</typeparam>
        private class MetricsContext<T> : IDisposable
        {
            private readonly ILogger<T> _logger;
            private readonly string _operationName;
            private readonly Dictionary<string, object> _additionalData;
            private readonly Stopwatch _stopwatch;

            public MetricsContext(ILogger<T> logger, string operationName, Dictionary<string, object> additionalData)
            {
                _logger = logger;
                _operationName = operationName;
                _additionalData = additionalData;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                LoggingUtility.LogMetric(_logger, $"{_operationName}_duration", _stopwatch.ElapsedMilliseconds, "ms", _additionalData);
            }
        }
    }
}
