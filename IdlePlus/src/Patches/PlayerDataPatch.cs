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
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(PlayerData.Start))]
		public static void PostfixStart(PlayerData __instance) {
			Events.Player.OnLogin.Call(new PlayerLoginEventContext(__instance));
		}
	}
}