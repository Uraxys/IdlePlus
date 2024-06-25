using System.Collections.Generic;
using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdlePlus.Utilities {
	
	[HarmonyPatch]
	public static class ModLocalization {
		
		/// <summary>
		/// Modded localization keys to be injected into the game's localization
		/// manager. The key will be automatically prefixed with the mod ID to
		/// avoid overriding existing keys.
		/// </summary>
		private static readonly Dictionary<string, string> LocalizationKeys = new Dictionary<string, string> {
			{ "edit_offer", "Edit offer" },
		};
		
		/// <summary>
		/// Injects the modded localization keys into the game's localization
		/// manager after the game has loaded a language.
		/// </summary>
		private static void Initialize() {
			var languages = new List<Il2CppSystem.Collections.Generic.Dictionary<string, string>> {
					LocalizationManager._localizedEnglish,
					LocalizationManager._localizedSpanish,
					LocalizationManager._localizedFinnish,
					LocalizationManager._localizedSwedish,
					LocalizationManager._localizedGerman,
					LocalizationManager._localizedRussian,
					LocalizationManager._localizedPortuguese,
					LocalizationManager._localizedFrench,
					LocalizationManager._localizedItalian,
					LocalizationManager._localizedKorean,
					LocalizationManager._localizedTurkish,
					LocalizationManager._localizedPolish,
					LocalizationManager._localizedJapanese
				};
			
			foreach (var pair in LocalizationKeys) {
				var key = $"{IdlePlus.ModID}_{pair.Key}";
				foreach (var language in languages) {
					if (language == null) continue;
					language[key] = pair.Value;
				}
			}
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.SetupLanguage))]
		private static void PrefixInitialize(Task __result) {
			__result.ContinueWith((Action<Task>) delegate {
				Initialize();
			});
		}

		/// <summary>
		/// Override the localization key with a modded key for the given
		/// GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject we're overriding.</param>
		/// <param name="key">The new localization key, shouldn't include the mod id.</param>
		public static void SetModdedLocalizationKey(GameObject gameObject, string key) {
			SetLocalizationKey(gameObject.transform, $"{IdlePlus.ModID}_{key}");
		}
		
		/// <summary>
		/// Override the localization key with a modded key for the given
		/// Transform.
		/// </summary>
		/// <param name="transform">The Transform of the GameObject we're overriding.</param>
		/// <param name="key">The new localization key, shouldn't include the mod id.</param>
		public static void SetModdedLocalizationKey(Transform transform, string key) {
			SetLocalizationKey(transform, $"{IdlePlus.ModID}_{key}");
		}

		/// <summary>
		/// Override the localization key for the given GameObject.
		/// </summary>
		/// <param name="obj">The GameObject we're overriding.</param>
		/// <param name="key">The new localization key.</param>
		public static void SetLocalizationKey(GameObject obj, string key) {
			SetLocalizationKey(obj.transform, key);
		}
		
		/// <summary>
		/// Override the localization key for the given Transform.
		/// </summary>
		/// <param name="obj">The Transform of the GameObject we're overriding.</param>
		/// <param name="key">The new localization key.</param>
		public static void SetLocalizationKey(Transform obj, string key) {
			LocalizationText localizationText = obj.GetComponent<LocalizationText>();
			if (localizationText == null) {
				IdleLog.Warn($"GameObject {obj.name} doesn't have a LocalizationText component!");
				return;
			}
			
			localizationText.SetKeyRuntime(key);
		}

		/// <summary>
		/// Get a modded localized value for the given key.
		/// </summary>
		/// <param name="key">The localization key, shouldn't include the mod id.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns></returns>
		public static string GetModdedLocalizedValue(string key, params Object[] arguments) {
			return GetLocalizedValue($"{IdlePlus.ModID}_{key}", arguments);
		}
		
		/// <summary>
		/// Get a localized value for the given key.
		/// </summary>
		/// <param name="key">The localization key.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns></returns>
		public static string GetLocalizedValue(string key, params Object[] arguments) {
			const int maxArguments = 6;
			if (arguments.Length > maxArguments) {
				IdleLog.Warn($"Too many arguments for localization key (>6): {maxArguments}");
				return key;
			}
			
			if (arguments.Length == 0) return LocalizationManager.GetLocalizedValue(key);
			
			var arg1 = GetArgument(arguments, 0);
			if (arguments.Length == 1) return LocalizationManager.GetLocalizedValue(key, arg1);
			
			var arg2 = GetArgument(arguments, 1);
			if (arguments.Length == 2) return LocalizationManager.GetLocalizedValue(key, arg1, arg2);
			
			var arg3 = GetArgument(arguments, 2);
			if (arguments.Length == 3) return LocalizationManager.GetLocalizedValue(key, arg1, arg2, arg3);
			
			var arg4 = GetArgument(arguments, 3);
			if (arguments.Length == 4) return LocalizationManager.GetLocalizedValue(key, arg1, arg2, arg3, arg4);
			
			var arg5 = GetArgument(arguments, 4);
			if (arguments.Length == 5) return LocalizationManager.GetLocalizedValue(key, arg1, arg2, arg3, arg4, arg5);
			
			var arg6 = GetArgument(arguments, 5);
			return LocalizationManager.GetLocalizedValue(key, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		private static string GetArgument(Object[] args, int number) {
			if (args == null || args.Length <= number || args[number] == null) return $"null{number}";
			return args[number].ToString();
		} 
	}
}