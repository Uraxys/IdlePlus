using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace IdlePlus.Utilities.Atlas {
	public class AtlasTexture {

		private static Il2CppStructArray<Color> _cachedDefaultTexture;

		public readonly Texture2D Texture;
		public readonly Dictionary<string, Rectangle> Sources = new Dictionary<string, Rectangle>();
		private readonly ImageNode _node;

		public AtlasTexture(int width, int height) {
			Texture = new Texture2D(width, height, TextureFormat.RGBA32, 1, true);
			Object.DontDestroyOnLoad(Texture);
			Texture.hideFlags = HideFlags.HideAndDontSave;
			Texture.filterMode = FilterMode.Point;

			// Set background color to transparent.
			if (_cachedDefaultTexture == null) {
				_cachedDefaultTexture = new Color[width * height];
				var color = new Color(0f, 0f, 0f, 0f);
				for (var i = 0; i < _cachedDefaultTexture.Length; i++)
					_cachedDefaultTexture[i] = color;
			}
			
			Texture.SetPixels(_cachedDefaultTexture);
			_node = new ImageNode(0, 0, width, height);
		}

		public bool AddTexture(AtlasTextureEntry entry, int padding) {
			var node = _node.Insert(entry.Texture, padding);
			if (node == null) return false;

			var width = entry.Texture.width;
			var height = entry.Texture.height;
			
			var pixels = entry.Texture.GetPixels(0);
			Texture.SetPixels(node.Rect.X, node.Rect.Y, width, height, pixels);
			
			Sources[entry.Path] = node.Rect;
            
			return true;
		}

		public void Build() {
			Texture.Apply();
		}

		public void Destroy() {
			Object.Destroy(Texture);
		}
		
		public void DebugWrite(string name) {
			// Write the image to file.
			var bytes = Texture.EncodeToPNG();
			var path = System.IO.Path.Combine(BepInEx.Paths.PluginPath, "IdlePlus", $"{name}.png");
			System.IO.File.WriteAllBytes(path, bytes);
		}
	}
}