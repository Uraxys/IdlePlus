using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdlePlus.Utilities.Atlas {
	public class Atlas {
		
		private static readonly Vector2 Pivot = new Vector2(0.5f, 0.5f);
        
		private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
		private readonly Dictionary<string, AtlasTexture> _pathToTexture = new Dictionary<string, AtlasTexture>();
		private readonly List<AtlasTexture> _textures;

		public Atlas(List<AtlasTexture> textures) {
			_textures = textures;
			
			// Create the sprites.
			foreach (var atlas in _textures) {
				foreach (var source in atlas.Sources) {
					var rect = source.Value;
					var sprite = Sprite.Create(atlas.Texture, rect.ToRect(), Pivot, 100f, 1, SpriteMeshType.FullRect);
					Object.DontDestroyOnLoad(sprite);
					sprite.hideFlags = HideFlags.HideAndDontSave;
					_sprites[source.Key] = sprite;
					_pathToTexture[source.Key] = atlas;
				}
			}
		}
		
		public Sprite TryGetSprite(string path) {
			if (!_sprites.TryGetValue(path, out var sprite)) return null;
			IdleLog.Info($"Trying to get sprite for path '{path}'.");

			// We need to be careful, as unity overrides the == operator,
			// returning null if the object is destroyed.
			if (sprite != null) {
				IdleLog.Info($"Found sprite not null, returning.");
				return sprite;
			}
			
			// Unity... or SOMEONE... has destroyed out sprite, which
			// DOESN'T destroy the underlying texture... So, just
			// recreate it without any issue.
			var atlas = _pathToTexture[path];
			var rect = atlas.Sources[path];
			_sprites[path] = sprite = Sprite.Create(atlas.Texture, rect.ToRect(), Pivot, 100f, 1, SpriteMeshType.FullRect);

			return sprite;
		}
		
		public void Destroy() {
			foreach (var sprite in _sprites.Values) Object.Destroy(sprite);
			foreach (var atlas in _textures) atlas.Destroy();
			_sprites.Clear();
			_textures.Clear();
		}
	}
}