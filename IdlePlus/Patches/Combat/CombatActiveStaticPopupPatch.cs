using Combat;
using HarmonyLib;
using IdlePlus.TexturePack;
using IdlePlus.Utilities;
using Tasks;

namespace IdlePlus.Patches.Combat {
	
	[HarmonyPatch(typeof(CombatActiveStaticPopup))]
	public class CombatActiveStaticPopupPatch {
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(CombatActiveStaticPopup.Setup))]
		private static void PostfixSetup(CombatActiveStaticPopup __instance, JobTask task) {
			if (TexturePackManager.CurrentPack == null) return;
			
			var identifier = task.IdentifiableType;
			var name = task.Name;
			
			if (identifier == null) {
				// Clan bosses
				if (name.Equals("malignant_spider") || name.Equals("skeleton_warrior") ||
				    name.Equals("otherworldly_golem")) {
					identifier = "clan_boss";
				}
			}
			
			if (string.IsNullOrEmpty(identifier)) {
				IdleLog.Warn($"Couldn't get combat task sprite for combat panel, no identifiable type for {name}.");
				return;
			}
			
			var sprite = TexturePackManager.CurrentPack.TryGetCombatSprite(identifier, task.Name);
			if (sprite == null) return;
			__instance._taskImage.sprite = sprite;
		}
	}
}