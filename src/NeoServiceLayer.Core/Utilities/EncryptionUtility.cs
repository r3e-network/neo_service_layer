using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NeoServiceLayer.Core.Utilities
{
    /// <summary>
    /// Utility class for encryption and decryption operations
    /// </summary>
    public static class EncryptionUtility
    {
        private const int KeySize = 32; // 256 bits
        private const int IvSize = 16; // 128 bits
        private const int SaltSize = 16; // 128 bits
        private const int Iterations = 10000;

        /// <summary>
        /// Encrypt a string using AES-256 with a password
        /// </summary>
        /// <param name="plainText">The plain text to encrypt</param>
        /// <param name="password">The password to use for encryption</param>
        /// <returns>The encrypted text as a Base64 string</returns>
        public static string EncryptWithPassword(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));
            
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Derive a key from the password and salt
            byte[] key = DeriveKeyFromPassword(password, salt);

            // Encrypt the plain text
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Generate a random IV
                aes.GenerateIV();
                byte[] iv = aes.IV;

                // Create the encryptor
                using (var encryptor = aes.CreateEncryptor(key, iv))
                using (var ms = new MemoryStream())
                {
                    // Write the salt and IV to the beginning of the stream
                    ms.Write(salt, 0, salt.Length);
                    ms.Write(iv, 0, iv.Length);

                    // Encrypt the plain text
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    // Return the salt + IV + encrypted data as a Base64 string
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypt a string using AES-256 with a password
        /// </summary>
        /// <param name="encryptedText">The encrypted text as a Base64 string</param>
        /// <param name="password">The password to use for decryption</param>
        /// <returns>The decrypted plain text</returns>
        public static string DecryptWithPassword(string encryptedText, string password)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("Encrypted text cannot be null or empty", nameof(encryptedText));
            
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Decode the Base64 string
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            // Extract the salt and IV from the beginning of the data
            byte[] salt = new byte[SaltSize];
            byte[] iv = new byte[IvSize];
            
            if (encryptedBytes.Length < SaltSize + IvSize)
                throw new ArgumentException("Encrypted text is too short", nameof(encryptedText));

            Buffer.BlockCopy(encryptedBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(encryptedBytes, SaltSize, iv, 0, IvSize);

            // Derive the key from the password and salt
            byte[] key = DeriveKeyFromPassword(password, salt);

            // Decrypt the data
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Create the decryptor
                using (var decryptor = aes.CreateDecryptor(key, iv))
                using (var ms = new MemoryStream(encryptedBytes, SaltSize + IvSize, encryptedBytes.Length - SaltSize - IvSize))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    // Return the decrypted plain text
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Encrypt a string using AES-256 with a key
        /// </summary>
        /// <param name="plainText">The plain text to encrypt</param>
        /// <param name="key">The key to use for encryption</param>
        /// <returns>The encrypted text as a Base64 string</returns>
        public static string EncryptWithKey(string plainText, byte[] key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));
            
            if (key == null || key.Length != KeySize)
                throw new ArgumentException($"Key must be {KeySize} bytes", nameof(key));

            // Encrypt the plain text
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Generate a random IV
                aes.GenerateIV();
                byte[] iv = aes.IV;

                // Create the encryptor
                using (var encryptor = aes.CreateEncryptor(key, iv))
                using (var ms = new MemoryStream())
                {
                    // Write the IV to the beginning of the stream
                    ms.Write(iv, 0, iv.Length);

                    // Encrypt the plain text
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    // Return the IV + encrypted data as a Base64 string
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypt a string using AES-256 with a key
        /// </summary>
        /// <param name="encryptedText">The encrypted text as a Base64 string</param>
        /// <param name="key">The key to use for decryption</param>
        /// <returns>The decrypted plain text</returns>
        public static string DecryptWithKey(string encryptedText, byte[] key)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("Encrypted text cannot be null or empty", nameof(encryptedText));
            
            if (key == null || key.Length != KeySize)
                throw new ArgumentException($"Key must be {KeySize} bytes", nameof(key));

            // Decode the Base64 string
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            // Extract the IV from the beginning of the data
            byte[] iv = new byte[IvSize];
            
            if (encryptedBytes.Length < IvSize)
                throw new ArgumentException("Encrypted text is too short", nameof(encryptedText));

            Buffer.BlockCopy(encryptedBytes, 0, iv, 0, IvSize);

            // Decrypt the data
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Create the decryptor
                using (var decryptor = aes.CreateDecryptor(key, iv))
                using (var ms = new MemoryStream(encryptedBytes, IvSize, encryptedBytes.Length - IvSize))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    // Return the decrypted plain text
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Generate a random key
        /// </summary>
        /// <returns>A random key</returns>
        public static byte[] GenerateRandomKey()
        {
            byte[] key = new byte[KeySize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        /// <summary>
        /// Generate a random password
        /// </summary>
        /// <param name="length">The length of the password</param>
        /// <returns>A random password</returns>
        public static string GenerateRandomPassword(int length = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";
            
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8 characters", nameof(length));

            byte[] randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var result = new StringBuilder(length);
            foreach (byte b in randomBytes)
            {
                result.Append(chars[b % chars.Length]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Compute a hash of a string using SHA-256
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The hash as a Base64 string</returns>
        public static string ComputeHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input cannot be null or empty", nameof(input));

            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Derive a key from a password and salt using PBKDF2
        /// </summary>
        /// <param name="password">The password</param>
        /// <param name="salt">The salt</param>
        /// <returns>The derived key</returns>
        private static byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(KeySize);
            }
        }
    }
}
