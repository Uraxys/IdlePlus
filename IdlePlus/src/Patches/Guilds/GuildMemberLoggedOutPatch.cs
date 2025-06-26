using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnGuildMemberLoggedOut")]
	public class OnGuildMemberLoggedOut {
		[HarmonyPostfix]
		public static void Postfix(string guildMemberName) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.MemberLoggedOut },
					{ "username", guildMemberName }
				}
			);
		}
	}
}