using Databases;
using HarmonyLib;

namespace IdlePlus.Patches {
	
	[HarmonyPatch(typeof(Item))]
	public class ItemPatch {

		// TODO: Texture pack?
		/*[HarmonyPrefix]
		[HarmonyPatch(nameof(Item.LoadSpriteFromResources))]
		private static bool PostfixLoadSpriteFromResources(Item __instance, ref Sprite __result) {
			var itemId = __instance.ItemId;
			if (itemId != 561 && itemId != 53) return true;
			
			var sprite = AssetLoader.LoadSprite(itemId == 561 ? "fish.png" : "pickaxe.png");
			if (sprite == null) return true;
			
			__result = sprite;
			return false;
		}*/
	}
}