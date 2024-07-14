using System;
using IdlePlus.Settings.Unity;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using IdlePlus.Utilities.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using Object = UnityEngine.Object;

namespace IdlePlus.Settings.Types {
	public class ToggleSetting : Setting {

		private static GameObject _prefab;
		public const string ToggleField = "Toggle";
		
		/// <summary>
		/// The value of the setting.
		/// </summary>
		public bool Value { get; private set; }

		/// <summary>
		/// The current state of the setting, shouldn't be used to get the value
		/// of the setting, instead the Value property should be used.
		/// </summary>
		public bool State { get; private set; }
		
		public readonly bool DefaultState;

		public Action<bool> OnValueChanged;
		
		private ToggleSetting(string id, bool requireRestart, string description, bool defaultState) {
			Id = id;
			RequireRestart = requireRestart;
			Description = description;
			Dirty = false;
			Value = defaultState;
			State = defaultState;
			
			DefaultState = defaultState;
		}
		
		public void Set(bool value) {
			if (!RequireRestart) {
				State = value;
				Value = value;
				OnValueChanged?.Invoke(value);
				return;
			}
			
			State = value;
			Dirty = value != Value;
			OnValueChanged?.Invoke(value);
		}
		
		public override byte[] Serialize() {
			return new [] { (byte) (State ? 1 : 0) };
		}

		public override void Deserialize(byte[] data) {
			try {
				State = data[0] != 0;
				Value = State;
				Dirty = false;
				IdleLog.Info($"Deserialized toggle setting: {Id}, value: {Value}");
			} catch (Exception e) {
				IdleLog.Error($"Failed to deserialize dropdown setting: {Id}", e);
			}
		}

		#region Prefab

		public override void Initialize(GameObject obj) {
			var entry = obj.With<ToggleSettingsEntry>();
			entry.Initialize();
		}
		
		public override GameObject GetPrefab() {
			if (_prefab == null) CreatePrefab();
			var obj = Object.Instantiate(_prefab);
			obj.With<ToggleSettingsEntry>().Setting = this;
			obj.name = Id;
			return obj;
		}

		private static void CreatePrefab() {
			var obj = GameObjects.NewRect("ToggleSettingPrefab", UiHelper.PrefabContainer);
			obj.With<RectTransform>().sizeDelta = Vec2.Vec(350, 100);
			obj.With<ProceduralImage>().color = new Color(0, 0, 0, 0.13f);
			obj.With<UniformModifier>().radius = 10;
			
			// Toggle
			var toggleObj = UiHelper.CreateToggle(ToggleField, obj.transform);
			toggleObj.transform.localPosition = Vec3.Vec(-170, 0, 0);

			// Description
			var descObj = GameObjects.NewRect<TextMeshProUGUI>(DescriptionField, obj);
			descObj.transform.localPosition = Vec3.Vec(40, 0, 0);
			var descRect = descObj.Use<RectTransform>();
			descRect.sizeDelta = Vec2.Vec(260, 75);
			var descText = descObj.Use<TextMeshProUGUI>();
			descText.SetText("Description");
			descText.fontSizeMax = 18;
			descText.enableAutoSizing = true;
			descText.alignment = TextAlignmentOptions.Left;

			// Restart required text
			var restartObj = GameObjects.NewRect<TextMeshProUGUI>(RestartField, obj);
			restartObj.transform.localPosition = Vec3.Vec(0, 50, 0);
			var restartRect = restartObj.Use<RectTransform>();
			restartRect.pivot = new Vector2(0.5f, 1);
			restartRect.sizeDelta = Vec2.Vec(200, 18);
			var restartText = restartObj.Use<TextMeshProUGUI>();
			restartText.fontSize = 15;
			restartText.alignment = TextAlignmentOptions.Top;
			restartText.text = "Restart required";
			restartText.color = new Color(1, 0.3f, 0.3f, 1f);
			
			// Save the prefab for future use.
			_prefab = obj;
		}
		
		#endregion

		// Static creator methods.
		
		public static ToggleSetting Create(string id, string description, bool defaultValue) {
			return Create(id, false, description, defaultValue);
		}
		
		public static ToggleSetting Create(string id, bool requireRestart, string description,
			bool defaultValue) {
			return new ToggleSetting(id, requireRestart, description, defaultValue);
		}
	}
}