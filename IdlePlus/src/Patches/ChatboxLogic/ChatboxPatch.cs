using System;
using System.Linq;
using System.Threading;
using ChatboxLogic;
using HarmonyLib;
using IdlePlus.API.Utility;
using IdlePlus.Attributes;
using IdlePlus.Command;
using IdlePlus.Settings;
using IdlePlus.TexturePack;
using IdlePlus.Unity.Chat;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Il2CppSystem.Threading.Tasks;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace IdlePlus.Patches.ChatboxLogic {
	
	[HarmonyPatch(typeof(Chatbox))]
	public class ChatboxPatch {
		
		private static Chatbox _chatbox;
		private static GameObject _inputFieldObject;
		
		private static ChatSuggestionBox _suggestionBox;
		private static ChatUsageBox _usageBox;
		
		private static string _lastMessage = "";
		private static int _lastCursorPos = -1;
		
		private static CommandSuggestResult _commandSuggestResult;
		private static CancellationTokenSource _commandSuggestCancellationToken;

		[InitializeOnce(OnSceneLoad = Scenes.MainMenu)]
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
				
				if (!result.Suggestions.IsEmpty()) {
					_usageBox.SetEnabled(false);
					_suggestionBox.Setup(result.Suggestions.Range.Start, result.Suggestions.List);
				} else if (!result.Usage.IsEmpty()) {
					_suggestionBox.SetEnabled(false);
					_usageBox.Setup(result.UsageStartIndex, result.Usage.First());
				}
			});
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(Chatbox.Update))]
		private static void PrefixUpdate(Chatbox __instance) {
			if (!ModSettings.UI.EnhancedChatCommands.Value) return;
			
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
				var sender = new CommandSender();
				var resultTask = CommandManager.HandleSuggestion(sender, _lastMessage, _lastCursorPos, token.Token);
				resultTask.Wait(token.Token);
				var result = resultTask.Result;
				if (token.IsCancellationRequested) return;
				_commandSuggestResult = result;
			});
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(Chatbox.SendMessageToServer))]
		private static bool PrefixSendMessageToServer(Chatbox __instance) {
			if (!ModSettings.UI.EnhancedChatCommands.Value) return true;
			
			var message = __instance._inputField.text;
			if (message.Length <= 0 || message[0] != '/') return true;

			var sender = new CommandSender();
			var result = CommandManager.Handle(sender, message);

			if (result == null) return true;
			if (!result.Success) {
				if (result.Response != null) {
					IdleLog.Error(result.Response);
					_usageBox.SetupError(result.UnknownCommand ? 1 : 0, result.Response);
				}
				__instance._inputField.ActivateInputField();
				__instance._inputField.caretPosition = __instance._inputField.text.Length;
				return false;
			}
			
			__instance._inputField.text = "";
			__instance._inputField.ActivateInputField();
			__instance._inputField.caretPosition = __instance._inputField.text.Length;
			return false;
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(Chatbox.InsertMessageIntoChatbox))]
		private static void PostfixInsertMessageIntoChatbox(Chatbox __instance, Transform parent, GameMode gameMode, bool isPremium,
			bool isPremiumPlus) {
			if (TexturePackManager.CurrentPack == null) return;
			
			var chatObject = parent.GetChild(parent.GetChildCount() - 1);
			var chatEntry = chatObject.Use<ChatboxMessageEntry>();
			
			var pack = TexturePackManager.CurrentPack;
			var ironmanIcon = chatEntry._ironmanIconGO;
			var premiumIcon = chatEntry._premiumGO;
			var gildedIcon = chatEntry._premiumPlusGO;

			if (gameMode == GameMode.Ironman) 
				pack.TryApplyMiscSprite("ironman_icon", ironmanIcon);
			else if (gameMode == GameMode.GroupIronman)
				pack.TryApplyMiscSprite("group_ironman_icon", ironmanIcon);

			if (isPremium) pack.TryApplyMiscSprite("premium_icon", premiumIcon);
			if (isPremiumPlus) pack.TryApplyMiscSprite("gilded_icon", gildedIcon);
		}
	}
}