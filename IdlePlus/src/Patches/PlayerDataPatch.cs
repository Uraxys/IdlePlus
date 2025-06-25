using HarmonyLib;
using IdlePlus.API.Event;
using IdlePlus.API.Event.Contexts;
using Player;

namespace IdlePlus.Patches {
	
	/// <summary>
	/// Patch to know when the player has logged into the game.
	/// </summary>
	[HarmonyPatch(typeof(PlayerData))]
	public class PlayerDataPatch {
		
		[HarmonyFinalizer]
		[HarmonyPatch(nameof(PlayerData.Start))]
		private static void FinalizerStart(PlayerData __instance) {
			Events.Player.OnLogin.Call(new PlayerLoginEventContext(__instance));
		}
	}
}