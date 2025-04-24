using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NeoServiceLayer.Core.Extensions
{
    /// <summary>
    /// Extension methods for strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Convert a string to Base64
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>The Base64 encoded string</returns>
        public static string ToBase64(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Convert a Base64 string to a regular string
        /// </summary>
        /// <param name="base64">The Base64 string to convert</param>
        /// <returns>The decoded string</returns>
        public static string FromBase64(this string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return base64;

            byte[] bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Compute the SHA-256 hash of a string
        /// </summary>
        /// <param name="str">The string to hash</param>
        /// <returns>The hash as a hex string</returns>
        public static string ToSha256(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                
                var builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                
                return builder.ToString();
            }
        }

        /// <summary>
        /// Truncate a string to a maximum length
        /// </summary>
        /// <param name="str">The string to truncate</param>
        /// <param name="maxLength">The maximum length</param>
        /// <param name="suffix">The suffix to append if truncated</param>
        /// <returns>The truncated string</returns>
        public static string Truncate(this string str, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str;

            return str.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// Convert a string to title case
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>The title case string</returns>
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Convert a string to camel case
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>The camel case string</returns>
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length == 1)
                return str.ToLower();

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Convert a string to snake case
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>The snake case string</returns>
        public static string ToSnakeCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return Regex.Replace(
                str,
                @"([a-z0-9])([A-Z])",
                "$1_$2").ToLower();
        }

        /// <summary>
        /// Convert a string to kebab case
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>The kebab case string</returns>
        public static string ToKebabCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return Regex.Replace(
                str,
                @"([a-z0-9])([A-Z])",
                "$1-$2").ToLower();
        }

        /// <summary>
        /// Check if a string is a valid email address
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <returns>True if the string is a valid email address, false otherwise</returns>
        public static bool IsValidEmail(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            try
            {
                var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return regex.IsMatch(str);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a string is a valid URL
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <returns>True if the string is a valid URL, false otherwise</returns>
        public static bool IsValidUrl(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            return Uri.TryCreate(str, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Check if a string is a valid Neo address
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <returns>True if the string is a valid Neo address, false otherwise</returns>
        public static bool IsValidNeoAddress(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            try
            {
                var regex = new Regex(@"^N[A-Za-z0-9]{33}$");
                return regex.IsMatch(str);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a string is a valid script hash
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <returns>True if the string is a valid script hash, false otherwise</returns>
        public static bool IsValidScriptHash(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            try
            {
                var regex = new Regex(@"^0x[a-fA-F0-9]{40}$");
                return regex.IsMatch(str);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a string is a valid WIF
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <returns>True if the string is a valid WIF, false otherwise</returns>
        public static bool IsValidWif(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            try
            {
                var regex = new Regex(@"^[5KL][1-9A-HJ-NP-Za-km-z]{50,51}$");
                return regex.IsMatch(str);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Mask a string for logging or display
        /// </summary>
        /// <param name="str">The string to mask</param>
        /// <param name="visibleChars">The number of characters to leave visible at the beginning and end</param>
        /// <param name="maskChar">The character to use for masking</param>
        /// <returns>The masked string</returns>
        public static string Mask(this string str, int visibleChars = 4, char maskChar = '*')
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length <= visibleChars * 2)
                return new string(maskChar, str.Length);

            return str.Substring(0, visibleChars) + new string(maskChar, str.Length - visibleChars * 2) + str.Substring(str.Length - visibleChars);
        }

        /// <summary>
        /// Mask an email address for logging or display
        /// </summary>
        /// <param name="email">The email address to mask</param>
        /// <param name="maskChar">The character to use for masking</param>
        /// <returns>The masked email address</returns>
        public static string MaskEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            if (!email.Contains("@"))
                return email.Mask();

            var parts = email.Split('@');
            var name = parts[0];
            var domain = parts[1];

            if (name.Length <= 2)
                return name + "@" + domain;

            return name.Substring(0, 2) + new string('*', name.Length - 2) + "@" + domain;
        }
    }
}
