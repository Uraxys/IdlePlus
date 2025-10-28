using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scripts.Shared.Data.Content.Skills;

namespace IdlePlus.Utilities.Helpers {
	/// <summary>
	/// Helper class for JSON operations.
	/// </summary>
	public static class JsonHelper {
		/// <summary>
		/// Represents a delegate for mapping JSON string values to objects.
		/// </summary>
		private delegate object JsonStringMapper(string value);

		/// <summary>
		/// Represents a collection of mappings used to convert JSON string values to objects of various types.
		/// </summary>
		private static readonly Dictionary<Type, JsonStringMapper> StringMappers =
			new Dictionary<Type, JsonStringMapper> { 
				{ typeof(Skill), s => { 
					if (s == "attack") s = "rigour"; 
					return Enum.Parse(typeof(Skill), s, true); 
				}}
			};
		
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

		/// <summary>
		/// Converts a JSON token to a specified type. We're using this instead of an in-built method
		/// because not all types works correctly when used with Il2Cpp.
		/// </summary>
		/// <param name="token">The JSON token to convert.</param>
		/// <typeparam name="T">The type to convert the token to.</typeparam>
		/// <returns>The converted value of the JSON token as the specified type.</returns>
		public static T ToValue<T>(JToken token) {
			if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined) 
				return default;
			
			var targetType = typeof(T);
			var value = token.Cast<JValue>();
			var type = value.Type;

			if (StringMappers.TryGetValue(targetType, out var mapper)) {
				return (T)mapper(value.ToString());
			}
			
			if (targetType.IsEnum) {
				return (T) Enum.Parse(targetType, value.ToString(), true);
			}

			// IL2CPP double issues.
			if (targetType == typeof(double) && type == JTokenType.Float) {
				return (T) (object) (double) value;
			}
			
			return value.ToObject<T>();
		}

		/// <summary>
		/// Converts a JToken representing a JSON object into a dictionary with specified key and value types.
		/// Supports only simple types, such as int, string, enums, etc.
		/// </summary>
		/// <param name="token">The JToken representing the JSON object to convert.</param>
		/// <typeparam name="TA">The type of the dictionary key.</typeparam>
		/// <typeparam name="TB">The type of the dictionary value.</typeparam>
		/// <returns>A Dictionary containing the converted key-value pairs.</returns>
		/// <exception cref="ArgumentException">Thrown when the input JToken is not a JSON object.</exception>
		public static Dictionary<TA, TB> ToSimpleDictionary<TA, TB>(JToken token) {
			if (token.Type != JTokenType.Object)
				throw new ArgumentException(
					$"Expected a JSON object, but got {token.Type} while converting to dictionary.");
			
			var obj = token.Cast<JObject>();
			var dict = new Dictionary<TA, TB>();
			if (obj._properties?._dictionary == null)
				return dict;
			
			foreach (var entry in obj._properties._dictionary) {
				var key = SimpleDictionaryKeyMapper<TA>(entry.Key);
				if (key == null) throw new ArgumentException($"Failed to convert entry: {entry.Key}");
				
				var property = entry.Value.Cast<JProperty>();
				var value = ToValue<TB>(property.Value);
				
				dict.Add(key, value);
			}

			return dict;
		}
		
		public static Il2CppSystem.Collections.Generic.Dictionary<TA, TB> ToSimpleIL2CppDictionary<TA, TB>(JToken token) {
			if (token.Type != JTokenType.Object)
				throw new ArgumentException(
					$"Expected a JSON object, but got {token.Type} while converting to dictionary.");
			
			var obj = token.Cast<JObject>();
			var dict = new Il2CppSystem.Collections.Generic.Dictionary<TA, TB>();
			if (obj._properties?._dictionary == null)
				return dict;
			
			foreach (var entry in obj._properties._dictionary) {
				var key = SimpleDictionaryKeyMapper<TA>(entry.Key);
				if (key == null) throw new ArgumentException($"Failed to convert entry: {entry.Key}");
				
				var property = entry.Value.Cast<JProperty>();
				var value = ToValue<TB>(property.Value);
				
				dict.Add(key, value);
			}

			return dict;
		}
		
		private static TA SimpleDictionaryKeyMapper<TA>(string key) {
			if (string.IsNullOrEmpty(key)) return default;
			var type = typeof(TA);
			
			if (StringMappers.TryGetValue(type, out var mapper)) {
				return (TA)mapper(key);
			}
			
			if (type.IsEnum) {
				return (TA)Enum.Parse(type, key, true);
			}
			
			// Basic types
			switch (Type.GetTypeCode(type)) {
				case TypeCode.Byte: return (TA)(object)byte.Parse(key);
				case TypeCode.SByte: return (TA)(object)sbyte.Parse(key);
				case TypeCode.Int16: return (TA)(object)short.Parse(key);
				case TypeCode.UInt16: return (TA)(object)ushort.Parse(key);
				case TypeCode.Int32: return (TA)(object)int.Parse(key);
				case TypeCode.UInt32: return (TA)(object)uint.Parse(key);
				case TypeCode.Int64: return (TA)(object)long.Parse(key);
				case TypeCode.UInt64: return (TA)(object)ulong.Parse(key);
				case TypeCode.Single: return (TA)(object)float.Parse(key);
				case TypeCode.Double: return (TA)(object)double.Parse(key);
				case TypeCode.String: return (TA)(object)key;
				default: 
					throw new InvalidOperationException($"Cannot convert key '{key}' to type '{typeof(TA).Name}'.");
			}
		}
	}
}