using HarmonyLib;
using Popups;

namespace IdlePlus.API.Popup {
	
	[HarmonyPatch]
	public static class PlusPopupManager {
		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(PopupManager), nameof(PopupManager.Awake))]
		private static void PrefixAwake(PopupManager __instance) {
			TestPopup.InitializeOnce();
		}
		
		
	}
}