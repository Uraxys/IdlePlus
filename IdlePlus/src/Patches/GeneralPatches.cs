using HarmonyLib;
using IdlePlus.Attributes;
using IdlePlus.Settings;
using IdlePlus.Utilities;
using Player;
using TMPro;

namespace IdlePlus.Patches {
	
	[HarmonyPatch]
	public class GeneralPatches {

		[InitializeOnce(OnSceneLoad = "*")]
		private static void InitializeOnce() {
			ModSettings.MarketValue.DisplayFormat.OnValueChanged += value => {
				if (PlayerData.Instance?.Inventory?._goldAmountText == null) return;
				var inventory = PlayerData.Instance.Inventory;
				
				// Default format is the full number.
				if (value == 0) {
					inventory._goldAmountText.text = inventory.Gold.ToString("#,0");
					return;
				}
				
				inventory._goldAmountText.text = Numbers.FormatBasedOnSetting((long)inventory.Gold);
			};
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(TMP_Text), nameof(TMP_Text.text), MethodType.Setter)]
		public static void SetupPostfix(TMP_Text __instance, ref string value) {
			if (PlayerData.Instance?.Inventory?._goldAmountText?.GetInstanceID() != __instance.GetInstanceID()) return;
			if (ModSettings.MarketValue.DisplayFormat.Value == 0) return;
			value = Numbers.FormatBasedOnSetting((long) PlayerData.Instance.Inventory.Gold);
		}
	}
}