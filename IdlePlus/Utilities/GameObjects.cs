using System;
using System.Linq;
using IdlePlus.Utilities.Extensions;
using Il2CppInterop.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdlePlus.Utilities {
	internal static class GameObjects {
		
		public static GameObject FindByName(string name) {
			Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
			return (from t in objs where t.name == name select t.gameObject).FirstOrDefault();
		}

		public static T FindByPathNonNull<T>(string path) where T : Component {
			var obj = FindByPath(path);
			if (obj == null) throw new System.Exception($"GameObject not found by path: {path}");
			return obj.GetComponent<T>();
		}
		
		public static T FindByPath<T>(string path) where T : Component {
			var obj = FindByPath(path);
			return obj?.GetComponent<T>();
		}
		
		public static GameObject FindByPathNonNull(string path) {
			var obj = FindByPath(path);
			if (obj == null) throw new System.Exception($"GameObject not found by path: {path}");
			return obj;
		}
		
		public static GameObject FindByPath(string path) {
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
		
		public static GameObject NewRect(string name, GameObject parent = null, params Type[] components) {
			var types = new Il2CppSystem.Type[components.Length + 1];
			types[0] = Il2CppType.From(typeof(RectTransform));
			
			for (var i = 0; i < components.Length; i++) types[i + 1] = Il2CppType.From(components[i]);
			var obj = new GameObject(name, types);
			if (parent != null) obj.SetParent(parent);
			return obj;
		}

		public static GameObject NewRect<TA>(string name, GameObject parent = null) {
			return NewRect(name, parent, typeof(TA));
		}
		
		public static GameObject NewRect<TA, TB>(string name, GameObject parent = null) {
			return NewRect(name, parent, typeof(TA), typeof(TB));
		}
		
		public static GameObject NewRect<TA, TB, TC>(string name, GameObject parent = null) {
			return NewRect(name, parent, typeof(TA), typeof(TB), typeof(TC));
		}
		
		public static GameObject NewRect<TA, TB, TC, TD>(string name, GameObject parent = null) {
			return NewRect(name, parent, typeof(TA), typeof(TB), typeof(TC), typeof(TD));
		}

		public static GameObject New(string name, params Type[] components) {
			var types = new Il2CppSystem.Type[components.Length];
			for (var i = 0; i < components.Length; i++) types[i] = Il2CppType.From(components[i]);
			return new GameObject(name, types);
		}
		
		public static GameObject Instantiate(GameObject original, GameObject parent, bool worldPositionStays) {
			return Object.Instantiate(original, parent.transform, worldPositionStays);
		}
	}
}