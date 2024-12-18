using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IdlePlus.Utilities.Atlas {
	public class AtlasGenerator {

		// Pretty high default number, but if we didn't then cosmetic effects
		// might bleed into each other.
		public int Padding { get; set; } = 32;
		public int AtlasSize { get; set; } = 2048;
		
		private readonly List<AtlasTextureEntry> _entries = new List<AtlasTextureEntry>();
		private readonly List<AtlasTexture> _textures = new List<AtlasTexture>();
		private bool _built;

		public AtlasGenerator() {}
		public AtlasGenerator(int padding) {
			Padding = padding;
		}
		
		public void AddTexture(Texture2D texture, string path) {
			if (texture == null) throw new System.ArgumentNullException(nameof(texture));
			if (string.IsNullOrEmpty(path)) throw new System.ArgumentNullException(nameof(path));
			_entries.Add(new AtlasTextureEntry(texture, path));
		}

		public Atlas Build() {
			if (_built) throw new System.InvalidOperationException("Atlas has already been built.");
			_built = true;
			
			// Validate all textures before proceeding.
			foreach (var entry in _entries) {
				var texture = entry.Texture;
				if (texture.height > AtlasSize || texture.width > AtlasSize) {
					throw new System.ArgumentException($"Texture is too large: {entry.Path}, size: " +
					                                   $"{texture.width}x{texture.height}");
				}
			}
			
			// Build the image.
			foreach (var entry in _entries) {
				var added = _textures.Any(a => a.AddTexture(entry, Padding));
				if (added) continue;
				
				var atlasTexture = new AtlasTexture(AtlasSize, AtlasSize);
				if (!atlasTexture.AddTexture(entry, Padding)) 
					throw new System.Exception("Failed to add texture to newly created atlas.");
				_textures.Add(atlasTexture);
			}
			
			// Build the textures.
			foreach (var textures in _textures) textures.Build();
			
			// Destroy the backing textures.
			foreach (var entry in _entries) entry.Destroy();
			_entries.Clear();
			
			// Create the sprites.
			var atlas = new Atlas(_textures);
			
			// Debug write the atlas.
			/*for (var i = 0; i < _textures.Count; i++) {
				var tex = _textures[i];
				tex.DebugWrite($"atlas_{i}.png");
			}*/

			return atlas;
		}
	}
}