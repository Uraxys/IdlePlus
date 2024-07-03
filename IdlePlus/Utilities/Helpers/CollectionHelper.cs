using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace IdlePlus.Utilities.Helpers {
	public static class CollectionHelper {
		
		public static Il2CppReferenceArray<T> RefArrayInsert<T>(int index, T item, Il2CppReferenceArray<T> array)
			where T : Il2CppObjectBase {
			
			var newArray = new Il2CppReferenceArray<T>(array.Length + 1);
			for (var i = 0; i < index; i++) newArray[i] = array[i];
			newArray[index] = item;
			for (var i = index; i < array.Length; i++) newArray[i + 1] = array[i];
			return newArray;
		}
		
	}
}