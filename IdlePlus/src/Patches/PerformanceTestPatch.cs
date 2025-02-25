using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Ads;
using Assets.Scripts.Player;
using ChatboxLogic;
using DebugTools;
using Equipment;
using GameContent;
using Guilds;
using HarmonyLib;
using IdleClansApi;
using IdlePlus.Unity;
using IdlePlus.Utilities;
using Il2CppSystem.Threading;
using InAppPurchasing;
using Main;
using Management;
using Minigames;
using Platforms.Steam;
using Player;
using Player.Inventory;
using PlayerMarket;
using PlayFab;
using Profile;
using Shops;
using Tasks;
using UI;
using UnityEngine;
using Upgrades;
using WatsonTcp;
using Debug = UnityEngine.Debug;
using Object = Il2CppSystem.Object;

namespace IdlePlus.Patches {
	public static class PerformanceTestPatch {
		
		internal static List<T> Methods<T>(params T[] items) => new List<T>(items);
		
		internal static void Patch(Harmony harmony) {

			// Disable for now.
			if (true) return;

			var prefix = typeof(PerformanceTestPatch).GetMethod("Prefix");
			var postfix = typeof(PerformanceTestPatch).GetMethod("Postfix");
			
			Dictionary<Type, List<string>> targets = new Dictionary<Type, List<string>>();
			targets.Add(typeof(AdsManager), Methods(nameof(AdsManager.Awake)));
			targets.Add(typeof(GuildManager), Methods(nameof(GuildManager.Awake)));
			targets.Add(typeof(PlayerData), Methods(nameof(PlayerData.Awake), nameof(PlayerData.SetupGameData), nameof(PlayerData.Start)));
			targets.Add(typeof(PlayerInventory), Methods(nameof(PlayerInventory.Awake), nameof(PlayerInventory.Setup), nameof(PlayerInventory.Start)));
			targets.Add(typeof(PlayerEquipment), Methods(nameof(PlayerEquipment.Awake), nameof(PlayerEquipment.Start), nameof(PlayerEquipment.OnEnable)));
			targets.Add(typeof(LoginTracker), Methods(nameof(LoginTracker.Start)));
			targets.Add(typeof(PlayerItemSendSuggestions), Methods(nameof(PlayerItemSendSuggestions.Awake)));
			targets.Add(typeof(PlayerQuests), Methods(nameof(PlayerQuests.Awake)));
			
			targets.Add(typeof(TaskManager), Methods(nameof(TaskManager.Awake)));
			targets.Add(typeof(GuestModeManager), Methods(nameof(GuestModeManager.Awake)));
			targets.Add(typeof(PlayerShopManager), Methods(nameof(PlayerShopManager.Awake)));
			targets.Add(typeof(ShopManager), Methods(nameof(ShopManager.Start)));
			targets.Add(typeof(UpgradeManager), Methods(nameof(UpgradeManager.Awake)));
			
			targets.Add(typeof(ProfilePanel), Methods(nameof(ProfilePanel.Awake), nameof(ProfilePanel.OnEnable), nameof(ProfilePanel.SetupData)));
			targets.Add(typeof(ProfileSkill), Methods(nameof(ProfileSkill.Setup)));
			targets.Add(typeof(WatsonTcpClient), Methods(nameof(WatsonTcpClient.SendAsync)));
			
			targets.Add(typeof(MinigameManager), Methods(nameof(MinigameManager.Awake)));
			
			targets.Add(typeof(PlayerMarketApiManager),
				Methods(nameof(PlayerMarketApiManager.GetLatestComprehensivePrices),
					nameof(PlayerMarketApiManager.GetLatestPrices), nameof(PlayerMarketApiManager.GetPlayerShopAsync),
					nameof(PlayerMarketApiManager.GetPlayerTradeHistoryAsync)));

			targets.Add(typeof(CustomSteamManager),
				Methods(nameof(CustomSteamManager.Awake), nameof(CustomSteamManager.LinkSteamAccountToPlayFab),
					nameof(CustomSteamManager.OnLoggedIn), nameof(CustomSteamManager.OnPlayFabSteamLinkFailure), nameof(CustomSteamManager.Start),
					nameof(CustomSteamManager.GetSteamAuthTicketAsString)));
			targets.Add(typeof(GameManager), Methods(nameof(GameManager.Awake), nameof(GameManager.ApplySettings), nameof(GameManager.InitializeDeploymentInfo)));
			targets.Add(typeof(GameBackgroundManager), Methods(nameof(GameBackgroundManager.Awake), nameof(GameBackgroundManager.OnSceneChanged)));
			targets.Add(typeof(DebugToolsManager), Methods(nameof(DebugToolsManager.Setup)));
			
			targets.Add(typeof(ChatboxManager), Methods(nameof(ChatboxManager.Awake), nameof(ChatboxManager.InitializePublicChatHistory),
				nameof(ChatboxManager.Start), nameof(ChatboxManager.SetupHistoricChatMessage), nameof(ChatboxManager.SetupChannel), nameof(ChatboxManager.SetupChatbox),
				nameof(ChatboxManager.AddNotification), nameof(ChatboxManager.HideUnreadMessageNotification), nameof(ChatboxManager.SelectChannelByIndex),
				nameof(ChatboxManager.OpenChat), nameof(ChatboxManager.ClearAllChatMessagesByUser), nameof(ChatboxManager.ClearAllChatMessages),
				nameof(ChatboxManager.RefreshNotifications), nameof(ChatboxManager.OnDisconnectedFromChat), nameof(ChatboxManager.UpdateChatLocalizations),
				nameof(ChatboxManager.GetChannelEntry), nameof(ChatboxManager.RemoveChatChannel), nameof(ChatboxManager.RemovePrivateChat),
				nameof(ChatboxManager.OnReceivePrivateMessage), nameof(ChatboxManager.OnReceiveChatboxMessage), nameof(ChatboxManager.ActivateChannel),
				nameof(ChatboxManager.DeactivateChannel), nameof(ChatboxManager.SelectPrivateChatByUsername), nameof(ChatboxManager.AddChatboxContentFromChannel),
				nameof(ChatboxManager.InitializeGuildChatHistory)));
			
			targets.Add(typeof(Debug), Methods(nameof(Debug.Log)));
			targets.Add(typeof(ChatApiManager),
				Methods(nameof(ChatApiManager.GetPublicChatHistoryAsync), nameof(ChatApiManager.OnDestroy)));
			targets.Add(typeof(IdleClansAPIManager), Methods(nameof(IdleClansAPIManager.ConfigureSecurityAndCreateClient),
				nameof(IdleClansAPIManager.Instance)));
			
			targets.Add(typeof(InAppPurchaseManager), Methods(nameof(InAppPurchaseManager.Awake), nameof(InAppPurchaseManager.GetItemAmountToBeGivenForPurchase),
				nameof(InAppPurchaseManager.GetItemByProductId), nameof(InAppPurchaseManager.GetProductIdFromIAPItem), nameof(InAppPurchaseManager.GetProductPrice),
				nameof(InAppPurchaseManager.OnLoggedIn), nameof(InAppPurchaseManager.OnReceiptValidated), nameof(InAppPurchaseManager.ProcessGoogleReceipts),
				nameof(InAppPurchaseManager.ProcessIOSReceipts), nameof(InAppPurchaseManager.PurchaseItem), nameof(InAppPurchaseManager.TryLoadUnvalidatedGoogleReceipts),
				nameof(InAppPurchaseManager.TryLoadUnvalidatedIOSReceipts)));
			
			targets.Add(typeof(PlayerRealMoneyPurchases), Methods(nameof(PlayerRealMoneyPurchases.Awake), nameof(PlayerRealMoneyPurchases.Start)));
			
			targets.Add(typeof(InAppPurchasesSteam), Methods(nameof(InAppPurchasesSteam.OnGetCatalogSuccess)));
			targets.Add(typeof(PlayFabClientAPI), Methods(nameof(PlayFabClientAPI.GetCatalogItems)));
			
			foreach (var entry in targets) {
				var type = entry.Key;
				var methods = entry.Value;
				foreach (var method in methods) {
					IdleLog.Info($"Registering: {type}/{method} - {prefix}:{postfix}");
					
					if (method == "SendAsync") {
						harmony.Patch(type.GetMethod(method, new [] { typeof(string), typeof(Il2CppSystem.Collections.Generic.Dictionary<string, Il2CppSystem.Object>), typeof(CancellationToken) }), new HarmonyMethod(prefix), new HarmonyMethod(postfix));	
						continue;
					}

					if (method == "Log") {
						harmony.Patch(type.GetMethod(method, new [] { typeof(Object) }), new HarmonyMethod(prefix), new HarmonyMethod(postfix));	
						continue;
					}

					if (method == "Instance" && type == typeof(IdleClansAPIManager)) {
						harmony.Patch(type.GetProperty(method).GetGetMethod(), new HarmonyMethod(prefix), new HarmonyMethod(postfix));	
						continue;
					}

					harmony.Patch(type.GetMethod(method), new HarmonyMethod(prefix), new HarmonyMethod(postfix));
				}
			}
		}
		
		public static void Prefix(out Stopwatch __state) {
			__state = new Stopwatch();
			__state.Start();
		}

		public static void Postfix(Stopwatch __state, MethodInfo __originalMethod) {
			__state.Stop();
			var second = DebugAwake.Watch?.ElapsedMilliseconds ?? -1;
			IdleLog.Info($"{__originalMethod.DeclaringType}#{__originalMethod.Name} - Took {__state.ElapsedMilliseconds}ms ({Time.frameCount} / {second}ms)");
		}
	}
}