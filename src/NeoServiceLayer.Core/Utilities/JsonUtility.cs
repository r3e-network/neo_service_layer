using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoServiceLayer.Core.Utilities
{
    /// <summary>
    /// Utility class for JSON serialization and deserialization
    /// </summary>
    public static class JsonUtility
    {
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter() }
        };

        private static readonly JsonSerializerOptions IndentedOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Serialize an object to a JSON string
        /// </summary>
        /// <param name="value">The object to serialize</param>
        /// <param name="indented">Whether to use indented formatting</param>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <returns>The JSON string</returns>
        public static string Serialize<T>(T value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, indented ? IndentedOptions : DefaultOptions);
        }

        /// <summary>
        /// Serialize an object to a JSON byte array
        /// </summary>
        /// <param name="value">The object to serialize</param>
        /// <param name="indented">Whether to use indented formatting</param>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <returns>The JSON byte array</returns>
        public static byte[] SerializeToUtf8Bytes<T>(T value, bool indented = false)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, indented ? IndentedOptions : DefaultOptions);
        }

        /// <summary>
        /// Deserialize a JSON string to an object
        /// </summary>
        /// <param name="json">The JSON string</param>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <returns>The deserialized object</returns>
        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }

        /// <summary>
        /// Deserialize a JSON byte array to an object
        /// </summary>
        /// <param name="utf8Json">The JSON byte array</param>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <returns>The deserialized object</returns>
        public static T Deserialize<T>(byte[] utf8Json)
        {
            return JsonSerializer.Deserialize<T>(utf8Json, DefaultOptions);
        }

        /// <summary>
        /// Try to deserialize a JSON string to an object
        /// </summary>
        /// <param name="json">The JSON string</param>
        /// <param name="result">The deserialized object</param>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <returns>True if deserialization was successful, false otherwise</returns>
        public static bool TryDeserialize<T>(string json, out T result)
        {
            try
            {
                result = Deserialize<T>(json);
                return true;
            }
            catch (JsonException)
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Try to deserialize a JSON byte array to an object
        /// </summary>
        /// <param name="utf8Json">The JSON byte array</param>
        /// <param name="result">The deserialized object</param>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <returns>True if deserialization was successful, false otherwise</returns>
        public static bool TryDeserialize<T>(byte[] utf8Json, out T result)
        {
            try
            {
                result = Deserialize<T>(utf8Json);
                return true;
            }
            catch (JsonException)
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Parse a JSON string to a JsonElement
        /// </summary>
        /// <param name="json">The JSON string</param>
        /// <returns>The JsonElement</returns>
        public static JsonElement Parse(string json)
        {
            return JsonSerializer.Deserialize<JsonElement>(json, DefaultOptions);
        }

        /// <summary>
        /// Parse a JSON byte array to a JsonElement
        /// </summary>
        /// <param name="utf8Json">The JSON byte array</param>
        /// <returns>The JsonElement</returns>
        public static JsonElement Parse(byte[] utf8Json)
        {
            return JsonSerializer.Deserialize<JsonElement>(utf8Json, DefaultOptions);
        }

        /// <summary>
        /// Try to parse a JSON string to a JsonElement
        /// </summary>
        /// <param name="json">The JSON string</param>
        /// <param name="result">The JsonElement</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public static bool TryParse(string json, out JsonElement result)
        {
            try
            {
                result = Parse(json);
                return true;
            }
            catch (JsonException)
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Try to parse a JSON byte array to a JsonElement
        /// </summary>
        /// <param name="utf8Json">The JSON byte array</param>
        /// <param name="result">The JsonElement</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public static bool TryParse(byte[] utf8Json, out JsonElement result)
        {
            try
            {
                result = Parse(utf8Json);
                return true;
            }
            catch (JsonException)
            {
                result = default;
                return false;
            }
        }
    }
}
