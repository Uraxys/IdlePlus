using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;
using Databases;
using IdlePlus.API.Utility.Game;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnDailySkillingQuestProgressed")]
	public class OnDailySkillingQuestProgressed {
		[HarmonyPostfix]
		public static void Postfix(Network.ProgressDailySkillingQuestMessage message) {
			string itemName = "unknown";
			if (ItemDatabase.ItemList.TryGetValue(message.ItemId, out var item)) {
				itemName = item.IdlePlus_GetLocalizedEnglishName();
			}

			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.DailySkillingQuestProgressed },
					{ "item_id", message.ItemId.ToString() },
					{ "item_name", itemName },
					{ "amount", message.Amount.ToString() },
					{ "original_amount", message.OriginalAmount.ToString() },
					{ "restored_amount", message.RestoredAmount.ToString() },
					{ "username", message.Username.ToString() }
				}
			);
		}
	}
}