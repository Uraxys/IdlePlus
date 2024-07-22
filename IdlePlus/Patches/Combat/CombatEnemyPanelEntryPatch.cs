using Combat;
using HarmonyLib;
using IdlePlus.TexturePack;
using IdlePlus.Utilities;
using Tasks;

namespace IdlePlus.Patches.Combat {
	
	[HarmonyPatch(typeof(CombatEnemyPanelEntry))]
	public class CombatEnemyPanelEntryPatch {
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(CombatEnemyPanelEntry.Setup))]
		private static void PostfixSetup(CombatEnemyPanelEntry __instance, JobTask task) {
			if (TexturePackManager.CurrentPack == null) return;
			
			var identifier = task.IdentifiableType;
			var name = task.Name;
			
			if (identifier.Length == 0) {
				IdleLog.Warn($"Couldn't get combat task sprite for combat panel, no identifiable type for {name}.");
				return;
			}
			
			var sprite = TexturePackManager.CurrentPack.TryGetCombatSprite(identifier, task.Name);
			if (sprite == null) return;
			__instance._iconImage.sprite = sprite;
		}
	}
}