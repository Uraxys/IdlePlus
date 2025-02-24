using System.Linq;

namespace IdlePlus.API.Utility.Game {
	public static class PlayerUtils {

		/// <summary>
		/// Checks if the given string is a valid username of a player.
		/// </summary>
		/// <param name="name">The username to check.</param>
		/// <returns>True if it's valid, false if it isn't.</returns>
		public static bool IsValidUsername(string name) {
			if (name.Length < 3 || name.Length > 20) return false;
			return name.All(char.IsLetterOrDigit);
		}
		
	}
}