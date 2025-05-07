using System;
using Newtonsoft.Json;

namespace IdlePlus.Utilities {
	/// <summary>
	/// Helper class for JSON operations.
	/// </summary>
	public static class JsonHelper {
		/// <summary>
		/// Takes a JSON string, validates it, and returns it if valid.
		/// </summary>
		/// <param name="json">The JSON string to validate.</param>
		/// <returns>The same JSON string if it is valid.</returns>
		/// <exception cref="ArgumentException">Thrown when the provided string is not valid JSON.</exception>
		public static string Serialize(string json) {
			if (string.IsNullOrEmpty(json))
				return json;

			if (!IsValidJson(json))
				throw new ArgumentException($"The provided string is not valid JSON. Value: {json}", nameof(json));

			return json;
		}

		/// <summary>
		/// Serializes an Il2CppSystem.Object using JsonConvert.
		/// </summary>
		/// <param name="il2cppObj">The Il2CppSystem.Object to serialize.</param>
		/// <returns>The resulting JSON string.</returns>
		/// <exception cref="InvalidOperationException">Thrown when serialization fails or produces invalid JSON.</exception>
		public static string Serialize(Il2CppSystem.Object il2cppObj) {
			if (il2cppObj == null)
				return null;

			try {
				string result = JsonConvert.SerializeObject(il2cppObj);
				if (IsValidJson(result)) {
					return result;
				} else {
					throw new InvalidOperationException($"Serialization produced invalid JSON: {result}");
				}
			} catch (Exception ex) {
				throw new InvalidOperationException("Error serializing the Il2Cpp object.", ex);
			}
		}

		/// <summary>
		/// Validates whether the given string is valid JSON.
		/// </summary>
		/// <param name="json">The JSON string to validate.</param>
		/// <returns>True if the string is valid JSON; otherwise, false.</returns>
		public static bool IsValidJson(string json) {
			if (string.IsNullOrEmpty(json))
				return false;

			json = json.Trim();

			if ((json.StartsWith("{") && json.EndsWith("}")) ||
				(json.StartsWith("[") && json.EndsWith("]"))) {
				try {
					// Use JsonConvert for validation
					JsonConvert.DeserializeObject(json);
					return true;
				} catch (Exception) {
					return false;
				}
			}

			return false;
		}
	}
}