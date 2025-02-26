using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ads;
using Assets.Scripts.Player;
using Buttons;
using ChatboxLogic;
using Client;
using Combat;
using DebugTools;
using Equipment;
using FTUE;
using Game.HolidayEvents;
using GameContent;
using Groups;
using Guilds;
using Guilds.ClanBossCanvas;
using Guilds.UI;
using HarmonyLib;
using IdleClansApi;
using IdlePlus.Unity;
using IdlePlus.Utilities;
using Il2CppSystem.Threading;
using InAppPurchasing;
using Leaderboards;
using Lobby;
using Main;
using Management;
using Minigames;
using Notifications;
using Platforms.Steam;
using Player;
using Player.Inventory;
using PlayerMarket;
using PlayFab;
using Popups;
using Profile;
using Raids;
using Scripts.Reviews;
using Shops;
using Sirenix.OdinInspector;
using Tasks;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Upgrades;
using WatsonTcp;
using WikiTools;
using Action = Il2CppSystem.Action;
using Debug = UnityEngine.Debug;
using Exception = Il2CppSystem.Exception;
using Object = Il2CppSystem.Object;

namespace IdlePlus.Patches {
	public static class PerformanceTestPatch {
		
		internal static List<T> Methods<T>(params T[] items) => new List<T>(items);
		
		internal static void Patch(Harmony harmony) {

			// Disable for now.
			//if (true) return;
			if (!IdlePlus.PerformanceTest) return;

			var prefix = typeof(PerformanceTestPatch).GetMethod("Prefix");
			var prefixStop = typeof(PerformanceTestPatch).GetMethod("PrefixStop");
			var postfix = typeof(PerformanceTestPatch).GetMethod("Postfix");
			
			Dictionary<Type, List<string>> targets = new Dictionary<Type, List<string>>();
			Dictionary<Type, List<string>> stopTargets = new Dictionary<Type, List<string>>();
			
			targets.Add(typeof(AdsManager), Methods(nameof(AdsManager.Awake)));
			targets.Add(typeof(GuildManager), Methods(nameof(GuildManager.Awake)));
			targets.Add(typeof(PlayerData), Methods(nameof(PlayerData.Awake), nameof(PlayerData.SetupGameData), nameof(PlayerData.Start)));
			targets.Add(typeof(PlayerInventory), Methods(nameof(PlayerInventory.Awake), nameof(PlayerInventory.Setup), nameof(PlayerInventory.Start)));
			targets.Add(typeof(PlayerEquipment), Methods(nameof(PlayerEquipment.Awake), nameof(PlayerEquipment.Start), nameof(PlayerEquipment.OnEnable)));
			targets.Add(typeof(LoginTracker), Methods(nameof(LoginTracker.Start), nameof(LoginTracker.OnUserLogin)));
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

			targets.Add(typeof(GenericPanelManager), Methods(nameof(GenericPanelManager.SetupRuntimeContent), nameof(GenericPanelManager.ResetRuntimeContent)));
			targets.Add(typeof(IAPItem), Methods(nameof(IAPItem.Start)));
			targets.Add(typeof(GuildEventEntrySkillingParty), Methods(nameof(GuildEventEntrySkillingParty.Awake), nameof(GuildEventEntrySkillingParty.InitializeUI),
				nameof(GuildEventEntrySkillingParty.Refresh), nameof(GuildEventEntrySkillingParty.SetupStateFromServer)));
			targets.Add(typeof(GuildListener), Methods(nameof(GuildListener.Awake), nameof(GuildListener.SetupClanView),
				nameof(GuildListener.LoadGuildData), nameof(GuildListener.OnLoginDataReceived)));
			GuildListener.OnLoginDataProcessed += (Action)delegate {
				var second = DebugAwake.Watch?.ElapsedMilliseconds ?? -1;
				IdleLog.Info($"GuildListener#OnLoginDataProcessed - Done ({Time.frameCount} / {second}ms)");
			};
			
			targets.Add(typeof(NetworkClient), Methods(nameof(NetworkClient.MessageReceived), nameof(NetworkClient.HandleReceivedMessage),
				nameof(NetworkClient.DeserializeMessage)));
			targets.Add(typeof(ChatboxMessageSerialization), Methods(nameof(ChatboxMessageSerialization.Start)));
			targets.Add(typeof(NotificationsManager), Methods(nameof(NotificationsManager.Awake), nameof(NotificationsManager.ProcessOfflineProgressNotifications)));
			targets.Add(typeof(HolidayEventGameManager), Methods(nameof(HolidayEventGameManager.Awake)));
			targets.Add(typeof(ReviewPromptingManager), Methods(nameof(ReviewPromptingManager.OnEnable)));
			targets.Add(typeof(GroupApplicationManager), Methods(nameof(GroupApplicationManager.Awake)));
			targets.Add(typeof(FTUEManager), Methods(nameof(FTUEManager.Start), nameof(FTUEManager.Awake)));
			targets.Add(typeof(NetworkClientMobileConnectionChecker), Methods(nameof(NetworkClientMobileConnectionChecker.AttemptToReachServerAsync)));
			targets.Add(typeof(RaidManager), Methods(nameof(RaidManager.OnEnable)));
			targets.Add(typeof(RaidInvitationManager), Methods(nameof(RaidInvitationManager.OnEnable), nameof(RaidInvitationManager.Awake)));
			targets.Add(typeof(RaidsLobbyManager), Methods(nameof(RaidsLobbyManager.OnEnable), nameof(RaidsLobbyManager.Awake)));
			targets.Add(typeof(RaidsPrepManager), Methods(nameof(RaidsPrepManager.OnEnable)));
			targets.Add(typeof(RaidsBattleManager), Methods(nameof(RaidsBattleManager.OnEnable)));
			targets.Add(typeof(RaidGuardiansOfTheCitadel), Methods(nameof(RaidGuardiansOfTheCitadel.OnEnable)));
			targets.Add(typeof(RaidCitadelBattleManager), Methods(nameof(RaidCitadelBattleManager.Awake)));
			targets.Add(typeof(MinigameManagerCombatEvents), Methods(nameof(MinigameManagerCombatEvents.Awake)));
			targets.Add(typeof(NetworkClientChatService), Methods(nameof(NetworkClientChatService.Awake)));



			var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var monoTypes = allAssemblies
				.Where(a => a.GetName().Name == "Assembly-CSharp")
				.SelectMany(a => a.GetTypes())
				.Where(t => t.IsClass && typeof(MonoBehaviour).IsAssignableFrom(t))
				.ToList();
			
			var patchableMethods = monoTypes
				.SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance |
				                              BindingFlags.Static | BindingFlags.Public))
				.Where(t => !t.ContainsGenericParameters)
				.Where(t => t.GetParameters().Length <= 3)
				.Where(t => !t.IsSpecialName)
				.Where(t => !t.IsConstructor)
				.ToList();

			var allowedNamespaces = new List<string> {
				"Client.", "ChatboxLogic.", "Game.", "Guilds.",
				"Management.", "Databases.", "FTUE.", "IdleClansApi.",
				"InAppPurchasing.", "DebugTools.", "Inventory.", "Main.",
				"Minigames.", "Network.", "Notifications.", "Platforms.",
				"Player."
			};
			
			IdleLog.Info($"Found {patchableMethods.Count} targets.");
			List<MethodInfo> infos = new List<MethodInfo>();
			foreach (var method in patchableMethods) {
				var type = method.DeclaringType;
				var name = method.Name;
				if (type == null) continue;
				if (type == typeof(UIBehaviour)) continue;
				if (type == typeof(UITask)) continue;
				if (type == typeof(OpenCombatLobbiesPopup)) continue;
				if (type == typeof(CombatFightPage)) continue;
				if (type == typeof(LayoutGroup)) continue;
				if (type == typeof(HoverableInformationText)) continue;
				if (type == typeof(Selectable)) continue;
				if (type == typeof(GUIColorExamplesComponent)) continue;
				if (type == typeof(MonoBehaviour)) continue;
				if (type == typeof(MinigameManagerCombatEvents)) continue;
				if (type == typeof(Object)) continue;
				if (type == typeof(SerializedMonoBehaviour)) continue;
				if (type == typeof(ModeSelectionMenu)) continue;
				if (type == typeof(LeaderboardTopListsPage)) continue;
				if (type == typeof(GuildEventPartyView)) continue;
				if (type == typeof(GuildEventsView)) continue;
				if (type == typeof(LoadoutDropdown)) continue;
				if (type == typeof(RemotePlayerEquipmentSlot)) continue;
				if (type == typeof(CombatGroupPlayerPanelEntry)) continue;
				if (type == typeof(CombatPlayerPanelEntry)) continue;
				if (type == typeof(TwoFactorAuthenticationPopup)) continue;
				if (type == typeof(SettingsPopupAudioTab)) continue;
				if (type == typeof(SettingsPopupBlocksTab)) continue;
				if (type == typeof(SettingsPopupInfoTab)) continue;
				if (type == typeof(SettingsPopupPopupsTab)) continue;
				if (type == typeof(WikiLookupEntry)) continue;
				
				if (type == typeof(ClanEventPlayerSkilling) && name == "GetGameObject") continue;
				if (type == typeof(ClanEventPlayerCombat) && name == "GetGameObject") continue;
				if (type == typeof(ClanEventPlayerCombatLoot) && name == "GetGameObject") continue;
				if (type == typeof(QuestsPopup) && name == "OnWeekliesPressed") continue;
				if (type == typeof(InventoryItem) && name == "OnDragItemEnded") continue;
				if (type == typeof(ClanBossFightManager) && name == "OnPlayerDied") continue;
				if (type == typeof(GuildManagementUI) && name == "OnGuildRecruitmentMessageUpdated") continue;
				if (type == typeof(ProfilePanel) && method.Name == "OnGooglePlayAchievementsButtonPressed") continue;

				var found = allowedNamespaces.Any(ns => type.FullName.StartsWith(ns));
				if (!found) continue;
				
				if (targets.ContainsKey(type)) {
					var entries = targets[type];
					if (entries.Contains(method.Name)) continue;
					//if (true) continue;
					entries.Add(method.Name);
					continue;
				}
				
				infos.Add(method);
			}

			var countAfter = infos.Count;
			IdleLog.Info($"Found {countAfter} after targets.");


			foreach (var e in infos) {
				harmony.Patch(e, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
				IdleLog.Info($"Doing {e.DeclaringType.FullName}#{e.Name}()");
			}
			

			// STOP
			stopTargets.Add(typeof(InAppPurchaseManager), Methods(nameof(InAppPurchaseManager.OnLoggedIn)));
			stopTargets.Add(typeof(LoginTracker), Methods(nameof(LoginTracker.Start)));
			stopTargets.Add(typeof(ChatboxManager), Methods(nameof(ChatboxManager.Start), nameof(ChatboxManager.InitializePublicChatHistory)));
			stopTargets.Add(typeof(ProfilePanel), Methods(nameof(ProfilePanel.SetupData)));
			stopTargets.Add(typeof(GuildListener), Methods(nameof(GuildListener.Awake), nameof(GuildListener.OnLoginDataReceived)));
			//stopTargets.Add(typeof(PlayerInventory), Methods(nameof(PlayerInventory.Setup)));
			
			foreach (var entry in targets) {
				var type = entry.Key;
				var methods = entry.Value;
				foreach (var method in methods) {
					IdleLog.Info($"{type.FullName}");
					IdleLog.Info($"Registering: {type}/{method} - {prefix}:{postfix}");

					try {
						if (method == "SendAsync") {
							harmony.Patch(
								type.GetMethod(method,
									new[] {
										typeof(string),
										typeof(Il2CppSystem.Collections.Generic.Dictionary<string,
											Il2CppSystem.Object>),
										typeof(CancellationToken)
									}), new HarmonyMethod(prefix), new HarmonyMethod(postfix));
							continue;
						}

						if (method == "Log") {
							harmony.Patch(type.GetMethod(method, new[] { typeof(Object) }), new HarmonyMethod(prefix),
								new HarmonyMethod(postfix));
							continue;
						}

						if (method == "Instance" && type == typeof(IdleClansAPIManager)) {
							harmony.Patch(type.GetProperty(method).GetGetMethod(), new HarmonyMethod(prefix),
								new HarmonyMethod(postfix));
							continue;
						}

						if (method == "LoginDataProcessed" && type == typeof(IdleClansAPIManager)) {
							harmony.Patch(type.GetProperty(method).GetGetMethod(), new HarmonyMethod(prefix),
								new HarmonyMethod(postfix));
							continue;
						}

						harmony.Patch(type.GetMethod(method), new HarmonyMethod(prefix), new HarmonyMethod(postfix));
					}
					catch (System.Exception e) {
						IdleLog.Error($"Error while registering {type.DeclaringType}", e);
					}
				}
			}

			foreach (var entry in stopTargets) {
				var type = entry.Key;
				var methods = entry.Value;
				foreach (var method in methods) {
					harmony.Patch(type.GetMethod(method), new HarmonyMethod(prefixStop));
				}
			}
		}

		public static bool PrefixStop(MethodInfo __originalMethod) {
			IdleLog.Warn($"Stopping {__originalMethod.DeclaringType}#{__originalMethod.Name}");
			IdleLog.Warn($"Stack: {new Exception()._stackTraceString}");
			return false;
		}
		public static void Prefix(out Stopwatch __state, MethodInfo __originalMethod) {
			__state = new Stopwatch();
			__state.Start();

			var second = DebugAwake.Watch?.ElapsedMilliseconds ?? -1;
			
			if (__originalMethod.DeclaringType == typeof(InAppPurchaseManager) &&
			    __originalMethod.Name == nameof(InAppPurchaseManager.OnLoggedIn)) {
				DebugAwake.StartFrame = Time.frameCount;
			}
		}

		public static void Postfix(Stopwatch __state, MethodInfo __originalMethod) {
			__state.Stop();
			var second = DebugAwake.Watch?.ElapsedMilliseconds ?? -1;
			if (second == 0) return;
			IdleLog.Info($"{__originalMethod.DeclaringType}#{__originalMethod.Name} - Took {__state.ElapsedMilliseconds}ms ({Time.frameCount} / {second}ms)");
		}
	}
}