using Guilds;
using HarmonyLib;
using Databases;
using IdlePlus.Utilities;
using System.Collections.Generic;
using IdlePlus.API.Utility.Game;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnItemWithdrawnFromVault")]
	public class OnItemWithdrawnFromVault {
		[HarmonyPostfix]
		public static void Postfix(Network.WithdrawItemFromGuildMessage message) {
			string itemName = "unknown";
			if (ItemDatabase.ItemList.TryGetValue(message.ItemId, out var item)) {
				itemName = item.IdlePlus_GetLocalizedEnglishName();
			}

			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.ItemWithdrawnFromVault },
					{ "username", message.Username.ToString() },
					{ "item_id", message.ItemId.ToString() },
					{ "item_name", itemName },
					{ "amount", message.ItemAmount.ToString() }
				}
			);
		}
	}
}