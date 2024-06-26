using BepInEx;
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
		public static Profiler _profiler;
		
		public override void Load() {
			IdleLog.Logger = Log;
			IdleLog.Info("Loading Idle Plus...");
			_profiler = new Profiler();
			
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
			_profiler.Start(); // Starts the root section.
			
			_profiler.Push("idleTasks");
			IdleTasks.Update();
			_profiler.Pop(); // Pops idleTasks
			
			_profiler.Push("test");
			// Some heavy work
			float result = 0;
			for (int i = 0; i < 1000; i++) {
				result += i - 1;
				result *= i + 1;
				result--;
			}
			_profiler.Push("innerTest");
			result = 0;
			for (int i = 0; i < 1000; i++) {
				result -= i + 1;
			}
			
			_profiler.Push("innerCheck");
			result = 0;
			while (result < 1000) {
				result++;
				if (result % 3 == 0) result++;
			}
			_profiler.Pop(); // Pops innerCheck
			_profiler.Pop(); // Pops innerTest
			_profiler.Pop(); // Pops test
			
			_profiler.Push("trackTotalInner");
			for (int i = 0; i < 1000; i++) {
				_profiler.Push("trackInner");
				result += i % 5;
				_profiler.Pop(); // Pops trackInner
			}
			_profiler.Pop(); // Pops trackTotalInner
			
			_profiler.Stop(); // "pops" the root section.
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