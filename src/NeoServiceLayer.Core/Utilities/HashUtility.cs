using System;
using System.Security.Cryptography;
using System.Text;

namespace NeoServiceLayer.Core.Utilities
{
    /// <summary>
    /// Utility class for hashing operations
    /// </summary>
    public static class HashUtility
    {
        /// <summary>
        /// Compute the SHA-256 hash of a string
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeSha256Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute the SHA-256 hash of a byte array
        /// </summary>
        /// <param name="input">The input byte array</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeSha256Hash(byte[] input)
        {
            if (input == null || input.Length == 0)
            {
                return string.Empty;
            }

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(input);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute the SHA-512 hash of a string
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeSha512Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            using (var sha512 = SHA512.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha512.ComputeHash(bytes);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute the SHA-512 hash of a byte array
        /// </summary>
        /// <param name="input">The input byte array</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeSha512Hash(byte[] input)
        {
            if (input == null || input.Length == 0)
            {
                return string.Empty;
            }

            using (var sha512 = SHA512.Create())
            {
                var hashBytes = sha512.ComputeHash(input);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute the MD5 hash of a string
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeMd5Hash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(bytes);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute the MD5 hash of a byte array
        /// </summary>
        /// <param name="input">The input byte array</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeMd5Hash(byte[] input)
        {
            if (input == null || input.Length == 0)
            {
                return string.Empty;
            }

            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(input);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute the HMAC-SHA256 hash of a string
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="key">The key</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeHmacSha256Hash(string input, string key)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);
            
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(inputBytes);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute the HMAC-SHA256 hash of a byte array
        /// </summary>
        /// <param name="input">The input byte array</param>
        /// <param name="key">The key byte array</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeHmacSha256Hash(byte[] input, byte[] key)
        {
            if (input == null || input.Length == 0 || key == null || key.Length == 0)
            {
                return string.Empty;
            }

            using (var hmac = new HMACSHA256(key))
            {
                var hashBytes = hmac.ComputeHash(input);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute the HMAC-SHA512 hash of a string
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="key">The key</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeHmacSha512Hash(string input, string key)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);
            
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(inputBytes);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Compute the HMAC-SHA512 hash of a byte array
        /// </summary>
        /// <param name="input">The input byte array</param>
        /// <param name="key">The key byte array</param>
        /// <returns>The hash as a hex string</returns>
        public static string ComputeHmacSha512Hash(byte[] input, byte[] key)
        {
            if (input == null || input.Length == 0 || key == null || key.Length == 0)
            {
                return string.Empty;
            }

            using (var hmac = new HMACSHA512(key))
            {
                var hashBytes = hmac.ComputeHash(input);
                
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLowerInvariant();
            }
        }
    }
}
