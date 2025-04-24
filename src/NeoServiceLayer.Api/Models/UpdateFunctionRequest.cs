using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for updating a function
    /// </summary>
    public class UpdateFunctionRequest
    {
        /// <summary>
        /// Name for the function
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Description of the function
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Entry point for the function
        /// </summary>
        [Required]
        public string EntryPoint { get; set; }

        /// <summary>
        /// Maximum execution time in milliseconds
        /// </summary>
        [Range(1000, 300000)]
        public int MaxExecutionTime { get; set; }

        /// <summary>
        /// Maximum memory in megabytes
        /// </summary>
        [Range(64, 1024)]
        public int MaxMemory { get; set; }
    }
}
