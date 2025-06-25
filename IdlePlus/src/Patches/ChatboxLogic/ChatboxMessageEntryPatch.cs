using System;
using System.Collections.Generic;
using System.Linq;
using ChatboxLogic;
using HarmonyLib;
using IdlePlus.API;
using IdlePlus.API.Popup;
using IdlePlus.API.Popup.Popups;
using IdlePlus.API.Utility.Game;
using IdlePlus.Settings;
using IdlePlus.Unity.Chat;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Il2CppSystem.Threading.Tasks;
using Popups;
using UnityEngine;

namespace IdlePlus.Patches.ChatboxLogic {
	
	[HarmonyPatch(typeof(ChatboxMessageEntry))]
	internal class ChatboxMessageEntryPatch {
		
		/// <summary>
		/// Characters allowed in front of an item name.
		/// </summary>
		private static readonly HashSet<char> AllowedPrefixes = new HashSet<char> 
			{ ' ', '(', '[', '{', '/' };
		/// <summary>
		/// Characters allowed behind an item name.
		/// </summary>
		private static readonly HashSet<char> AllowedPostfixes = new HashSet<char>
			{ ' ', ',', '.', '\'', '?', '!', ')', ']', '}', '/' };
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(ChatboxMessageEntry.Setup))]
		private static void PrefixSetup(ChatboxMessageEntry __instance, ref string message, GameMode gameMode) {
			// Check if this is a system message, if so, enable rich tags
			// and return early, as we don't need to worry about those
			// messages.
			if (gameMode == GameMode.NotSelected) {
				__instance._text.richText = true;
				return;
			}
			
			// Make sure we've enabled chat items.
			if (!ModSettings.Features.EnhancedChat.Value) return;
			
			// We require rich text tags.
			__instance._text.richText = true;
			
			// Make sure this is a valid player message, and get the content.
			if (!PlayerUtils.IsPlayerMessage(message, out _, out _, out var content)) return;

			// Escape any rich text tags currently in the message.
			var escaped = content.Replace("<", "<noparse><</noparse>");
			
			// Do a search to detect items in the sentence.
			var lowered = escaped.ToLower();
			var result = ItemUtils.ItemSearcher.Search(lowered, true);
			// Make sure it's valid, e.g. nothing in the middle of a word.
			result = result
				.Where(t => t.StartIndex <= 0 || AllowedPrefixes.Contains(lowered[t.StartIndex - 1]))
				.Where(t => {
					if (t.EndIndex >= lowered.Length) return true;
					if (AllowedPostfixes.Contains(lowered[t.EndIndex])) return true;
					// Allows 's' at the end, so long there is an allowed character
					// after that again.
					var prevChar = lowered[t.EndIndex - 1];
					var currChar = lowered[t.EndIndex];
					
					// If the last character ended with 's' then don't want to allow
					// another, we don't talk about 'es', shh.
					if (prevChar == 's') return false; // Can't have 'glasss'
					if (currChar != 's') return false; // Must have a 's' at the end.
					// Make sure the next character is an allowed one.
					if (t.EndIndex + 1 < lowered.Length && !AllowedPostfixes.Contains(lowered[t.EndIndex + 1])) 
						return false; // Didn't reach or invalid character after the 's'.
					t.MutableEndIndex += 1;
					return true;
				}).ToList();
			
			if (result.IsEmpty()) return;
			
			// Insert color into the escaped message.
			for (var i = result.Count - 1; i >= 0; i--) {
				var entry = result[i];
				
				var item = ItemUtils.TryGetItemFromLocalizedName(entry.Word);
				if (item == null) {
					IdleLog.Warn($"Failed to find marked item while parsing chat message: {entry.Word}");
					continue;
				}
				
				var pre = $"<color=#4dd8ff><link=\"ITEM:{item.ItemId}\">";
				const string post = "</link></color>";
				
				// Color the name
				escaped = escaped.Substring(0, entry.MutableEndIndex) + post + escaped.Substring(entry.MutableEndIndex);
				escaped = escaped.Substring(0, entry.MutableStartIndex) + pre + escaped.Substring(entry.MutableStartIndex);
				// Formatted name
				var startIndex = entry.StartIndex + pre.Length;
				var endIndex = entry.EndIndex + pre.Length;
				var formattedName = item.IdlePlus_GetLocalizedEnglishName();
				escaped = escaped.Substring(0, startIndex) + formattedName + escaped.Substring(endIndex);
			}
			
			// Update the message.
			var linkHoverable = __instance._text.transform.parent.With<ChatItemDisplayLink>();
			linkHoverable.Setup(__instance._text);
			
			message = message.Substring(0, message.Length - content.Length) + escaped;
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(ChatboxMessageEntry.Setup))]
		private static void PostfixSetup(ChatboxMessageEntry __instance, string message, GameMode gameMode) {
			if (gameMode == GameMode.NotSelected) return;
			if (!ModSettings.Features.EnhancedChat.Value) return;
			
			if (!PlayerUtils.IsPlayerMessageIndex(message, out var tagIndex, out var tag, out var nameIndex, 
				    out var name, out _, out _)) return;
			
			// Decorate both the name and clan tag if it exists.
			__instance._text.transform.parent.With<GeneralTextDecorationLink>().Setup(__instance._text, (id, data) => {
				switch (id) {
					case "onName":
						if (data.RightClickPressed) {
							GUIUtility.systemCopyBuffer = name;
							PopupUtils.ShowSoftPopup("Copied!", "Username copied to clipboard.", time: 1f);
							return;
						}

						if (!data.LeftClickPressed) return;
						var profilePopup = CustomPopupManager.Setup<PlayerProfilePopup>(PlayerProfilePopup.PopupKey);
						profilePopup.Setup(name);
						break;
					case "onTag":
						if (!data.LeftClickPressed) return;
						ClanApiManager.Instance.GetClanRecruitmentPage(tag).AcceptSync(page => { 
							var popup = PopupManager.Instance.SetupHardPopup<ClanRecruitmentResultPopup>();
							popup.Setup(page);
						}, exception => {
							IdleLog.Error("Failed to fetch clan recruitment page.", exception);
						});
						break;
				}
			});
			
			// Name
			var updatedName = $"<link=\"decorate:underline&color:e0e0e0&click:onName\">{name}</link>";
			message = message.Substring(0, nameIndex) + updatedName + message.Substring(nameIndex + name.Length);
			
			// Clan tag
			if (tag != null) {
				var updatedTag = $"<link=\"decorate:underline&color:e0e0e0&click:onTag\">{tag}</link>";
				message = message.Substring(0, tagIndex) + updatedTag + message.Substring(tagIndex + tag.Length);
			}
			
			__instance._text.text = message;
		}
	}
}