using HarmonyLib;
using IdlePlus.Unity;
using PlayerMarket;

namespace IdlePlus.Patches {
	
	/*[HarmonyPatch(typeof(HotItemEntry))]
	public class HotItemEntryPatch {
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(HotItemEntry.Setup))]
		private static void PostfixSetup(HotItemEntry __instance, int itemId) {
			var hotItemButton = __instance.gameObject.GetComponent<HotItemButton>();
			if (hotItemButton == null) hotItemButton = __instance.gameObject.AddComponent<HotItemButton>();
			hotItemButton.Setup(itemId);
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(HotItemEntry.SetupEmpty))]
		private static void PostfixSetupEmpty(HotItemEntry __instance) {
			var hotItemButton = __instance.gameObject.GetComponent<HotItemButton>();
			if (hotItemButton == null) hotItemButton = __instance.gameObject.AddComponent<HotItemButton>();
			hotItemButton.Setup(-1);
		}
	}*/
}