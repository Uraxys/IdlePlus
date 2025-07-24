using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnDailyCombatQuestProgressed")]
	public class OnDailyCombatQuestProgressed {
		[HarmonyPostfix]
		public static void Postfix(Network.ProgressDailyCombatQuestMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.DailyCombatQuestProgressed },
					{ "entity_id", message.EntityId.ToString() },
					{ "username", message.Username.ToString() }
				}
			);
		}
	}
}