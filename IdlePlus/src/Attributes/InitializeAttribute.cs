using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using IdlePlus.Utilities;

namespace IdlePlus.Attributes {
	
	/// <summary>
	/// Mark the method to be run at certain points in the game.
	/// If <c>OnSceneLoad</c> is set, then the method will only run
	/// if the scene that is specified is loaded. Otherwise, it'll
	/// run when the player has logged in.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class InitializeAttribute : Attribute {
		public int Priority = 0;
		public string OnSceneLoad = null;
	}
	
	public static class InitializeAttributeHandler {

		private static bool _loaded;
		private static List<MethodInfo> _initializeMethods;
		private static Dictionary<MethodInfo, string> _initializeMethodsOnScene;
		
		internal static void Load() {
			if (_loaded) throw new InvalidOperationException("Initialize methods have already been loaded!");
			_loaded = true;
			
			var enumerable =
				from t in Assembly.GetExecutingAssembly().GetTypes()
				where t.IsClass
				from m in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | 
				                       BindingFlags.Static | BindingFlags.Public)
				where m.GetCustomAttributes(typeof(InitializeAttribute), false).FirstOrDefault() != null
				select m;
			
			// Validate and sort methods.
			_initializeMethodsOnScene = new Dictionary<MethodInfo, string>();
			_initializeMethods = enumerable.OrderByDescending(x => {
				var attr = x.GetCustomAttribute(typeof(InitializeAttribute), false) as InitializeAttribute;
				return attr?.Priority ?? 0;
			}).Where(methodInfo => {
				var name = methodInfo.DeclaringType?.FullName ?? "null" + "." + methodInfo.Name;
				var attr = methodInfo.GetCustomAttribute(typeof(InitializeAttribute), false) as InitializeAttribute;
				
				if (!methodInfo.IsStatic) {
					IdleLog.Warn($"Couldn't run Initialize for {name}, method isn't static!");
					return false;
				}

				if (methodInfo.GetParameters().Length > 0) {
					IdleLog.Warn($"Couldn't run Initialize for {name}, method has parameters!");
					return false;
				}

				if (attr?.OnSceneLoad == null) return true;
				_initializeMethodsOnScene.Add(methodInfo, attr.OnSceneLoad);
				return false;
			}).ToList();
		}
		
		public static void Initialize(string scene = null) {
			var watch = Stopwatch.StartNew();
			var methods = scene == null ? 
				_initializeMethods : 
				_initializeMethodsOnScene.Keys.Where(x =>
					_initializeMethodsOnScene[x].Equals(scene) || _initializeMethodsOnScene[x].Equals("*")).ToList();
			
			foreach (var t in methods) {
				var name = t.DeclaringType?.FullName ?? "null" + "." + t.Name;
				try {
					t.Invoke(null, null);
					IdleLog.Info($"Ran Initialize for {name}, scene: {scene ?? "null"}");
				} catch (Exception e) {
					IdleLog.Error($"Error running Initialize for {name}, scene: {scene ?? "null"}!", e);
				}
			}
			
			watch.Stop();
			IdleLog.Info($"Ran Initialize in {watch.ElapsedMilliseconds}ms");
		}
	}
}