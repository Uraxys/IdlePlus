using Databases;
using HarmonyLib;
using IdlePlus.IdleClansAPI;
using IdlePlus.Utilities;
using Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace IdlePlus.Patches {
	
	[HarmonyPatch(typeof(InventoryItemHoverPopup))]
	public class InventoryItemHoverPopupPatch {
		
		private static GameObject _marketValue;

		private const float SingleYSize = 38.6147F;
		private const float DefaultYSize = 15F;
		
		public static void InitializeOnce() {
			var value = GameObjects.FindDisabledByPath("PopupManager/Canvas/HardPopups/InventoryItemHoverPopup/Background/Value");
			var background = value.transform.parent.gameObject;
			var hoverPopup = background.transform.parent.gameObject;
			
			// Duplicate the value to create our market value.
			_marketValue = Object.Instantiate(value, value.transform.parent, false);
			_marketValue.name = "MarketValue";
			_marketValue.transform.SetSiblingIndex(1);
			var marketValueRectTransform = _marketValue.GetComponent<RectTransform>();
			marketValueRectTransform.sizeDelta = new Vector2(marketValueRectTransform.sizeDelta.x, 15F);

			// Swap the icon for the market icon.
			var icon = GameObject.Find("GameCanvas/NavigationCanvas/CommunitySection/Tabs/PlayerMarketTab/ScalingObjects/Icon");
			var uiImage = icon.GetComponent<UnityEngine.UI.Image>();
			var sprite = uiImage.activeSprite;
			_marketValue.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().overrideSprite = sprite;
			
			// Fix the popup size.
			background.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			hoverPopup.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryItemHoverPopup.Setup))]
		public static void PostfixSetup(InventoryItemHoverPopup __instance, Item item) {
			if (item == null) return;
			
			var baseObj = __instance._itemValueText.transform.parent.gameObject;
			var marketObj = _marketValue;
			
			var basePriceText = __instance._itemValueText;
			var marketPriceText = _marketValue.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

			// Disable the base value if the item can't be sold.
			if (item.CanNotBeSoldToGameShop) {
				baseObj.SetActive(false);
			} else {
				baseObj.SetActive(true);
				basePriceText.text = Numbers.ToCompactFormat(item.BaseValue);
			}
			
			// Disable the market value if the item can't be sold.
			if (item.CanNotBeTraded) {
				marketObj.SetActive(false);
			} else {
				var price = IdleAPI.GetMarketEntry(item)?.GetSellBuyPrice();
				var text = price > 0 ? Numbers.ToCompactFormat(price.Value) : "???";
				
				marketObj.SetActive(true);
				marketPriceText.text = text;
				
				// Fix the rect transform size, as it changes if the item can't
				// be sold to the game shop.
				var rectTransform = marketObj.GetComponent<RectTransform>();
				var x = rectTransform.sizeDelta.x;
				
				if (item.CanNotBeSoldToGameShop) rectTransform.sizeDelta = new Vector2(x, SingleYSize);
				else rectTransform.sizeDelta = new Vector2(x, DefaultYSize);
			}
			
			__instance._contentRefresh.RefreshContentFitters();
		}
	}
}