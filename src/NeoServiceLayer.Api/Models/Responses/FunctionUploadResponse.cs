using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Api.Models.Responses
{
    /// <summary>
    /// Response model for a function upload
    /// </summary>
    public class FunctionUploadResponse
    {
        /// <summary>
        /// Gets or sets the file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file size
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the upload time
        /// </summary>
        public DateTime UploadTime { get; set; }

        /// <summary>
        /// Gets or sets the list of created functions
        /// </summary>
        public List<FunctionResponse> Functions { get; set; }
    }
}
