using System.Collections.Generic;
using System.IO;
using IdlePlus.Utilities.Atlas;
using UnityEngine;

namespace IdlePlus.Utilities {
	public static class AssetLoader {
		
		//private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
		private static readonly Dictionary<string, string> CachedPaths = new Dictionary<string, string>();
		private static bool _initialized;
		
		private static void InitializeCache() {
			if (_initialized) return;
			_initialized = true;
			
			var basePath = Path.Combine(BepInEx.Paths.PluginPath, "IdlePlus");
			foreach (var filePath in Directory.EnumerateFiles(basePath, "*.*", SearchOption.AllDirectories)) {
				// Make sure it's a PNG file.
				if (!filePath.EndsWith(".png")) continue;
				// Get the relative path from the plugin directory.
				var relativePath = filePath.Substring(basePath.Length + 1);
				CachedPaths[relativePath] = filePath;
			}
		}

		public static void DoTest() {
			var generator = new AtlasGenerator();
			IdleLog.Info("Adding textures to atlas...");
			InitializeCache();
			foreach (var entry in CachedPaths) {
				var tex = LoadSprite(entry.Key);
				generator.AddTexture(tex, entry.Key);
			}
			IdleLog.Info("Building atlas...");
			generator.Build();
		}
		
		public static Texture2D LoadSprite(string path) {
			InitializeCache();
			
			// Fix the path separators.
			path = path.Replace('/', Path.DirectorySeparatorChar);
			path = path.Replace('\\', Path.DirectorySeparatorChar);
			
			if (!CachedPaths.TryGetValue(path, out var cachedPath)) {
				IdleLog.Error($"Sprite not found: {path}");
				return null;
			}
			
			// Create the sprite.
			var bytes = File.ReadAllBytes(cachedPath);
			var tex = new Texture2D(2, 2);
			tex.ignoreMipmapLimit = true;
			tex.LoadImage(bytes);
			
			IdleLog.Info($"Loaded texture {cachedPath} ({tex.width}x{tex.height})");
			
			return tex;
		}
        
		/*public static Sprite LoadSprite(string path) {
			InitializeCache();
			
			// Fix the path separators.
			path = path.Replace('/', Path.DirectorySeparatorChar);
			path = path.Replace('\\', Path.DirectorySeparatorChar);
			
			if (Sprites.TryGetValue(path, out var sprite)) return sprite;
			if (!CachedPaths.TryGetValue(path, out var cachedPath)) {
				IdleLog.Error($"Sprite not found: {path}");
				return null;
			}
			
			// Create the sprite.
			var bytes = File.ReadAllBytes(cachedPath);
			var tex = new Texture2D(2, 2);
			tex.LoadImage(bytes);

			sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
			Sprites[path] = sprite;

			return sprite;
		}*/
	}
}