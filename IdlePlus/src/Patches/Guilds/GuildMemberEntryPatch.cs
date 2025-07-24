using Guilds;
using HarmonyLib;
using IdlePlus.TexturePack;

namespace IdlePlus.Patches.Guilds {

	[HarmonyPatch(typeof(GuildMemberEntry))]
	public class GuildMemberEntryPatch {

		[HarmonyPrefix]
		[HarmonyPatch(nameof(GuildMemberEntry.Setup))]
		private static void PrefixSetup(GuildMemberEntry __instance, GameMode gameMode, bool premium,
			bool premiumPlus) {
			if (TexturePackManager.CurrentPack == null) return;

			var pack = TexturePackManager.CurrentPack;
			var ironmanIcon = __instance._ironmanIconGO;
			var premiumIcon = __instance._premiumIconGO;
			var gildedIcon = __instance._premiumPlusIconGO;

			if (gameMode == GameMode.Ironman)
				pack.TryApplyMiscSprite("ironman_icon", ironmanIcon);
			else if (gameMode == GameMode.GroupIronman)
				pack.TryApplyMiscSprite("group_ironman_icon", ironmanIcon);

			if (premium) pack.TryApplyMiscSprite("premium_icon", premiumIcon);
			if (premiumPlus) pack.TryApplyMiscSprite("gilded_icon", gildedIcon);
		}
	}
}