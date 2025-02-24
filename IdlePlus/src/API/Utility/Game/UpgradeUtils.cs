using Guilds;
using Upgrades;

namespace IdlePlus.API.Utility.Game {
	public static class UpgradeUtils {
		
		/// <summary>
		/// Check if the player or clan has unlocked the specified upgrade.
		/// </summary>
		/// <param name="type">The upgrade type to check.</param>
		/// <returns>True if the upgrade is unlocked, false otherwise.</returns>
		public static bool IsUnlocked(UpgradeType type) {
			return IsUnlockedForPlayer(type) || IsUnlockedForClan(type);
		}
		
		/// <summary>
		/// Check if the player has unlocked the specified upgrade.
		/// </summary>
		/// <param name="type">The upgrade type to check.</param>
		/// <returns>True if the upgrade is unlocked, false otherwise.</returns>
		public static bool IsUnlockedForPlayer(UpgradeType type) {
			var upgradeManager = UpgradeManager.Instance;
			return upgradeManager != null && upgradeManager.IsUnlocked(type);
		}
		
		/// <summary>
		/// Check if the clan this player is in has unlocked the specified upgrade,
		/// or if the player is not in a clan, then this will return false.
		/// </summary>
		/// <param name="type">The upgrade type to check.</param>
		/// <returns>True if the upgrade is unlocked, false otherwise.</returns>
		public static bool IsUnlockedForClan(UpgradeType type) {
			var guildManager = GuildManager.Instance;
			if (guildManager == null) return false;
			var guild = guildManager.OurGuild;
			return guild != null && guild.UpgradeIsUnlocked(type);
		}
	}
}