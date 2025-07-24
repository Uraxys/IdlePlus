using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using System.Collections.Generic;
using Databases;
using IdlePlus.API.Utility.Game;
using IdlePlus.Utilities.Helpers;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnItemSentToVault")]
	public class OnItemSentToVault {
		[HarmonyPostfix]
		public static void Postfix(Network.SendItemToGuildMessage message) {
			string itemName = "unknown";
			if (ItemDatabase.ItemList.TryGetValue(message.ItemId, out var item)) {
				itemName = item.IdlePlus_GetLocalizedEnglishName();
			}

			var items = new Il2CppSystem.Collections.Generic.Dictionary<int, int>();
			items[message.ItemId] = message.ItemAmount;

			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.ItemsSentToVault },
					{ "items", JsonHelper.Serialize(items) },
					{ "username", message.Username.ToString() }
				}
			);
		}
	}
}