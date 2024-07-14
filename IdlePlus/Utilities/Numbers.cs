using System;
using IdlePlus.Settings;

namespace IdlePlus.Utilities {
	public static class Numbers {

		public static double TruncateToDecimalPlace(double value, int decimalPlaces) {
			var factor = Math.Pow(10, decimalPlaces);
			return Math.Truncate(value * factor) / factor;
		}
        
		public static string FormatBasedOnSetting(long number, bool vanillaFormat = false) {
			switch (ModSettings.MarketValue.DisplayFormat.Value) {
				case 0: return ToCompactFormat(number, vanillaFormat);
				case 1: return Format(number);
				case 2:
					return number < 1_000_000 ? 
						$"{TruncateToDecimalPlace((double)number / 1000, 1):0.#}K" : 
						$"{number / 1_000:#,0}K";
				case 3: 
					return number < 1_000_000_000 ? 
						$"{TruncateToDecimalPlace((double)number / 1_000_000, 1):0.#}M" : 
						$"{number / 1_000_000_000:#,0}B";
				default: throw new ArgumentOutOfRangeException();
			}
		}
		
		/// <summary>
		/// Converts the given number to a compact format.
		/// </summary>
		/// <param name="number">The number to compact.</param>
		/// <returns>The compacted number, numbers under 100,000 isn't compacted,
		/// but are instead formatted, while anything above 100,000 will be
		/// formatted as either 0K, 0M or 0B, depending on the amount.</returns>
		public static string ToCompactFormat(long number, bool vanillaFormat = false) {
			//if (number > 2_000_000_000) return ">2B";

			if (vanillaFormat) {
				if (number < 1000) return number.ToString("#,0");
				if (number < 1_000_000) return $"{TruncateToDecimalPlace((double)number / 1000, 1):0.#}K";
				if (number < 1_000_000_000) return $"{TruncateToDecimalPlace((double)number / 1_000_000, 1):0.#}M";
				return $"{TruncateToDecimalPlace((double)number / 1_000_000_000, 1):0.#}B";
				/*return number < 1_000_000_000 ? $"{(double) number / 1_000_000:0.#}M" : 
					$"{(double) number / 1_000_000_000:0.#}B";*/
			}
			
			if (number < 100_000) return number.ToString("#,0");
			if (number < 1_000_000) return $"{number / 1000}K";
			return number < 1_000_000_000 ? $"{number / 1_000_000.0:0.###}M" : $"{number / 1_000_000_000.0:0.###}B";
		}
		
		public static string Format(long number) => number.ToString("#,0");

		/// <summary>
		/// Parse the given input as a number.
		/// This method supports compact formats such as: 1k, 1m, 1b, 1kk, 1kkk, 0.5k, .5m, 1.b
		/// </summary>
		/// <param name="input">The input to parse.</param>
		/// <param name="format">The format the number was using.</param>
		/// <returns></returns>
		public static long ParseNumber(string input, out NumberModifier format) {
			format = NumberModifier.Unknown;
			NumberModifier modifier;

			// Check if the number has any modifiers.
			// AI doesn't like three k's.
			if (input.EndsWith("kk" + "k", StringComparison.OrdinalIgnoreCase)) format = NumberModifier.BillionK3;
			else if (input.EndsWith("kk", StringComparison.OrdinalIgnoreCase)) format = NumberModifier.MillionK2;
			else if (input.EndsWith("k", StringComparison.OrdinalIgnoreCase)) format = NumberModifier.Thousand;
			else if (input.EndsWith("m", StringComparison.OrdinalIgnoreCase)) format = NumberModifier.Million;
			else if (input.EndsWith("b", StringComparison.OrdinalIgnoreCase)) format = NumberModifier.Billion;

			// Set the modifier.
			if (format == NumberModifier.BillionK3) modifier = NumberModifier.Billion;
			else if (format == NumberModifier.MillionK2) modifier = NumberModifier.Million;
			else modifier = format;
			
			// If we found a modifier, remove it and parse the number.
			if (modifier != NumberModifier.Unknown) {
				var length = format == NumberModifier.BillionK3 ? 3 : format == NumberModifier.MillionK2 ? 2 : 1;
				input = input.Substring(0, input.Length - length);
				
				// If the number starts with a dot, add a zero in front of it,
				// or if it ends with a dot, remove it.
				if (input.StartsWith(".")) input = "0" + input;
				if (input.EndsWith(".")) input = input.TrimEnd('.');
				
				if (!double.TryParse(input, out var number)) return long.MinValue;
				return (long) (number * (long) modifier);
			}
			
			// Add a zero in front or behind the dot.
			if (input.StartsWith(".")) input = "0" + input;
			if (input.EndsWith(".")) input = input.TrimEnd('.');
			
			// Try to parse it as a normal number.
			if (!long.TryParse(input, out var result)) return long.MinValue;
			return result;
		}
		
		public enum NumberModifier {
			Unknown = 0,
			Thousand = 1_000,
			Million = 1_000_000,
			MillionK2 = 1_000_001,
			Billion = 1_000_000_000,
			BillionK3 = 1_000_000_001,
		}
	}
}