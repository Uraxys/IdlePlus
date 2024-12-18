using Databases;
using HarmonyLib;
using IdlePlus.TexturePack;
using UnityEngine;

namespace IdlePlus.Patches {
	
	[HarmonyPatch(typeof(Item))]
	public class ItemPatch {
		
		/// <summary>
		/// Patch to add support for texture packs, allowing anyone to override
		/// the textures of any item in the game.
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch(nameof(Item.LoadSpriteFromResources))]
		private static bool PrefixLoadSpriteFromResources(Item __instance, ref Sprite __result) {
			if (TexturePackManager.CurrentPack == null) return true;
			var sprite = TexturePackManager.CurrentPack.TryGetItemSprite(__instance.Name);
			if (sprite == null) return true;
			
			__result = sprite;
			return false;
		}
	}
}