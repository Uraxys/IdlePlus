using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChatboxLogic;
using HarmonyLib;
using IdlePlus.API.Event.Contexts;
using IdlePlus.API.Utility.Game;
using IdlePlus.Unity.Chat;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;

namespace IdlePlus.Patches.ChatboxLogic {
	
	[HarmonyPatch(typeof(ChatboxMessageEntry))]
	internal class ChatboxMessageEntryPatch {
		
		private static readonly HashSet<char> AllowedPrefixes = new HashSet<char> 
			{ ' ', '(', '[', '{', '/' };
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
			
			// By default, players shouldn't be able to use rich tags.
			__instance._text.richText = true; // TODO: Should be false.
			
			// Make sure this is a valid player message, and get the content.
			if (!ChatMessageEventContext.IsPlayerMessage(message, out var tag, out var name, 
				    out var content)) return;
			
			IdleLog.Info("Searching for items in message.");
			
			var watch = new Stopwatch();
			watch.Start();

			// Do search to detect the words.
			var escaped = content.Replace("<", "<noparse><</noparse>");
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
			
			watch.Stop();
			IdleLog.Info($"AhoCorasick search took {watch.ElapsedMilliseconds}ms");
			IdleLog.Info("Result:");
			result.ForEach(s => IdleLog.Info($"- {s.StartIndex} / {s.Word} / {lowered.Substring(s.StartIndex, s.MutableLength)}"));
			
			var markerDebug = "";
			foreach (var entry in result) {
				if (entry.MutableStartIndex > markerDebug.Length) markerDebug += new string('.', entry.StartIndex - markerDebug.Length);
				markerDebug += new string('^', entry.MutableLength);
			}
			IdleLog.Info(lowered);
			IdleLog.Info(markerDebug);
			
			// Insert color into the escaped message.

			for (var i = result.Count - 1; i >= 0; i--) {
				var entry = result[i];
				
				var item = ItemUtils.TryGetItemFromLocalizedName(entry.Word);
				if (item == null) {
					IdleLog.Warn($"Failed to find marked item while parsing chat message: {entry.Word}");
					continue;
				}
				
				var pre = $"<color=#4dd8ff><link=\"ITEM:{item.ItemId}\">";
				//var pre = $"<color=#4dd8ff><b><link=\"ITEM:{item.ItemId}\">";
				var post = "</link></color>";
				//var post = "</link></b></color>";
				
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
			var linkHoverable = __instance._text.transform.parent.With<ChatItemLinkHoverable>();
			linkHoverable.Setup(__instance._text);
			message = message.Substring(0, message.Length - content.Length) + escaped;
		}
		
	}
}