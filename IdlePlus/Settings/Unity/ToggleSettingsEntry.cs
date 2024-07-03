using IdlePlus.Settings.Types;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using Lean.Gui;
using TMPro;
using UnityEngine;

namespace IdlePlus.Settings.Unity {
	
	[RegisterIl2Cpp]
	public class ToggleSettingsEntry : MonoBehaviour {
		
		public ToggleSetting Setting;
		private bool _initialized;

		private TextMeshProUGUI _description;
		private LeanToggle _toggle;
		private GameObject _restart;

		public void Initialize() {
			if (_initialized) return;
			_initialized = true;
			
			_description = gameObject.FindAndUse<TextMeshProUGUI>(Types.Setting.DescriptionField);
			_toggle = gameObject.Find(ToggleSetting.ToggleField).Use<LeanToggle>();
			_restart = gameObject.Find(Types.Setting.RestartField);

			_description.SetText(Setting.Description);
			_toggle.Set(Setting.State);
			_restart.SetActive(Setting.Dirty);
			
			_toggle.OnOn.Listen(OnOn);
			_toggle.OnOff.Listen(OnOff);
		}

		public void Setup() {
			_toggle.Set(Setting.State);
			Refresh();
		}

		public void Refresh() {
			_restart.SetActive(Setting.Dirty);
		}
		
		private void OnOn() {
			Setting.Set(true);
			Refresh();
			ModSettings.Save();
		}
        
		private void OnOff() {
			Setting.Set(false);
			Refresh();
			ModSettings.Save();
		}
	}
}