using HarmonyLib;
using IdlePlus.API.Utility;
using IdlePlus.Attributes;
using IdlePlus.Settings;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Player;
using Popups;
using TMPro;
using Upgrades;

namespace IdlePlus.Patches.Popups {
	
	/// <summary>
	/// Patch to modify the behavior of the item interaction popup, to add functionality
	/// like being able to open maximum amount of treasure chests at once, display the
	/// amount of time when drinking potions, etc.
	/// </summary>
	[HarmonyPatch(typeof(InventoryItemInteractionPopup))]
	public class InventoryItemInteractionPopupPatch {

		private static TextMeshProUGUI _drinkButtonText;

		[InitializeOnce]
		private static void Initialize() {
			if (!ModSettings.Features.PotionTime.Value) return;
			var popup = GameObjects.FindByCachedPath("PopupManager/Canvas/HardPopups/InventoryItemInteractionPopup");
			var drinkButton = popup.Find("ButtonContainer/DrinkButton");
			_drinkButtonText = drinkButton.GetComponentInChildren<TextMeshProUGUI>();
			_drinkButtonText.gameObject.DestroyComponent<LocalizationText>();
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryItemInteractionPopup.SetupButtons))]
		public static void PostfixSetupButtons(InventoryItemInteractionPopup __instance) {
			if (!ModSettings.Features.PotionTime.Value) return;
			HandlePotion(__instance);
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryItemInteractionPopup.OnAmountInputFieldModified))]
		public static void PostfixOnAmountInputFieldModified(InventoryItemInteractionPopup __instance) {
			if (!ModSettings.Features.PotionTime.Value) return;
			HandlePotion(__instance);
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryItemInteractionPopup.OnIncrementButtonPressed))]
		public static void PostfixOnIncrementButtonPressed(InventoryItemInteractionPopup __instance) {
			if (!ModSettings.Features.PotionTime.Value) return;
			HandlePotion(__instance);
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryItemInteractionPopup.OnDecrementButtonPressed))]
		public static void PostfixOnDecrementButtonPressed(InventoryItemInteractionPopup __instance) {
			if (!ModSettings.Features.PotionTime.Value) return;
			HandlePotion(__instance);
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(InventoryItemInteractionPopup.OnMaxButtonPressed))]
		public static bool PrefixOnMaxButtonPressed(InventoryItemInteractionPopup __instance) {
			var itemId = __instance._item?.ItemId;
			// We only care if the item is a treasure chest.
			// 381 = common chest, 382 = rare chest, 383 = exceptional chest
			if (itemId == null || (itemId != 381 && itemId != 382 && itemId != 383)) return true;
			if (!ModSettings.Features.MaxOpenableChests.Value) return true;

			var ownedAmount = __instance._ownedAmount;
			var amount = ownedAmount;
			var freeSpace = PlayerData.Instance.Inventory.GetFreeInventorySpace();
			if (ownedAmount <= 0) return true; // shrug
			
			// If we have more treasure chests than free spaces, then use the free spaces
			// as the maximum amount.
			if (amount > freeSpace) amount = freeSpace;
			
			// Same behavior as vanilla, using #SetText to update the input field.
			__instance._itemAmountInputField.SetText(amount.ToString(), false);
			__instance._itemAmountSlider.Set((float) amount / ownedAmount, false);
			return false;
		}
		
		private static void HandlePotion(InventoryItemInteractionPopup __instance) {
			if (__instance._item == null) return;
			if (__instance._item.PotionEffectDurationSeconds <= 0) return;
			
			var duration = __instance._item.PotionEffectDurationSeconds;
			var amount = __instance._itemAmountInputField.text.Length == 0 ? 1 : 
				int.Parse(__instance._itemAmountInputField.text);
			var time = GetRealPotionTime(duration) * amount;
			
			_drinkButtonText.text = $"Drink ({SecondsToTimeString(time)})";
		}
		
		// Helpers

		private static string SecondsToTimeString(int seconds) {
			var days = seconds / 86400;
			var hours = seconds % 86400 / 3600;
			var minutes = seconds % 3600 / 60;
			var secs = seconds % 60;
			
			var time = "";
			if (days > 0) time += $"{days}D ";
			if (hours > 0) time += $"{hours}H ";
			if (minutes > 0) time += $"{minutes}M ";
			if (secs > 0) time += $"{secs}S ";
			return time.Trim();
		}

		private static int GetRealPotionTime(int seconds) {
			var modifier = 1.0D;
			if (UpgradeUtils.IsUnlockedForPlayer(UpgradeType.upgrade_responsible_drinking)) modifier *= 1.1D;
			if (UpgradeUtils.IsUnlockedForClan(UpgradeType.clan_upgrade_bigger_bottles)) modifier *= 1.25D;
			return (int) (seconds * modifier);
		}
	}
}