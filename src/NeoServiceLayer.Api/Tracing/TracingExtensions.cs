using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace NeoServiceLayer.API.Tracing
{
    /// <summary>
    /// Extension methods for distributed tracing
    /// </summary>
    public static class TracingExtensions
    {
        /// <summary>
        /// Adds distributed tracing services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDistributedTracing(this IServiceCollection services, IConfiguration configuration)
        {
            // Add tracing options
            services.Configure<TracingOptions>(configuration.GetSection("Tracing"));
            
            // Get tracing options
            var tracingOptions = configuration.GetSection("Tracing").Get<TracingOptions>();
            
            if (tracingOptions == null || !tracingOptions.Enabled)
            {
                return services;
            }

            // Create activity source
            var activitySource = new ActivitySource(tracingOptions.ServiceName);
            services.AddSingleton(activitySource);

            // Configure OpenTelemetry
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    // Configure resource
                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(tracingOptions.ServiceName)
                        .AddTelemetrySdk()
                        .AddEnvironmentVariableDetector());

                    // Add sources
                    builder.AddSource(tracingOptions.ServiceName)
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithHttpRequest = (activity, request) =>
                            {
                                activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());
                                activity.SetTag("http.request_id", request.HttpContext.TraceIdentifier);
                            };
                            options.EnrichWithHttpResponse = (activity, response) =>
                            {
                                activity.SetTag("http.response_length", response.ContentLength);
                            };
                        })
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithHttpRequestMessage = (activity, request) =>
                            {
                                activity.SetTag("http.request_content_length", request.Content?.Headers.ContentLength);
                                activity.SetTag("http.request_content_type", request.Content?.Headers.ContentType?.ToString());
                            };
                            options.EnrichWithHttpResponseMessage = (activity, response) =>
                            {
                                activity.SetTag("http.response_content_length", response.Content?.Headers.ContentLength);
                                activity.SetTag("http.response_content_type", response.Content?.Headers.ContentType?.ToString());
                            };
                        })
                        .AddSqlClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.SetDbStatementForText = true;
                            options.SetDbStatementForStoredProcedure = true;
                        });

                    // Configure sampling
                    builder.SetSampler(new TraceIdRatioBasedSampler(tracingOptions.SamplingRate));

                    // Configure exporter
                    switch (tracingOptions.ExporterType?.ToLower())
                    {
                        case "jaeger":
                            builder.AddJaegerExporter(options =>
                            {
                                options.Endpoint = new Uri(tracingOptions.JaegerEndpoint);
                            });
                            break;
                        
                        case "zipkin":
                            builder.AddZipkinExporter(options =>
                            {
                                options.Endpoint = new Uri(tracingOptions.ZipkinEndpoint);
                            });
                            break;
                        
                        case "otlp":
                            builder.AddOtlpExporter(options =>
                            {
                                options.Endpoint = new Uri(tracingOptions.OtlpEndpoint);
                            });
                            break;
                        
                        case "console":
                        default:
                            builder.AddConsoleExporter();
                            break;
                    }
                });

            return services;
        }

        /// <summary>
        /// Adds distributed tracing middleware to the application
        /// </summary>
        /// <param name="builder">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseDistributedTracing(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TracingMiddleware>();
        }
    }
}
