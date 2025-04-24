using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an item in the function marketplace
    /// </summary>
    public class FunctionMarketplaceItem
    {
        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the publisher ID
        /// </summary>
        public Guid PublisherId { get; set; }

        /// <summary>
        /// Gets or sets the publisher name
        /// </summary>
        public string PublisherName { get; set; }

        /// <summary>
        /// Gets or sets the version of the item
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the price of the item
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the currency of the price
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Gets or sets a value indicating whether the item is free
        /// </summary>
        public bool IsFree { get; set; } = false;

        /// <summary>
        /// Gets or sets the category of the item
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the tags for the item
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime for the item
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the published at timestamp
        /// </summary>
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is published
        /// </summary>
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the item is featured
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the item is verified
        /// </summary>
        public bool IsVerified { get; set; } = false;

        /// <summary>
        /// Gets or sets the rating of the item
        /// </summary>
        public double Rating { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of ratings
        /// </summary>
        public int RatingCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of downloads
        /// </summary>
        public int DownloadCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the license of the item
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// Gets or sets the documentation URL
        /// </summary>
        public string DocumentationUrl { get; set; }

        /// <summary>
        /// Gets or sets the source code URL
        /// </summary>
        public string SourceCodeUrl { get; set; }

        /// <summary>
        /// Gets or sets the website URL
        /// </summary>
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Gets or sets the support URL
        /// </summary>
        public string SupportUrl { get; set; }

        /// <summary>
        /// Gets or sets the screenshots
        /// </summary>
        public List<string> Screenshots { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the icon URL
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Gets or sets the readme content
        /// </summary>
        public string ReadmeContent { get; set; }

        /// <summary>
        /// Gets or sets the dependencies
        /// </summary>
        public List<FunctionDependency> Dependencies { get; set; } = new List<FunctionDependency>();

        /// <summary>
        /// Gets or sets the required permissions
        /// </summary>
        public List<string> RequiredPermissions { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the terms and conditions
        /// </summary>
        public string TermsAndConditions { get; set; }

        /// <summary>
        /// Gets or sets the privacy policy
        /// </summary>
        public string PrivacyPolicy { get; set; }
    }
}
