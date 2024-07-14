using HarmonyLib;
using IdlePlus.Settings;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using Player;
using TMPro;

namespace IdlePlus.Patches {
	
	[HarmonyPatch]
	public class GeneralPatches {

		[InitializeOnce(OnSceneLoad = "*")]
		private static void InitializeOnce() {
			ModSettings.MarketValue.DisplayFormat.OnValueChanged += value => {
				if (PlayerData.Instance?.Inventory?._goldAmountText == null) return;
				PlayerData.Instance.Inventory._goldAmountText.text =
					Numbers.FormatBasedOnSetting((long)PlayerData.Instance.Inventory.Gold, value == 0);
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