namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Options for storage service
    /// </summary>
    public class StorageOptions
    {
        /// <summary>
        /// Gets or sets the AWS region
        /// </summary>
        public string AwsRegion { get; set; }

        /// <summary>
        /// Gets or sets the S3 bucket name
        /// </summary>
        public string S3BucketName { get; set; }

        /// <summary>
        /// Gets or sets the S3 endpoint URL
        /// </summary>
        public string S3EndpointUrl { get; set; }

        /// <summary>
        /// Gets or sets the AWS access key ID
        /// </summary>
        public string AwsAccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets the AWS secret access key
        /// </summary>
        public string AwsSecretAccessKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use local storage
        /// </summary>
        public bool UseLocalStorage { get; set; }

        /// <summary>
        /// Gets or sets the local storage path
        /// </summary>
        public string LocalStoragePath { get; set; }

        /// <summary>
        /// Gets or sets the base URL for file access
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the maximum file size in bytes
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 104857600; // 100 MB

        /// <summary>
        /// Gets or sets the maximum storage per account in bytes
        /// </summary>
        public long MaxStoragePerAccountBytes { get; set; } = 1073741824; // 1 GB

        /// <summary>
        /// Gets or sets the maximum storage per function in bytes
        /// </summary>
        public long MaxStoragePerFunctionBytes { get; set; } = 104857600; // 100 MB

        /// <summary>
        /// Gets or sets the maximum key-value pairs per account
        /// </summary>
        public int MaxKeyValuePairsPerAccount { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the maximum key-value pairs per function
        /// </summary>
        public int MaxKeyValuePairsPerFunction { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum key size in bytes
        /// </summary>
        public int MaxKeySizeBytes { get; set; } = 1024; // 1 KB

        /// <summary>
        /// Gets or sets the maximum value size in bytes
        /// </summary>
        public int MaxValueSizeBytes { get; set; } = 102400; // 100 KB
    }
}
