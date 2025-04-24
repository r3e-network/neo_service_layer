namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents resources for a function deployment
    /// </summary>
    public class FunctionDeploymentResources
    {
        /// <summary>
        /// Gets or sets the CPU allocation in millicores
        /// </summary>
        public int CpuMillicores { get; set; } = 100;

        /// <summary>
        /// Gets or sets the memory allocation in megabytes
        /// </summary>
        public int MemoryMb { get; set; } = 128;

        /// <summary>
        /// Gets or sets the disk allocation in megabytes
        /// </summary>
        public int DiskMb { get; set; } = 512;

        /// <summary>
        /// Gets or sets the number of instances
        /// </summary>
        public int Instances { get; set; } = 1;

        /// <summary>
        /// Gets or sets the minimum number of instances
        /// </summary>
        public int MinInstances { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum number of instances
        /// </summary>
        public int MaxInstances { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether to enable auto-scaling
        /// </summary>
        public bool EnableAutoScaling { get; set; } = true;

        /// <summary>
        /// Gets or sets the CPU target utilization percentage for auto-scaling
        /// </summary>
        public int CpuTargetUtilizationPercentage { get; set; } = 70;

        /// <summary>
        /// Gets or sets the memory target utilization percentage for auto-scaling
        /// </summary>
        public int MemoryTargetUtilizationPercentage { get; set; } = 70;

        /// <summary>
        /// Gets or sets the concurrency per instance
        /// </summary>
        public int ConcurrencyPerInstance { get; set; } = 10;

        /// <summary>
        /// Gets or sets the maximum concurrency
        /// </summary>
        public int MaxConcurrency { get; set; } = 100;

        /// <summary>
        /// Gets or sets the timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the idle timeout in seconds
        /// </summary>
        public int IdleTimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Gets or sets a value indicating whether to enable GPU
        /// </summary>
        public bool EnableGpu { get; set; } = false;

        /// <summary>
        /// Gets or sets the GPU type
        /// </summary>
        public string GpuType { get; set; } = "none";

        /// <summary>
        /// Gets or sets the number of GPUs
        /// </summary>
        public int GpuCount { get; set; } = 0;
    }
}
