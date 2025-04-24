namespace NeoServiceLayer.API.Tracing
{
    /// <summary>
    /// Options for distributed tracing
    /// </summary>
    public class TracingOptions
    {
        /// <summary>
        /// Gets or sets whether tracing is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the service name
        /// </summary>
        public string ServiceName { get; set; } = "NeoServiceLayer";

        /// <summary>
        /// Gets or sets the exporter type
        /// </summary>
        public string ExporterType { get; set; } = "Console";

        /// <summary>
        /// Gets or sets the Jaeger endpoint
        /// </summary>
        public string JaegerEndpoint { get; set; } = "http://localhost:14268/api/traces";

        /// <summary>
        /// Gets or sets the Zipkin endpoint
        /// </summary>
        public string ZipkinEndpoint { get; set; } = "http://localhost:9411/api/v2/spans";

        /// <summary>
        /// Gets or sets the OTLP endpoint
        /// </summary>
        public string OtlpEndpoint { get; set; } = "http://localhost:4317";

        /// <summary>
        /// Gets or sets the sampling rate (0.0 to 1.0)
        /// </summary>
        public double SamplingRate { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets whether to include request headers in spans
        /// </summary>
        public bool IncludeRequestHeaders { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to include response headers in spans
        /// </summary>
        public bool IncludeResponseHeaders { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to include request body in spans
        /// </summary>
        public bool IncludeRequestBody { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to include response body in spans
        /// </summary>
        public bool IncludeResponseBody { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum request/response body size to include in spans
        /// </summary>
        public int MaxBodySizeBytes { get; set; } = 4096;
    }
}
