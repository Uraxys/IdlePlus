using System.Collections.Generic;

namespace IdlePlus.Utilities.Extensions {
	public static class ListExtension {
		
		public static T RemoveAndGet<T>(this List<T> list, int index) {
			var item = list[index];
			list.RemoveAt(index);
			return item;
		}
		
		public static bool IsEmpty<T>(this List<T> list) {
			return list.Count == 0;
		}
		
		public static bool IsEmpty<T>(this ICollection<T> collection) {
			return collection.Count == 0;
		}
	}
}