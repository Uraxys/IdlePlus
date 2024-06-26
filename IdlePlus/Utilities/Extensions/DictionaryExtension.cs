using System;
using System.Collections.Generic;

namespace IdlePlus.Utilities.Extensions {
	public static class DictionaryExtension {
		
		public static void ReplaceAll<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, 
			Func<TKey, TValue, TValue> replaceFunc) {
			foreach (var key in dictionary.Keys) {
				dictionary[key] = replaceFunc(key, dictionary[key]);
			}
		}
		
	}
}