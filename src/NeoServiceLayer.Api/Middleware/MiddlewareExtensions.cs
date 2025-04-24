using Microsoft.AspNetCore.Builder;

namespace NeoServiceLayer.API.Middleware
{
    /// <summary>
    /// Extension methods for middleware
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds global error handling middleware to the application
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }

        /// <summary>
        /// Adds performance monitoring middleware to the application
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceMonitoringMiddleware>();
        }

        /// <summary>
        /// Adds request logging middleware to the application
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
