using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Client;
using HarmonyLib;
using IdlePlus.IdleClansAPI;
using IdlePlus.Settings;
using IdlePlus.TexturePack;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using Il2CppInterop.Runtime.Injection;
using PlayerMarket;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace IdlePlus {
	
	[BepInPlugin(ModGuid, ModName, ModVersion)]
	public class IdlePlus : BasePlugin {

		public const string
			ModName = "Idle Plus",
			ModAuthor = "Uraxys",
			ModID = "idleplus",
			ModGuid = "dev.uraxys.idleplus",
			ModVersion = "1.3.0"
#if DEBUG
			             + "-DEBUG";
#else
			             ;
#endif
		
		private static bool _initializedOnce;
		
		private static List<MethodInfo> _initializeOnceMethods;
		private static List<MethodInfo> _initializeMethods;
		
		private static Dictionary<MethodInfo, string> _initializeOnceMethodsOnScene;
		private static Dictionary<MethodInfo, string> _initializeMethodsOnScene;
		
		public override void Load() {
			IdleLog.Logger = Log;
			IdleLog.Info($"Loading Idle Plus v{ModVersion}...");
			
			TexturePackManager.Load();
			ModSettings.Load();
			RegisterIl2CppTypes();
			LoadInitializeMethods();
			IdlePlusBehaviour.Create();
			
			// Load harmony patches.
			var harmony = new Harmony(ModGuid);
			harmony.PatchAll();
			
			// Create the market prices update task.
			IdleTasks.Repeat(0, 60, task => {
				if (!NetworkClient.IsConnected()) return;
				IdleAPI.UpdateMarketPrices();
			});
			
			IdleLog.Info($"Idle Plus v{ModVersion} loaded!");
		}

		internal static void Update() {
			IdleTasks.Tick();
		}

		internal static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			IdleLog.Info($"Scene loaded: {scene.name}");
			GameObjects.InitializeCache();
			CallInitializeOnce(scene.name);
			CallInitialize(scene.name);
		}
		
		internal static void OnLogin() {
			// Do one time initialization for objects that are only created once.
			if (!_initializedOnce) {
				_initializedOnce = true;
				CallInitializeOnce();
			}
			
			// After we've logged in, find the PlayerMarketPage and "initialize"
			// it, this will allow us to create sell offers from our inventory
			// without having to manually open the player market first.
			IdleTasks.Run(() => { // TODO: Don't run a frame later when the client exception is fixed.
				var playerMarket = Object.FindObjectOfType<PlayerMarketPage>(true);
				playerMarket.gameObject.SetActive(true);
				playerMarket.gameObject.SetActive(false);
				
				// Do initialization for objects that are recreated on login.
				CallInitialize();
				
				// Update market prices.
				IdleAPI.UpdateMarketPrices();
			});
		}

		private static void RegisterIl2CppTypes() {
			var il2CPPTypes = 
				from t in Assembly.GetExecutingAssembly().GetTypes()
				let attributes = t.GetCustomAttributes(typeof(RegisterIl2Cpp), true)
				where attributes != null && attributes.Length > 0
				select new { Type = t, Attributes = attributes.Cast<RegisterIl2Cpp>() };
			
			foreach (var type in il2CPPTypes) {
				var attribute = type.Attributes.First();
				var registerOptions = RegisterTypeOptions.Default;
				
				if (attribute.Interfaces != null && attribute.Interfaces.Length > 0) {
					registerOptions = new RegisterTypeOptions();
					var interfaceCollection = (Il2CppInterfaceCollection) attribute.Interfaces;

					// I.. I... just... ...
					Traverse.Create(registerOptions)
						.Property("Interfaces").SetValue(interfaceCollection)
						.Property("LogSuccess").SetValue(true);
				}
				
				ClassInjector.RegisterTypeInIl2Cpp(type.Type, registerOptions);
			}
		}
		
		private static void LoadInitializeMethods() {
			// This method can probably be optimized, but eh.
			
			// InitializeOnce methods.
			var enumerable = 
				from t in Assembly.GetExecutingAssembly().GetTypes()
				where t.IsClass
				from m in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | 
				                       BindingFlags.Static | BindingFlags.Public)
				where m.GetCustomAttributes(typeof(InitializeOnce), false).FirstOrDefault() != null
				select m;

			_initializeOnceMethodsOnScene = new Dictionary<MethodInfo, string>();
			_initializeOnceMethods = enumerable.OrderByDescending(x => {
				var attr = x.GetCustomAttribute(typeof(InitializeOnce), false) as InitializeOnce;
				return attr?.Priority ?? 0;
			}).Where(methodInfo => {
				var name = methodInfo.DeclaringType?.FullName ?? "null" + "." + methodInfo.Name;
				var attr = methodInfo.GetCustomAttribute(typeof(InitializeOnce), false) as InitializeOnce;

				// Check if the method is static.
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
			
			// Initialize methods.
			enumerable =
				from t in Assembly.GetExecutingAssembly().GetTypes()
				where t.IsClass
				from m in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | 
				                       BindingFlags.Static | BindingFlags.Public)
				where m.GetCustomAttributes(typeof(Initialize), false).FirstOrDefault() != null
				select m;
			
			// Validate and sort methods.
			_initializeMethodsOnScene = new Dictionary<MethodInfo, string>();
			_initializeMethods = enumerable.OrderByDescending(x => {
				var attr = x.GetCustomAttribute(typeof(InitializeOnce), false) as InitializeOnce;
				return attr?.Priority ?? 0;
			}).Where(methodInfo => {
				var name = methodInfo.DeclaringType?.FullName ?? "null" + "." + methodInfo.Name;
				var attr = methodInfo.GetCustomAttribute(typeof(Initialize), false) as Initialize;

				// Check if the method is static.
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
		
		private static void CallInitializeOnce(string scene = null) {
			var watch = Stopwatch.StartNew();
			
			if (scene != null) {
				if (_initializeOnceMethodsOnScene.Count == 0) return;
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
				return;
			}
				
			// Run the methods.
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
		}

		private static void CallInitialize(string scene = null) {
			var watch = Stopwatch.StartNew();
			
			if (scene != null) {
				if (_initializeMethodsOnScene.Count == 0) return;
				foreach (var method in _initializeMethodsOnScene
					         .Where(method => method.Value.Equals(scene) || method.Value.Equals("*"))) {
					var name = method.Key.DeclaringType?.FullName ?? "null" + "." + method.Key.Name;
					try {
						method.Key.Invoke(null, null);
						IdleLog.Info($"Ran Initialize on scene {scene} for {name}");
					} catch (Exception e) {
						IdleLog.Error($"Error running Initialize on scene {scene} for {name}!", e);
					}
				}
				
				watch.Stop();
				IdleLog.Info($"Ran Initialize for scene {scene} in {watch.ElapsedMilliseconds}ms");
				return;
			}
			
			// Run the methods.
			foreach (var method in _initializeMethods) {
				var name = method.DeclaringType?.FullName ?? "null" + "." + method.Name;
				try {
					method.Invoke(null, null);
					IdleLog.Info($"Ran Initialize for {name}");
				}
				catch (Exception e) {
					IdleLog.Error($"Error running Initialize for {name}!", e);
				}
			}
			
			watch.Stop();
			IdleLog.Info($"Ran Initialize in {watch.ElapsedMilliseconds}ms");
		}
	}
}