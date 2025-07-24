using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnUpgradePurchased")]
	public class OnUpgradePurchased {
		[HarmonyPostfix]
		public static void Postfix(Network.ClanUpgradePurchasedMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.ClanUpgradePurchased },
					{ "upgrade_type", message.Upgrade.ToString() },
				}
			);
		}
	}
}