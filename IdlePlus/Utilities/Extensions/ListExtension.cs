using System.Collections.Generic;

namespace IdlePlus.Utilities.Extensions {
	public static class ListExtension {
		
		public static T RemoveAndGet<T>(this List<T> list, int index) {
			var item = list[index];
			list.RemoveAt(index);
			return item;
		}
		
	}
}