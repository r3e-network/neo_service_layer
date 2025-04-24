using System;
using System.Collections.Generic;
using System.Linq;
using NeoServiceLayer.Core.Exceptions;

namespace NeoServiceLayer.Services.Common.Utilities
{
    /// <summary>
    /// Utility for validation
    /// </summary>
    public static class ValidationUtility
    {
        /// <summary>
        /// Validates that a value is not null
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="paramName">Parameter name</param>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static void ValidateNotNull(object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
            }
        }

        /// <summary>
        /// Validates that a string is not null or empty
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="paramName">Parameter name</param>
        /// <exception cref="ArgumentException">Thrown when value is null or empty</exception>
        public static void ValidateNotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
            }
        }

        /// <summary>
        /// Validates that a collection is not null or empty
        /// </summary>
        /// <typeparam name="T">Collection item type</typeparam>
        /// <param name="collection">Collection to validate</param>
        /// <param name="paramName">Parameter name</param>
        /// <exception cref="ArgumentException">Thrown when collection is null or empty</exception>
        public static void ValidateNotNullOrEmpty<T>(IEnumerable<T> collection, string paramName)
        {
            if (collection == null || !collection.Any())
            {
                throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
            }
        }

        /// <summary>
        /// Validates that a GUID is not empty
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="paramName">Parameter name</param>
        /// <exception cref="ArgumentException">Thrown when GUID is empty</exception>
        public static void ValidateGuid(Guid value, string paramName)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException($"{paramName} cannot be empty", paramName);
            }
        }

        /// <summary>
        /// Validates that a value is greater than zero
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="paramName">Parameter name</param>
        /// <exception cref="ArgumentException">Thrown when value is less than or equal to zero</exception>
        public static void ValidateGreaterThanZero(int value, string paramName)
        {
            if (value <= 0)
            {
                throw new ArgumentException($"{paramName} must be greater than zero", paramName);
            }
        }

        /// <summary>
        /// Validates that a value is greater than zero
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="paramName">Parameter name</param>
        /// <exception cref="ArgumentException">Thrown when value is less than or equal to zero</exception>
        public static void ValidateGreaterThanZero(decimal value, string paramName)
        {
            if (value <= 0)
            {
                throw new ArgumentException($"{paramName} must be greater than zero", paramName);
            }
        }

        /// <summary>
        /// Validates that a value is greater than or equal to zero
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="paramName">Parameter name</param>
        /// <exception cref="ArgumentException">Thrown when value is less than zero</exception>
        public static void ValidateGreaterThanOrEqualToZero(int value, string paramName)
        {
            if (value < 0)
            {
                throw new ArgumentException($"{paramName} must be greater than or equal to zero", paramName);
            }
        }

        /// <summary>
        /// Validates that a value is greater than or equal to zero
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="paramName">Parameter name</param>
        /// <exception cref="ArgumentException">Thrown when value is less than zero</exception>
        public static void ValidateGreaterThanOrEqualToZero(decimal value, string paramName)
        {
            if (value < 0)
            {
                throw new ArgumentException($"{paramName} must be greater than or equal to zero", paramName);
            }
        }
    }
}
