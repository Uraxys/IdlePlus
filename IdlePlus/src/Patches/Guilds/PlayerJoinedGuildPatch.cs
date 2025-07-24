using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Helpers;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnJoinedGuild")]
	public class OnJoinedGuild {
		[HarmonyPostfix]
		public static void Postfix(Network.PlayerJoinedGuildMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.MemberJoinedClan },
					{ "username", message.PlayerJoining.ToString() },
				}
			);
		}
	}
}