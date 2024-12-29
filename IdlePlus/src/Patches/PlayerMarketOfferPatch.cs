using System.Collections.Generic;
using System.Text.RegularExpressions;
using Databases;
using HarmonyLib;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using Il2CppSystem;
using Player;
using PlayerMarket;
using TMPro;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace IdlePlus.Patches {
	
	/// <summary>
	/// A patch to add support for compact numbers in the market offer price.
	/// </summary>
	[HarmonyPatch(typeof(PlayerMarketOffer))]
	public class PlayerMarketOfferPatch {

		private static readonly Dictionary<PlayerMarketOffer, string> PreviousPriceInputs = new Dictionary<PlayerMarketOffer, string>();
		private static readonly Dictionary<PlayerMarketOffer, string> PreviousQuantityInputs = new Dictionary<PlayerMarketOffer, string>();
		
		[Initialize]
		public static void Initialize() {
			// Find the price input field and set it no character validation.
			var priceInputObj = GameObjects.FindByCachedPath("GameCanvas/PageCanvas/PlayerMarket/Panel/PlayerMarketOfferPage/Price/PriceInputField");
			var priceInputField = priceInputObj.GetComponent<TMP_InputField>();
			priceInputField.characterValidation = TMP_InputField.CharacterValidation.None;
			
			// Get the quantity input field from the price input object.
			// "GameCanvas/PageCanvas/PlayerMarket/Panel/PlayerMarketOfferPage/Quantity/QuantityInputField"
			var quantityInputObj = priceInputObj.transform.parent.parent.Find("Quantity/QuantityInputField");
			var quantityInputField = quantityInputObj.GetComponent<TMP_InputField>();
			quantityInputField.characterValidation = TMP_InputField.CharacterValidation.None;
		}

		/// <summary>
		/// Add support for typing numbers as "k", "m" and "b" to represent
		/// thousands, millions and billions.
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnPriceInputFieldModified))]
		public static bool PrefixOnPriceInputFieldModified(PlayerMarketOffer __instance) {
			var previousText = PreviousPriceInputs.TryGetValue(__instance, out var previousInput) ? previousInput : "";
			var input = __instance._priceInputField.m_Text;

			// If the input is empty, then don't do anything.
			if (input.Length == 0) {
				PreviousPriceInputs[__instance] = "";
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
			var number = Numbers.ParseNumber(input, out var modifier);
			
			// Make sure it isn't an invalid number.
			if (number == long.MinValue) {
				__instance.SetupTotalPriceText();
				PreviousPriceInputs[__instance] = input;
				return false;
			}
			
			// Bounds check
			
			if (number < 0) {
				__instance._priceInputField.SetTextWithoutNotify("1");
				__instance.SetupTotalPriceText();
				PreviousPriceInputs[__instance] = "1";
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
			PreviousPriceInputs[__instance] = input;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnQuantityInputFieldModified))]
		public static bool PrefixOnQuantityInputFieldModified(PlayerMarketOffer __instance) {
			var previousText = PreviousQuantityInputs.TryGetValue(__instance, out var previousInput) ? previousInput : "";
			var input = __instance._quantityInputField.m_Text;

			// If the input is empty, then don't do anything.
			if (input.Length == 0) {
				PreviousQuantityInputs[__instance] = "";
				return false;
			}
            
			// Validate format, if it isn't valid then set it back to the previous text.
			if (input.Length != 0 && !Regex.IsMatch(input, @"^(\d+(\.\d+)?|\.(\d+)?|\d+\.)([kmb]|kk|kkk)?$", 
				    RegexOptions.IgnoreCase)) {
				__instance._quantityInputField.SetTextWithoutNotify(previousText);
				__instance.SetupTotalPriceText();
				return false;
			}

			// Parse the number.
			var number = Numbers.ParseNumber(input, out _);

			// Make sure it isn't an invalid number.
			if (number == long.MinValue) {
				__instance.SetupTotalPriceText();
				PreviousQuantityInputs[__instance] = input;
				return false;
			}

			// Bounds check
			
			if (number < 0) {
				__instance._quantityInputField.SetTextWithoutNotify("1");
				__instance.SetupTotalPriceText();
				PreviousQuantityInputs[__instance] = "1";
				return false;
			}

			// Buy offers doesn't have an upper limit.
			if (__instance.IsBuyOffer) {
				__instance._quantityInputField.SetTextWithoutNotify(input);
				__instance.SetupTotalPriceText();
				PreviousQuantityInputs[__instance] = input;
				return false;
			}

			var itemAmount = PlayerData.Instance.Inventory.GetItemAmount(__instance._selectedItem);
			if (number > itemAmount) {
				__instance._quantityInputField.SetTextWithoutNotify(itemAmount.ToString());
				__instance.SetupTotalPriceText();
				PreviousQuantityInputs[__instance] = itemAmount.ToString();
				return false;
			}
			
			__instance._quantityInputField.SetTextWithoutNotify(input);
			__instance.SetupTotalPriceText();
			PreviousQuantityInputs[__instance] = input;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.IncrementQuantityInputFieldAmount))]
		public static bool PrefixIncrementQuantityInputFieldAmount(PlayerMarketOffer __instance, int amount) {
			var input = __instance._quantityInputField.m_Text;
			var number = Numbers.ParseNumber(input, out _);
			
			// If the number is invalid, then set it to 1 + amount.
			if (number == long.MinValue) {
				number = 1 + amount;
				__instance._quantityInputField.SetText(number.ToString());
				return false;
			}

			number += amount;
			
			// Can't go below 1.
			if (number < 1) {
				__instance._quantityInputField.SetText("1");
				return false;
			}
			
			__instance._quantityInputField.SetText(number.ToString());
			return false;
		}
		
		// TODO: When the client is updated, we might be able to remove most of
		//       these patches and instead just patch the get price method.

		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.GetCurrentTotalPrice))]
		public static bool PrefixGetCurrentTotalPrice(PlayerMarketOffer __instance, ref Decimal __result) {
			var priceInput = __instance._priceInputField.m_Text;
			var quantityInput = __instance._quantityInputField.m_Text;
			
			var price = Numbers.ParseNumber(priceInput, out _);
			var quantity = Numbers.ParseNumber(quantityInput, out _);

			// If the price or quantity is invalid, then set the total price to
			// an invalid value.
			if (quantity < 1 || price < 1) {
				__result = Decimal.MinValue;
				return false;
			}
			
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
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnMaxButtonPressed))]
		public static void PrefixOnMaxButtonPressed(PlayerMarketOffer __instance, out string __state) {
			__state = __instance._priceInputField.m_Text;
			var number = Numbers.ParseNumber(__instance._priceInputField.m_Text, out _);
			__instance._priceInputField.SetTextWithoutNotify(number.ToString());
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnMaxButtonPressed))]
		public static void PostfixOnMaxButtonPressed(PlayerMarketOffer __instance, string __state) {
			__instance._priceInputField.SetTextWithoutNotify(__state);
		}
		
		// Handle confirm offer.
		// I'm not going to recreate the entire method... so a simple prefix and
		// postfix hack will do.
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnConfirmButtonPressed))]
		public static void PrefixOnConfirmButtonPressed(PlayerMarketOffer __instance,
			out Tuple<string, string> __state) {
			
			// Before OnConfirmButtonPressed is called, save the current text and
			// set it to the parsed number, we'll then restore it after the method
			// has been called.
			__state = Tuple.Create(__instance._priceInputField.m_Text, __instance._quantityInputField.m_Text);
			var number = Numbers.ParseNumber(__instance._priceInputField.m_Text, out _);
			__instance._priceInputField.SetTextWithoutNotify(number.ToString());
			
			// Also do the same hack for the quantity input field.
			number = Numbers.ParseNumber(__instance._quantityInputField.m_Text, out _);
			__instance._quantityInputField.SetTextWithoutNotify(number.ToString());
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerMarketOffer.OnConfirmButtonPressed))]
		public static void PostfixOnConfirmButtonPressed(PlayerMarketOffer __instance, 
			Tuple<string, string> __state) {
			
			// Restore the original text.
			__instance._priceInputField.SetTextWithoutNotify(__state.Item1);
			__instance._quantityInputField.SetTextWithoutNotify(__state.Item2);
		}
	}
}