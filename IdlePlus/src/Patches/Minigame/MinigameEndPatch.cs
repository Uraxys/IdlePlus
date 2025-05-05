using HarmonyLib;
using Minigames;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Minigame {
	[HarmonyPatch(typeof(MinigameManager), "EndMinigame")]
	public class EndGameEventPatch {
		[HarmonyPostfix]
		public static void Postfix() {
			WebhookManager.AddSendWebhook(
				WebhookType.Minigame,
				new Dictionary<string, string>
				{
					{ "action", "stop" },
					{ "type", MinigameTracker.LastEventType.ToString() }
				}
			);

			MinigameTracker.LastEventType = global::Guilds.UI.ClanEventType.None;
		}
	}
}
