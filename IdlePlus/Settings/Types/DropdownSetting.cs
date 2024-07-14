using System;
using IdlePlus.Settings.Unity;
using IdlePlus.Unity;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using IdlePlus.Utilities.Helpers;
using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using Object = UnityEngine.Object;

namespace IdlePlus.Settings.Types {
	public class DropdownSetting : Setting {

		private static GameObject _prefab;
		public const string DropdownField = "Dropdown";
		
		/// <summary>
		/// The value of the setting.
		/// </summary>
		public int Value { get; private set; }

		/// <summary>
		/// The current state of the setting, shouldn't be used to get the value
		/// of the setting, instead the Value property should be used.
		/// </summary>
		public int State { get; private set; }
		
		public readonly int DefaultValue;
		public readonly string[] Options;
		
		public Action<int> OnValueChanged;

		private DropdownSetting(string id, bool requireRestart, string description, int defaultValue, string[] options) {
			Id = id;
			RequireRestart = requireRestart;
			Description = description;
			Dirty = false;
			Value = defaultValue;
			State = defaultValue;

			DefaultValue = defaultValue;
			Options = options;
		}
		
		public void Set(int value) {
			if (value < 0 || value >= Options.Length) value = 0;
			
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
			return new [] { (byte) State };
		}

		public override void Deserialize(byte[] data) {
			try {
				var state = data[0];
				if (state >= Options.Length) state = 0;
				State = state;
				Value = State;
				Dirty = false;
				IdleLog.Info($"Deserialized dropdown setting: {Id}, value: {Value}");
			} catch (Exception e) {
				IdleLog.Error($"Failed to deserialize dropdown setting: {Id}", e);
			}
		}

		#region Prefab
		
		public override void Initialize(GameObject obj) {
			var entry = obj.Use<DropdownSettingsEntry>();
			entry.Initialize();
		}
		
		public override GameObject GetPrefab() {
			if (_prefab == null) CreatePrefab();
			var obj = Object.Instantiate(_prefab);
			obj.With<DropdownSettingsEntry>().Setting = this;
			obj.name = Id;
			return obj;
		}

		private static void CreatePrefab() {
			var obj = GameObjects.NewRect("DropdownSettingPrefab", UiHelper.PrefabContainer);
			obj.With<RectTransform>().sizeDelta = Vec2.Vec(350, 100);
			obj.With<ProceduralImage>().color = new Color(0, 0, 0, 0.13f);
			obj.With<UniformModifier>().radius = 10;
			
			// Description
			var descObj = GameObjects.NewRect<TextMeshProUGUI>(DescriptionField, obj);
			descObj.transform.localPosition = Vec3.Vec(0, 14, 0);
			var descRect = descObj.Use<RectTransform>();
			descRect.sizeDelta = Vec2.Vec(325, 50);
			var descText = descObj.Use<TextMeshProUGUI>();
			descText.SetText("Description");
			descText.fontSizeMax = 18;
			descText.enableAutoSizing = true;
			descText.alignment = TextAlignmentOptions.Justified;

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
			
			// Dropdown
			var dropdownObj = UiHelper.CreateDropdown(DropdownField, obj.transform);
			dropdownObj.transform.localPosition = Vec3.Vec(0, -27.5f, 0);
			dropdownObj.With<IdlePlusDropdown>();
			var dropdownRect = dropdownObj.Use<RectTransform>();
			dropdownRect.sizeDelta = Vec2.Vec(115, 0);
			
			// Save the prefab for future use.
			_prefab = obj;
		}
		
		#endregion
		
		// Static creator methods.
		
		public static DropdownSetting Create(string id, string description, int defaultValue, 
			params string[] options) {
			return Create(id, false, description, defaultValue, options);
		}
		
		public static DropdownSetting Create(string id, bool requireRestart, string description, int defaultValue, 
			params string[] options) {
			return new DropdownSetting(id, requireRestart, description, defaultValue, options);
		}
	}
}