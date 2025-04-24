using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeoServiceLayer.Core.Utilities
{
    /// <summary>
    /// Utility class for validation operations
    /// </summary>
    public static class ValidationUtility
    {
        private static readonly Regex EmailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        private static readonly Regex NeoAddressRegex = new Regex(@"^N[A-Za-z0-9]{33}$", RegexOptions.Compiled);
        private static readonly Regex ScriptHashRegex = new Regex(@"^0x[a-fA-F0-9]{40}$", RegexOptions.Compiled);
        private static readonly Regex WifRegex = new Regex(@"^[5KL][1-9A-HJ-NP-Za-km-z]{50,51}$", RegexOptions.Compiled);

        /// <summary>
        /// Validate that a value is not null or empty
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the value is null or empty</exception>
        public static void ValidateNotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }

        /// <summary>
        /// Validate that a value is not null
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentNullException">Thrown if the value is null</exception>
        public static void ValidateNotNull(object value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
        }

        /// <summary>
        /// Validate that a value is not default
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <exception cref="ArgumentException">Thrown if the value is default</exception>
        public static void ValidateNotDefault<T>(T value, string paramName) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(value, default))
                throw new ArgumentException($"{paramName} cannot be default", paramName);
        }

        /// <summary>
        /// Validate that a value is greater than zero
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the value is not greater than zero</exception>
        public static void ValidateGreaterThanZero(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException($"{paramName} must be greater than zero", paramName);
        }

        /// <summary>
        /// Validate that a value is greater than zero
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the value is not greater than zero</exception>
        public static void ValidateGreaterThanZero(long value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException($"{paramName} must be greater than zero", paramName);
        }

        /// <summary>
        /// Validate that a value is greater than zero
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the value is not greater than zero</exception>
        public static void ValidateGreaterThanZero(decimal value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException($"{paramName} must be greater than zero", paramName);
        }

        /// <summary>
        /// Validate that a value is greater than zero
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the value is not greater than zero</exception>
        public static void ValidateGreaterThanZero(double value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException($"{paramName} must be greater than zero", paramName);
        }

        /// <summary>
        /// Validate that a value is greater than or equal to zero
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the value is not greater than or equal to zero</exception>
        public static void ValidateGreaterThanOrEqualToZero(int value, string paramName)
        {
            if (value < 0)
                throw new ArgumentException($"{paramName} must be greater than or equal to zero", paramName);
        }

        /// <summary>
        /// Validate that a value is greater than or equal to zero
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the value is not greater than or equal to zero</exception>
        public static void ValidateGreaterThanOrEqualToZero(long value, string paramName)
        {
            if (value < 0)
                throw new ArgumentException($"{paramName} must be greater than or equal to zero", paramName);
        }

        /// <summary>
        /// Validate that a value is greater than or equal to zero
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the value is not greater than or equal to zero</exception>
        public static void ValidateGreaterThanOrEqualToZero(decimal value, string paramName)
        {
            if (value < 0)
                throw new ArgumentException($"{paramName} must be greater than or equal to zero", paramName);
        }

        /// <summary>
        /// Validate that a value is greater than or equal to zero
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the value is not greater than or equal to zero</exception>
        public static void ValidateGreaterThanOrEqualToZero(double value, string paramName)
        {
            if (value < 0)
                throw new ArgumentException($"{paramName} must be greater than or equal to zero", paramName);
        }

        /// <summary>
        /// Validate that a collection is not null or empty
        /// </summary>
        /// <param name="collection">The collection to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <typeparam name="T">The type of the collection elements</typeparam>
        /// <exception cref="ArgumentException">Thrown if the collection is null or empty</exception>
        public static void ValidateNotNullOrEmpty<T>(IEnumerable<T> collection, string paramName)
        {
            if (collection == null || !collection.Any())
                throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid email address
        /// </summary>
        /// <param name="email">The email address to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the email address is not valid</exception>
        public static void ValidateEmail(string email, string paramName)
        {
            ValidateNotNullOrEmpty(email, paramName);
            
            if (!EmailRegex.IsMatch(email))
                throw new ArgumentException($"{paramName} is not a valid email address", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid Neo address
        /// </summary>
        /// <param name="address">The Neo address to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the Neo address is not valid</exception>
        public static void ValidateNeoAddress(string address, string paramName)
        {
            ValidateNotNullOrEmpty(address, paramName);
            
            if (!NeoAddressRegex.IsMatch(address))
                throw new ArgumentException($"{paramName} is not a valid Neo address", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid script hash
        /// </summary>
        /// <param name="scriptHash">The script hash to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the script hash is not valid</exception>
        public static void ValidateScriptHash(string scriptHash, string paramName)
        {
            ValidateNotNullOrEmpty(scriptHash, paramName);
            
            if (!ScriptHashRegex.IsMatch(scriptHash))
                throw new ArgumentException($"{paramName} is not a valid script hash", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid WIF
        /// </summary>
        /// <param name="wif">The WIF to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the WIF is not valid</exception>
        public static void ValidateWif(string wif, string paramName)
        {
            ValidateNotNullOrEmpty(wif, paramName);
            
            if (!WifRegex.IsMatch(wif))
                throw new ArgumentException($"{paramName} is not a valid WIF", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid password
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the password is not valid</exception>
        public static void ValidatePassword(string password, string paramName)
        {
            ValidateNotNullOrEmpty(password, paramName);
            
            if (password.Length < 8)
                throw new ArgumentException($"{paramName} must be at least 8 characters long", paramName);
            
            if (!password.Any(char.IsUpper))
                throw new ArgumentException($"{paramName} must contain at least one uppercase letter", paramName);
            
            if (!password.Any(char.IsLower))
                throw new ArgumentException($"{paramName} must contain at least one lowercase letter", paramName);
            
            if (!password.Any(char.IsDigit))
                throw new ArgumentException($"{paramName} must contain at least one digit", paramName);
            
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                throw new ArgumentException($"{paramName} must contain at least one special character", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid GUID
        /// </summary>
        /// <param name="guid">The GUID to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the GUID is not valid</exception>
        public static void ValidateGuid(Guid guid, string paramName)
        {
            if (guid == Guid.Empty)
                throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid GUID string
        /// </summary>
        /// <param name="guidString">The GUID string to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the GUID string is not valid</exception>
        public static void ValidateGuidString(string guidString, string paramName)
        {
            ValidateNotNullOrEmpty(guidString, paramName);
            
            if (!Guid.TryParse(guidString, out var guid) || guid == Guid.Empty)
                throw new ArgumentException($"{paramName} is not a valid GUID", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid date
        /// </summary>
        /// <param name="date">The date to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the date is not valid</exception>
        public static void ValidateDate(DateTime date, string paramName)
        {
            if (date == default)
                throw new ArgumentException($"{paramName} cannot be default", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid date string
        /// </summary>
        /// <param name="dateString">The date string to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the date string is not valid</exception>
        public static void ValidateDateString(string dateString, string paramName)
        {
            ValidateNotNullOrEmpty(dateString, paramName);
            
            if (!DateTime.TryParse(dateString, out var date) || date == default)
                throw new ArgumentException($"{paramName} is not a valid date", paramName);
        }

        /// <summary>
        /// Validate that a value is a valid URL
        /// </summary>
        /// <param name="url">The URL to validate</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentException">Thrown if the URL is not valid</exception>
        public static void ValidateUrl(string url, string paramName)
        {
            ValidateNotNullOrEmpty(url, paramName);
            
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || 
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException($"{paramName} is not a valid URL", paramName);
        }
    }
}
