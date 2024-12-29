using IdlePlus.Attributes;
using IdlePlus.Settings.Types;
using IdlePlus.Unity;
using IdlePlus.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace IdlePlus.Settings.Unity {
	
	[RegisterIl2Cpp]
	public class DropdownSettingsEntry : MonoBehaviour {

		public DropdownSetting Setting;
		private bool _initialized;
		
		private TextMeshProUGUI _description;
		private IdlePlusDropdown _dropdown;
		private GameObject _restart;

		public void Initialize() {
			if (_initialized) return;
			_initialized = true;

			_description = gameObject.FindAndUse<TextMeshProUGUI>(Types.Setting.DescriptionField);
			_dropdown = gameObject.Find(DropdownSetting.DropdownField).Use<IdlePlusDropdown>();
			_restart = gameObject.Find(Types.Setting.RestartField);

			_description.SetText(Setting.Description);
			_restart.SetActive(Setting.Dirty);
			
			_dropdown.Items = Setting.Options;
			_dropdown.SelectedIndex = Setting.State;
			_dropdown.OnValueChanged.Listen(OnSelectedChanged);
		}
		
		public void Setup() {
			_dropdown.SelectedIndex = Setting.State;
			Refresh();
		}

		public void Refresh() {
			_restart.SetActive(Setting.Dirty);
		}
		
		private void OnSelectedChanged(int index) {
			Setting.Set(index);
			Refresh();
			ModSettings.Save();
		}
	}
}