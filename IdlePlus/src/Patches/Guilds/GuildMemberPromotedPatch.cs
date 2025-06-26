using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnGuildMemberPromoted")]
	public class OnGuildMemberPromoted {
		[HarmonyPostfix]
		public static void Postfix(Network.GuildMemberPromotedMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.MemberPromoted },
					{ "username", message.PlayerName },
					{ "new_rank", message.NewRank.ToString() },
				}
			);
		}
	}
}