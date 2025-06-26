using Guilds;
using Databases;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnClanBossModifierPurchased")]
	public class OnClanBossModifierPurchased {
		[HarmonyPostfix]
		public static void Postfix(Network.ClanBossPurchaseModifierPurchasedMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.ClanBossModifierPurchased },
					{ "boss_type", message.BossType.ToString() },
					{ "modifier_type", message.ModifierType.ToString() },
					{ "username", message.Username.ToString() },
					{ "new_tier", message.NewTier.ToString() },
				}
			);
		}
	}
}