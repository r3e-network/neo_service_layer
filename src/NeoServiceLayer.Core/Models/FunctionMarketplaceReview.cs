using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a review for a function marketplace item
    /// </summary>
    public class FunctionMarketplaceReview
    {
        /// <summary>
        /// Gets or sets the review ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Gets or sets the reviewer ID
        /// </summary>
        public Guid ReviewerId { get; set; }

        /// <summary>
        /// Gets or sets the reviewer name
        /// </summary>
        public string ReviewerName { get; set; }

        /// <summary>
        /// Gets or sets the rating
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Gets or sets the title of the review
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the content of the review
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the review is verified
        /// </summary>
        public bool IsVerified { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of helpful votes
        /// </summary>
        public int HelpfulCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of unhelpful votes
        /// </summary>
        public int UnhelpfulCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the version of the item that was reviewed
        /// </summary>
        public string ItemVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the review is hidden
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// Gets or sets the reason for hiding the review
        /// </summary>
        public string HiddenReason { get; set; }
    }
}
