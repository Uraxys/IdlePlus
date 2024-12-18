using System.Diagnostics;
using System.IO;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Atlas;
using IdlePlus.Utilities.Extensions;
using Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePlus.TexturePack {
	public class TexturePack {
		
		private const string ItemsFolder = "items";
		private const string TasksFolder = "tasks";
		private const string MiscFolder = "misc";
        
		public readonly string Path;
		public readonly PackMeta Meta;

		private Atlas _itemAtlas;
		private Atlas _taskAtlas;
		private Atlas _miscAtlas;

		public TexturePack(string path, PackMeta meta) {
			Path = path;
			Meta = meta;
		}

		public void Load() {
			var watch = Stopwatch.StartNew();
            
			if (_itemAtlas == null) _itemAtlas = CreateAtlas(ItemsFolder);
			if (_taskAtlas == null) _taskAtlas = CreateAtlas(TasksFolder);
			if (_miscAtlas == null) _miscAtlas = CreateAtlas(MiscFolder);

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
			return _itemAtlas?.TryGetSprite($@"{ItemsFolder}\{name}.png");
		}
		
		public void TryApplyItemSprite(string name, object obj) {
			TryApplySprite(TryGetItemSprite(name), obj, name);
		}

		public Sprite TryGetCombatSprite(string category, string name) {
			return _taskAtlas?.TryGetSprite($@"{TasksFolder}\combat\{category}\{name}.png");
		}
		
		public void TryApplyCombatSprite(string category, string name, GameObject obj) {
			TryApplySprite(TryGetCombatSprite(category, name), obj, name);
		}

		public Sprite TryGetMiscSprite(string name) {
			return _miscAtlas?.TryGetSprite($@"{MiscFolder}\{name}.png");
		}
		
		public void TryApplyMiscSprite(string name, object obj) {
			TryApplySprite(TryGetMiscSprite(name), obj, name);
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
		
		private static void TryApplySprite(Sprite sprite, object obj, string name) {
			if (sprite == null) return;
			var image = TryGetImage(obj, name);
			if (image != null) image.sprite = sprite;
		}
        
		private static Image TryGetImage(object obj, string name) {
			switch (obj) {
				case Image img: return img;
				case GameObject go: return go.Use<Image>();
				case Transform tr: return tr.Use<Image>();
			}
			IdleLog.Warn($"Couldn't get Image component when trying to apply sprite {name}.");
			return null;
		}
	}
}