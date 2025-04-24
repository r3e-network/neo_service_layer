using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for updating a secret's allowed functions
    /// </summary>
    public class UpdateAllowedFunctionsRequest
    {
        /// <summary>
        /// List of function IDs that are allowed to access this secret
        /// </summary>
        [Required]
        public List<Guid> AllowedFunctionIds { get; set; }
    }
}
