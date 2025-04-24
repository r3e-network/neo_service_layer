using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace NeoServiceLayer.API.Tracing
{
    /// <summary>
    /// Middleware for distributed tracing
    /// </summary>
    public class TracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TracingMiddleware> _logger;
        private readonly TracingOptions _options;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly ActivitySource _activitySource;

        /// <summary>
        /// Initializes a new instance of the <see cref="TracingMiddleware"/> class
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger</param>
        /// <param name="options">Tracing options</param>
        /// <param name="activitySource">Activity source</param>
        public TracingMiddleware(
            RequestDelegate next,
            ILogger<TracingMiddleware> logger,
            IOptions<TracingOptions> options,
            ActivitySource activitySource)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _activitySource = activitySource;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            // Start activity for the request
            using var activity = _activitySource.StartActivity(
                $"{context.Request.Method} {context.Request.Path}",
                ActivityKind.Server);

            if (activity == null)
            {
                await _next(context);
                return;
            }

            // Add HTTP request information to the activity
            activity.SetTag("http.method", context.Request.Method);
            activity.SetTag("http.url", $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");
            activity.SetTag("http.host", context.Request.Host.ToString());
            activity.SetTag("http.path", context.Request.Path.ToString());
            activity.SetTag("http.query_string", context.Request.QueryString.ToString());
            activity.SetTag("http.scheme", context.Request.Scheme);
            activity.SetTag("http.request_content_length", context.Request.ContentLength);
            activity.SetTag("http.request_content_type", context.Request.ContentType);
            activity.SetTag("http.client_ip", context.Connection.RemoteIpAddress?.ToString());

            // Add request headers if enabled
            if (_options.IncludeRequestHeaders)
            {
                foreach (var header in context.Request.Headers)
                {
                    activity.SetTag($"http.request.header.{header.Key.ToLowerInvariant()}", header.Value.ToString());
                }
            }

            // Capture request body if enabled
            string requestBody = null;
            if (_options.IncludeRequestBody && context.Request.ContentLength > 0 && context.Request.ContentLength <= _options.MaxBodySizeBytes)
            {
                context.Request.EnableBuffering();
                
                using (var requestStream = _recyclableMemoryStreamManager.GetStream())
                {
                    await context.Request.Body.CopyToAsync(requestStream);
                    requestStream.Position = 0;
                    
                    using (var streamReader = new StreamReader(requestStream))
                    {
                        requestBody = await streamReader.ReadToEndAsync();
                        activity.SetTag("http.request.body", requestBody);
                    }
                    
                    context.Request.Body.Position = 0;
                }
            }

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBodyStream;

            try
            {
                // Start timing the request execution
                var stopwatch = Stopwatch.StartNew();
                
                // Call the next middleware
                await _next(context);
                
                // Stop timing
                stopwatch.Stop();
                
                // Add response information to the activity
                activity.SetTag("http.status_code", context.Response.StatusCode);
                activity.SetTag("http.response_content_length", context.Response.ContentLength);
                activity.SetTag("http.response_content_type", context.Response.ContentType);
                activity.SetTag("http.duration_ms", stopwatch.ElapsedMilliseconds);

                // Add response headers if enabled
                if (_options.IncludeResponseHeaders)
                {
                    foreach (var header in context.Response.Headers)
                    {
                        activity.SetTag($"http.response.header.{header.Key.ToLowerInvariant()}", header.Value.ToString());
                    }
                }

                // Capture response body if enabled
                if (_options.IncludeResponseBody && context.Response.ContentLength > 0 && context.Response.ContentLength <= _options.MaxBodySizeBytes)
                {
                    responseBodyStream.Position = 0;
                    using var streamReader = new StreamReader(responseBodyStream);
                    var responseBody = await streamReader.ReadToEndAsync();
                    activity.SetTag("http.response.body", responseBody);
                }

                // Copy the response body back to the original stream
                responseBodyStream.Position = 0;
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                // Record the exception in the activity
                activity.SetTag("error", true);
                activity.SetTag("error.type", ex.GetType().FullName);
                activity.SetTag("error.message", ex.Message);
                activity.SetTag("error.stack_trace", ex.StackTrace);
                
                // Re-throw the exception
                throw;
            }
            finally
            {
                // Restore the original response body
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
