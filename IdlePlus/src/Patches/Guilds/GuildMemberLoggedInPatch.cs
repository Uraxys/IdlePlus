using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnGuildMemberLoggedIn")]
	public class OnGuildMemberLoggedIn {
		[HarmonyPostfix]
		public static void Postfix(string guildMemberName) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.MemberLoggedIn },
					{ "username", guildMemberName }
				}
			);
		}
	}
}