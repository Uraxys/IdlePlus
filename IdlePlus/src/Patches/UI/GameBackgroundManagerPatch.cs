using System.Collections.Generic;
using HarmonyLib;
using IdlePlus.API.Event;
using IdlePlus.TexturePack;
using UI;

namespace IdlePlus.Patches.UI {
	[HarmonyPatch(typeof(GameBackgroundManager))]
	internal class GameBackgroundManagerPatch {

		private static bool _initialized;
		private static Dictionary<BackgroundType, string> _backgrounds = new Dictionary<BackgroundType, string> {
			{ BackgroundType.Lobby | BackgroundType.Loading | BackgroundType.Game, "default" },
			{ BackgroundType.Minigame, "minigame" },
			{ BackgroundType.Raids, "raid" },
			{ BackgroundType.BossFights, "boss_fight" }
		};

		private static void Initialize(GameBackgroundManager instance) {
			if (_initialized) return;
			_initialized = true;
			
			// Try to override the background textures when the texture pack
			// been loaded.
			Events.IdlePlus.OnTexturepackLoaded.Register(() => {
				// Check if we have any backgrounds in the texture pack.
				BackgroundType? updated = null;
				foreach (var entry in _backgrounds) {
					var sprite = TexturePackManager.CurrentPack?.TryGetMiscSprite($@"background\{entry.Value}");
					if (!sprite) continue;
					
					instance._backgroundSpritesByType[entry.Key] = sprite;
					if (updated == null) updated = entry.Key;
					else updated = updated.Value | entry.Key;
				}

				// Force update the background if we changed the currently
				// active background.
				var previous = instance.ActiveBackground.Type;
				if (!updated.HasValue) return;
				if ((updated.Value & previous) <= 0) return;

				// Hacky workaround to refresh the background.
				instance.SetBackground(previous == BackgroundType.Raids ? BackgroundType.Game : BackgroundType.Raids);
				instance.SetBackground(previous);
			});
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(GameBackgroundManager.Awake))]
		private static void PostfixAwake(GameBackgroundManager __instance) {
			Initialize(__instance);
		}
	}
}