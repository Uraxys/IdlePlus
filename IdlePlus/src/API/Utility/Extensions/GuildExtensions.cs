using System;
using JetBrains.Annotations;
using Player;

namespace IdlePlus.API.Utility.Extensions {
	public static class GuildExtensions {

		public static bool IsPlayerInGuild(this Guild guild, [NotNull] string username, bool ignoreCase = false) {
			if (guild.AllMembers.ContainsKey(username)) return true;
			if (!ignoreCase) return false;

			foreach (var member in guild.AllMembers) {
				if (!string.Equals(member.key, username, StringComparison.CurrentCultureIgnoreCase)) continue;
				return true;
			}

			return false;
		}

		public static bool LocalPlayerHasAdminPrivileges(this Guild guild) =>
			guild.PlayerHasAdminPriviledges(PlayerData.Instance.Username);
	}
}