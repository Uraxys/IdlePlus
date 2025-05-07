using HarmonyLib;
using Minigames;
using IdlePlus.Utilities;
using System.Collections.Generic;

namespace IdlePlus.Patches.Minigame {
	[HarmonyPatch(typeof(MinigameManager), "StartGame")]
	public class StartGameEventPatch {
		[HarmonyPostfix]
		public static void Postfix(Minigames.Minigame minigame) {
			MinigameTracker.LastEventType = minigame.EventType;

			WebhookManager.AddSendWebhook(
				WebhookType.Minigame,
				new Dictionary<string, string>
				{
					{ "action", "start" },
					{ "type", minigame.EventType.ToString() }
				}
			);
		}
	}
}
