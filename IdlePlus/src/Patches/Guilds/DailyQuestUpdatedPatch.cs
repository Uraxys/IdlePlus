using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnDailyQuestUpdated")]
	public class OnDailyQuestUpdated {
		[HarmonyPostfix]
		public static void Postfix(Network.RefreshDailyQuestsMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.DailyQuestsUpdated },
				}
			);
		}
	}
}