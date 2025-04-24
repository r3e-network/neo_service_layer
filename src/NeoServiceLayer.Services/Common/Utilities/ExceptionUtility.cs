using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NeoServiceLayer.Services.Common.Utilities
{
    /// <summary>
    /// Utility for handling exceptions
    /// </summary>
    public static class ExceptionUtility
    {
        /// <summary>
        /// Executes a function with exception handling
        /// </summary>
        /// <typeparam name="TLogger">Logger type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="logger">Logger</param>
        /// <param name="func">Function to execute</param>
        /// <param name="operationName">Operation name</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>Result of the function</returns>
        public static async Task<(bool success, TResult result)> ExecuteWithExceptionHandlingAsync<TLogger, TResult>(
            ILogger<TLogger> logger,
            Func<Task<TResult>> func,
            string operationName,
            string requestId,
            Dictionary<string, object> additionalData = null)
        {
            try
            {
                var result = await func();
                return (true, result);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(logger, $"Error executing {operationName}", ex, requestId, additionalData);
                return (false, default);
            }
        }

        /// <summary>
        /// Executes a function with exception handling
        /// </summary>
        /// <typeparam name="TLogger">Logger type</typeparam>
        /// <param name="logger">Logger</param>
        /// <param name="func">Function to execute</param>
        /// <param name="operationName">Operation name</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>Success flag</returns>
        public static async Task<bool> ExecuteWithExceptionHandlingAsync<TLogger>(
            ILogger<TLogger> logger,
            Func<Task> func,
            string operationName,
            string requestId,
            Dictionary<string, object> additionalData = null)
        {
            try
            {
                await func();
                return true;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(logger, $"Error executing {operationName}", ex, requestId, additionalData);
                return false;
            }
        }
    }
}
