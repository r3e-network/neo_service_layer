using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServiceLayer.Enclave.Enclave.Utilities
{
    /// <summary>
    /// Utility for JSON serialization and deserialization
    /// </summary>
    public static class JsonUtility
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        /// <summary>
        /// Serializes an object to a JSON string
        /// </summary>
        /// <param name="value">The object to serialize</param>
        /// <returns>JSON string</returns>
        public static string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, _options);
        }

        /// <summary>
        /// Serializes an object to a UTF-8 encoded JSON byte array
        /// </summary>
        /// <param name="value">The object to serialize</param>
        /// <returns>UTF-8 encoded JSON byte array</returns>
        public static byte[] SerializeToUtf8Bytes(object value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of type T
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="json">The JSON string</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON byte array to an object of type T
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="utf8Json">The UTF-8 encoded JSON byte array</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(byte[] utf8Json)
        {
            return JsonSerializer.Deserialize<T>(utf8Json, _options);
        }

        /// <summary>
        /// Deserializes a UTF-8 encoded JSON byte array to an object of type T
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="utf8Json">The UTF-8 encoded JSON byte array</param>
        /// <param name="options">JSON serializer options</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(byte[] utf8Json, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(utf8Json, options);
        }
    }
}
