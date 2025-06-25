using HarmonyLib;
using IdlePlus.API.Popup;
using IdlePlus.API.Popup.Popups;
using IdlePlus.Settings;
using Raids;
using TMPro;

namespace IdlePlus.Patches.Raids {
	
	[HarmonyPatch(typeof(RaidManager))]
	public class RaidManagerPatch {
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(RaidManager.OnPlayerNameButtonPressed))]
		private static bool PrefixOnPlayerNameButtonPressed(RaidManager __instance, TextMeshProUGUI tmp) {
			if (!ModSettings.Features.DetailedRaidPlayer.Value) return true;
			
			var name = __instance.ExtractPlayerNameFromEntryText(tmp.text);
			if (name == null) return true;
			var profilePopup = CustomPopupManager.Setup<PlayerProfilePopup>(PlayerProfilePopup.PopupKey);
			profilePopup.Setup(name, true);
			
			return false;
		}
	}
}