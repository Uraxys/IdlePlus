using System.Linq;
using UnityEngine;

namespace IdlePlus.Utilities {
	internal static class GameObjects {
		
		public static GameObject FindDisabledByName(string name) {
			Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
			return (from t in objs where t.name == name select t.gameObject).FirstOrDefault();
		}

		public static GameObject FindDisabledByPath(string path) {
			// Okay, three deep breaths...
			// *breathes in* I hate unity *breathes out* x 3
			// Probably a better solution out there.
			var parts = path.Split('/');
			Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
			foreach (var t in objs) {
				if (t.name != parts[0]) continue;
				var obj = t;
				for (var j = 1; j < parts.Length; j++) {
					obj = obj.Find(parts[j]);
					if (obj == null) {
						break;
					}
				}
				if (obj != null) return obj.gameObject;
			}
			return null;
		}

		public static GameObject FindChild(this GameObject gameObject, string name) {
			var obj = gameObject.transform.Find(name);
			return obj.gameObject;
		}
	}
}