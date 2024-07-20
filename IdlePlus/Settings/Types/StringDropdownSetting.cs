using System;
using IdlePlus.Utilities;

namespace IdlePlus.Settings.Types {
	public class StringDropdownSetting : DropdownSetting {

		public new Action<int, string> OnValueChanged;
		public new Action<int, string> OnLoad;
		
		protected StringDropdownSetting(string id, bool requireRestart, string description, int defaultValue, string[] options) : 
			base(id, requireRestart, description, defaultValue, options) {
		}

		public override void Set(int value) {
			if (value < 0 || value >= Options.Length) value = 0;
			var strValue = Options[value];
			
			if (!RequireRestart) {
				State = value;
				Value = value;
				OnValueChanged?.Invoke(value, strValue);
				return;
			}
			
			State = value;
			Dirty = value != Value;
			OnValueChanged?.Invoke(value, strValue);
		}

		public override byte[] Serialize() {
			var value = Options[State];
			return System.Text.Encoding.UTF8.GetBytes(value);
		}

		public override void Deserialize(byte[] data) {
			try {
				var value = System.Text.Encoding.UTF8.GetString(data);
				var index = Array.IndexOf(Options, value);
				if (index == -1) index = 0;
				State = index;
				Value = index;
				Dirty = false;
				OnLoad?.Invoke(index, value);
			} catch (Exception e) {
				IdleLog.Error($"Failed to deserialize dropdown setting: {Id}", e);
			}
		}
		
		// Static creator methods.
		
		public new static StringDropdownSetting Create(string id, string description, int defaultValue, 
			params string[] options) {
			return Create(id, false, description, defaultValue, options);
		}
		
		public new static StringDropdownSetting Create(string id, bool requireRestart, string description, int defaultValue, 
			params string[] options) {
			return new StringDropdownSetting(id, requireRestart, description, defaultValue, options);
		}
	}
}