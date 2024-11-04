using System;
using Crosstales;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdlePlus.Utilities.Extensions {
	public static class GameObjectExtension {
		
		public static void SetParent(this GameObject gameObject, GameObject parent, bool worldPositionStays = false,
			int siblingIndex = -1) {
			gameObject.SetParent(parent.transform, worldPositionStays, siblingIndex);
		}
		
		public static void SetParent(this GameObject gameObject, Transform parent, bool worldPositionStays = false, 
			int siblingIndex = -1) {
			gameObject.transform.SetParent(parent, worldPositionStays);
			if (siblingIndex >= 0) gameObject.transform.SetSiblingIndex(siblingIndex);
		}
		
		public static void DestroyChildren(this GameObject gameObject, bool immediate = false) {
			foreach (var child in gameObject.transform.getAllChildren()) Destroy(child.gameObject, immediate);
		}
		
		public static GameObject Find(this GameObject gameObject, string path, Action<GameObject> action = null) {
			var child = gameObject.transform.Find(path);
			if (child == null) return null;
			action?.Invoke(child.gameObject);
			return child.gameObject;
		}
		
		public static GameObject FindNonNull(this GameObject gameObject, string path, Action<GameObject> action = null) {
			var child = gameObject.transform.Find(path);
			if (child == null) {
				IdleLog.Error($"GameObject not found by path: {path}");
				throw new Exception($"GameObject not found by path: {path}");
			}
			action?.Invoke(child.gameObject);
			return child.gameObject;
		}
		
		#region Destroy Component

		public static void DestroyComponent<TA>(this GameObject gameObject, bool immediate = false) 
			where TA : Component {
			Destroy(gameObject.GetComponent<TA>(), immediate);
		}
		
		public static void DestroyComponent<TA, TB>(this GameObject gameObject, bool immediate = false) 
			where TA : Component where TB : Component {
			Destroy(gameObject.GetComponent<TA>(), immediate);
			Destroy(gameObject.GetComponent<TB>(), immediate);
		}
		
		public static void DestroyComponent<TA, TB, TC>(this GameObject gameObject, bool immediate = false)
			where TA : Component where TB : Component where TC : Component {
			Destroy(gameObject.GetComponent<TA>(), immediate);
			Destroy(gameObject.GetComponent<TB>(), immediate);
			Destroy(gameObject.GetComponent<TC>(), immediate);
		}
		
		public static void DestroyComponent<TA, TB, TC, TD>(this GameObject gameObject, bool immediate = false)
			where TA : Component where TB : Component where TC : Component where TD : Component {
			Destroy(gameObject.GetComponent<TA>(), immediate);
			Destroy(gameObject.GetComponent<TB>(), immediate);
			Destroy(gameObject.GetComponent<TC>(), immediate);
			Destroy(gameObject.GetComponent<TD>(), immediate);
		}
		
		private static void Destroy(Component component, bool immediate = false) {
			if (component == null) return;
			if (immediate) Object.DestroyImmediate(component);
			else Object.Destroy(component);
		}
		
		private static void Destroy(GameObject component, bool immediate = false) {
			if (component == null) return;
			if (immediate) Object.DestroyImmediate(component);
			else Object.Destroy(component);
		}

		#endregion
		
		// Ehh, still not sure, it feels good, but not sure if it'll be more or less readable.

		public static T With<T>(this Transform transform, Action<T> action = null) where T : Component {
			return With<T>(transform.gameObject, action);
		}
		
		/// <summary>
		/// Get or add a component to a GameObject and perform an action on it.
		/// </summary>
		/// <param name="gameObject">The game object to get or add the component to.</param>
		/// <param name="action">The action to perform on the component.</param>
		/// <typeparam name="T">The type of component to get or add.</typeparam>
		public static T With<T>(this GameObject gameObject, Action<T> action = null) where T : Component {
			var component = gameObject.GetComponent<T>();
			if (component == null) component = gameObject.AddComponent<T>();
			action?.Invoke(component);
			return component;
		}
		
		/// <summary>
		/// Get or add a component to a GameObject and perform an action on it.
		/// </summary>
		/// <param name="gameObject">The game object to get or add the component to.</param>
		/// <param name="action">The action to perform on the component.</param>
		/// <typeparam name="T">The type of component to get or add.</typeparam>
		/// <typeparam name="TA">The type of component to add if missing.</typeparam>
		/// <returns></returns>
		public static T With<T, TA>(this GameObject gameObject, Action<T> action = null) 
			where T : Component where TA : Component {
			var component = gameObject.GetComponent<T>();
			if (component == null) component = gameObject.AddComponent<T>();
			if (component.GetComponent<TA>() == null) gameObject.AddComponent<TA>();
			action?.Invoke(component);
			return component;
		}

		public static T Use<T>(this Transform transform, Action<T> action = null) where T : Component {
			return Use<T>(transform.gameObject, action);
		}

		/// <summary>
		/// Get a component from a GameObject and perform an action on it.
		/// </summary>
		/// <param name="gameObject">The game object to get the component from.</param>
		/// <param name="action">The action to perform on the component.</param>
		/// <typeparam name="T">The type of component to get.</typeparam>
		/// <exception cref="Exception">Thrown if the component is not found on the GameObject.</exception>
		public static T Use<T>(this GameObject gameObject, Action<T> action = null) where T : Component {
			var component = gameObject.GetComponent<T>();
			if (component == null) {
				IdleLog.Error($"Component {typeof(T)} not found on GameObject {gameObject.name}");
				throw new Exception($"Component {typeof(T)} not found on GameObject {gameObject.name}");
			}
			action?.Invoke(component);
			return component;
		}

		public static GameObject FindAndUse(this GameObject gameObject, string path, Action<GameObject> action = null) {
			var gameObj = gameObject.Find(path);
			if (gameObj == null) {
				IdleLog.Error($"GameObject not found by path: {path}");
				throw new Exception($"GameObject not found by path: {path}");
			}
			action?.Invoke(gameObj);
			return gameObj;
		}

		/// <summary>
		/// Get a component from a GameObject and perform an action on it.
		/// </summary>
		/// <param name="gameObject">The game object to get the component from.</param>
		/// <param name="path">The path to the GameObject to get the component from.</param>
		/// <param name="action">The action to perform on the component.</param>
		/// <typeparam name="T">The type of component to get.</typeparam>
		/// <exception cref="Exception">Thrown if the component is not found on the GameObject.</exception>
		public static T FindAndUse<T>(this GameObject gameObject, string path, Action<T> action = null) {
			var gameObj = gameObject.Find(path);
			if (gameObj == null) {
				IdleLog.Error($"GameObject not found by path: {path}");
				throw new Exception($"GameObject not found by path: {path}");
			}
			var component = gameObj.GetComponent<T>();
			if (component == null) {
				IdleLog.Error($"Component {typeof(T)} not found on GameObject {gameObject.name}");
				throw new Exception($"Component {typeof(T)} not found on GameObject {gameObject.name}");
			}
			action?.Invoke(component);
			return component;
		}
		
		/// <summary>
		/// Get a component from a GameObject and perform an action on it.
		/// </summary>
		/// <param name="gameObject">The game object to get the component from.</param>
		/// <param name="path">The path to the GameObject to get the component from.</param>
		/// <param name="action">The action to perform on the component.</param>
		/// <typeparam name="T">The type of component to get.</typeparam>
		/// <exception cref="Exception">Thrown if the component is not found on the GameObject.</exception>
		public static T FindAndUse<T>(this GameObject gameObject, string path, Action<GameObject, T> action)
			where T : Component {
			var gameObj = gameObject.Find(path);
			if (gameObj == null) {
				IdleLog.Error($"GameObject not found by path: {path}");
				throw new Exception($"GameObject not found by path: {path}");
			}
			var component = gameObj.GetComponent<T>();
			if (component == null) {
				IdleLog.Error($"Component {typeof(T)} not found on GameObject {gameObject.name}");
				throw new Exception($"Component {typeof(T)} not found on GameObject {gameObject.name}");
			}
			action(gameObj, component);
			return component;
		}
	}
}