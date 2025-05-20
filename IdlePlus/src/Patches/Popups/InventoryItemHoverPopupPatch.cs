using Databases;
using HarmonyLib;
using IdlePlus.Attributes;
using IdlePlus.IdleClansAPI;
using IdlePlus.Settings;
using IdlePlus.Unity.Items;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Player;
using Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using Image = UnityEngine.UI.Image;

namespace IdlePlus.Patches.Popups {
	
	/// <summary>
	/// Patch to add in multiple new features to the item hover popup, those
	/// being the following:<br/>
	/// - Enhanced item tooltip<br/>
	/// - Equipment stats info<br/>
	/// - Market value<br/>
	/// - Scroll info<br/>
	/// - Internal item names<br/>
	/// <br/>
	/// The three last features should also work with or without enhanced item
	/// tooltip enabled.
	/// </summary>
	[HarmonyPatch(typeof(InventoryItemHoverPopup))]
	public class InventoryItemHoverPopupPatch {
		
		private const float SingleSellSizeX = 71.66F;
		private const float DefaultSellSizeX = 95F;

		private static readonly Color PopupColor = new Color(0.1294f, 0.619f, 0.5249f, 0.9f);
		private static readonly Color GrayColor = new Color(0.9f, 0.9f, 0.9f, 1f);

		private static bool _enhancedTooltip;
        
		private static GameObject _popup;
		private static GameObject _background;

		private static GameObject _nameContainer;
		private static GameObject _valueContainer;

		private static GameObject _itemName;
		private static GameObject _sellValue;
		private static GameObject _marketValue;
		
		private static Image _sellValueIcon;
		private static TextMeshProUGUI _sellValueText;
		private static Image _marketValueIcon;
		private static TextMeshProUGUI _marketValueText;
		
		// Only used if enhanced tooltips are enabled.
		private static RectTransform _sellTitleRect;
		private static GameObject _split1;
		
		// Features
		private static ScrollInfo _scrollInfo;
		private static EquipmentStatsInfo _equipmentStatsInfo;
		
		[InitializeOnce]
		public static void InitializeOnce() {
			_enhancedTooltip = ModSettings.UI.EnhancedInventoryItemTooltip.Value;
			_popup = GameObjects.FindByCachedPath("PopupManager/Canvas/HardPopups/InventoryItemHoverPopup");
			var popupBehavior = _popup.Use<InventoryItemHoverPopup>();
			
			_background = _popup.transform.GetChild(1).gameObject;
			_sellValue = _background.transform.GetChild(0).gameObject;
			_itemName = _background.transform.GetChild(1).gameObject;
			
			_sellValueIcon = _sellValue.transform.GetChild(0).Use<Image>();
			_sellValueText = _sellValue.transform.GetChild(1).Use<TextMeshProUGUI>();

			// Initialize the changes that can't be disabled.
			// - ValueContainer
			_valueContainer = GameObjects.NewRect("ValueContainer", _background);
			_valueContainer.With<VerticalLayoutGroup>().DisableChildStates().SetSpacing(5).SetPadding(0, 0, 3, 3);
			_valueContainer.With<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			_sellValue.SetParent(_valueContainer);
			_sellValue.Use<RectTransform>(rect => rect.sizeDelta = rect.sizeDelta.SetY(25));
			_sellValue.With<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			// - NameContainer
			_nameContainer = GameObjects.NewRect("NameContainer", _background);
			_nameContainer.With<VerticalLayoutGroup>().DisableChildStates().SetSpacing(-5).SetPadding(0, 0, 3, 3);
			_nameContainer.With<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			_itemName.SetParent(_nameContainer);
			_itemName.Use<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			// - Background
			_background.Use<VerticalLayoutGroup>().SetPadding(10, 10, 2, 2);

			// If we have enhanced tooltips enabled, we need to make some more drastic changes.
			if (_enhancedTooltip) {
				// Darker background for the tooltip content and change expand setting.
				_background.Use<VerticalLayoutGroup>().SetChildForceExpand(true, false).childAlignment =
					TextAnchor.MiddleLeft;
				_background.With<LayoutElement>().minHeight = 40;
				_background.With<FreeModifier>().radius = new Vector4(0, 5, 5, 0);
				_background.With<ProceduralImage>().color = new Color(0, 0, 0, 0.3F);
				_popup.Use<HorizontalLayoutGroup>().SetPadding(5, 0, 0, 0).SetSpacing(5);
				_popup.Use<ProceduralImage>().color = PopupColor;
                
				// Move the name to the top and set the name as bold.
				_nameContainer.transform.SetSiblingIndex(0);
				_nameContainer.Use<VerticalLayoutGroup>().SetSpacing(-3);
				_itemName.Use<TextMeshProUGUI>(text => {
					text.fontStyle = FontStyles.Normal | FontStyles.Bold;
					text.fontSize = 16;
					text.m_fontSizeMax = 16;
				});
				
				// Change value container spacing.
				_valueContainer.Use<VerticalLayoutGroup>().SetSpacing(1);
				
				// Add a split between the name container and the value container.
				_split1 = NewSplit(_background);
				_split1.transform.SetSiblingIndex(1);
				
				// Change the sell value.
				// - Change spacing.
				_sellValue.Use<HorizontalLayoutGroup>().SetSpacing(5);
				_sellValue.Use<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
				// - Add title.
				var sellValueTitle = GameObjects.NewRect("SellTitle", _sellValue);
				_sellTitleRect = sellValueTitle.Use<RectTransform>(rect => rect.sizeDelta = Vec2.Vec(95, 20));
				sellValueTitle.With<TextMeshProUGUI>(text => {
					text.fontSize = 16;
					text.fontSizeMax = 16;
					text.color = GrayColor;
					text.alignment = TextAlignmentOptions.Left;
					text.text = "Sell Value:";
				});
				sellValueTitle.transform.SetSiblingIndex(0);
				// - Change size of the icon.
				_sellValueIcon.transform.Use<RectTransform>().sizeDelta = Vec2.Vec(18);
				// - Change the value text.
				_sellValueText.transform.Use<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
				_sellValueText.fontSize = 16;
				_sellValueText.fontSizeMax = 16;
			}
			
			// - Market value - No need to check if it's enabled, as it can be toggled
			//   without restarting the game.
			CreateMarketValue();
			
			// - Scroll info - No need to check if it's enabled, as it can be toggled
			//   without restarting the game.
			CreateScrollInfo();
			
			// - Equipment stats info - No need to check if it's enabled, as it can be toggled
			//   without restarting the game.
			CreateEquipmentStatsInfo();
			
			// - Popup description.
			//   Ehh, I really don't know, keeping it here just in case.
			
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
			IdleTasks.Update(_popup, () => {
				if (!ModSettings.MarketValue.ShiftForTotal.Value) return;
				var shift = Input.GetKey(KeyCode.LeftShift);
				if (shift == heldShift) return;
				heldShift = shift;
				
				// Update the text.
				UpdateItemValue(popupBehavior);
			});
		}

		private static void CreateMarketValue() {
			// Duplicate the value to create our market value.
			_marketValue = GameObjects.Instantiate(_sellValue, _valueContainer, false, "MarketValue");
			_marketValue.transform.SetSiblingIndex(0);
			_marketValue.Use<RectTransform>(rect => rect.sizeDelta = rect.sizeDelta.SetY(25));
			_marketValue.With<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			_marketValueIcon = _marketValue.Find("GoldImage").Use<Image>();
			_marketValueText = _marketValue.Find("ValueText").Use<TextMeshProUGUI>();

			// Swap the icon for the market icon.
			var icon = GameObjects.FindByCachedPath("GameCanvas/NavigationCanvas/CommunitySection/Tabs/PlayerMarketTab/ScalingObjects/Icon");
			_marketValueIcon.overrideSprite = icon.Use<Image>().activeSprite;
			
			// Fix the popup size.
			_background.Use<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			_popup.Use<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			if (!_enhancedTooltip) return;
			// If we're using enhanced tooltips, then we also need to change the title.
			var marketValueTitle = _marketValue.transform.GetChild(0);
			marketValueTitle.Use<TextMeshProUGUI>().text = "Market Value:";
		}
		
		private static void CreateScrollInfo() {
			var scrollInfo = GameObjects.NewRect("ScrollInfo", _nameContainer);
			scrollInfo.With<HorizontalLayoutGroup>().DisableChildStates();
			scrollInfo.With<ContentSizeFitter>().SetFit(ContentSizeFitter.FitMode.PreferredSize);
			_scrollInfo = scrollInfo.With<ScrollInfo>();
		}

		private static void CreateEquipmentStatsInfo() {
			if (!_enhancedTooltip) return; // Enhanced tooltip is required for equipment stats info.
			var equipmentStatsContainer = GameObjects.NewRect("EquipmentStatsInfo", _background);
			equipmentStatsContainer.With<VerticalLayoutGroup>().SetChildStates(true, false, true, false, false, false);
			equipmentStatsContainer.With<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			// Create the split before we add the EquipmentStatsInfo component, in case it
			// awakes.
			NewSplit(equipmentStatsContainer);
			// Then add the component.
			_equipmentStatsInfo = equipmentStatsContainer.With<EquipmentStatsInfo>();
		}
		
		/*
		 * Helper
		 */

		private static int _currentSplitIndex;
		public static GameObject NewSplit(GameObject parent, bool secondary = false) {
			var split = GameObjects.NewRect($"Split{_currentSplitIndex++}", parent);
			split.Use<RectTransform>().sizeDelta = Vec2.Vec(0, 1);
			split.With<ProceduralImage, UniformModifier>().color = new Color(0.1294F, 0.549F, 0.4549F, secondary ? 0.5F : 1F);
			return split;
		}
		
		/*
		 * Patch
		 */
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(InventoryItemHoverPopup.Setup))]
		public static void PostfixSetup(InventoryItemHoverPopup __instance, Item item) {
			if (item == null) return;
			
			// Internal name setting.
			if (ModSettings.Miscellaneous.InternalItemNames.Value) 
				__instance._itemNameText.text = $"{item.Name} ({item.ItemId})";
            
			// Enable or disable the scroll info depending on the item and setting.
			_scrollInfo.gameObject.SetActive(item.ScrollType != EnchantmentScrollType.None &&
			                                 ModSettings.Features.ScrollInfo.Value &&
			                                 _scrollInfo.Setup(item));
			
			// Enable or disable the equipment stats info depending on the item and setting.
			_equipmentStatsInfo.gameObject.SetActive(ModSettings.Features.EquipmentStatsInfo.Value &&
			                                         _enhancedTooltip &&
			                                         _equipmentStatsInfo.Setup(item));
			
			// Update the value.
			UpdateItemValue(__instance, item);
		}
		
		private static void UpdateItemValue(InventoryItemHoverPopup __instance, Item item = null) {
			if (item == null) item = __instance.AttachedItem;
			
			var baseObj = __instance._itemValueText.transform.parent.gameObject;

			var basePriceText = _sellValueText;//__instance._itemValueText;
			var marketPriceText = _marketValueText;//_marketValue.transform.GetChild(1).Use<TextMeshProUGUI>();

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
				
				// If we're using enhanced tooltips, we need to change the size of the
				// sell value title.
				if (_enhancedTooltip) {
					var size = _sellTitleRect.sizeDelta;
					_sellTitleRect.sizeDelta = size.SetX(canNotBeTraded ? SingleSellSizeX : DefaultSellSizeX);
				}
			}
			
			// Disable the market value if the item can't be sold.
			if (canNotBeTraded) _marketValue.SetActive(false);
			else {
				var price = OldIdleAPI.GetMarketEntry(item)?.GetPriceDependingOnSetting();
				var text = price == null || price <= 0 ? "???" : !shift ? 
						Numbers.FormatBasedOnSetting(price.Value) : 
						$"{Numbers.FormatBasedOnSetting(price.Value * amount)} = {amountText} x " +
						$"{Numbers.FormatBasedOnSetting(price.Value)}";
				
				_marketValue.SetActive(true);
				marketPriceText.text = text;
			}
			
			// If we're in enhanced mode and the item can't be sold or traded,
			// then disable the split and value container.
			if (_enhancedTooltip) {
				var state = !canNotBeSold || !canNotBeTraded;
				_valueContainer.SetActive(state);
				_split1.SetActive(state);
			}
			
			__instance._contentRefresh.RefreshContentFitters();
		}
	}
}