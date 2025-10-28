using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnGuildHousePurchased")]
	public class OnGuildHousePurchased {
		[HarmonyPostfix]
		public static void Postfix(Network.PurchaseGuildHouseMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.ClanHousePurchased },
				}
			);
		}
	}
}