using Crosstales;
using IdlePlus.Attributes;
using IdlePlus.Utilities.Extensions;
using Lean.Gui;
using Localizations;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace IdlePlus.Utilities.Helpers {
	public static class UiHelper {

		public static GameObject PrefabContainer;
		private static GameObject _baseDropdown;
		private static GameObject _baseToggle;
		private static GameObject _baseInputField;
		
		[InitializeOnce(Priority = InitPriority.UiHelper, OnSceneLoad = "*")]
		private static void InitializeOnce() {
			PrefabContainer = GameObjects.New("IdlePlusPrefabContainer");
			PrefabContainer.SetActive(false);
			PrefabContainer.hideFlags = HideFlags.HideAndDontSave;
			Object.DontDestroyOnLoad(PrefabContainer);
			
			InitializeDropdown();
			InitializeToggle();
			InitializeInputField();
		}

		private static void InitializeDropdown() {
			var copyDropdown =
				GameObjects.FindByPathNonNull(
					"PopupManager/Canvas/HardPopups/SettingsPopup2/Panels/GeneralSection/LanguageDropdown (1)");
			
			_baseDropdown = Object.Instantiate(copyDropdown, PrefabContainer.transform, false);
			_baseDropdown.name = "IdlePlusDropdown";

			// An exception might be thrown by the line under.
			Object.DestroyImmediate(_baseDropdown.GetComponent<LocalizationDropdown>());
			
			var dropdown = _baseDropdown.GetComponent<CustomDropdown>();
			dropdown.items.Clear();
			dropdown.items.Add(new CustomDropdown.Item() { itemName = "ignore" });
			dropdown.onValueChanged = new CustomDropdown.DropdownEvent();
			dropdown.onItemTextChanged = new CustomDropdown.ItemTextChangedEvent();
			dropdown.onDropdownClicked = new UnityEvent();
			
			// Remove unused components and objects.
			Object.DestroyImmediate(_baseDropdown.Find("Trigger").GetComponent<EventTrigger>());
			var itemList = _baseDropdown.Find("Content/Item List/Scroll Area/List");
			foreach (var obj in itemList.transform.getAllChildren()) {
				Object.DestroyImmediate(obj.gameObject);
			}
		}

		private static void InitializeToggle() {
			var copyToggle = GameObjects.FindByPathNonNull(
				"PopupManager/Canvas/HardPopups/SettingsPopup2/Panels/GeneralSection/ScreenSleepToggle/Toggle");
			
			_baseToggle = Object.Instantiate(copyToggle, PrefabContainer.transform, false);
			_baseToggle.name = "IdlePlusToggle";

			var leanToggle = _baseToggle.GetComponent<LeanToggle>();
			leanToggle.onOff = new UnityEvent();
			leanToggle.onOn = new UnityEvent();
		}

		private static void InitializeInputField() {
			var copyInputField = GameObjects.FindByPathNonNull("PopupManager/Canvas/HardPopups/SettingsPopup2/Panels/BlocksSection/BlockPlayerInputField");
			if (copyInputField != null) {
				_baseInputField = Object.Instantiate(copyInputField, PrefabContainer.transform, false);
				_baseInputField.name = "IdlePlusInputField";
			} else {
				IdleLog.Info("InputField prefab not found by path!");
			}
		}



		public static GameObject CreateDropdown(string name, Transform parent = null) {
			var dropdown = Object.Instantiate(_baseDropdown);
			dropdown.name = name;

			var customDropdown = dropdown.GetComponent<CustomDropdown>();
			customDropdown.items.Clear();
			
			if (parent != null) dropdown.transform.SetParent(parent, false);
			return dropdown;
		}

		public static GameObject CreateToggle(string name, Transform parent = null) {
			var toggle = Object.Instantiate(_baseToggle);
			toggle.name = name;
			if (parent != null) toggle.transform.SetParent(parent, false);
			return toggle;
		}

		public static GameObject CreateInputField(string name, Transform parent = null) {
			if (_baseInputField == null) {
				InitializeInputField();
			}
			if (_baseInputField == null) {
				var fallback = new GameObject(name);
				fallback.AddComponent<RectTransform>();
				return fallback;
			}
			var inputField = Object.Instantiate(_baseInputField);
			inputField.name = name;
			if (parent != null) {
				inputField.transform.SetParent(parent, false);
			}
			return inputField;
		}
	}
}