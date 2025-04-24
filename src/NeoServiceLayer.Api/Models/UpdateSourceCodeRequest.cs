using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for updating a function's source code
    /// </summary>
    public class UpdateSourceCodeRequest
    {
        /// <summary>
        /// Source code of the function
        /// </summary>
        [Required]
        public string SourceCode { get; set; }
    }
}
