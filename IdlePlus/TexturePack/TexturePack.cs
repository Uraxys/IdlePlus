using System.Diagnostics;
using System.IO;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Atlas;
using Tasks;
using UnityEngine;

namespace IdlePlus.TexturePack {
	public class TexturePack {
		
		private const string ItemsFolder = "items";
		private const string TasksFolder = "tasks";
        
		public readonly string Path;
		public readonly PackMeta Meta;

		private Atlas _itemAtlas;
		private Atlas _taskAtlas;

		public TexturePack(string path, PackMeta meta) {
			Path = path;
			Meta = meta;
		}

		public void Load() {
			var watch = Stopwatch.StartNew();
            
			if (_itemAtlas == null) _itemAtlas = CreateAtlas(ItemsFolder);
			if (_taskAtlas == null) _taskAtlas = CreateAtlas(TasksFolder);

			watch.Stop();
			var elapsed = watch.ElapsedMilliseconds;
			IdleLog.Info($"Loaded texture pack {Meta.Name} by {Meta.Author} in {elapsed}ms.");
		}

		public void Unload() {
			_itemAtlas?.Destroy();
			_taskAtlas?.Destroy();
			_itemAtlas = null;
			_taskAtlas = null;
		}
		
		public Sprite TryGetItemSprite(string name) {
			return _itemAtlas?.TryGetSprite($@"items\{name}.png");
		}

		public Sprite TryGetCombatSprite(string category, string name) {
			return _taskAtlas?.TryGetSprite($@"tasks\combat\{category}\{name}.png");
		}

		private Atlas CreateAtlas(string folder) {
			AtlasGenerator generator = null;

			if (!Directory.Exists(System.IO.Path.Combine(Path, folder))) return null;
			var i = 0;
			foreach (var filePath in Directory.EnumerateFiles(System.IO.Path.Combine(Path, folder), "*.png", 
				         SearchOption.AllDirectories)) {
				var relativePath = filePath.Substring(Path.Length + 1);
				i++;
				
				var bytes = File.ReadAllBytes(filePath);
				var tex = new Texture2D(2, 2);
				tex.LoadImage(bytes);
				tex.ignoreMipmapLimit = true;
				tex.filterMode = FilterMode.Point;
				tex.anisoLevel = 1;

				if (generator == null) generator = new AtlasGenerator();
				generator.AddTexture(tex, relativePath);
			}
			
			return generator?.Build();
		}
	}
}