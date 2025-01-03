﻿using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Client;
using HarmonyLib;
using IdlePlus.API.Event;
using IdlePlus.API.Event.Contexts;
using IdlePlus.Attributes;
using IdlePlus.Command;
using IdlePlus.IdleClansAPI;
using IdlePlus.Settings;
using IdlePlus.TexturePack;
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
			ModVersion = "1.4.0"
#if DEBUG
			             + "-DEBUG";
#else
			             ;
#endif
		
		public static IntPtr WindowHandle = IntPtr.Zero;
		
		public override void Load() {
			IdleLog.Logger = Log;
			IdleLog.Info($"Loading Idle Plus v{ModVersion}...");
			
			TexturePackManager.Load();
			ModSettings.Load();
			CommandManager.Load();
			
			// Attributes
			RegisterIl2CppAttributeHandler.Register();
			InitializeAttributeHandler.Load();
			InitializeOnceAttributeHandler.Load();
			
			// Create the IdlePlus game object with our custom behaviour.
			IdlePlusBehaviour.Create();
			
			// Load harmony patches.
			var harmony = new Harmony(ModGuid);
			harmony.PatchAll();
			
			// Events
			Events.Scene.OnLobby.Register(OnSceneLobby);
			Events.Player.OnLogin.Register(OnLogin);
			
			// Create the market prices update task.
			IdleTasks.Repeat(0, 60, task => {
				if (!NetworkClient.IsConnected()) return;
				IdleAPI.UpdateMarketPrices();
			});
			
			IdleLog.Info($"Idle Plus v{ModVersion} loaded!");
		}
		
		private static void OnLogin(PlayerLoginContext ctx) {
			// Do one time initialization for objects that are only created once.
			InitializeOnceAttributeHandler.InitializeOnce();
			
			// Find the player market and "initialize" it.
			var playerMarket = Object.FindObjectOfType<PlayerMarketPage>(true);
			playerMarket.gameObject.SetActive(true);
			playerMarket.gameObject.SetActive(false);
				
			// Do initialization for objects that are recreated on login.
			InitializeAttributeHandler.Initialize();
				
			// Update market prices.
			IdleAPI.UpdateMarketPrices();
			
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