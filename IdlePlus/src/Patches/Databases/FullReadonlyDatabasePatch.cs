using Databases;
using HarmonyLib;
using IdlePlus.API.Event;
using IdlePlus.Utilities;

namespace IdlePlus.Patches.Databases {
	
	[HarmonyPatch(typeof(FullReadonlyDatabase))]
	internal class FullReadonlyDatabasePatch {

		/// <summary>
		/// Patch to detect when the config data has been loaded and is ready
		/// to be used.
		/// </summary>
		[HarmonyPostfix]
		[HarmonyPatch(nameof(FullReadonlyDatabase.SetupDatabasesFromConfigData))]
		private static void PostfixSetupDatabasesFromConfigData() {
			Events.Game.OnConfigDataLoaded.Call();
		}
		
	}
}