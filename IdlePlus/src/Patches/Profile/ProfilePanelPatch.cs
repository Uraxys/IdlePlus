using System;
using Equipment;
using HarmonyLib;
using IdlePlus.API.Utility;
using IdlePlus.Attributes;
using IdlePlus.IdleClansAPI;
using IdlePlus.Settings;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Player;
using Profile;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdlePlus.Patches.Profile {
	
	[HarmonyPatch(typeof(ProfilePanel))]
	public class ProfilePanelPatch {
		
		private static GameObject _totalWealth;

		[InitializeOnce(OnSceneLoad = Scenes.Game)]
		private static void InitializeOnce() {
			if (!ModSettings.Features.TotalWealth.Value) return;
			IdleAPI.OnMarketPricesFetched += OnMarketPricesFetched;
		}
		
		[Initialize(OnSceneLoad = Scenes.MainMenu)]
		private static void Initialize() {
			if (!ModSettings.Features.TotalWealth.Value) return;
			_totalWealth = null;
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(ProfilePanel.SetupData))]
		private static void PostfixSetupData(ProfilePanel __instance) {
			if (!ModSettings.Features.TotalWealth.Value) return;
			// When the object is destroyed the equals check is overriden by unity
			// to return true even if the object isn't null.
			if (_totalWealth == null) {
				var userInfoObj = __instance._usernameText.transform.parent;
				
				// Move all the fields up.
				var pos = __instance._usernameText.transform.localPosition;
				__instance._usernameText.transform.localPosition = pos.SetY(90);
				__instance._modeText.transform.localPosition = pos.SetY(65);
				__instance._currentActivityText.transform.localPosition = pos.SetY(40);
				__instance._offlineProgressCapText.transform.localPosition = pos.SetY(15);
				__instance._totalLevelText.transform.localPosition = pos.SetY(-10);
				
				// Create the total wealth field.
				_totalWealth = Object.Instantiate(__instance._totalLevelText.gameObject, userInfoObj.transform, false);
				_totalWealth.name = "TotalWealthText";
				_totalWealth.transform.localPosition = pos.SetY(-35);
			}

			UpdateTotalWealth();
		}

		private static void OnMarketPricesFetched(bool first) {
			if (!first) return;
			UpdateTotalWealth();
		}
		
		private static void UpdateTotalWealth() {
			if (_totalWealth == null) return;
			var text = _totalWealth.Use<TextMeshProUGUI>();

			if (!IdleAPI.IsInitialized()) {
				text.text = "Total Wealth: ...";
				return;
			}

			// If we're using vendor value instead of market value.
			var vendorValue = ModSettings.Features.TotalWealthVendorValue.Value;
			
			// Calculate total wealth.
			var inventory = PlayerData.Instance.Inventory;
			var equipment = PlayerData.Instance.Equipment;
			var marketPrices = IdleAPI.MarketPrices;
			var wealth = 0L;

			// Gold
			wealth += (long) inventory.Gold;
			
			// Inventory
			if (inventory.InventoryItems == null) return;
			foreach (var inventoryItem in inventory.InventoryItems) {
				var item = inventoryItem.Item;
				var amount = inventoryItem.ItemAmount;
				if (item == null || amount <= 0) continue;
			
				if (vendorValue || !marketPrices.TryGetValue(item.ItemId, out var price)) {
					if (item.CanNotBeSoldToGameShop) continue;
					wealth += (long) item.BaseValue * amount;
					continue;
				}
				
				var value = price.GetPriceDependingOnSetting();
				if (value == -1) {
					if (item.CanNotBeSoldToGameShop) continue;
					value = item.BaseValue;
				}
				wealth += (long) value * amount;
			}
			
			// Equipment
			if (equipment._equippedItems == null) return;
			foreach (var entry in equipment._equippedItems) {
				var slot = entry.Key;
				var item = entry.Value;
				if (item == null) continue;

				var multiplier = 1;
				if (slot == EquipmentSlot.Ammunition) multiplier = equipment._equippedAmmunitionAmount;
				
				if (vendorValue || !marketPrices.TryGetValue(item.ItemId, out var price)) {
					if (item.CanNotBeSoldToGameShop) continue;
					wealth += item.BaseValue * multiplier;
					continue;
				}
				
				var value = price.GetPriceDependingOnSetting();
				if (value == -1) {
					if (item.CanNotBeSoldToGameShop) continue;
					value = item.BaseValue;
				}
				wealth += value * multiplier;
			}
		
			// And the total wealth is...
			text.text = $"Total Wealth: {Numbers.Format(wealth)}";
		}
	}
}