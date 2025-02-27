using Chat;
using HarmonyLib;

namespace IdlePlus.Patches.ChatboxLogic {
	
	[HarmonyPatch(typeof(ChatboxInputValidator))]
	internal class ChatboxInputValidatorPatch {

		/// <summary>
		/// Patch to disable chatbox validation if the gameobject is disabled.
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch(nameof(ChatboxInputValidator.ValidateInput))]
		private static bool PrefixValidateInput(ChatboxInputValidator __instance) {
			return __instance.isActiveAndEnabled;
		}
		
	}
}