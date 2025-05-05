using System;
using System.Reflection;
using System.Text.RegularExpressions;
using IdlePlus.Attributes;
using IdlePlus.Settings.Types;
using IdlePlus.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdlePlus.Utilities;

namespace IdlePlus.Settings.Unity {
	[RegisterIl2Cpp]
	public class StringSettingEntry : MonoBehaviour {
		public StringSetting Setting;
		private bool _initialized;
		private TMP_InputField _inputField;
		private TextMeshProUGUI _errorText;

		public void Initialize() {
			if (_initialized) return;
			_initialized = true;

			_inputField = gameObject.Find("InputField")?.GetComponent<TMP_InputField>();
			if (_inputField == null) {
				IdleLog.Info("StringSettingEntry: InputField not found.");
				return;
			}

			_errorText = gameObject.Find("ErrorText")?.GetComponent<TextMeshProUGUI>();

			// Remove restrictions and set character limit.
			_inputField.characterValidation = TMP_InputField.CharacterValidation.None;
			_inputField.characterLimit = 200;
			_inputField.text = Setting.Value;

			_inputField.onEndEdit.AddListener((UnityEngine.Events.UnityAction<string>)((string s) => OnInputFieldChanged(s)));

			// Set placeholder text after a short delay.
			Invoke(nameof(SetPlaceholder), 0.5f);
		}

		public void Setup() {
			if (_inputField != null)
				_inputField.text = Setting.Value;
		}

		private void SetPlaceholder() {
			if (_inputField != null && _inputField.placeholder != null) {
				var tmp = _inputField.placeholder.GetComponent<TextMeshProUGUI>();
				if (tmp != null) {
					tmp.text = Setting.Description;
				}
			}
		}

		private void OnInputFieldChanged(string newValue) {
			if (_inputField != null) {
				// If input is empty, accept it without validation.
				if (string.IsNullOrEmpty(newValue)) {
					Setting.Set(newValue);
					if (_errorText != null)
						_errorText.text = "";
					ModSettings.Save();
					return;
				}

				// If a regex pattern is provided, validate the input.
				if (!string.IsNullOrEmpty(Setting.RegexPattern)) {
					if (!Regex.IsMatch(newValue, Setting.RegexPattern, RegexOptions.IgnoreCase)) {
						// If validation fails, display the provided error message (or a default one) and revert.
						if (_errorText != null) {
							_errorText.text = !string.IsNullOrEmpty(Setting.ErrorMessage)
								? Setting.ErrorMessage
								: "Invalid input format.";
						}
						_inputField.text = Setting.Value;
						return;
					} else {
						if (_errorText != null)
							_errorText.text = "";
					}
				}

				Setting.Set(newValue);
				ModSettings.Save();
			}
		}

		public void SetSetting(StringSetting setting) {
			Setting = setting;
			Setup();
		}
	}
}
