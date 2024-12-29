using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;

namespace IdlePlus.Attributes {
	
	[AttributeUsage(AttributeTargets.Method)]
	public class InitializeOnceAttribute : Attribute {
		public int Priority = 0;
		public string OnSceneLoad = null;
	}

	public static class InitializeOnceAttributeHandler {
		
		private static bool _loaded;
		private static bool _initialized;
		private static List<MethodInfo> _initializeOnceMethods;
		private static Dictionary<MethodInfo, string> _initializeOnceMethodsOnScene;

		public static void Load() {
			if (_loaded) throw new InvalidOperationException("InitializeOnce methods have already been loaded!");
			_loaded = true;
			
			// InitializeOnce methods.
			var enumerable = 
				from t in Assembly.GetExecutingAssembly().GetTypes()
				where t.IsClass
				from m in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | 
				                       BindingFlags.Static | BindingFlags.Public)
				where m.GetCustomAttributes(typeof(InitializeOnceAttribute), false).FirstOrDefault() != null
				select m;

			_initializeOnceMethodsOnScene = new Dictionary<MethodInfo, string>();
			_initializeOnceMethods = enumerable.OrderByDescending(x => {
				var attr = x.GetCustomAttribute(typeof(InitializeOnceAttribute), false) as InitializeOnceAttribute;
				return attr?.Priority ?? 0;
			}).Where(methodInfo => {
				var name = methodInfo.DeclaringType?.FullName ?? "null" + "." + methodInfo.Name;
				var attr = methodInfo.GetCustomAttribute(typeof(InitializeOnceAttribute), false) as InitializeOnceAttribute;
				
				if (!methodInfo.IsStatic) {
					IdleLog.Warn($"Couldn't run InitializeOnce for {name}, method isn't static!");
					return false;
				}

				if (methodInfo.GetParameters().Length > 0) {
					IdleLog.Warn($"Couldn't run InitializeOnce for {name}, method has parameters!");
					return false;
				}

				if (attr?.OnSceneLoad == null) return true;
				_initializeOnceMethodsOnScene.Add(methodInfo, attr.OnSceneLoad);
				return false;
			}).ToList();
		}

		public static void InitializeOnce(string scene = null) {
			var watch = Stopwatch.StartNew();

			if (scene == null) {
				if (_initialized) return;
				_initialized = true;
				
				// Run the methods without a scene.
				foreach (var method in _initializeOnceMethods) {
					var name = method.DeclaringType?.FullName ?? "null" + "." + method.Name;
					try {
						method.Invoke(null, null);
						IdleLog.Info($"Ran InitializeOnce for {name}");
					} catch (Exception e) {
						IdleLog.Error($"Error running InitializeOnce for {name}!", e);
					}
				}
			
				watch.Stop();
				IdleLog.Info($"Ran InitializeOnce in {watch.ElapsedMilliseconds}ms");
				return;
			}
			
			// Handle scene-specific methods.
			
			if (_initializeOnceMethodsOnScene.IsEmpty()) return;
			var removed = new List<MethodInfo>();

			// Run the methods, then remove them from the dictionary, as they should only run once.
			foreach (var method in _initializeOnceMethodsOnScene) {
				if (!method.Value.Equals(scene) && !method.Value.Equals("*")) continue;
				var name = method.Key.DeclaringType?.FullName ?? "null" + "." + method.Key.Name;
				removed.Add(method.Key);

				try {
					method.Key.Invoke(null, null);
					IdleLog.Info($"Ran InitializeOnce on scene {scene} for {name}");
					removed.Add(method.Key);
				}
				catch (Exception e) {
					IdleLog.Error($"Error running InitializeOnce on scene {scene} for {name}!", e);
				}
			}

			foreach (var method in removed) {
				_initializeOnceMethodsOnScene.Remove(method);
			}
			
			watch.Stop();
			IdleLog.Info($"Ran InitializeOnce for scene {scene} in {watch.ElapsedMilliseconds}ms");
		}
	}
}