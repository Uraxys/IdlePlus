using ChatboxLogic;
using HarmonyLib;
using IdlePlus.Command;
using IdlePlus.TexturePack;
using IdlePlus.Utilities.Extensions;
using UnityEngine;

namespace IdlePlus.Patches.ChatboxLogic {
	
	[HarmonyPatch(typeof(Chatbox))]
	public class ChatboxPatch {
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(Chatbox.SendMessageToServer))]
		private static bool PrefixSendMessageToServer(Chatbox __instance) {
			var message = __instance._inputField.text;
			var result = CommandManager.Handle(message);

			if (!result) return true;
			__instance._inputField.text = "";
			return false;
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(Chatbox.InsertMessageIntoChatbox))]
		private static void PrefixSetup(Chatbox __instance, Transform parent, GameMode gameMode, bool isPremium,
			bool isPremiumPlus) {
			if (TexturePackManager.CurrentPack == null) return;
			
			var chatObject = parent.GetChild(0);
			var chatEntry = chatObject.Use<ChatboxMessageEntry>();
            
			var pack = TexturePackManager.CurrentPack;
			var ironmanIcon = chatEntry._ironmanIconGO;
			var premiumIcon = chatEntry._premiumGO;
			var gildedIcon = chatEntry._premiumPlusGO;
			
			if (gameMode == GameMode.Ironman) pack.TryApplyMiscSprite("ironman_icon", ironmanIcon);
			if (isPremium) pack.TryApplyMiscSprite("premium_icon", premiumIcon);
			if (isPremiumPlus) pack.TryApplyMiscSprite("gilded_icon", gildedIcon);
		}
	}
}