using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdlePlus.Settings.Types;
using Path = System.IO.Path;

namespace IdlePlus.Settings {
	
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public static class ModSettings {
		
		public static readonly SettingCategory MarketValueCategory = SettingCategory.Create("Item Value",
			MarketValue.Enabled, MarketValue.HideForIronman, MarketValue.ShiftForTotal, 
			MarketValue.DisplayType);
		
		public static readonly SettingCategory[] Categories = { MarketValueCategory };
		
		public static class MarketValue {
			public static readonly ToggleSetting Enabled = ToggleSetting.Create(
				"marketvalue",
				"Display the market value in the item tooltip.",
				true
			);

			public static readonly ToggleSetting HideForIronman = ToggleSetting.Create(
				"marketvalue_hideforim",
				"Hide the market value for ironman accounts.",
				true
			);
			
			public static readonly ToggleSetting ShiftForTotal = ToggleSetting.Create(
				"marketvalue_shifttotal",
				"Hold shift to display the total value.",
				true
			);
		
			public static readonly DropdownSetting DisplayType = DropdownSetting.Create(
				"marketvalue_display",
				"Which value to display as the market value.",
				0,
				"Sell then buy", "Buy then sell", "Sell only", "Buy only", "Average"
			);
		}
		
		public static async void Load() {
			await Task.Run(() => {
				var path = Path.Combine(BepInEx.Paths.PluginPath, "IdlePlus");
				if (!Directory.Exists(path)) return;
				
				var filePath = Path.Combine(path, "settings.dat");
				if (!File.Exists(filePath)) return;
				var file = File.Open(filePath, FileMode.Open, FileAccess.Read);
				var data = new BinaryReader(file);
				
				var entries = data.ReadInt32();
				
				for (var i = 0; i < entries; i++) {
					var id = data.ReadString();
					var length = data.ReadByte();
					var bytes = data.ReadBytes(length);
					
					var setting = Categories.SelectMany(category => category.Settings)
						.FirstOrDefault(s => s.Id == id);
					setting?.Deserialize(bytes);
				}
				
				data.Close();
			});
		}

		public static async void Save() {
			await Task.Run(() => {
				var path = Path.Combine(BepInEx.Paths.PluginPath, "IdlePlus");
				Directory.CreateDirectory(path);
			
				var filePath = Path.Combine(path, "settings.dat");
				var file = File.Open(filePath, FileMode.Create, FileAccess.Write);
				var data = new BinaryWriter(file);
			
				var entries = Categories.Sum(category => category.Settings.Length);
				data.Write(entries);
			
				foreach (var category in Categories) {
					foreach (var setting in category.Settings) {
						var id = setting.Id;
						var bytes = setting.Serialize();
						
						data.Write(id);
						data.Write((byte)bytes.Length);
						data.Write(bytes);
					}
				}
				data.Close();
			});
		}
	}
}