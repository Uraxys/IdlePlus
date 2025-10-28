﻿using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Client;
using HarmonyLib;
using IdlePlus.API.Event;
using IdlePlus.API.Event.Contexts;
using IdlePlus.API.Popup;
using IdlePlus.API.Popup.Popups;
using IdlePlus.Attributes;
using IdlePlus.IdleClansAPI;
using IdlePlus.Patches;
using IdlePlus.Settings;
using IdlePlus.TexturePack;
using IdlePlus.Utilities;
using PlayerMarket;
using Object = UnityEngine.Object;

namespace IdlePlus {
	
	[BepInPlugin(ModGuid, ModName, ModVersion)]
	public class IdlePlus : BasePlugin {
		
		internal const string
			ModName = "Idle Plus",
			ModAuthor = "Uraxys",
			ModID = "idleplus",
			ModGuid = "dev.uraxys.idleplus",
			ModVersion = "1.6.0"
#if DEBUG
			             + "-DEBUG";
#else
			             ;
#endif

		internal const bool PerformanceTest = false;
		internal static IntPtr WindowHandle = IntPtr.Zero;
		
		public override void Load() {
			IdleLog.Logger = Log;
			IdleLog.Info($"Loading Idle Plus v{ModVersion}...");
			
			TexturePackManager.Load();
			ModSettings.Load();

			// Attributes
			RegisterIl2CppAttributeHandler.Register();
			InitializeAttributeHandler.Load();
			InitializeOnceAttributeHandler.Load();
			
			// Create the IdlePlus game object with our custom behaviour.
			IdlePlusBehaviour.Create();
			
			// Load harmony patches.
			var harmony = new Harmony(ModGuid);
			harmony.PatchAll();
			PerformanceTestPatch.Patch(harmony);
			
			// Events
			Events.Scene.OnLobby.Register(OnSceneLobby);
			Events.Player.OnLogin.Register(OnLogin);
			
			// Popup testing
			TestPopupTwo.PopupKey = CustomPopupManager.Register("IdlePlus:TestPopupTwo", TestPopupTwo.Create);
			PlayerProfilePopup.PopupKey = CustomPopupManager.Register("IdlePlus:PlayerProfilePopup", PlayerProfilePopup.Create);
			
			// Create the market prices update task.
			IdleTasks.Repeat(0, 60, task => {
				if (!NetworkClient.IsConnected()) return;
				OldIdleAPI.UpdateMarketPrices();
			});
			
			IdleLog.Info($"Idle Plus v{ModVersion} loaded!");
		}
		
		private static void OnLogin(PlayerLoginEventContext ctx) {
			// Do one time initialization for objects that are only created once.
			InitializeOnceAttributeHandler.InitializeOnce();
			// Do initialization for objects that are recreated on login.
			InitializeAttributeHandler.Initialize();
				
			// Update market prices.
			OldIdleAPI.UpdateMarketPrices();
			
			// Update the window title to display the current player name.
			// NOTE: Only works on windows.
			if (WindowHandle != IntPtr.Zero) {
				WindowUtils.SetWindowText(WindowHandle, $"Idle Clans - {ctx.PlayerData.Username ?? "Null/Name"}");
			}
		}

		private static void OnSceneLobby() {
			// Update the window title to display "Idle Clans".
			// NOTE: Only works on windows.
			if (WindowHandle != IntPtr.Zero) {
				WindowUtils.SetWindowText(WindowHandle, "Idle Clans");
			}
		}
	}
}