using Guilds;
using HarmonyLib;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Helpers;
using System.Collections.Generic;

namespace IdlePlus.Patches.Guilds {
	[HarmonyPatch(typeof(GuildListener), "OnItemsSentToVault")]
	public class OnItemsSentToVault {
		[HarmonyPostfix]
		public static void Postfix(Network.SendItemsToGuildMessage message) {
			WebhookManager.AddSendWebhook(
				WebhookType.ClanAction,
				new Dictionary<string, string>
				{
					{ "action", ClanActionWebhooks.ItemsSentToVault },
					{ "items", JsonHelper.Serialize(message.Items) },
					{ "username", message.Username.ToString() }
				}
			);
		}
	}
}