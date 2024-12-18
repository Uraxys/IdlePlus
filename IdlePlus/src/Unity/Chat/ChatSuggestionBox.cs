using System.Collections.Generic;
using Brigadier.NET.Suggestion;
using HarmonyLib;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.Unity.Chat {

	[HarmonyPatch(typeof(TMP_InputField))]
	internal class ChatSuggestionBox_ChatInputFieldPatch {
		
		internal static TMP_InputField TargetInputField;
		internal static bool DisableArrowKeys = false;
		
		[HarmonyPrefix]
		[HarmonyPatch(nameof(TMP_InputField.KeyPressed))]
		private static bool PrefixKeyPressed(TMP_InputField __instance, ref Event evt) {
			if (__instance != TargetInputField) return true;
			if (!DisableArrowKeys) return true;
			return evt.keyCode != KeyCode.UpArrow && evt.keyCode != KeyCode.DownArrow;
		}
		
	}
	
	[RegisterIl2Cpp]
	internal class ChatSuggestionBox : MonoBehaviour {

		private const int MaxVisibleSuggestions = 10;
		private const float ScrollContinuousDelay = 0.3f;
		private const float ScrollDelay = 0.05f;

		private bool _disableOnInactive = true;
		private bool _initialized;
		
		private List<Suggestion> _suggestions = new List<Suggestion>();
		private readonly List<ChatSuggestionEntry> _entries = new List<ChatSuggestionEntry>();
		private int _selected = -1;
		private int _lastSelected = -1;
		private int _scroll = 0;
		
		private TMP_InputField _inputField;
		private TMP_Text _inputText;
		private TextMeshProUGUI _ghostText;

		private float _lastScrollStartTime = 0f;
		private float _lastScrollTime = 0f;

		/*
		 * Unity
		 */

		public void OnDisable() {
			ChatSuggestionBox_ChatInputFieldPatch.DisableArrowKeys = false;
			if (this._ghostText == null) return;
			this._ghostText.gameObject.SetActive(false);
		}

		public void Update() {
			if (!this.IsInputFocused()) {
				if (!this._disableOnInactive) return;
				this.SetEnabled(false);
				return;
			}
			
			// Check for key presses.
			// - Tab to use the current selection.
			if (Input.GetKeyDown(KeyCode.Tab)) {
				if (this._selected < 0) return;
				if (this._selected >= this._suggestions.Count) return;
				
				var text = this._inputField.text;
				var suggestion = this._suggestions[this._selected];
				var range = suggestion.Range;
				
				if (text.Length < range.Start) return;
				if (text.Length < range.End) return;
				
				text = text.Remove(range.Start, range.Length).Insert(range.Start, suggestion.Text);
				this._inputField.text = text;
				this._inputField.caretPosition = range.Start + suggestion.Text.Length;
				return;
			}
			// - Arrow keys to cycle through suggestions.
			//   - Up
			if (Input.GetKeyDown(KeyCode.UpArrow)) {
				this._lastScrollStartTime = Time.time;
				this._lastScrollTime = Time.time;
				this.Scroll(-1);
				return;
			}
			if (Input.GetKey(KeyCode.UpArrow)) {
				if (Time.time - this._lastScrollStartTime < ScrollContinuousDelay) return;
				if (Time.time - this._lastScrollTime < ScrollDelay) return;
				this._lastScrollTime = Time.time;
				this.Scroll(-1);
				return;
			}
			//   - Down
			if (Input.GetKeyDown(KeyCode.DownArrow)) {
				this._lastScrollStartTime = Time.time;
				this._lastScrollTime = Time.time;
				this.Scroll(1);
				return;
			}
			if (Input.GetKey(KeyCode.DownArrow)) {
				if (Time.time - this._lastScrollStartTime < ScrollContinuousDelay) return;
				if (Time.time - this._lastScrollTime < ScrollDelay) return;
				this._lastScrollTime = Time.time;
				this.Scroll(1);
				return;
			}
		}

		/*
		 * Internal
		 */
		
		internal void Initialize() {
			if (this._initialized) return;
			this._initialized = true;

			// Get the input field.
			this._inputField = this.transform.parent.Use<TMP_InputField>();
			this._inputText = this._inputField.textComponent;
			ChatSuggestionBox_ChatInputFieldPatch.TargetInputField = this._inputField;
			
			// Create the suggestion box.
			var uniform = this.gameObject.With<UniformModifier>();
			var image = this.gameObject.With<ProceduralImage>();
			uniform.Radius = 0;
			image.color = new Color(0, 0, 0, 0.75f);

			var rect = this.gameObject.Use<RectTransform>();
			rect.SetAnchors(0, 0, 0, 0);
			rect.pivot = Vec2.Vec(0);

			var layout = this.gameObject.With<VerticalLayoutGroup>();
			layout.SetPadding(1, 1, 0, 0);
			
			var fitter = this.gameObject.With<ContentSizeFitter>();
			fitter.SetFit(ContentSizeFitter.FitMode.PreferredSize);
			
			// Create the entries.
			for (var i = 0; i < MaxVisibleSuggestions; i++) {
				var entry = GameObjects.NewRect<ChatSuggestionEntry>("Entry" + i, this.gameObject);
				var entryComponent = entry.Use<ChatSuggestionEntry>();
				entryComponent.Initialize();
				this._entries.Add(entryComponent);
			}
			
			// Create the ghost text.
			var ghostTextObject = GameObjects.Instantiate(this._inputText.transform.parent.GetChild(1).gameObject, 
				this._inputText.transform.parent.gameObject, true, "GhostText");
			this._ghostText = ghostTextObject.Use<TextMeshProUGUI>();
			this._ghostText.text = "";
			this._ghostText.richText = true;
			this._ghostText.color = new Color(1, 1, 1, 0.5f);
			this._ghostText.fontStyle = FontStyles.Normal;
		}
		
		internal void SetEnabled(bool state) {
			this.gameObject.SetActive(state);
		}
		
		private void Scroll(int direction) {
			var select = this._selected + direction;
			if (select >= this._suggestions.Count) select = 0;
			if (select < 0) select = this._suggestions.Count - 1;
			this.SetSelected(select);
			this.UpdateRender();
		}
		
		private void SetSelected(int index) {
			if (index < 0 || index >= this._suggestions.Count) {
				this._selected = -1;
				this._lastSelected = -1;
				return;
			}
			
			this._lastSelected = this._selected;
			this._selected = index;
		}
		
		internal void Setup(int index, List<Suggestion> entries) {
			this._suggestions = entries;
			this._selected = 0;
			this._scroll = 0;
			
			// Move the suggestion box to the correct position.
			if (index < 0 || index >= this._inputText.textInfo.characterCount) {
				IdleLog.Error($"Tried to setup suggestions with invalid index: {index}");
				this.SetEnabled(false);
				return;
			}

			var position = this._inputText.transform.TransformPoint(this._inputText.textInfo.characterInfo[index].topLeft);
			var startPosition = this._inputText.transform.TransformPoint(this._inputText.textInfo.characterInfo[0].topLeft);
			position.y = startPosition.y;
			this.transform.localPosition = this._inputField.transform.InverseTransformPoint(position);
			
			// Set up the entries.
			for (var i = 0; i < MaxVisibleSuggestions; i++) {
				var entry = this._entries[i];
				if (this._suggestions.Count <= i) {
					entry.SetEnabled(false);
					continue;
				}
				
				var suggestion = this._suggestions[i];
				entry.Setup(suggestion, i == this._selected);
				entry.SetEnabled(true);
			}
			
			// Done - enable the suggestion box.
			ChatSuggestionBox_ChatInputFieldPatch.DisableArrowKeys = true;
			this.SetEnabled(true);
			this.SetupGhost();
		}

		private void UpdateRender() {
			var updatedScroll = false;
			
			// Check if we need to update the scroll.
			if (this._selected > (this._scroll + MaxVisibleSuggestions - 1)) {
				this._scroll = this._selected - MaxVisibleSuggestions + 1;
				updatedScroll = true;
			}
			if (this._selected < this._scroll && this._scroll > 0) {
				this._scroll = this._selected;
				updatedScroll = true;
			}
			
			// If we aren't updating the scroll, check if we need to update the selected entry.
			if (!updatedScroll && this._selected != this._lastSelected) {
				var oldEntry = this._entries[this._lastSelected - this._scroll];
				if (oldEntry != null) oldEntry.Select(false);
				var newEntry = this._entries[this._selected - this._scroll];
				if (newEntry != null) newEntry.Select(true);
				this.SetupGhost();
				return;
			}
			
			// Update the entries if needed.
			if (!updatedScroll) return;
			for (var i = 0; i < MaxVisibleSuggestions; i++) {
				var entry = this._entries[i];
				var suggestionIndex = i + this._scroll;
				if (suggestionIndex >= this._suggestions.Count) {
					entry.SetEnabled(false);
					continue;
				}
				
				var suggestion = this._suggestions[suggestionIndex];
				entry.Setup(suggestion, suggestionIndex == this._selected);
				entry.SetEnabled(true);
			}
			this.SetupGhost();
		}
		
		// - Ghost Text
		
		internal void SetupGhost() {
			if (!this.IsInputFocused() || !this.gameObject.IsActive()) return;
			if (this._selected < 0) return;
			
			var text = this._inputField.text;
			var suggestion = this._suggestions[this._selected];
			var range = suggestion.Range;
			
			if (range.Start > text.Length) {
				this._ghostText.gameObject.SetActive(false);
				return;
			}

			var match = text.Substring(range.Start, text.Length - range.Start);
			
			// If the suggestion doesn't start with the match, then disable
			// the ghost text, as it isn't needed anymore.
			if (!suggestion.Text.StartsWith(match)) {
				this._ghostText.gameObject.SetActive(false);
				return;
			}

			var hiddenText = text.Substring(0, range.Start + match.Length);
			var suggestedText = suggestion.Text.Substring(match.Length);
			
			// If there isn't any suggested text to display, then disable it,
			// as it isn't needed when the suggestion is fully typed.
			if (suggestedText.Length == 0) {
				this._ghostText.gameObject.SetActive(false);
				return;
			}
			
			// Create the result and display it.
			var result = $"<color=#0000><noparse>{hiddenText}</noparse></color><noparse>{suggestedText}</noparse>";
			this._ghostText.text = result;
			this._ghostText.gameObject.SetActive(true);
		}
		
		// Helpers

		private bool IsInputFocused() {
			return this._inputField.isFocused;
		}
	}

	[RegisterIl2Cpp]
	internal class ChatSuggestionEntry : MonoBehaviour {
		
		private static readonly Color SelectedColor = new Color(1f, 1f, 0f);
		private static readonly Color DefaultColor = new Color(1f, 1f, 1f);
		
		private bool _initialized;
		private TextMeshProUGUI _text;

		private Suggestion _suggestion;
		private bool _selected;

		internal void Initialize() {
			if (this._initialized) return;
			this._initialized = true;
			
			this._text = this.gameObject.With<TextMeshProUGUI>();
			this._text.text = "Test";
			
			this.SetEnabled(false);
		}
		
		internal void Setup(Suggestion suggestion, bool selected = false) {
			this._suggestion = suggestion;
			this._selected = selected;
			
			this._text.color = selected ? SelectedColor : DefaultColor;
			this._text.text = this.GetSuggestion();
		}
		
		internal void Select(bool state) {
			if (!this.gameObject.IsActive()) return;
			if (this._selected == state) return;
			this._selected = state;
			this._text.color = state ? SelectedColor : DefaultColor;
		}
		
		internal void SetEnabled(bool state) {
			this.gameObject.SetActive(state);
		}
		
		// Helpers
		
		private string GetSuggestion() {
			return this._suggestion.Text;
		}
	}
}