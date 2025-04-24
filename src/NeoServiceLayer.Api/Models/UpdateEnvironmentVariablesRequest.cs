using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for updating a function's environment variables
    /// </summary>
    public class UpdateEnvironmentVariablesRequest
    {
        /// <summary>
        /// Environment variables for the function
        /// </summary>
        [Required]
        public Dictionary<string, string> EnvironmentVariables { get; set; }
    }
}
