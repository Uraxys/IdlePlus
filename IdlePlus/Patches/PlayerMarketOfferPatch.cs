using System.Collections.Generic;
using System.Text.RegularExpressions;
using Databases;
using HarmonyLib;
using IdlePlus.Utilities;
using Il2CppSystem;
using PlayerMarket;
using TMPro;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace IdlePlus.Patches {
	
	[HarmonyPatch(typeof(PlayerMarketOffer))]
	public class PlayerMarketOfferPatch {

		private static readonly Dictionary<PlayerMarketOffer, string> PreviousInputs = new Dictionary<PlayerMarketOffer, string>();
		
		public static void Initialize() {
			// Find the price input field and set it no character validation.
			var priceInputObj = GameObjects.FindDisabledByPath("GameCanvas/PageCanvas/PlayerMarket/Panel/PlayerMarketOfferPage/Price/PriceInputField");
			var priceInputField = priceInputObj.GetComponent<TMPro.TMP_InputField>();
			priceInputField.characterValidation = TMP_InputField.CharacterValidation.None;
		}

		/// <summary>
		/// Add support for typing numbers as "k", "m" and "b" to represent
		/// thousands, millions and billions.
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnPriceInputFieldModified))]
		public static bool PrefixOnPriceInputFieldModified(PlayerMarketOffer __instance) {
			var previousText = PreviousInputs.TryGetValue(__instance, out var previousInput) ? previousInput : "";
			var input = __instance._priceInputField.m_Text;

			// If the input is empty, then don't do anything.
			if (input.Length == 0) {
				PreviousInputs[__instance] = "";
				return false;
			}
            
			// Validate format, if it isn't valid then set it back to the previous text.
			if (input.Length != 0 && !Regex.IsMatch(input, @"^(\d+(\.\d+)?|\.(\d+)?|\d+\.)([kmb]|kk|kkk)?$", 
				    RegexOptions.IgnoreCase)) {
				__instance._priceInputField.SetTextWithoutNotify(previousText);
				__instance.SetupTotalPriceText();
				return false;
			}

			// Parse the number.
			Numbers.NumberModifier modifier;
			var number = Numbers.ParseNumber(input, out modifier);
			
			// Make sure it isn't an invalid number.
			if (number == -1) {
				__instance.SetupTotalPriceText();
				PreviousInputs[__instance] = input;
				return false;
			}
			
			// Bounds check
			
			if (number < 0) {
				__instance._priceInputField.SetTextWithoutNotify("1");
				__instance.SetupTotalPriceText();
				PreviousInputs[__instance] = "1";
				return false;
			}

			var maxGold = (long) SettingsDatabase.SharedSettings.MaxPlayerGold;
			if (number > maxGold) {
				switch (modifier) {
					case Numbers.NumberModifier.Unknown:
						input = maxGold.ToString();
						break;
					case Numbers.NumberModifier.Thousand:
						input = maxGold / 1000 + "k";
						break;
					case Numbers.NumberModifier.Million:
						input = maxGold / 1_000_000 + "m";
						break;
					case Numbers.NumberModifier.MillionK2:
						input = maxGold / 1_000_000 + "kk";
						break;
					case Numbers.NumberModifier.Billion:
						input = maxGold / 1_000_000_000 + "b";
						break;
					case Numbers.NumberModifier.BillionK3:
						input = maxGold / 1_000_000_000 + "kk" + "k"; // AI doesn't like three k's.
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			
			__instance._priceInputField.SetTextWithoutNotify(input);
			__instance.SetupTotalPriceText();
			PreviousInputs[__instance] = input;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.GetCurrentTotalPrice))]
		public static bool PrefixGetCurrentTotalPrice(PlayerMarketOffer __instance, ref Decimal __result) {
			var priceInput = __instance._priceInputField.m_Text;
			var quantityInput = __instance._quantityInputField.m_Text;
			
			var price = Numbers.ParseNumber(priceInput, out _);
			var quantity = Numbers.ParseNumber(quantityInput, out _);
			
			__result = price * quantity;
			return false;
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnPriceIncrementButtonPressed))]
		public static bool PrefixOnPriceIncrementButtonPressed(PlayerMarketOffer __instance) {
			var input = __instance._priceInputField.m_Text;
			var number = Numbers.ParseNumber(input, out _) + 1;
			
			var maxGold = (long)SettingsDatabase.SharedSettings.MaxPlayerGold;
			if (number > maxGold) number = maxGold;
			
			__instance._priceInputField.SetText(number.ToString());
			return false;
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnPriceDecrementButtonPressed))]
		public static bool PrefixOnPriceDecrementButtonPressed(PlayerMarketOffer __instance) {
			var input = __instance._priceInputField.m_Text;
			var number = Numbers.ParseNumber(input, out _) - 1;
			if (number < 1) number = 1;
			
			__instance._priceInputField.SetText(number.ToString());
			return false;
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnIncrementByFivePercentageButtonPressed))]
		public static bool PrefixOnIncrementByFivePercentageButtonPressed(PlayerMarketOffer __instance) {
			var input = __instance._priceInputField.m_Text;
			var number = (long) (Numbers.ParseNumber(input, out _) * 1.05);
			
			var maxGold = (long)SettingsDatabase.SharedSettings.MaxPlayerGold;
			if (number > maxGold) number = maxGold;
			
			__instance._priceInputField.SetText(number.ToString());
			return false;
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnDecrementByFivePercentageButtonPressed))]
		public static bool PrefixOnDecrementByFivePercentageButtonPressed(PlayerMarketOffer __instance) {
			var input = __instance._priceInputField.m_Text;
			var number = (long) (Numbers.ParseNumber(input, out _) * 0.95);
			if (number < 1) number = 1;
			
			__instance._priceInputField.SetText(number.ToString());
			return false;
		}
		
		// Handle confirm offer.
		// I'm not going to recreate the entire method... so a simple prefix and
		// postfix hack will do.
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnConfirmButtonPressed))]
		public static void PrefixOnConfirmButtonPressed(PlayerMarketOffer __instance, out string __state) {
			// Before OnConfirmButtonPressed is called, save the current text and
			// set it to the parsed number, we'll then restore it after the method
			// has been called.
			__state = __instance._priceInputField.m_Text;
			var number = Numbers.ParseNumber(__state, out _);
			__instance._priceInputField.SetTextWithoutNotify(number.ToString());
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnConfirmButtonPressed))]
		public static void PostfixOnConfirmButtonPressed(PlayerMarketOffer __instance, string __state) {
			// Restore the original text.
			__instance._priceInputField.SetTextWithoutNotify(__state);
		}
	}
}