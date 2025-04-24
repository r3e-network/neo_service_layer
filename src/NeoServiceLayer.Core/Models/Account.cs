using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a user account in the Neo Service Layer
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Unique identifier for the account
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Username for the account
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Email address for the account
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Hashed password for the account
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Salt used for password hashing
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Neo N3 address associated with the account
        /// </summary>
        public string NeoAddress { get; set; }

        /// <summary>
        /// Indicates whether the account is verified
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Indicates whether the account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates whether the account has administrator privileges
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Date and time when the account was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the account was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Current balance of credits for the account
        /// </summary>
        public decimal Credits { get; set; }
    }
}
