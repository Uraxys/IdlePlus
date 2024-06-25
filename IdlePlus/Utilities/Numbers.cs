using System;

namespace IdlePlus.Utilities {
	public static class Numbers {

		public static string ToCompactFormat(long number) {
			if (number < 100_000) return number.ToString("#,0");
			if (number < 1_000_000) return $"{number / 1000}K";
			if (number < 1_000_000_000) return $"{number / 1_000_000.0:0.###}M";
			return $"{number / 1_000_000_000.0:0.###}B";
		}

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
				
				if (!double.TryParse(input, out var number)) return -1;
				return (long) (number * (long) modifier);
			}
			
			// Check if the number starts or ends with a dot, if it does, remove it.
			input = input.Trim('.');
			
			// Try to parse it as a normal number.
			if (!long.TryParse(input, out var result)) return -1;
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