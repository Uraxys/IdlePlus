using System.Collections.Generic;
using Scripts.Shared.Data.Content.Skills;
using UnityEngine;

namespace IdlePlus.Utilities {
	
	public static class IdleAssets {

		private const string SkillPath = "SkillTaskIcons/";
		
		private static readonly Dictionary<string, Sprite> CachedSprites = new Dictionary<string, Sprite>();
		
		public static Sprite GetSkillSprite(Skill skill) {
			var path = $"{SkillPath}{skill.ToString()}";
			if (CachedSprites.TryGetValue(path, out var sprite)) return sprite;
			
			sprite = Resources.Load<Sprite>(path);
			if (sprite == null) return null;
			
			CachedSprites[path] = sprite;
			return sprite;
		}
	}
}