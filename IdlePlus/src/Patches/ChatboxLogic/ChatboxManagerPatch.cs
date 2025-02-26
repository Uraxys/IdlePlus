using ChatboxLogic;
using HarmonyLib;
using IdlePlus.API.Event;
using IdlePlus.API.Event.Contexts;
using Networking.Chat;

namespace IdlePlus.Patches.ChatboxLogic {
	[HarmonyPatch(typeof(ChatboxManager))]
	public class ChatboxManagerPatch {

		[HarmonyPrefix]
		[HarmonyPatch(nameof(ChatboxManager.OnReceiveChatboxMessage))]
		private static void PrefixOnReceiveChatboxMessage(ChatboxMessage message) {
			// Currently, the history received from the public API contains
			// the Sender field, but the packet doesn't, which allows us to
			// filter out the previous history messages.
			if (message.Sender != null) return;
			Events.Chat.OnPublicMessage.Call(new ChatMessageEventContext(message));
		}
		
	}
}