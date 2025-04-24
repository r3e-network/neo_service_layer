using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Exceptions;

namespace NeoServiceLayer.API.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions globally
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger</param>
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception has occurred: {Message}", exception.Message);

            var code = HttpStatusCode.InternalServerError; // 500 if unexpected
            var message = "An unexpected error occurred.";
            var details = exception.Message;

            // Determine the status code based on the exception type
            if (exception is ValidationException validationEx)
            {
                code = HttpStatusCode.BadRequest; // 400
                message = "Validation failed.";

                // Include validation errors in the response
                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = new
                    {
                        code = (int)code,
                        message,
                        details,
                        validationErrors = validationEx.Errors,
                        traceId = context.TraceIdentifier
                    }
                }));
            }
            else if (exception is ResourceNotFoundException)
            {
                code = HttpStatusCode.NotFound; // 404
                message = "Resource not found.";
            }
            else if (exception is ResourceAlreadyExistsException)
            {
                code = HttpStatusCode.Conflict; // 409
                message = "Resource already exists.";
            }
            else if (exception is ForbiddenAccessException)
            {
                code = HttpStatusCode.Forbidden; // 403
                message = "Access forbidden.";
            }
            else if (exception is ArgumentException || exception is FormatException)
            {
                code = HttpStatusCode.BadRequest; // 400
                message = "Invalid request.";
            }
            else if (exception is UnauthorizedAccessException)
            {
                code = HttpStatusCode.Unauthorized; // 401
                message = "Unauthorized access.";
            }
            else if (exception is InvalidOperationException)
            {
                code = HttpStatusCode.BadRequest; // 400
                message = "Invalid operation.";
            }

            // Create the response
            var result = JsonSerializer.Serialize(new
            {
                error = new
                {
                    code = (int)code,
                    message,
                    details,
                    traceId = context.TraceIdentifier
                }
            });

            // Set the response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}
