using Databases;
using HarmonyLib;
using IdlePlus.IdleClansAPI;
using IdlePlus.Utilities;
using Player;
using Popups;
using TMPro;
using UnityEngine;

namespace IdlePlus.Patches {
	
	[HarmonyPatch(typeof(ItemInfoPopup))]
	public class ItemInfoPopupPatch {
		
		private static readonly Vector3 ValueDefaultPosition = new Vector3(-0.41f, 13.25f, 0f);
		private static readonly Vector3 BaseValuePosition = new Vector3(-0.41f, 31.4348f, 0f);
		private static readonly Vector3 MarketValuePosition = new Vector3(-0.41f, 2f, 0f);

		private static GameObject _baseValue;
		private static GameObject _marketValue;
		
		public static void InitializeOnce() {
			// Create the market value object.
			_baseValue = GameObjects.FindDisabledByPath("PopupManager/Canvas/HardPopups/ItemInfoPopup/VisualInfo/Value");
			_marketValue = Object.Instantiate(_baseValue, _baseValue.transform.parent, false);
			_marketValue.name = "MarketValue";
			_marketValue.transform.SetSiblingIndex(1);
			
			// Swap the icon for the market icon.
			var icon = GameObject.Find("GameCanvas/NavigationCanvas/CommunitySection/Tabs/PlayerMarketTab/ScalingObjects/Icon");
			var uiImage = icon.GetComponent<UnityEngine.UI.Image>();
			var sprite = uiImage.activeSprite;
			_marketValue.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().overrideSprite = sprite;
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(ItemInfoPopup.InitializeItem))]
		private static void PostfixInitializeItem(ItemInfoPopup __instance, Item item) {
			if (item == null) return;

			var baseText = _baseValue.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
			var marketText = _marketValue.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
			
			var canNotBeSold = item.CanNotBeSoldToGameShop;
			var canNotBeTraded = item.CanNotBeTraded || PlayerData.Instance.GameMode == GameMode.Ironman;
			
			if (canNotBeSold) _baseValue.SetActive(false);
			else {
				_baseValue.SetActive(true);
				baseText.text = Numbers.ToCompactFormat(item.BaseValue);
				
				// If the market value is disabled, move the base value to the default position.
				_baseValue.transform.localPosition = canNotBeTraded ? ValueDefaultPosition : BaseValuePosition;
			}
			
			if (canNotBeTraded) _marketValue.SetActive(false);
			else {
				var price = IdleAPI.GetMarketEntry(item)?.GetSellBuyPrice();
				var text = price > 0 ? Numbers.ToCompactFormat(price.Value) : "???";
				_marketValue.SetActive(true);
				marketText.text = text;
				
				// If the base value is disabled, move the market value to the default position.
				_marketValue.transform.localPosition = canNotBeSold ? ValueDefaultPosition : MarketValuePosition;
			}
			
			__instance._contentRefresh.RefreshContentFitters();
		}
	}
}