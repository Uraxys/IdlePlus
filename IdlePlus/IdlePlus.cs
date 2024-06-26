﻿using BepInEx;
using BepInEx.Unity.IL2CPP;
using Client;
using HarmonyLib;
using IdlePlus.IdleClansAPI;
using IdlePlus.Patches;
using IdlePlus.Utilities;
using PlayerMarket;
using Object = UnityEngine.Object;

namespace IdlePlus {
	
	[BepInPlugin(ModGuid, ModName, ModVersion)]
	public class IdlePlus : BasePlugin {
		
		public const string
			ModName = "Idle Plus",
			ModAuthor = "Uraxys",
			ModID = "idleplus",
			ModGuid = "dev.uraxys.idleplus",
			ModVersion = "1.0.1";
		
		private static bool _initialized;
		
		public override void Load() {
			IdleLog.Logger = Log;
			IdleLog.Info($"Loading Idle Plus v{ModVersion}...");
			
			// Create the IdleClansPlusBehaviour instance.
			IdlePlusBehaviour.Create();
			
			// Load harmony patches.
			var harmony = new Harmony(ModGuid);
			harmony.PatchAll();
			
			// Create the market prices update task.
			IdleTasks.Repeat(0, 60, task => {
				if (!NetworkClient.IsConnected()) return;
				IdleAPI.UpdateMarketPrices();
			});
			
			IdleLog.Info($"Idle Plus v{ModVersion} loaded!");
		}

		internal static void Update() {
			IdleTasks.Update();
		}

		internal static void OnLogin() {
			// Do one time initialization for objects that are only created once.
			if (!_initialized) {
				_initialized = true;
				ItemInfoPopupPatch.InitializeOnce();
				InventoryItemHoverPopupPatch.InitializeOnce();
				ViewPlayerMarketOfferPopupPatch.InitializeOnce();
				AdsViewPopupPatch.InitializeOnce();
			}
			
			// After we've logged in, find the PlayerMarketPage and "initialize"
			// it, this will allow us to create sell offers from our inventory
			// without having to manually open the player market first.
			IdleTasks.Run(() => { // TODO: Don't run a frame later when the client exception is fixed.
				var playerMarket = Object.FindObjectOfType<PlayerMarketPage>(true);
				playerMarket.gameObject.SetActive(true);
				playerMarket.gameObject.SetActive(false);
				
				// Do initialization for objects that are recreated on login.
				PlayerMarketOfferPatch.Initialize();
				ViewPlayerMarketOfferPopupPatch.Initialize();
				
				// Update market prices.
				IdleAPI.UpdateMarketPrices();
			});
		}
	}
}