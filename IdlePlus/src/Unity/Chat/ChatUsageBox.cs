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

		private bool _initialized;

		private TMP_InputField _inputField;
		private TMP_Text _inputText;
		private TextMeshProUGUI _text;

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
		
		internal void Setup(int index, string usage) {
			var text = this._inputText.text;
			if (index >= text.Length) return;
			
			var position = this._inputText.transform.TransformPoint(this._inputText.textInfo.characterInfo[index].topLeft);
			var startPosition = this._inputText.transform.TransformPoint(this._inputText.textInfo.characterInfo[0].topLeft);
			position.y = startPosition.y;
			this.transform.localPosition = this._inputField.transform.InverseTransformPoint(position);
			this._text.text = usage;
			
			this.SetEnabled(true);
		}
		
		internal void SetEnabled(bool state) {
			this.gameObject.SetActive(state);
		}
	}
}