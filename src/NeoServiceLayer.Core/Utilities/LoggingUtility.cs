using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NeoServiceLayer.Core.Utilities
{
    /// <summary>
    /// Utility class for standardized logging across the application
    /// </summary>
    public static class LoggingUtility
    {
        /// <summary>
        /// Log an operation start event
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operation">The operation name</param>
        /// <param name="requestId">The request ID</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <typeparam name="T">The logger type</typeparam>
        public static void LogOperationStart<T>(ILogger<T> logger, string operation, string requestId, Dictionary<string, object> additionalData = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["RequestId"] = requestId,
                ["Operation"] = operation,
                ["Status"] = "Started",
                ["Timestamp"] = DateTime.UtcNow
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            logger.LogInformation("Operation {Operation} started. RequestId: {RequestId}", operation, requestId);
        }

        /// <summary>
        /// Log an operation success event
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operation">The operation name</param>
        /// <param name="requestId">The request ID</param>
        /// <param name="durationMs">The operation duration in milliseconds</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <typeparam name="T">The logger type</typeparam>
        public static void LogOperationSuccess<T>(ILogger<T> logger, string operation, string requestId, long durationMs, Dictionary<string, object> additionalData = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["RequestId"] = requestId,
                ["Operation"] = operation,
                ["Status"] = "Succeeded",
                ["DurationMs"] = durationMs,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            logger.LogInformation("Operation {Operation} succeeded in {DurationMs}ms. RequestId: {RequestId}", operation, durationMs, requestId);
        }

        /// <summary>
        /// Log an operation failure event
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="operation">The operation name</param>
        /// <param name="requestId">The request ID</param>
        /// <param name="exception">The exception that caused the failure</param>
        /// <param name="durationMs">The operation duration in milliseconds</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <typeparam name="T">The logger type</typeparam>
        public static void LogOperationFailure<T>(ILogger<T> logger, string operation, string requestId, Exception exception, long durationMs, Dictionary<string, object> additionalData = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["RequestId"] = requestId,
                ["Operation"] = operation,
                ["Status"] = "Failed",
                ["ErrorMessage"] = exception.Message,
                ["ErrorType"] = exception.GetType().Name,
                ["DurationMs"] = durationMs,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            logger.LogError(exception, "Operation {Operation} failed after {DurationMs}ms. Error: {ErrorMessage}. RequestId: {RequestId}", 
                operation, durationMs, exception.Message, requestId);
        }

        /// <summary>
        /// Log a security event
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="eventType">The security event type</param>
        /// <param name="requestId">The request ID</param>
        /// <param name="userId">The user ID</param>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceId">The resource ID</param>
        /// <param name="action">The action performed</param>
        /// <param name="result">The result of the action</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <typeparam name="T">The logger type</typeparam>
        public static void LogSecurityEvent<T>(
            ILogger<T> logger, 
            string eventType, 
            string requestId, 
            string userId, 
            string resourceType, 
            string resourceId, 
            string action, 
            string result, 
            Dictionary<string, object> additionalData = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["RequestId"] = requestId,
                ["EventType"] = eventType,
                ["UserId"] = userId,
                ["ResourceType"] = resourceType,
                ["ResourceId"] = resourceId,
                ["Action"] = action,
                ["Result"] = result,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            logger.LogInformation(
                "Security event: {EventType}. User: {UserId}, Resource: {ResourceType}/{ResourceId}, Action: {Action}, Result: {Result}. RequestId: {RequestId}", 
                eventType, userId, resourceType, resourceId, action, result, requestId);
        }

        /// <summary>
        /// Log a metrics event
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="metricName">The metric name</param>
        /// <param name="metricValue">The metric value</param>
        /// <param name="metricUnit">The metric unit</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <typeparam name="T">The logger type</typeparam>
        public static void LogMetric<T>(
            ILogger<T> logger, 
            string metricName, 
            double metricValue, 
            string metricUnit, 
            Dictionary<string, object> additionalData = null)
        {
            var logData = new Dictionary<string, object>
            {
                ["MetricName"] = metricName,
                ["MetricValue"] = metricValue,
                ["MetricUnit"] = metricUnit,
                ["Timestamp"] = DateTime.UtcNow
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            logger.LogInformation("Metric: {MetricName} = {MetricValue} {MetricUnit}", metricName, metricValue, metricUnit);
        }
    }
}
