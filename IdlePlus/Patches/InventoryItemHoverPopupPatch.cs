using Databases;
using HarmonyLib;
using IdlePlus.IdleClansAPI;
using IdlePlus.Settings;
using IdlePlus.Unity.Items;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using Player;
using Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
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
		private const float DefaultYSize = 20F;
		
		private static GameObject _marketValue;
		//private static GameObject _description;
		
		// TODO: Move into a behavior.
		//private static GameObject _scrollInfo;
		private static ScrollInfo _scrollInfo;
		
		[InitializeOnce]
		public static void InitializeOnce() {
			var hoverPopup =
				GameObjects.FindByCachedPath("PopupManager/Canvas/HardPopups/InventoryItemHoverPopup");
			var popup = hoverPopup.Use<InventoryItemHoverPopup>();
			
			var image = popup.transform.GetChild(0).gameObject;
			var background = popup.transform.GetChild(1).gameObject;
			var value = background.transform.GetChild(0).gameObject;
			var name = background.transform.GetChild(1).gameObject;
			
			// - Dark item background.
			background.With<FreeModifier>(modifier => modifier.radius = new Vector4(0, 5, 5, 0));
			background.With<ProceduralImage>(procedural => procedural.color = new Color(0, 0, 0, 0.3F));
			hoverPopup.Use<HorizontalLayoutGroup>(group => {
				group.padding = new RectOffset(5, 0, 0, 0);
				group.spacing = 5;
			});
			
			// - Market value.
			CreateMarketValue(hoverPopup, background, value);
			
			// - Scroll info.
			if (ModSettings.UI.EnhancedInventoryItemTooltip.Value) CreateScrollInfo(background, name);
			
			// - Popup description.
			
			/*_description = GameObjects.Instantiate(name, background, false, "Description");
			_description.Use<RectTransform>(rect => rect.sizeDelta = rect.sizeDelta.SetY(20));
			_description.Use<TextMeshProUGUI>(text => {
				text.text = "Default Description";
				text.fontSize = 16;
				text.fontSizeMax = 16;
				text.color = new Color(0.9F, 0.9F, 0.9F, 1);
			});*/
			
			// "Update" method.
			var heldShift = false;
			IdleTasks.Update(hoverPopup, () => {
				if (!ModSettings.MarketValue.ShiftForTotal.Value) return;
				var shift = Input.GetKey(KeyCode.LeftShift);
				if (shift == heldShift) return;
				heldShift = shift;
				
				// Update the text.
				UpdateText(popup);
			});
		}

		private static void CreateMarketValue(GameObject hoverPopup, GameObject background, GameObject value) {
			// Duplicate the value to create our market value.
			_marketValue = Object.Instantiate(value, value.transform.parent, false);
			_marketValue.name = "MarketValue";
			_marketValue.transform.SetSiblingIndex(1);
			var marketValueRectTransform = _marketValue.GetComponent<RectTransform>();
			marketValueRectTransform.sizeDelta = new Vector2(marketValueRectTransform.sizeDelta.x, DefaultYSize);

			// Swap the icon for the market icon.
			var icon = GameObjects.FindByCachedPath(
				"GameCanvas/NavigationCanvas/CommunitySection/Tabs/PlayerMarketTab/ScalingObjects/Icon");
			var uiImage = icon.GetComponent<Image>();
			var sprite = uiImage.activeSprite;
			_marketValue.transform.GetChild(0).GetComponent<Image>().overrideSprite = sprite;
			
			// Fix the popup size.
			background.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			hoverPopup.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		}
		
		private static void CreateScrollInfo(GameObject background, GameObject name) {
			var nameContainer = GameObjects.NewRect<VerticalLayoutGroup, ContentSizeFitter>("NameContainer", background);
			nameContainer.Use<VerticalLayoutGroup>(group => {
				group.childControlHeight = false;
				group.childControlWidth = false;
				group.childForceExpandHeight = false;
				group.childForceExpandWidth = false;
				group.spacing = -10;
			});
			nameContainer.Use<ContentSizeFitter>(fitter => fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize);
			
			name.SetParent(nameContainer);
			
			var scrollInfo = GameObjects.NewRect("ScrollInfo", nameContainer);
			scrollInfo.With<HorizontalLayoutGroup>(group => {
				group.childControlHeight = false;
				group.childControlWidth = false;
				group.childForceExpandHeight = false;
				group.childForceExpandWidth = false;
			});
			scrollInfo.With<ContentSizeFitter>(fitter => {
				fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
				fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			});
			_scrollInfo = scrollInfo.With<ScrollInfo>();
			
			var scrollText = GameObjects.NewRect<TextMeshProUGUI, ContentSizeFitter>("Text", scrollInfo);
			var scrollImg1 = GameObjects.NewRect<Image>("Image1", scrollInfo);
			var scrollImg2 = GameObjects.NewRect<Image>("Image2", scrollInfo);
			var scrollImg3 = GameObjects.NewRect<Image>("Image3", scrollInfo);
			var scrollImg4 = GameObjects.NewRect<Image>("Image4", scrollInfo);
			
			scrollText.Use<TextMeshProUGUI>(text => {
				text.text = "Can be applied to ";
				text.fontSize = 16;
				text.fontSizeMax = 16;
				text.color = new Color(0.9F, 0.9F, 0.9F, 1);
			});
			scrollText.Use<ContentSizeFitter>(fitter => {
				fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
				fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			});
			
			scrollImg1.Use<Image>(img => img.sprite = ItemDatabase.ItemList[428].LoadSpriteFromResources());
			scrollImg1.Use<RectTransform>(rect => rect.sizeDelta = Vec2.Vec(20));
			scrollImg2.Use<Image>(img => img.sprite = ItemDatabase.ItemList[426].LoadSpriteFromResources());
			scrollImg2.Use<RectTransform>(rect => rect.sizeDelta = Vec2.Vec(20));
			scrollImg3.Use<Image>(img => img.sprite = ItemDatabase.ItemList[425].LoadSpriteFromResources());
			scrollImg3.Use<RectTransform>(rect => rect.sizeDelta = Vec2.Vec(20));
			scrollImg4.Use<Image>(img => img.sprite = ItemDatabase.ItemList[427].LoadSpriteFromResources());
			scrollImg4.Use<RectTransform>(rect => rect.sizeDelta = Vec2.Vec(20));
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryItemHoverPopup.Setup))]
		public static void PostfixSetup(InventoryItemHoverPopup __instance, Item item) {
			if (item == null) return;
			
			if (ModSettings.UI.EnhancedInventoryItemTooltip.Value) {
				_scrollInfo.gameObject.SetActive(item.ScrollType != EnchantmentScrollType.None && 
				                                 _scrollInfo.Setup(item));
			}
			
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