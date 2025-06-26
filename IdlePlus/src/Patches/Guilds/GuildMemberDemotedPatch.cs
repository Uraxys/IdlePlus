using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnGuildMemberDemoted")]
	public class OnGuildMemberDemoted {
		[HarmonyPostfix]
		public static void Postfix(string playerName) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.MemberDemoted },
					{ "username", playerName },
				}
			);
		}
	}
}