using Databases;
using HarmonyLib;
using IdlePlus.IdleClansAPI;
using IdlePlus.Settings;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using Player;
using Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

namespace IdlePlus.Patches {
	
	/// <summary>
	/// A patch to add the market value to the inventory item hover popup,
	/// while also removing the item value if the item can't be sold to the
	/// game shop.
	/// </summary>
	[HarmonyPatch(typeof(InventoryItemHoverPopup))]
	public class InventoryItemHoverPopupPatch {
		
		private const float SingleYSize = 38.6147F;
		private const float DefaultYSize = 15F;
		
		private static GameObject _marketValue;
		
		[InitializeOnce]
		public static void InitializeOnce() {
			var inventoryItemHoverPopup =
				GameObjects.FindByCachedPath("PopupManager/Canvas/HardPopups/InventoryItemHoverPopup");
			var popup = inventoryItemHoverPopup.Use<InventoryItemHoverPopup>();
							
			var value = inventoryItemHoverPopup.Find("Background/Value");
			var background = value.transform.parent.gameObject;
			var hoverPopup = background.transform.parent.gameObject;
			
			// Duplicate the value to create our market value.
			_marketValue = Object.Instantiate(value, value.transform.parent, false);
			_marketValue.name = "MarketValue";
			_marketValue.transform.SetSiblingIndex(1);
			var marketValueRectTransform = _marketValue.GetComponent<RectTransform>();
			marketValueRectTransform.sizeDelta = new Vector2(marketValueRectTransform.sizeDelta.x, 15F);

			// Swap the icon for the market icon.
			var icon = GameObjects.FindByCachedPath(
				"GameCanvas/NavigationCanvas/CommunitySection/Tabs/PlayerMarketTab/ScalingObjects/Icon");
			var uiImage = icon.GetComponent<Image>();
			var sprite = uiImage.activeSprite;
			_marketValue.transform.GetChild(0).GetComponent<Image>().overrideSprite = sprite;
			
			// Fix the popup size.
			background.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			hoverPopup.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			// "Update" method.
			var heldShift = false;
			IdleTasks.Update(inventoryItemHoverPopup, () => {
				if (!ModSettings.MarketValue.ShiftForTotal.Value) return;
				var shift = Input.GetKey(KeyCode.LeftShift);
				if (shift == heldShift) return;
				heldShift = shift;
				
				// Update the text.
				UpdateText(popup);
			});
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryItemHoverPopup.Setup))]
		public static void PostfixSetup(InventoryItemHoverPopup __instance, Item item) {
			if (item == null) return;
			UpdateText(__instance, item);
		}
		
		private static void UpdateText(InventoryItemHoverPopup __instance, Item item = null) {
			if (item == null) item = __instance.AttachedItem;
			
			// Check if we should display the internal item name or not.
			if (ModSettings.Miscellaneous.InternalItemNames.Value)
				__instance._itemNameText.text = $"{item.Name} ({item.ItemId})";
			
			var baseObj = __instance._itemValueText.transform.parent.gameObject;
			var marketObj = _marketValue;
			
			var basePriceText = __instance._itemValueText;
			var marketPriceText = _marketValue.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

			var canNotBeSold = item.CanNotBeSoldToGameShop;
			var canNotBeTraded = item.CanNotBeTraded || !ModSettings.MarketValue.Enabled.Value ||
			                     (PlayerData.Instance.GameMode == GameMode.Ironman &&
			                      ModSettings.MarketValue.HideForIronman.Value);

			var shift = Input.GetKey(KeyCode.LeftShift) && ModSettings.MarketValue.ShiftForTotal.Value;
			var amount = (long) PlayerData.Instance.Inventory.GetItemAmount(item);
			var amountText = Numbers.ToCompactFormat(amount);
			
			// Disable the base value if the item can't be sold.
			if (canNotBeSold) baseObj.SetActive(false);
			else {
				var value = ModSettings.MarketValue.IncludeNegotiation.Value ? 
					ItemDatabase.GetItemSellValue(item) : 
					item.BaseValue;
				
				var gold = shift ? value * amount : value;
				var text = !shift ? 
					Numbers.FormatBasedOnSetting(gold) : 
					$"{Numbers.FormatBasedOnSetting(gold)} = {amountText} x " +
					$"{Numbers.FormatBasedOnSetting(value)}";
				
				baseObj.SetActive(true);
				basePriceText.text = text;
			}
			
			// Disable the market value if the item can't be sold.
			if (canNotBeTraded) marketObj.SetActive(false);
			else {
				var price = IdleAPI.GetMarketEntry(item)?.GetPriceDependingOnSetting();
				var text = price == null || price <= 0 ? "???" : !shift ? 
						Numbers.FormatBasedOnSetting(price.Value) : 
						$"{Numbers.FormatBasedOnSetting(price.Value * amount)} = {amountText} x " +
						$"{Numbers.FormatBasedOnSetting(price.Value)}";
				
				marketObj.SetActive(true);
				marketPriceText.text = text;
				
				// Fix the rect transform size, as it changes if the item can't
				// be sold to the game shop.
				var rectTransform = marketObj.GetComponent<RectTransform>();
				var x = rectTransform.sizeDelta.x;
				rectTransform.sizeDelta = canNotBeSold ? new Vector2(x, SingleYSize) : new Vector2(x, DefaultYSize);
			}
			
			__instance._contentRefresh.RefreshContentFitters();
		}
	}
}