using HarmonyLib;
using IdlePlus.TexturePack;
using IdlePlus.Utilities;
using Tasks;

namespace IdlePlus.Patches.Tasks {
	
	[HarmonyPatch(typeof(UITaskCombat))]
	public class UITaskCombatPatch {
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(UITaskCombat.Setup))]
		private static void PostfixSetup(UITaskCombat __instance, TaskType taskType, JobTask task) {
			if (TexturePackManager.CurrentPack == null) return;
			
			var identifier = task.IdentifiableType;
			var name = task.Name;
			
			if (identifier.Length == 0) {
				IdleLog.Warn($"Couldn't get combat task sprite for UI task, no identifiable type for {name}.");
				return;
			}

			var sprite = TexturePackManager.CurrentPack.TryGetCombatSprite(identifier, name);
			if (sprite == null) return;
			__instance._iconImage.sprite = sprite;
		}
		
	}
}