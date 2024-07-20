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
			if (TexturePackManager.Instance.CurrentTexturePack == null) return;
			IdleLog.Info($"Trying to get sprite for combat task {task.IdentifiableType}/{task.Name}.");
			var sprite = TexturePackManager.Instance.CurrentTexturePack.TryGetCombatSprite(task.Name);
			if (sprite == null) return;
			__instance._iconImage.sprite = sprite;
		}
	}

	[HarmonyPatch(typeof(UITask))]
	public class UITaskPatch {
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(UITask.Setup))]
		private static void PostfixSetup(UITask __instance, TaskType taskType, JobTask task) {
			IdleLog.Info($"Trying to setup task for {taskType}, {task.IdentifiableType}/{task.Name}.");
		}
	}
}