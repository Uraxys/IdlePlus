using System;
using HarmonyLib;
using IdlePlus.Utilities;
using Player;

namespace IdlePlus.Patches {
	
	/// <summary>
	/// Patch to know when the player has logged into the game.
	/// </summary>
	[HarmonyPatch(typeof(PlayerData))]
	public class PlayerDataPatch {
		
		// TODO: Switch back to using HarmonyPostfix when the vanilla bug is fixed.
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PlayerData.Start))]
		public static void PrefixStart() {
			try {
				IdleLog.Info("Player has logged in.");
				IdlePlus.OnLogin();
			} catch (Exception e) {
				IdleLog.Error("Error in PlayerDataPatch.PrefixStart.", e);
			}
		}
	}
}