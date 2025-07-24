using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnSkillingTicketReceived")]
	public class OnSkillingTicketReceived {
		[HarmonyPostfix]
		public static void Postfix(Network.ReceiveSkillingTicketMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.SkillingTicketReceived },
					{ "skill", message.Skill.ToString() },
					{ "amount", message.Amount.ToString() },
					{ "username", message.Username.ToString() }
				}
			);
		}
	}
}