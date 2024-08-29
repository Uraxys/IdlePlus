using HarmonyLib;
using IdlePlus.Settings;
using Player;
using Popups;

namespace IdlePlus.Patches.Popups {
	
	/// <summary>
	/// Patch to modify how the max button works when interacting with treasure chests.
	/// Instead of selecting the maximum amount of items, it will select the maximum amount
	/// of treasure chests you can open at once (up to the amount of free inventory spaces).
	/// </summary>
	[HarmonyPatch(typeof(InventoryItemInteractionPopup))]
	public class InventoryItemInteractionPopupPatch {
		
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
		
	}
}