using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdlePlus.Settings.Types;
using Path = System.IO.Path;

namespace IdlePlus.Settings {
	
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public static class ModSettings {

		#region Settings
        
		#region Features
		
		// Features category
		public static readonly SettingCategory FeaturesCategory = SettingCategory.Create("Features",
			Features.TotalWealth, Features.TotalWealthVendorValue, Features.ScrollInfo, 
			Features.EquipmentStatsInfo, Features.MaxOpenableChests, Features.PotionTime);
        
		// Feature settings
		public static class Features {
			public static readonly ToggleSetting TotalWealth = ToggleSetting.Create(
				"feature_totalWealth", true,
				"Total wealth displayed in the profile tab.",
				true
			);

			public static readonly ToggleSetting TotalWealthVendorValue = ToggleSetting.Create(
				"feature_totalWealthVendorValue",
				"If total wealth should use vendor value instead of the market value.",
				false
			);
			
			public static readonly ToggleSetting ScrollInfo = ToggleSetting.Create(
				"feature_scrollInfo",
				"Scrolls display which items that can be enchanted.",
				true
			);

			public static readonly ToggleSetting EquipmentStatsInfo = ToggleSetting.Create(
				"feature_equipmentStatsInfo",
				"Equipment displays their stat differences if equipped.",
				true
			);
			
			public static readonly ToggleSetting MaxOpenableChests = ToggleSetting.Create(
				"feature_maxOpenableChests",
				"Pressing 'Max' selects the maximum amount of treasure chests you can open at once.",
				true
			);

			public static readonly ToggleSetting PotionTime = ToggleSetting.Create(
				"feature_potionTime", true,
				"Display the total time a potion will last when drinking it.",
				true
			);
		}
		#endregion

		#region UI
		
		// UI category
		public static readonly SettingCategory UICategory = SettingCategory.Create("UI",
			UI.EnhancedInventoryStats, UI.EnhancedInventoryItemTooltip, UI.EnhancedChatCommands);
		
		// UI settings
		public static class UI {
			public static readonly ToggleSetting EnhancedInventoryStats = ToggleSetting.Create(
				"ui_enhancedInventoryStats", true,
				"Enhanced inventory equipment stats.",
				true
			);
			
			public static readonly ToggleSetting EnhancedInventoryItemTooltip = ToggleSetting.Create(
				"ui_enhancedInventoryItemTooltip", true,
				"Enhanced inventory item tooltip.",
				true
			);
			
			public static readonly ToggleSetting EnhancedChatCommands = ToggleSetting.Create(
				"ui_enhancedChatCommands", true,
				"Enhanced chat commands.",
				true
			);
		}
		#endregion
        
		#region MarketValue
		
		// Market Value category
		public static readonly SettingCategory MarketValueCategory = SettingCategory.Create("Item Value",
			MarketValue.Enabled, MarketValue.HideForIronman, MarketValue.ShiftForTotal, 
			MarketValue.IncludeNegotiation, MarketValue.DisplayType, MarketValue.DisplayFormat);
		
		// Market Value settings
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
			
			public static readonly ToggleSetting IncludeNegotiation = ToggleSetting.Create(
				"marketvalue_includeNegotiation",
				"Include the negotiation boost in the item value.",
				false
			);
		
			public static readonly DropdownSetting DisplayType = DropdownSetting.Create(
				"marketvalue_display",
				"Which value to display as the market value.",
				0,
				"Sell then buy", "Buy then sell", "Sell only", "Buy only", "Average"
			);
			
			public static readonly DropdownSetting DisplayFormat = DropdownSetting.Create(
				"marketvalue_format",
				"Which format to display the values in.",
				0,
				"Default", "Full", "Thousands", "Millions"
			);
		}
		#endregion

		#region TexturePack
		
		// Texture Pack category
		public static readonly SettingCategory TexturePackCategory = SettingCategory.Create("Texture Pack",
			TexturePack.CurrentPack);
		
		// Texture Pack settings
		public static class TexturePack {
			public static StringDropdownSetting CurrentPack;
		}
		#endregion
        
		#region Miscellaneous
		
		// Miscellaneous category
		public static readonly SettingCategory MiscellaneousCategory = SettingCategory.Create("Miscellaneous",
			Miscellaneous.InternalItemNames);
		
		// Miscellaneous settings
		public static class Miscellaneous {
			public static readonly ToggleSetting InternalItemNames = ToggleSetting.Create(
				"misc_internalItemNames",
				"Display the internal item name instead of the display name.",
				false
			);
		}
		#endregion
		
		#endregion
		
		// Category registration, each category must be registered here, if not
		// it won't be loaded or saved.
		public static readonly SettingCategory[] Categories = { FeaturesCategory, UICategory, MarketValueCategory,
			TexturePackCategory, MiscellaneousCategory };
		
		#region Save/Load
		
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
		
		#endregion
		
	}
}