using BepInEx;
using BepInEx.Logging;
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
			ModVersion = "1.0.0";
		
		private static bool _initialized;
		
		public override void Load() {
			IdleLog.Logger = Log;
			IdleLog.Info("Loading Idle Plus...");
			
			// Create the IdleClansPlusBehaviour instance.
			IdlePlusBehaviour.Create();
			
			// Load harmony patches.
			var harmony = new Harmony(ModGuid);
			harmony.PatchAll();
			
			// Create the market prices update task.
			IdleTasks.Interval(60, task => {
				if (!NetworkClient.IsConnected()) return;
				IdleAPI.UpdateMarketPrices();
			});
			
			IdleLog.Info("Idle Plus loaded!");
		}

		internal static void Update() {
			IdleTasks.Update();
		}

		internal static void OnLogin() {
			// Do initialization for objects that are recreated on login.
			PlayerMarketOfferPatch.Initialize();
			
			// Do one time initialization for objects that are only created once.
			if (!_initialized) {
				_initialized = true;
				InventoryItemHoverPopupPatch.InitializeOnce();
				ViewPlayerMarketOfferPopupPatch.InitializeOnce();
			}
			
			// After we've logged in, find the PlayerMarketPage and "initialize"
			// it, this will allow us to create sell offers from our inventory
			// without having to manually open the player market first.
			var playerMarket = Object.FindObjectOfType<PlayerMarketPage>(true);
			if (playerMarket == null) IdleLog.Warn("Couldn't find PlayerMarketPage in OnLogin!");
			else {
				playerMarket.gameObject.SetActive(true);
				playerMarket.gameObject.SetActive(false);
			}
			
			// Update market prices.
			IdleAPI.UpdateMarketPrices();
		}
	}
}