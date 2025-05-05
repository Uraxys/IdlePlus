using System;
using System.Text;
using IdlePlus.Settings.Unity;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using IdlePlus.Utilities.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using Object = UnityEngine.Object;

namespace IdlePlus.Settings.Types {
	public class StringSetting : Setting {
		private static GameObject _prefab;
		public const string InputFieldName = "InputField";
		public const string ErrorTextName = "ErrorText";

		public string Value { get; private set; }

		public string RegexPattern { get; private set; }

		public string ErrorMessage { get; private set; }

		public StringSetting(string id, string description, string defaultValue, string regexPattern = null, string errorMessage = null) {
			Id = id;
			Description = description;
			Value = defaultValue;
			RegexPattern = regexPattern;
			ErrorMessage = errorMessage;
		}

		public void Set(string newValue) {
			Value = newValue;
		}

		#region Serialization

		public override byte[] Serialize() {
			return Encoding.UTF8.GetBytes(Value);
		}

		public override void Deserialize(byte[] data) {
			try {
				Value = Encoding.UTF8.GetString(data);
				IdleLog.Info($"Deserialized string setting: {Id}, value: {Value}");
			} catch (Exception e) {
				IdleLog.Error($"Failed to deserialize string setting: {Id}", e);
			}
		}

		#endregion

		#region Prefab

		public override void Initialize(GameObject obj) {
			var entry = obj.With<StringSettingEntry>();
			entry.Initialize();
		}

		public override GameObject GetPrefab() {
			if (_prefab == null)
				CreatePrefab();

			var obj = Object.Instantiate(_prefab);
			obj.With<StringSettingEntry>().Setting = this;
			obj.name = Id;
			return obj;
		}

		private static void CreatePrefab() {
			// Create a container rect object.
			var obj = GameObjects.NewRect("StringSettingPrefab", UiHelper.PrefabContainer);
			obj.With<RectTransform>().sizeDelta = Vec2.Vec(350, 100);
			obj.With<ProceduralImage>().color = new Color(0, 0, 0, 0.13f);
			obj.With<UniformModifier>().radius = 10;

			// Create an InputField for text input.
			var inputObj = UiHelper.CreateInputField(InputFieldName, obj.transform);
			inputObj.name = InputFieldName;
			inputObj.transform.localPosition = Vec3.Vec(160, 0, 0);
			var inputRect = inputObj.GetComponent<RectTransform>();
			inputRect.sizeDelta = new Vector2(200, 0);

			// Create an error text element for validation messages.
			var errorObj = GameObjects.NewRect<TextMeshProUGUI>(ErrorTextName, obj);
			errorObj.name = ErrorTextName;
			errorObj.transform.localPosition = Vec3.Vec(0, -30, 0);
			var errorRect = errorObj.Use<RectTransform>();
			errorRect.sizeDelta = Vec2.Vec(200, 20);
			var errorText = errorObj.Use<TextMeshProUGUI>();
			errorText.text = ""; // start empty
			errorText.fontSize = 14;
			errorText.color = Color.red;

			_prefab = obj;
		}

		#endregion

		public static StringSetting Create(string id, string description, string defaultValue, string regexPattern = null, string errorMessage = null) {
			return new StringSetting(id, description, defaultValue, regexPattern, errorMessage);
		}
	}
}
