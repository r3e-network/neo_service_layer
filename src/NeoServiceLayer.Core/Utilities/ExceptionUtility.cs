using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NeoServiceLayer.Core.Utilities
{
    /// <summary>
    /// Utility class for exception handling
    /// </summary>
    public static class ExceptionUtility
    {
        /// <summary>
        /// Execute an action with exception handling
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="action">The action to execute</param>
        /// <param name="operationName">The name of the operation</param>
        /// <param name="requestId">The request ID</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <param name="exceptionHandler">Custom exception handler</param>
        /// <typeparam name="T">The logger type</typeparam>
        /// <returns>True if the action executed successfully, false otherwise</returns>
        public static bool ExecuteWithExceptionHandling<T>(
            ILogger<T> logger,
            Action action,
            string operationName,
            string requestId,
            Dictionary<string, object> additionalData = null,
            Func<Exception, bool> exceptionHandler = null)
        {
            try
            {
                LoggingUtility.LogOperationStart(logger, operationName, requestId, additionalData);
                
                var executionTimeMs = MetricsUtility.MeasureExecutionTime(action);
                
                LoggingUtility.LogOperationSuccess(logger, operationName, requestId, executionTimeMs, additionalData);
                
                return true;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(logger, operationName, requestId, ex, 0, additionalData);
                
                if (exceptionHandler != null)
                {
                    return exceptionHandler(ex);
                }
                
                return false;
            }
        }

        /// <summary>
        /// Execute a function with exception handling
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="func">The function to execute</param>
        /// <param name="operationName">The name of the operation</param>
        /// <param name="requestId">The request ID</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <param name="exceptionHandler">Custom exception handler</param>
        /// <typeparam name="T">The logger type</typeparam>
        /// <typeparam name="TResult">The return type of the function</typeparam>
        /// <returns>A tuple containing the result of the function and a flag indicating whether the function executed successfully</returns>
        public static (TResult result, bool success) ExecuteWithExceptionHandling<T, TResult>(
            ILogger<T> logger,
            Func<TResult> func,
            string operationName,
            string requestId,
            Dictionary<string, object> additionalData = null,
            Func<Exception, (TResult result, bool success)> exceptionHandler = null)
        {
            try
            {
                LoggingUtility.LogOperationStart(logger, operationName, requestId, additionalData);
                
                var (result, executionTimeMs) = MetricsUtility.MeasureExecutionTime(func);
                
                LoggingUtility.LogOperationSuccess(logger, operationName, requestId, executionTimeMs, additionalData);
                
                return (result, true);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(logger, operationName, requestId, ex, 0, additionalData);
                
                if (exceptionHandler != null)
                {
                    return exceptionHandler(ex);
                }
                
                return (default, false);
            }
        }

        /// <summary>
        /// Execute an async action with exception handling
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="action">The async action to execute</param>
        /// <param name="operationName">The name of the operation</param>
        /// <param name="requestId">The request ID</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <param name="exceptionHandler">Custom exception handler</param>
        /// <typeparam name="T">The logger type</typeparam>
        /// <returns>A task that represents the asynchronous operation, containing a flag indicating whether the action executed successfully</returns>
        public static async Task<bool> ExecuteWithExceptionHandlingAsync<T>(
            ILogger<T> logger,
            Func<Task> action,
            string operationName,
            string requestId,
            Dictionary<string, object> additionalData = null,
            Func<Exception, Task<bool>> exceptionHandler = null)
        {
            try
            {
                LoggingUtility.LogOperationStart(logger, operationName, requestId, additionalData);
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await action();
                stopwatch.Stop();
                
                LoggingUtility.LogOperationSuccess(logger, operationName, requestId, stopwatch.ElapsedMilliseconds, additionalData);
                
                return true;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(logger, operationName, requestId, ex, 0, additionalData);
                
                if (exceptionHandler != null)
                {
                    return await exceptionHandler(ex);
                }
                
                return false;
            }
        }

        /// <summary>
        /// Execute an async function with exception handling
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="func">The async function to execute</param>
        /// <param name="operationName">The name of the operation</param>
        /// <param name="requestId">The request ID</param>
        /// <param name="additionalData">Additional data to log</param>
        /// <param name="exceptionHandler">Custom exception handler</param>
        /// <typeparam name="T">The logger type</typeparam>
        /// <typeparam name="TResult">The return type of the function</typeparam>
        /// <returns>A task that represents the asynchronous operation, containing a tuple with the result of the function and a flag indicating whether the function executed successfully</returns>
        public static async Task<(TResult result, bool success)> ExecuteWithExceptionHandlingAsync<T, TResult>(
            ILogger<T> logger,
            Func<Task<TResult>> func,
            string operationName,
            string requestId,
            Dictionary<string, object> additionalData = null,
            Func<Exception, Task<(TResult result, bool success)>> exceptionHandler = null)
        {
            try
            {
                LoggingUtility.LogOperationStart(logger, operationName, requestId, additionalData);
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var result = await func();
                stopwatch.Stop();
                
                LoggingUtility.LogOperationSuccess(logger, operationName, requestId, stopwatch.ElapsedMilliseconds, additionalData);
                
                return (result, true);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogOperationFailure(logger, operationName, requestId, ex, 0, additionalData);
                
                if (exceptionHandler != null)
                {
                    return await exceptionHandler(ex);
                }
                
                return (default, false);
            }
        }

        /// <summary>
        /// Get a detailed exception message including inner exceptions
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns>A detailed exception message</returns>
        public static string GetDetailedExceptionMessage(Exception exception)
        {
            if (exception == null)
                return string.Empty;

            var message = exception.Message;
            var innerException = exception.InnerException;
            
            while (innerException != null)
            {
                message += $" -> {innerException.Message}";
                innerException = innerException.InnerException;
            }
            
            return message;
        }

        /// <summary>
        /// Get exception details as a dictionary
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns>A dictionary containing exception details</returns>
        public static Dictionary<string, object> GetExceptionDetails(Exception exception)
        {
            if (exception == null)
                return new Dictionary<string, object>();

            var details = new Dictionary<string, object>
            {
                ["ExceptionType"] = exception.GetType().Name,
                ["ExceptionMessage"] = exception.Message,
                ["StackTrace"] = exception.StackTrace
            };
            
            if (exception.InnerException != null)
            {
                details["InnerException"] = GetExceptionDetails(exception.InnerException);
            }
            
            return details;
        }
    }
}
