using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace NeoServiceLayer.API.Middleware
{
    /// <summary>
    /// Middleware for logging HTTP requests and responses
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger</param>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Skip logging for certain paths (e.g., health checks, static files)
            if (ShouldSkipLogging(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Log the request
            await LogRequest(context);

            // Capture the response
            var originalBodyStream = context.Response.Body;
            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            try
            {
                // Call the next middleware
                await _next(context);

                // Log the response
                await LogResponse(context, responseBody, originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            var requestId = context.TraceIdentifier;
            var method = context.Request.Method;
            var path = context.Request.Path;
            var queryString = context.Request.QueryString;
            var headers = context.Request.Headers;
            var contentType = context.Request.ContentType;
            var contentLength = context.Request.ContentLength;

            _logger.LogInformation(
                "Request {RequestId}: {Method} {Path}{QueryString} - Content-Type: {ContentType}, Content-Length: {ContentLength}",
                requestId, method, path, queryString, contentType, contentLength);

            // Log request headers (excluding sensitive headers)
            foreach (var header in headers)
            {
                if (!IsSensitiveHeader(header.Key))
                {
                    _logger.LogDebug("Request {RequestId} Header: {Key}={Value}", requestId, header.Key, header.Value);
                }
            }

            // Log request body for non-GET requests (limited to a reasonable size)
            if (method != "GET" && contentLength > 0 && contentLength < 10240) // 10KB limit
            {
                await using var requestStream = _recyclableMemoryStreamManager.GetStream();
                await context.Request.Body.CopyToAsync(requestStream);
                requestStream.Seek(0, SeekOrigin.Begin);

                var requestBody = await new StreamReader(requestStream).ReadToEndAsync();
                _logger.LogDebug("Request {RequestId} Body: {Body}", requestId, requestBody);

                // Reset the request body position
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }
        }

        private async Task LogResponse(HttpContext context, MemoryStream responseBody, Stream originalBodyStream)
        {
            var requestId = context.TraceIdentifier;
            var statusCode = context.Response.StatusCode;
            var contentType = context.Response.ContentType;
            var headers = context.Response.Headers;

            _logger.LogInformation(
                "Response {RequestId}: {StatusCode} - Content-Type: {ContentType}, Content-Length: {ContentLength}",
                requestId, statusCode, contentType, responseBody.Length);

            // Log response headers (excluding sensitive headers)
            foreach (var header in headers)
            {
                if (!IsSensitiveHeader(header.Key))
                {
                    _logger.LogDebug("Response {RequestId} Header: {Key}={Value}", requestId, header.Key, header.Value);
                }
            }

            // Log response body (limited to a reasonable size)
            if (responseBody.Length > 0 && responseBody.Length < 10240) // 10KB limit
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
                _logger.LogDebug("Response {RequestId} Body: {Body}", requestId, responseBodyText);
            }

            // Copy the response body back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private bool ShouldSkipLogging(PathString path)
        {
            // Skip logging for health checks, static files, etc.
            return path.StartsWithSegments("/health") ||
                   path.StartsWithSegments("/static") ||
                   path.StartsWithSegments("/favicon.ico");
        }

        private bool IsSensitiveHeader(string headerName)
        {
            // List of sensitive headers that should not be logged
            return headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Cookie", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("X-API-Key", StringComparison.OrdinalIgnoreCase);
        }
    }
}
