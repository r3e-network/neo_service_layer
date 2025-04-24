using System;
using System.Text.RegularExpressions;

namespace NeoServiceLayer.Services.Common.Extensions
{
    /// <summary>
    /// Extensions for string
    /// </summary>
    public static class StringExtensions
    {
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex NeoAddressRegex = new Regex(@"^N[A-Za-z0-9]{33}$", RegexOptions.Compiled);

        /// <summary>
        /// Checks if a string is a valid email
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            return EmailRegex.IsMatch(email);
        }

        /// <summary>
        /// Checks if a string is a valid Neo address
        /// </summary>
        /// <param name="address">Address to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidNeoAddress(this string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            return NeoAddressRegex.IsMatch(address);
        }
    }
}
