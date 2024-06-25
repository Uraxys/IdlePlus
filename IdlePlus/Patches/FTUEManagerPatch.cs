using FTUE;
using HarmonyLib;

namespace IdlePlus.Patches {
	
	/// <summary>
	/// Patch for FTUEManager to detect when the client has connected to the
	/// server. There is probably a better way to do this, but I couldn't find
	/// one in my 10 minutes of searching.
	/// </summary>
	[HarmonyPatch(typeof(FTUEManager))]
	public class FTUEManagerPatch {
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(FTUEManager.InitializeTutorial))]
		public static void PostfixInitializeTutorial() {
			IdlePlus.OnLogin();
		}
	}
}