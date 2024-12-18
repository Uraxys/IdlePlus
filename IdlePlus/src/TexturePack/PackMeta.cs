using IdlePlus.Utilities;

namespace IdlePlus.TexturePack {
	public class PackMeta {
		
		private const string NameEntry = "name=";
		private const string AuthorEntry = "author="; // Optional
		private const string PackVersionEntry = "pack-version=";
        
		public readonly string Name;
		public readonly string Author;
		public readonly int PackVersion;
		
		
		private PackMeta(string name, string author, int packVersion) {
			Name = name;
			Author = author;
			PackVersion = packVersion;
		}

		public static PackMeta Load(string path, string[] lines) {
			string name = null;
			var author = "Unknown";
			var packVersion = -1;
			
			// Refactor if more entries are ever added.
			foreach (var line in lines) {
				if (line.Length == 0 || line.StartsWith("#")) continue;
				
				if (line.StartsWith(NameEntry)) 
					name = line.Substring(NameEntry.Length);
				if (line.StartsWith(AuthorEntry)) 
					author = line.Substring(AuthorEntry.Length);
				if (line.StartsWith(PackVersionEntry))
					packVersion = Numbers.TryParseInt(line.Substring(PackVersionEntry.Length), -1);
			}

			if (name == null) {
				IdleLog.Warn($"Failed to load texture pack at {path}, missing name entry in texturepack.txt.");
				return null;
			}
			
			if (packVersion == -1) {
				IdleLog.Warn($"Failed to load texture pack {name}, missing pack version entry in texturepack.txt.");
				return null;
			}

			if (packVersion != TexturePackManager.PackVersion) {
				IdleLog.Warn($"Failed to load texture pack {name}, pack version mismatch. Expected " +
				             $"{TexturePackManager.PackVersion}, got {packVersion}.");
				return null;
			}

			return new PackMeta(name, author, packVersion);
		}
	}
}