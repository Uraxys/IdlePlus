using System;
using Newtonsoft.Json;

namespace IdlePlus.Utilities
{
    /// <summary>
    /// Provides methods for converting objects to JSON strings.
    /// Supports overloads for System.String, Il2CppSystem.String, and Il2CppSystem.Object.
    /// </summary>
    public static class JsonSerializer
    {
        /// <summary>
        /// Returns the provided System.String unchanged.
        /// </summary>
        /// <param name="data">A System.String representing a JSON string.</param>
        /// <returns>The same string.</returns>
        public static string ToJsonString(string data)
        {
            return data;
        }

        /// <summary>
        /// Converts an Il2CppSystem.String to a System.String.
        /// </summary>
        /// <param name="data">An Il2CppSystem.String representing a JSON string.</param>
        /// <returns>The string representation of the Il2CppSystem.String.</returns>
        public static string ToJsonString(Il2CppSystem.String data)
        {
            return data.ToString();
        }

        /// <summary>
        /// Serializes the provided Il2CppSystem.Object (that is not a string) to a JSON string.
        /// </summary>
        /// <param name="data">An Il2CppSystem.Object to serialize.</param>
        /// <returns>
        /// A JSON string representing the object, or "{}" if serialization fails.
        /// </returns>
        public static string ToJsonString(Il2CppSystem.Object data)
        {
            try
            {
                return JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                IdleLog.Error($"[JsonSerializer] Error serializing data: {ex.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Overload for handling objects by casting them to Il2CppSystem.Object.
        /// Note: The provided object must be compatible with Il2CppSystem.Object.
        /// </summary>
        /// <param name="data">An object to serialize.</param>
        /// <returns>
        /// A JSON string representing the object, or "{}" if serialization fails.
        /// </returns>
        public static string ToJsonString(object data)
        {
            try
            {
                return JsonConvert.SerializeObject((Il2CppSystem.Object)data);
            }
            catch (Exception ex)
            {
                IdleLog.Error($"[JsonSerializer] Error serializing data: {ex.Message}");
                return "{}";
            }
        }
    }
}
