using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NeoServiceLayer.Services.Common.Utilities
{
    /// <summary>
    /// Utility for logging
    /// </summary>
    public static class LoggingUtility
    {
        /// <summary>
        /// Logs an operation start
        /// </summary>
        /// <typeparam name="T">Logger type</typeparam>
        /// <param name="logger">Logger</param>
        /// <param name="operationName">Operation name</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="additionalData">Additional data</param>
        public static void LogOperationStart<T>(ILogger<T> logger, string operationName, string requestId, Dictionary<string, object> additionalData = null)
        {
            logger.LogInformation("[{RequestId}] Starting {OperationName} operation. {AdditionalData}", 
                requestId, operationName, additionalData ?? new Dictionary<string, object>());
        }

        /// <summary>
        /// Logs an operation success
        /// </summary>
        /// <typeparam name="T">Logger type</typeparam>
        /// <param name="logger">Logger</param>
        /// <param name="operationName">Operation name</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="elapsedMs">Elapsed milliseconds</param>
        /// <param name="additionalData">Additional data</param>
        public static void LogOperationSuccess<T>(ILogger<T> logger, string operationName, string requestId, long elapsedMs, Dictionary<string, object> additionalData = null)
        {
            logger.LogInformation("[{RequestId}] Successfully completed {OperationName} operation in {ElapsedMs}ms. {AdditionalData}", 
                requestId, operationName, elapsedMs, additionalData ?? new Dictionary<string, object>());
        }

        /// <summary>
        /// Logs an operation failure
        /// </summary>
        /// <typeparam name="T">Logger type</typeparam>
        /// <param name="logger">Logger</param>
        /// <param name="operationName">Operation name</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="exception">Exception</param>
        /// <param name="elapsedMs">Elapsed milliseconds</param>
        /// <param name="additionalData">Additional data</param>
        public static void LogOperationFailure<T>(ILogger<T> logger, string operationName, string requestId, Exception exception, long elapsedMs, Dictionary<string, object> additionalData = null)
        {
            logger.LogError(exception, "[{RequestId}] Failed to complete {OperationName} operation after {ElapsedMs}ms. {AdditionalData}", 
                requestId, operationName, elapsedMs, additionalData ?? new Dictionary<string, object>());
        }

        /// <summary>
        /// Logs an error
        /// </summary>
        /// <typeparam name="T">Logger type</typeparam>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="additionalData">Additional data</param>
        public static void LogError<T>(ILogger<T> logger, string message, Exception exception, string requestId, Dictionary<string, object> additionalData = null)
        {
            logger.LogError(exception, "[{RequestId}] {Message}. {AdditionalData}", 
                requestId, message, additionalData ?? new Dictionary<string, object>());
        }

        /// <summary>
        /// Logs a warning
        /// </summary>
        /// <typeparam name="T">Logger type</typeparam>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="requestId">Request ID</param>
        /// <param name="additionalData">Additional data</param>
        public static void LogWarning<T>(ILogger<T> logger, string message, string requestId, Dictionary<string, object> additionalData = null)
        {
            logger.LogWarning("[{RequestId}] {Message}. {AdditionalData}", 
                requestId, message, additionalData ?? new Dictionary<string, object>());
        }
    }
}
