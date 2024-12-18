using IdlePlus.Settings.Types;

namespace IdlePlus.Settings {
	public class SettingCategory {

		public readonly string Title;
		public readonly Setting[] Settings;

		private SettingCategory(string title, Setting[] settings) {
			Title = title;
			Settings = settings;
		}
		
		public static SettingCategory Create(string title, params Setting[] settings) {
			return new SettingCategory(title, settings);
		}
	}
}