using System.Collections.Generic;
using System.IO;
using IdlePlus.Settings;
using IdlePlus.Settings.Types;
using IdlePlus.Utilities;

namespace IdlePlus.TexturePack {
	public class TexturePackManager {
		
		public const int PackVersion = 1;
		private const string TexturePackFile = "texturepack.txt";
		
		public static TexturePackManager Instance;
		public static TexturePack CurrentPack { get; private set; }

		private readonly List<TexturePack> _texturePacks = new List<TexturePack>();
		
		public static void Load() {
			Instance = new TexturePackManager();
			
			// Create the texture pack directory if it doesn't exist.
			var path = Path.Combine(BepInEx.Paths.PluginPath, "IdlePlus", "texturepacks");
			Directory.CreateDirectory(path);
			
			// Create a README.txt file if it doesn't exist.
			var readmePath = Path.Combine(path, "README.txt");
			if (!File.Exists(readmePath)) File.WriteAllText(readmePath, "Place your texture packs here.");
			
			// Find all folders inside the texture pack directory.
			var directories = Directory.GetDirectories(path);
			foreach (var directory in directories) {
				// Make sure it is a texture pack by looking after the texture pack file.
				if (!File.Exists(Path.Combine(directory, TexturePackFile))) continue;
				var lines = File.ReadAllLines(Path.Combine(directory, TexturePackFile));
				
				var meta = PackMeta.Load(directory, lines);
				if (meta == null) continue;
				
				var texturePack = new TexturePack(directory, meta);
				Instance._texturePacks.Add(texturePack);
			}
			
			// Create the texture pack options.
			var options = new string[Instance._texturePacks.Count + 1];
			options[0] = "None";
			for (var i = 0; i < Instance._texturePacks.Count; i++)
				options[i + 1] = Instance._texturePacks[i].Meta.Name;
			
			// Create the texture pack setting and handle the loading.
			ModSettings.TexturePack.CurrentPack = StringDropdownSetting.Create("texturepack_current",
				true, "Which texture pack to use.", 0, options);
			ModSettings.TexturePack.CurrentPack.OnLoad = (index, value) => {
				IdleTasks.Run(() => {
					if (index == 0) {
						CurrentPack?.Unload();
						CurrentPack = null;
						return;
					}
					CurrentPack = Instance._texturePacks.Find(pack => pack.Meta.Name == value);
					CurrentPack?.Load();
				});
			};
		}
	}
}