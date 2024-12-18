/*using Ads;
using Databases;
using GameContent;
using HarmonyLib;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;*/

namespace IdlePlus.Patches {
	
	// Will probably patch the ads view popup later to disable buttons,
	// so I'll just leave everything commented out for now.
	
	//[HarmonyPatch(typeof(AdsViewPopup))]
	public class AdsViewPopupPatch {

		/*private static GameObject _claimBtnObj;
		private static GameObject _claimAllBtnObj;*/
		
		/*[InitializeOnce]
		public static void InitializeOnce() {
			_claimBtnObj = GameObjects.FindByCachedPath("PopupManager/Canvas/HardPopups/AdsViewPopup/WatchAdButton");
			_claimAllBtnObj = Object.Instantiate(_claimBtnObj, _claimBtnObj.transform.parent, false);
			
			// Edit position and rect transform.
			_claimBtnObj.transform.localPosition = new Vector3(-120, -165, 0);
			_claimBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 40);
			_claimAllBtnObj.transform.localPosition = new Vector3(120, -165, 0);
			_claimAllBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 40);
			
			// Update the text of the claim all button.
			var claimAllBtnText = _claimAllBtnObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
			claimAllBtnText.text = ModLocalization.GetModdedValue("claim_all");
			
			// Clear the button events on the claim all button.
			var button = _claimAllBtnObj.GetComponent<UnityEngine.UI.Button>();
			button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
			button.onClick.AddListener((UnityAction)OnClaimAllButtonPressed);
		}*/

		/*private static void OnClaimAllButtonPressed() {
			// Keep original limitations, if not the server will disagree and
			// just display a "Loading" popup.
			
			if (!PlayerRealMoneyPurchases.Instance.IsPremium) {
				PopupHelper.GenericLocalized("ads_not_available_on_this_platform", true);
				return;
			}
			
			var adsManager = AdsManager.Instance;
			var timeNow = DateTime.UtcNow.TimeOfDay;

			if (!adsManager.DailyLimitReached() && timeNow > adsManager.GetTimeUntilAdCanBeWatched()) {
				// Claim our remaining boosts.
				var maxAds = SettingsDatabase.SharedSettings.DailyAdsLimit;
				for (var i = adsManager.AdWatchesToday; i < maxAds; i++) {
					adsManager.WatchAd();
				}
				return;
			}
			
			// We can't claim the boost.

			if (adsManager.DailyLimitReached()) {
				PopupHelper.GenericLocalized("ads_cooldown_until_reset");
				return;
			}
			
			var timeUntilNextAd = adsManager.GetTimeUntilAdCanBeWatched() - timeNow;
			var minutes = ((int) timeUntilNextAd.TotalMinutes).ToString();
			PopupHelper.GenericText(ModLocalization.GetValue("ads_boost_cooldown", minutes));
		}*/
		
		// TODO: Settings to turn on / off.
		
		/*[HarmonyPostfix]
		[HarmonyPatch(nameof(AdsViewPopup.OnEnable))]
		private static void OnEnable(AdsViewPopup __instance) {
			CheckButtons();
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(AdsViewPopup.OnAdBoostReceived))]
		private static void PostfixOnAdBoostReceived(AdsViewPopup __instance) {
			CheckButtons();
		}

		private static void CheckButtons() {
			var adsManager = AdsManager.Instance;
			var claimBtn = _claimBtnObj.GetComponent<UnityEngine.UI.Button>();
			var claimAllBtn = _claimAllBtnObj.GetComponent<UnityEngine.UI.Button>();
			
			// If we've reached the daily limit, disable the buttons.
			if (adsManager.DailyLimitReached()) {
				claimBtn.interactable = false;
				claimAllBtn.interactable = false;
				return;
			}
			
			// if not, enable the buttons.
			claimBtn.interactable = true;
			claimAllBtn.interactable = true;
		}*/
	}
}