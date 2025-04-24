using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for user login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Username or email address
        /// </summary>
        [Required]
        public string UsernameOrEmail { get; set; }

        /// <summary>
        /// Password for the account
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
