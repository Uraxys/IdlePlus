using IdlePlus.Attributes;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.Unity.Chat {
	
	[RegisterIl2Cpp]
	public class ChatUsageBox : MonoBehaviour {
		
		private static readonly Color ErrorColor1 = new Color(255f / 255f, 87f / 255f, 74f / 255f);
		private static readonly Color ErrorColor2 = new Color(255f / 255f, 118f / 255f, 107f / 255f);

		private bool _initialized;

		private TMP_InputField _inputField;
		private TMP_Text _inputText;
		private TextMeshProUGUI _text;

		private bool _error;
		private float _errorTime;
		private bool _errorState;
		
		// If the usage box should be disabled next frame.
		private bool _disableInput;

		public void Update() {
			if (!this.IsInputFocused()) {
				if (this._disableInput) return;
				IdleTasks.Run(() => {
					if (this.IsInputFocused()) return;
					this.SetEnabled(false);
				});
				return;
			}

			if (this._error) {
				var timeSince = Time.time - this._errorTime;
				if (timeSince > 2) {
					this._text.color = Color.white;
					return;
				}
				
				var timeIndex = (int)(timeSince / 0.5f) % 2 == 0;
				if (timeIndex != this._errorState) {
					this._errorState = timeIndex;
					this._text.color = this._errorState ? ErrorColor1 : ErrorColor2;
				}
			}

			// The input box is enabled, don't disable it.
			this._disableInput = false;
		}

		internal void Initialize() {
			if (this._initialized) return;
			this._initialized = true;
			
			// Get the input field.
			this._inputField = this.transform.parent.Use<TMP_InputField>();
			this._inputText = this._inputField.textComponent;
			
			var rect = this.gameObject.Use<RectTransform>();
			rect.SetAnchors(0, 0, 0, 0);
			rect.pivot = Vec2.Vec(0, 0);
			
			var layout = this.gameObject.With<HorizontalLayoutGroup>();
			layout.SetPadding(1, 1, 0, 0);
			
			this._text = GameObjects.NewRect<TextMeshProUGUI>("Text", this.gameObject).Use<TextMeshProUGUI>();
			this.gameObject.With<ContentSizeFitter>().SetFit(ContentSizeFitter.FitMode.PreferredSize);
			var uniform = this.gameObject.With<UniformModifier>();
			var image = this.gameObject.With<ProceduralImage>();
			uniform.Radius = 0;
			image.color = new Color(0, 0, 0, 0.75f);
		}

		internal void SetupError(int index, string usage) {
			this.Setup(index, usage);
			this._error = true;
			this._errorTime = Time.time;
			this._text.color = ErrorColor1;
		}
		
		internal void Setup(int index, string usage) {
			var text = this._inputText.text;
			if (index >= text.Length) return;

			// Reset error message.
			this._error = false;
			this._errorTime = -1;
			
			var position = this._inputText.transform.TransformPoint(this._inputText.textInfo.characterInfo[index].topLeft);
			var startPosition = this._inputText.transform.TransformPoint(this._inputText.textInfo.characterInfo[0].topLeft);
			position.y = startPosition.y;
			this.transform.localPosition = this._inputField.transform.InverseTransformPoint(position);
			this._text.text = usage;
			this._text.color = Color.white;
			
			this.SetEnabled(true);
		}
		
		// Helpers

		private bool IsInputFocused() {
			return this._inputField.isFocused;
		}
		
		internal void SetEnabled(bool state) {
			this.gameObject.SetActive(state);
		}
	}
}