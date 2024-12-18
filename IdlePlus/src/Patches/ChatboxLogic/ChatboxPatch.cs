using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChatboxLogic;
using Crosstales;
using HarmonyLib;
using IdlePlus.Command;
using IdlePlus.Settings;
using IdlePlus.TexturePack;
using IdlePlus.Unity.Chat;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using Il2CppSystem.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using Random = System.Random;

namespace IdlePlus.Patches.ChatboxLogic {
	
	[HarmonyPatch(typeof(Chatbox))]
	public class ChatboxPatch {
		
		private static readonly Random Random = new Random();

		private static Chatbox _chatbox;
		private static GameObject _inputFieldObject;
		
		private static ChatSuggestionBox _suggestionBox;
		private static ChatUsageBox _usageBox;
		
		private static string _lastMessage = "";
		private static int _lastCursorPos = -1;
		
		private static CommandSuggestResult _commandSuggestResult;
		private static CancellationTokenSource _commandSuggestCancellationToken;

		[InitializeOnce]
		private static void Initialize() {
			if (!ModSettings.UI.EnhancedChatCommands.Value) return;
			_chatbox = GameObjects.FindByCachedPath("PopupManager/Canvas/HardPopups/ChatboxPopup/Chatbox").Use<Chatbox>();
			_inputFieldObject = _chatbox._inputField.gameObject;
			
			// Disable auto-sizing for the input field.
			var inputField = _inputFieldObject.Use<TMP_InputField>();
			var inputComponent = inputField.textComponent;
			inputComponent.enableAutoSizing = false;
			
			// Suggestion box.
			var suggestionObject = GameObjects.NewRect<ChatSuggestionBox>("SuggestionContainer", _inputFieldObject);
			_suggestionBox = suggestionObject.Use<ChatSuggestionBox>();
			_suggestionBox.Initialize();
			
			// Usage box.
			var usageObject = GameObjects.NewRect<ChatUsageBox>("UsageContainer", _inputFieldObject);
			_usageBox = usageObject.Use<ChatUsageBox>();
			_usageBox.Initialize();
			
			// "Update" loop.
			IdleTasks.Update(_inputFieldObject, () => {
				if (_commandSuggestCancellationToken == null) return;
				if (_commandSuggestCancellationToken.IsCancellationRequested) {
					_commandSuggestCancellationToken = null;
					_commandSuggestResult = null;
				}
				
				var result = _commandSuggestResult;
				if (result == null) return; // Wait until we get a result.
				_commandSuggestResult = null;
				_commandSuggestCancellationToken = null;
				
				// Make sure it's the command we're currently trying to suggest.
				if (inputField.text != result.RequestedCommand) return;
				
				// If we didn't get any suggestions or usage, then disable the boxes.
				if (result.Usage.IsEmpty() && result.Suggestions.IsEmpty()) {
					_suggestionBox.SetEnabled(false);
					_usageBox.SetEnabled(false);
					return;
				}

				//IdleLog.Info("Usage:");
				//result.Usage.ForEach(u => IdleLog.Info($"- {u}"));
				//IdleLog.Info("Suggestions:");
				//result.Suggestions.List.ForEach(s => IdleLog.Info($"- {s}"));
				//IdleLog.Info("");
				//IdleLog.Info("Result:");
				
				//_suggestionBox.SetEnabled(!result.Suggestions.IsEmpty());
				
				if (!result.Suggestions.IsEmpty()) {
					//IdleLog.Info(result.Suggestions.List.Join());
					_usageBox.SetEnabled(false);
					_suggestionBox.Setup(result.Suggestions.Range.Start, result.Suggestions.List);
				}
				else if (!result.Usage.IsEmpty()) {
					//IdleLog.Info(result.Usage.First());
					_suggestionBox.SetEnabled(false);
					_usageBox.Setup(result.UsageStartIndex, result.Usage.First());
				}
			});
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(Chatbox.Update))]
		private static void PrefixUpdate(Chatbox __instance) {
			var inputField = __instance._inputField;
			if (inputField.caretPosition == _lastCursorPos && inputField.text == _lastMessage) return;
			_lastCursorPos = inputField.caretPosition;
			_lastMessage = __instance._inputField.text;
			
			if (!_lastMessage.StartsWith("/")) {
				_suggestionBox.SetEnabled(false);
				_usageBox.SetEnabled(false);
				return;
			}

			try {
				_suggestionBox.SetupGhost();
			} catch (Exception e) {
				IdleLog.Error("Failed to setup ghost suggestion box.", e);
			}

			_commandSuggestCancellationToken?.Cancel();
			var token = new CancellationTokenSource();
			_commandSuggestCancellationToken = token;

			Task.Run((System.Action) delegate {
				var resultTask = CommandManager.HandleSuggestion(_lastMessage, _lastCursorPos, token.Token);
				resultTask.Wait(token.Token);
				var result = resultTask.Result;
				if (token.IsCancellationRequested) return;
				_commandSuggestResult = result;
			});
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(Chatbox.SendMessageToServer))]
		private static bool PrefixSendMessageToServer(Chatbox __instance) {
			var message = __instance._inputField.text;
			var result = CommandManager.Handle(message);

			if (result == null) return true;
			if (!result.Success) {
				if (result.Response != null) IdleLog.Error(result.Response);
				return false;
			}
			
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