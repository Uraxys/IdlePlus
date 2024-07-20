using UnityEngine;

namespace IdlePlus.Utilities.Atlas {
	public class AtlasTextureEntry {
		
		public readonly Texture2D Texture;
		public readonly string Path;

		public AtlasTextureEntry(Texture2D texture, string path) {
			Texture = texture;
			Path = path;
		}
		
		public void Destroy() {
			Object.Destroy(Texture);
		}
	}
}