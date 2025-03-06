using System.Collections.Generic;
using System.Linq;
using Databases;
using GameContent;
using IdlePlus.API.Event;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Strings;
using Il2CppSystem;

namespace IdlePlus.API.Utility.Game {
	public static class ItemUtils {

		internal static readonly AhoCorasick ItemSearcher = new AhoCorasick();
		internal static readonly Dictionary<string, Item> ItemsByLocalizedName = new Dictionary<string, Item>();
		internal static readonly Dictionary<string, Item> ItemsByLocalizedNameLowered = new Dictionary<string, Item>();
		
		
		[InitializeOnce(OnSceneLoad = "*")]
		private static void InitializeOnce() {
			Events.Game.OnConfigDataLoaded.Register(() => {
				foreach (var item in ItemDatabase.ItemList._values) {
					if (item.CosmeticScrollEffect != WeaponEffectType.None) continue;

					string name;
					if (item.MasteryCapeType == MasteryCapeType.None) 
						name = LocalizationManager.GetLocalizedValue(item.Name, Language.English);
					else {
						var tier = item.ExtractMasteryCapeTier();
						if (item.MasteryCapeType == MasteryCapeType.Completionist) {
							var part = LocalizationManager.GetLocalizedValue("completionist", Language.English);
							name = LocalizationManager.GetLocalizedValue("mastery_cape_name", Language.English);
							name = String.Format(name, part, tier);
						}
						else {
							var skill = item.SkillBoost.Skill.ToString().ToLower();
							var part = LocalizationManager.GetLocalizedValue(skill, Language.English);
							name = LocalizationManager.GetLocalizedValue("mastery_cape_name", Language.English);
							name = String.Format(name, part, tier);
						}
					}

					if (name.Contains("_")) continue;
					if (ItemsByLocalizedName.ContainsKey(name)) {
						IdleLog.Warn($"Failed to cache localized name for {item.ItemId}, already exists: {name}");
						continue;
					}

					ItemsByLocalizedName[name] = item;
					ItemsByLocalizedNameLowered[name.ToLower()] = item;

					if (!name.Contains("'")) continue;
					var stripped = name.Replace("'", "");
					if (!ItemsByLocalizedName.ContainsKey(stripped))
						ItemsByLocalizedName[stripped] = item;
					if (!ItemsByLocalizedNameLowered.ContainsKey(stripped))
						ItemsByLocalizedNameLowered[stripped.ToLower()] = item;
				}
				
				// Create the AhoCorasick.
				foreach (var entry in ItemsByLocalizedName.Keys) {
					ItemSearcher.Insert(entry.ToLower());
					ItemSearcher.BuildFailureLinks();
				}
			});
		}
		
		/// <summary>
		/// Try to get an <see cref="Item"/> from the provided localized name.
		/// If an item matching the provided name couldn't be found, then null
		/// is returned instead.
		/// </summary>
		/// <param name="name">The localized name of the item.</param>
		/// <returns>The <see cref="Item"/> that was found, if not null.</returns>
		public static Item TryGetItemFromLocalizedName(string name) {
			Asserts.NotNull(name, "name");
			var loweredName = name.ToLower();
			if (ItemsByLocalizedNameLowered.TryGetValue(loweredName, out var i1)) return i1;
			return !ItemsByLocalizedNameLowered.TryGetValue(loweredName.Replace("'", ""), out var i2) ? null : i2;
		}
		
		// Extensions

		public static string IdlePlus_GetLocalizedEnglishName(this Item item) {
			if (item.CosmeticScrollEffect != WeaponEffectType.None) return item.Name;
			
			if (item.MasteryCapeType == MasteryCapeType.None) 
				return LocalizationManager.GetLocalizedValue(item.Name, Language.English);
			
			string name;
			var tier = item.ExtractMasteryCapeTier();
			if (item.MasteryCapeType == MasteryCapeType.Completionist) {
				var part = LocalizationManager.GetLocalizedValue("completionist", Language.English);
				name = LocalizationManager.GetLocalizedValue("mastery_cape_name", Language.English);
				return String.Format(name, part, tier);
			} else {
				var skill = item.SkillBoost.Skill.ToString().ToLower();
				var part = LocalizationManager.GetLocalizedValue(skill, Language.English);
				name = LocalizationManager.GetLocalizedValue("mastery_cape_name", Language.English);
				return String.Format(name, part, tier);
			}
		}
	}
}