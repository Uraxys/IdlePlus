using System;
using Newtonsoft.Json.Linq;

namespace IdlePlus.Utilities.Extensions {
	public static class JsonExtension {

		public static T Do<T>(this T obj, Action<T> action) where T : JToken {
			action.Invoke(obj);
			return obj;
		}

		public static void Set(this JObject obj, string key, JObject value) => obj.Add(new JProperty(key, value));
		public static void Set(this JObject obj, string key, JArray value) => obj.Add(new JProperty(key, value));
		
		public static void Set(this JObject obj, string key, string value) => obj.Add(new JProperty(key, value));
		public static void Set(this JObject obj, string key, string[] values) => obj.Add(new JProperty(key,
			new JArray().Do(array => { foreach (var v in values) array.Add(v); })));
		
		public static void Set(this JObject obj, string key, double value) => obj.Add(new JProperty(key, value));
		public static void Set(this JObject obj, string key, float value) => obj.Add(new JProperty(key, value));
		public static void Set(this JObject obj, string key, long value) => obj.Add(new JProperty(key, value));
		public static void Set(this JObject obj, string key, int value) => obj.Add(new JProperty(key, value));
		public static void Set(this JObject obj, string key, short value) => obj.Add(new JProperty(key, value));
		public static void Set(this JObject obj, string key, byte value) => obj.Add(new JProperty(key, value));
		public static void Set(this JObject obj, string key, bool value) => obj.Add(new JProperty(key, value));
	}
}