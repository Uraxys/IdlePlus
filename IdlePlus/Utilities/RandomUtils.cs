using UnityEngine;
using Random = System.Random;

namespace IdlePlus.Utilities {
	public static class RandomUtils {

		private static readonly Random RandomInstance = new Random();
		
		public static double Random() {
			return RandomInstance.NextDouble();
		}

		public static Color RandomColor() {
			return new Color((float) Random(), (float) Random(), (float) Random());
		}
	}
}