using System.Collections.Generic;
using Client;
using Databases;
using HarmonyLib;
using IdlePlus.IdleClansAPI;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using Network;
using Player;
using PlayerMarket;
using Popups;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.Patches {
	
	/// <summary>
	/// A patch to add an "Edit offer" button to the marker offer popup.
	/// </summary>
	[HarmonyPatch(typeof(ViewPlayerMarketOfferPopup))]
	public class ViewPlayerMarketOfferPopupPatch {

		private static readonly HashSet<string> EditingCancellingOffers = new HashSet<string>();
		private static readonly HashSet<string> EditingCollectingOffers = new HashSet<string>();

		private static GameObject _playerMarketOfferPopup;
		private static GameObject _abortBtnObj;
		private static GameObject _editBtnObj;
		
		[InitializeOnce]
		public static void InitializeOnce() {
			_playerMarketOfferPopup = GameObjects.FindByCachedPath("PopupManager/Canvas/HardPopups/ViewPlayerMarketOfferPopup");
			
			// "PopupManager/Canvas/HardPopups/ViewPlayerMarketOfferPopup/AbortOfferButton"
			_abortBtnObj = _playerMarketOfferPopup.Find("AbortOfferButton").gameObject;
			// "PopupManager/Canvas/HardPopups/ViewPlayerMarketOfferPopup/CloseButton"
			var closeBtn = _playerMarketOfferPopup.Find("CloseButton").gameObject;
			
			_editBtnObj = Object.Instantiate(closeBtn, closeBtn.transform.parent, false);
			_editBtnObj.name = "EditOfferButton";
			var editBtnText = _editBtnObj.transform.GetChild(0).gameObject;
			ModLocalization.SetModdedKey(editBtnText, "edit_offer");

			// Edit position and rect transform.
			var rect = _abortBtnObj.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(175, rect.sizeDelta.y);
			_abortBtnObj.transform.localPosition = new Vector3(-200, -205, 0);
			// then the edit button.
			rect = _editBtnObj.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(175, rect.sizeDelta.y);
			_editBtnObj.transform.localPosition = new Vector3(0, -205, 0);
			// then the close button.
			rect = closeBtn.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(175, rect.sizeDelta.y);
			closeBtn.transform.localPosition = new Vector3(200, -205, 0);
			
			// Clear the button events on the edit button and set our own.
			var button = _editBtnObj.GetComponent<UnityEngine.UI.Button>();
			button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
			button.onClick.AddListener((UnityAction)OnEditOfferButtonPressed);
			
			// Set the color.
			var buttonProceduralImage = _editBtnObj.GetComponent<ProceduralImage>();
			buttonProceduralImage.color = new Color(0.1294F, 0.549F, 0.6349F, 1F);
		}

		[Initialize]
		public static void Initialize() {
			var popup = _playerMarketOfferPopup;
			//var popup = GameObjects.FindByCachedPath("PopupManager/Canvas/HardPopups/ViewPlayerMarketOfferPopup");
			var popupComponent = popup.GetComponent<ViewPlayerMarketOfferPopup>();
			
			var playerMarket = GameObjects.FindByCachedPath("GameCanvas/PageCanvas/PlayerMarket");
			var playerMarketComponent = playerMarket.GetComponent<PlayerMarketPage>();
			
			// Update the player market page reference.
			popupComponent._playerMarketPage = playerMarketComponent;
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(ViewPlayerMarketOfferPopup.Setup))]
		private static void PostfixSetup(ViewPlayerMarketOfferPopup __instance, bool isBuyOffer, 
			MarketOfferNetwork offer) {
			var abortProceduralImage = _abortBtnObj.GetComponent<ProceduralImage>();
			var abortButton = _abortBtnObj.GetComponent<UnityEngine.UI.Button>();
			var abortText = _abortBtnObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
			
			var editProceduralImage = _editBtnObj.GetComponent<ProceduralImage>();
			var editButton = _editBtnObj.GetComponent<UnityEngine.UI.Button>();
			var editText = _editBtnObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
			
			if (offer.Status == MarketOfferStatus.Completed || offer.Status == MarketOfferStatus.Aborted) {
				// The offer isn't "active", so disable the button.
				editProceduralImage.color = new Color(0.25f, 0.45f, 0.5f);
				editButton.interactable = false;
				editText.color = new Color(0.75f, 0.75f, 0.75f);
				
				abortProceduralImage.color = new Color(0.54f, 0.27f, 0.27f);
				abortButton.interactable = false;
				abortText.color = new Color(0.75f, 0.75f, 0.75f);
				return;
			}
			
			// The offer is "active".
			editProceduralImage.color = new Color(0.1294F, 0.549F, 0.6349F, 1F);
			editButton.interactable = true;
			editText.color = new Color(1F, 1F, 1F);
			
			abortProceduralImage.color = new Color(0.702F, 0.2235F, 0.2235F, 1F);
			abortButton.interactable = true;
			abortText.color = new Color(1F, 1F, 1F);
			
			// Add the current lowest price to the "Price: " text.
			var prefix = __instance._isBuyOffer ? "Highest" : "Lowest";
			var marketEntry = IdleAPI.GetMarketEntry(__instance._item);
			var price = marketEntry == null ? "???" : __instance._isBuyOffer ? 
				Numbers.Format(marketEntry.GetBuyPrice()) : Numbers.Format(marketEntry.GetSellPrice());
			__instance._priceText.m_text += $" <color=#aaa>({prefix} {price})</color>";
		}

		private static void OnEditOfferButtonPressed() {
			var popup = _playerMarketOfferPopup;
			//var popup = GameObjects.FindByPath("PopupManager/Canvas/HardPopups/ViewPlayerMarketOfferPopup");
			var playerMarketPopup = popup.GetComponent<ViewPlayerMarketOfferPopup>();
			var marketOffer = playerMarketPopup._marketOffer;

			if (marketOffer == null) {
				IdleLog.Warn("Marker offer is null!");
				return;
			}

			if (marketOffer.AmountLeft < 1) {
				// ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
				var generic = new GenericPopup(PopupManager.Instance.SetupHardPopup(HardPopup.GenericPopup).Pointer);
				generic.SetupAndLocalize("cant_abort_fulfilled_offer");
				return;
			}
			
			PopupManager.Instance.SetupDefaultLoadingPopup();
			NetworkMessage cancelPacket;

			if (playerMarketPopup._isBuyOffer) cancelPacket = new CancelBuyOfferMessage { DocumentId = marketOffer.Id };
			else cancelPacket = new CancelSellOfferMessage { DocumentId = marketOffer.Id };
			
			EditingCancellingOffers.Add(marketOffer.Id);
			NetworkClient.SendData(cancelPacket);
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(ViewPlayerMarketOfferPopup.OnSellOfferCancelled))]
		private static bool PrefixOnSellOfferCancelled(ViewPlayerMarketOfferPopup __instance,
			CancelSellOfferMessage message) {
			if (!EditingCancellingOffers.Contains(message.DocumentId)) return true;
			EditingCancellingOffers.Remove(message.DocumentId);
			
			var marketOffer = __instance._marketOffer;
			if (marketOffer.ClaimableItems + marketOffer.AmountLeft < 1) return true;
			
			marketOffer.ClaimableItems += marketOffer.AmountLeft;
			marketOffer.Status = MarketOfferStatus.Aborted;
			
			ClaimItemsAndEdit(__instance);
			return false;
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(ViewPlayerMarketOfferPopup.OnBuyOfferCancelled))]
		private static bool PrefixOnBuyOfferCancelled(ViewPlayerMarketOfferPopup __instance,
			CancelBuyOfferMessage message) {
			if (!EditingCancellingOffers.Contains(message.DocumentId)) return true;
			EditingCancellingOffers.Remove(message.DocumentId);
			
			var marketOffer = __instance._marketOffer;
			marketOffer.ReimbursedGold = marketOffer.Price * marketOffer.AmountLeft;
			marketOffer.AmountLeft = 0;
			marketOffer.Status = MarketOfferStatus.Aborted;
			
			ClaimItemsAndEdit(__instance);
			return false;
		}
		
		private static void ClaimItemsAndEdit(ViewPlayerMarketOfferPopup instance) {
			var marketOffer = instance._marketOffer;

			var inventory = PlayerData.Instance.Inventory;
			var item = ItemDatabase.ItemList[marketOffer.ItemId];
			var gold = ItemDatabase.ItemList[ItemDatabase.GOLD_ITEM_ID];

			if (marketOffer.ClaimableItems > 0) {
				if (!inventory.CanAddItem(item, marketOffer.ClaimableItems)) {
					// ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
					var generic = new GenericPopup(PopupManager.Instance.SetupHardPopup(HardPopup.GenericPopup).Pointer);
					generic.SetupAndLocalize("inventory_is_full2");
					IdleLog.Warn("Couldn't edit market offer, inventory (item) is full.");
					return;
				}
			}
			
			if (marketOffer.ReimbursedGold > 0) {
				if (!inventory.CanAddItem(gold, marketOffer.ReimbursedGold)) {
					// ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
					var generic = new GenericPopup(PopupManager.Instance.SetupHardPopup(HardPopup.GenericPopup).Pointer);
					generic.SetupAndLocalize("inventory_is_full2");
					IdleLog.Warn("Couldn't edit market offer, inventory (gold) is full.");
					return;
				}
			}
			
			var claimItemsMessage = new ClaimPlayerMarketOrderItemsMessage {
				DocumentId = marketOffer.Id,
				IsBuyOffer = instance._isBuyOffer
			};
				
			EditingCollectingOffers.Add(marketOffer.Id);
			NetworkClient.SendData(claimItemsMessage);
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(ViewPlayerMarketOfferPopup.OnOfferItemsClaimed))]
		private static void PostfixOnOfferItemsClaimed(ViewPlayerMarketOfferPopup __instance,
			PlayerMarketOfferItemsClaimedMessage message) {
			var markerOffer = message.MarketOffer;
			if (!EditingCollectingOffers.Contains(markerOffer.Id)) return;
			EditingCollectingOffers.Remove(markerOffer.Id);
			var playerMarket = __instance._playerMarketPage;
			
			var item = ItemDatabase.ItemList[markerOffer.ItemId];
			var amount = __instance._isBuyOffer ? 
				markerOffer.OriginalFullAmount - markerOffer.TotalPurchasedItems : 
				markerOffer.ClaimableItems;
			var price = markerOffer.Price;

			if (__instance._isBuyOffer) playerMarket.CreateBuyOfferWithPreselectedItem(item, amount, price);
			else playerMarket.CreateSellOfferWithPreselectedItem(item, amount, price);
		}
	}
}