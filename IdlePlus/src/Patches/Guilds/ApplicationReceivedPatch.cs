using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnApplicationReceived")]
	public class OnApplicationReceived {
		[HarmonyPostfix]
		public static void Postfix(Network.ReceiveGuildApplicationMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.ApplicationReceived },
					{ "player_applying", message.PlayerApplying.ToString() },
					{ "player_total_level", message.PlayerTotalLevel.ToString() },
					{ "message", message.Message.ToString() }
				}
			);
		}
	}
}