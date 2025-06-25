using System;
using System.IO;
using Brigadier.NET;
using Brigadier.NET.Context;
using Databases;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Player;
using Client;
using IdlePlus.API;
using IdlePlus.API.Popup;
using IdlePlus.API.Popup.Popups;
using Lobby;
using Login;
using Popups;
using Tasks;
using MulticastDelegate = Il2CppSystem.MulticastDelegate;

namespace IdlePlus.Command.Commands {
	internal static class DevelopmentCommand {

		private static int Count = 0;
		
		internal static void Register(CommandDispatcher<CommandSender> registry) {
			var command = Literal.Of("dev");

			command.Then(Literal.Of("export")
				.Then(Literal.Of("items").Executes(HandleExportItems)));
				//.Then(Literal.Of("tasks").Executes(HandleExportTasks)));

			command.Then(Literal.Of("debug")
				.Executes(HandleDebug));
				
			command.Then(Literal.Of("print")
				.Then(Argument.Of("message", Arguments.GreedyString())
					.Executes(HandlePrint)));
			
			command.Then(Literal.Of("say")
				.Then(Argument.Of("message", Arguments.GreedyString())
					.Executes(HandleSay)));

			// Webhook Commands with more descriptive names
			var webhookCommand = Literal.Of("webhook");

			// Run all predefined tests
			webhookCommand.Then(Literal.Of("run-test")
				.Executes(HandleWebhookRunTests));

			// Start repeating test runner
			webhookCommand.Then(Literal.Of("run-test-repeater")
				.Executes(context => HandleWebhookStartTestRepeater(context, 5))
				.Then(Argument.Of("seconds-interval", Arguments.Integer(1, 60))
					.Executes(context => HandleWebhookStartTestRepeater(context, context.GetArgument<int>("seconds-interval")))));

			// Stop repeating test runner
			webhookCommand.Then(Literal.Of("stop-test-repeater").Executes(HandleWebhookStopTestRepeater));

			// Display status information
			webhookCommand.Then(Literal.Of("status-test-repeater").Executes(HandleWebhookStatus));

			// Metrics and statistics commands
			webhookCommand.Then(Literal.Of("show-metrics").Executes(HandleWebhookShowMetrics));
			webhookCommand.Then(Literal.Of("reset-metrics").Executes(HandleWebhookResetMetrics));

			// Add the webhook command to the main command
			command.Then(webhookCommand);

			registry.Register(command);
		}
		
		/*
		 * Export
		 */

		private static void HandleExportItems(CommandContext<CommandSender> context) {
			var path = Path.Combine(BepInEx.Paths.PluginPath, "IdlePlus", "export");
			Directory.CreateDirectory(path);

			var root = new JObject();
			root.Add("exported_at", new JValue($"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.FFFZ}"));
			root.Add("version", new JObject(
				new JProperty("config", SettingsDatabase.SharedSettings.ConfigVersion),
				new JProperty("latest_build", SettingsDatabase.SharedSettings.LatestBuildVersion),
				new JProperty("required_build", SettingsDatabase.SharedSettings.RequiredBuildVersion))
			);
			root.Add("items", new JArray().Do(arr => {
				foreach (var item in ItemDatabase.ItemList._values) {
					arr.Add(item.ToJson());
				}
			}));

			var json = root.ToString(Formatting.Indented);
			File.WriteAllText(Path.Combine(path, "items.json"), json);
			
			context.Source.SendMessage("Exported item data to 'IdlePlus/export/items.json'.");
		}

		private static void HandleExportTasks(CommandContext<CommandSender> context) {
			if (true) return;
			var path = Path.Combine(BepInEx.Paths.PluginPath, "IdlePlus", "export");
			Directory.CreateDirectory(path);

			var root = new JObject();
			root.Add("exported_at", new JValue($"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.FFFZ}"));
			root.Add("version", new JObject(
				new JProperty("config", SettingsDatabase.SharedSettings.ConfigVersion),
				new JProperty("latest_build", SettingsDatabase.SharedSettings.LatestBuildVersion),
				new JProperty("required_build", SettingsDatabase.SharedSettings.RequiredBuildVersion))
			);
			
			root.Add("tasks", new JArray().Do(arr => {
				foreach (var entry in TaskDatabase.Tasks) {
					var type = entry.key;
				}
			}));
		}

		private static void HandleDebug(CommandContext<CommandSender> context) {

			IdleAPI.Player.GetPlayerProfile("iC0mpl3xN4m3IIiilIII").AcceptSync(profile => {
				IdleLog.Info("Profile: " + profile);
			});
			
			if (true) return;
			
			Count = 0;
			
			PrintDelegates("LobbyManager.OnMenuChanged", LobbyManager.OnMenuChanged);
			PrintDelegates("AuthenticationLogic.OnAccountRecoveryFailed", AuthenticationLogic.OnAccountRecoveryFailed);
			PrintDelegates("AuthenticationLogic.OnLoggedIn", AuthenticationLogic.OnLoggedIn);
			PrintDelegates("AuthenticationLogic.OnLoginFailed", AuthenticationLogic.OnLoginFailed);
			PrintDelegates("AuthenticationLogic.OnLoginSuccessful", AuthenticationLogic.OnLoginSuccessful);
			PrintDelegates("AuthenticationLogic.OnRegistrationFailed", AuthenticationLogic.OnRegistrationFailed);
			PrintDelegates("BaseHardPopup.OnClosePopup", BaseHardPopup.OnClosePopup);
			PrintDelegates("BaseHardPopup.OnOpenPopup", BaseHardPopup.OnOpenPopup);
            IdleLog.Info("---");
			PrintDelegates("OnNetworkMessageReceived", NetworkClient.OnNetworkMessageReceived);
			PrintDelegates("OnDisconnectedFromServer", NetworkClient.OnDisconnectedFromServer);
			PrintDelegates("OnExceptionReceived", NetworkClient.OnExceptionReceived);
			PrintDelegates("OnLoginDataReceived", NetworkClient.OnLoginDataReceived);
			PrintDelegates("OnGameModeSelectionMessageReceived", NetworkClient.OnGameModeSelectionMessageReceived);
			PrintDelegates("OnBeginRestoringConfigMatch", NetworkClient.OnBeginRestoringConfigMatch);
			PrintDelegates("OnConfigVersionMatchRestored", NetworkClient.OnConfigVersionMatchRestored);
			PrintDelegates("OnTaskCompleteMessageReceived", NetworkClient.OnTaskCompleteMessageReceived);
			PrintDelegates("OnPlunderTaskCompleteMessageReceived", NetworkClient.OnPlunderTaskCompleteMessageReceived);
			PrintDelegates("OnPlunderTaskFailMessageReceived", NetworkClient.OnPlunderTaskFailMessageReceived);
			PrintDelegates("OnStartCombat", NetworkClient.OnStartCombat);
			PrintDelegates("OnPlayerHitsEnemy", NetworkClient.OnPlayerHitsEnemy);
			PrintDelegates("OnEnemyHitsPlayer", NetworkClient.OnEnemyHitsPlayer);
			PrintDelegates("OnGroupMemberConsumedFood", NetworkClient.OnGroupMemberConsumedFood);
			PrintDelegates("OnPlayerJoinedOurGroup", NetworkClient.OnPlayerJoinedOurGroup);
			PrintDelegates("OnJoinedExistingGroup", NetworkClient.OnJoinedExistingGroup);
			PrintDelegates("OnEndCombat", NetworkClient.OnEndCombat);
			PrintDelegates("OnPlayerLeftLobby", NetworkClient.OnPlayerLeftLobby);
			PrintDelegates("OnPlayerIsReadyForCombat", NetworkClient.OnPlayerIsReadyForCombat);
			PrintDelegates("OnGuildCreated", NetworkClient.OnGuildCreated);
			PrintDelegates("OnPlayerJoinedGuild", NetworkClient.OnPlayerJoinedGuild);
			PrintDelegates("OnGuildApplicationReceived", NetworkClient.OnGuildApplicationReceived);
			PrintDelegates("OnGuildInviteReceived", NetworkClient.OnGuildInviteReceived);
			PrintDelegates("OnGuildMemberLoggedIn", NetworkClient.OnGuildMemberLoggedIn);
			PrintDelegates("OnGuildMemberLoggedOut", NetworkClient.OnGuildMemberLoggedOut);
			PrintDelegates("OnRemoveGuildApplication", NetworkClient.OnRemoveGuildApplication);
			PrintDelegates("OnRemoveGuildInvitation", NetworkClient.OnRemoveGuildInvitation);
			PrintDelegates("OnPlayerLeftGuild", NetworkClient.OnPlayerLeftGuild);
			PrintDelegates("OnGuildDeleted", NetworkClient.OnGuildDeleted);
			PrintDelegates("OnGuildMemberKicked", NetworkClient.OnGuildMemberKicked);
			PrintDelegates("OnGuildMemberPromoted", NetworkClient.OnGuildMemberPromoted);
			PrintDelegates("OnGuildMemberDemoted", NetworkClient.OnGuildMemberDemoted);
			PrintDelegates("OnGuildMemberProfileReceived", NetworkClient.OnGuildMemberProfileReceived);
			PrintDelegates("OnGuildStateReceived", NetworkClient.OnGuildStateReceived);
			PrintDelegates("OnItemSentToGuildVault", NetworkClient.OnItemSentToGuildVault);
			PrintDelegates("OnItemsSentToGuildVault", NetworkClient.OnItemsSentToGuildVault);
			PrintDelegates("OnItemWithdrawnFromGuildVault", NetworkClient.OnItemWithdrawnFromGuildVault);
			PrintDelegates("OnMemberVaultAccessUpdated", NetworkClient.OnMemberVaultAccessUpdated);
			PrintDelegates("OnGuildHousePurchased", NetworkClient.OnGuildHousePurchased);
			PrintDelegates("OnDailyQuestUpdated", NetworkClient.OnDailyQuestUpdated);
			PrintDelegates("OnDailySkillingQuestProgressed", NetworkClient.OnDailySkillingQuestProgressed);
			PrintDelegates("OnDailyCombatQuestProgressed", NetworkClient.OnDailyCombatQuestProgressed);
			PrintDelegates("OnGuildMemberPremiumUpdated", NetworkClient.OnGuildMemberPremiumUpdated);
			PrintDelegates("OnPlayerSentEmoji", NetworkClient.OnPlayerSentEmoji);
			PrintDelegates("OnGroupMemberStatsUpdated", NetworkClient.OnGroupMemberStatsUpdated);
			PrintDelegates("OnGroupMemberEquipmentChanged", NetworkClient.OnGroupMemberEquipmentChanged);
			PrintDelegates("OnReceiveCombatGroupInvitation", NetworkClient.OnReceiveCombatGroupInvitation);
			PrintDelegates("OnGroupJoinResponseReceived", NetworkClient.OnGroupJoinResponseReceived);
			PrintDelegates("OnRealMoneyPurchaseReceiptValidated", NetworkClient.OnRealMoneyPurchaseReceiptValidated);
			PrintDelegates("OnPlayerShopItemPurchaseResponseReceived", NetworkClient.OnPlayerShopItemPurchaseResponseReceived);
			PrintDelegates("OnOurPlayerShopItemGotPurchased", NetworkClient.OnOurPlayerShopItemGotPurchased);
			PrintDelegates("OnServerUpdateMessageReceived", NetworkClient.OnServerUpdateMessageReceived);
			PrintDelegates("OnServerUpdateCancelMessageReceived", NetworkClient.OnServerUpdateCancelMessageReceived);
			PrintDelegates("OnPlayerJoinedMinigame", NetworkClient.OnPlayerJoinedMinigame);
			PrintDelegates("OnAdBoostReceived", NetworkClient.OnAdBoostReceived);
			PrintDelegates("OnLeaderLeftGuild", NetworkClient.OnLeaderLeftGuild);
			PrintDelegates("OnGuildPurchasedUpgrade", NetworkClient.OnGuildPurchasedUpgrade);
			PrintDelegates("OnClanIAPItemPurchased", NetworkClient.OnClanIAPItemPurchased);
			PrintDelegates("OnItemSpecialEffectsActivated", NetworkClient.OnItemSpecialEffectsActivated);
			PrintDelegates("OnItemSpecialEffectsActivatedGroup", NetworkClient.OnItemSpecialEffectsActivatedGroup);
			PrintDelegates("OnGuildActionResponseReceived", NetworkClient.OnGuildActionResponseReceived);
			PrintDelegates("OnGuildApplicationsCleared", NetworkClient.OnGuildApplicationsCleared);
			PrintDelegates("OnGroupCombatMemberDied", NetworkClient.OnGroupCombatMemberDied);
			PrintDelegates("OnDisplayNameChangeReceived", NetworkClient.OnDisplayNameChangeReceived);
			PrintDelegates("OnPlayerLeftCombatGroup", NetworkClient.OnPlayerLeftCombatGroup);
			PrintDelegates("OnPlayerShopItemAdded", NetworkClient.OnPlayerShopItemAdded);
			PrintDelegates("OnPlayerShopItemRemoved", NetworkClient.OnPlayerShopItemRemoved);
			PrintDelegates("OnPlayerAddedOrRemovedFromWhiteList", NetworkClient.OnPlayerAddedOrRemovedFromWhiteList);
			PrintDelegates("OnRaidLobbyCreated", NetworkClient.OnRaidLobbyCreated);
			PrintDelegates("OnRaidInvitationSent", NetworkClient.OnRaidInvitationSent);
			PrintDelegates("OnPlayerInvitedToRaids", NetworkClient.OnPlayerInvitedToRaids);
			PrintDelegates("OnRemotePlayerJoinedRaid", NetworkClient.OnRemotePlayerJoinedRaid);
			PrintDelegates("OnPlayerLeftRaidsLobby", NetworkClient.OnPlayerLeftRaidsLobby);
			PrintDelegates("RaidsOnPlayerReadyStatusChanged", NetworkClient.RaidsOnPlayerReadyStatusChanged);
			PrintDelegates("OnRaidPhaseStarted", NetworkClient.OnRaidPhaseStarted);
			PrintDelegates("OnRaidStarted", NetworkClient.OnRaidStarted);
			PrintDelegates("OnLocalPlayerJoinedRaid", NetworkClient.OnLocalPlayerJoinedRaid);
			PrintDelegates("OnRaidPlayerEquippedItem", NetworkClient.OnRaidPlayerEquippedItem);
			PrintDelegates("OnRaidPlayerUnEquippedItem", NetworkClient.OnRaidPlayerUnEquippedItem);
			PrintDelegates("OnRaidAddLootToVault", NetworkClient.OnRaidAddLootToVault);
			PrintDelegates("OnRaidPlayerCompletedTask", NetworkClient.OnRaidPlayerCompletedTask);
			PrintDelegates("OnRaidPlayerCompletedTaskWithCosts", NetworkClient.OnRaidPlayerCompletedTaskWithCosts);
			PrintDelegates("OnRaidPlayerStartedPrepActivity", NetworkClient.OnRaidPlayerStartedPrepActivity);
			PrintDelegates("OnRaidPlayerEndedPrepActivity", NetworkClient.OnRaidPlayerEndedPrepActivity);
			PrintDelegates("OnRaidPlayerFailedToStartTask", NetworkClient.OnRaidPlayerFailedToStartTask);
			PrintDelegates("OnRaidPlayerResourcesForTaskRanOut", NetworkClient.OnRaidPlayerResourcesForTaskRanOut);
			PrintDelegates("OnRaidAddAndRemoveItemFromVault", NetworkClient.OnRaidAddAndRemoveItemFromVault);
			//PrintDelegates("OnRaidPartyDefeated", NetworkClient.OnRaidPartyDefeated);
			PrintDelegates("OnRaidCombatStarted", NetworkClient.OnRaidCombatStarted);
			PrintDelegates("OnRaidEnemyAttackedPlayers", NetworkClient.OnRaidEnemyAttackedPlayers);
			PrintDelegates("OnRaidPlayerAttackedEnemy", NetworkClient.OnRaidPlayerAttackedEnemy);
			PrintDelegates("OnRaidPlayerRefinedWeapon", NetworkClient.OnRaidPlayerRefinedWeapon);
			PrintDelegates("OnRaidEnded", NetworkClient.OnRaidEnded);
			PrintDelegates("OnAttemptedToAcceptExpiredRaidInvitation", NetworkClient.OnAttemptedToAcceptExpiredRaidInvitation);
			PrintDelegates("OnPvmStatsReceived", NetworkClient.OnPvmStatsReceived);
			PrintDelegates("OnGuildLeadershipChanged", NetworkClient.OnGuildLeadershipChanged);
			PrintDelegates("OnJoinedClanEventMessageReceived", NetworkClient.OnJoinedClanEventMessageReceived);
			PrintDelegates("OnTaskStartedMessageReceived", NetworkClient.OnTaskStartedMessageReceived);
			PrintDelegates("OnHolidayEventStatusMessageReceived", NetworkClient.OnHolidayEventStatusMessageReceived);
			PrintDelegates("OnDonationConfirmationMessageReceived", NetworkClient.OnDonationConfirmationMessageReceived);
			PrintDelegates("OnTopHolidayDonorsStatusMessageReceived", NetworkClient.OnTopHolidayDonorsStatusMessageReceived);
			PrintDelegates("OnClaimedHolidayExperience", NetworkClient.OnClaimedHolidayExperience);
			PrintDelegates("OnLoadoutMessageReceived", NetworkClient.OnLoadoutMessageReceived);
			PrintDelegates("OnLoadoutSaveSuccessful", NetworkClient.OnLoadoutSaveSuccessful);
			PrintDelegates("OnTaskLoadoutsReceived", NetworkClient.OnTaskLoadoutsReceived);
			IdleLog.Info("Count: " + Count);
		}
		
		private static void PrintDelegates(string name, MulticastDelegate obj) {
			try {
				var count = obj?.delegates?.Length;
				if (count == null || count == 0) IdleLog.Error($"{name} / {count}");
				else {
					Count += count.Value;
					IdleLog.Warn($"{name} / {count}");
				}
			} catch (Exception e) {
				IdleLog.Error(e);
			}
		}
		
		/*
		 * Say
		 */

		private static void HandlePrint(CommandContext<CommandSender> context) {
			var sender = context.Source;
			var message = context.GetArgument<string>("message");
			sender.SendMessage(message);
		}
		
		private static void HandleSay(CommandContext<CommandSender> context) {
			var sender = context.Source;
			var message = context.GetArgument<string>("message");
			message = $"[00:00:00] [TAG] {PlayerData.Instance.Username}: {message}";
			sender.SendMessage(message, mode: GameMode.Default, premium: true, moderator: true);
		}

		/*
         * Webhook Command Handlers
         */

		/// <summary>
		/// Runs all predefined webhook tests
		/// </summary>
		private static int HandleWebhookRunTests(CommandContext<CommandSender> context) {
			try {
				IdleLog.Info("[DevCommand] Running all predefined webhook tests...");
				WebhookTests.RunTests();
				IdleLog.Info("[DevCommand] All webhook tests have been queued.");
				return 1;
			} catch (Exception ex) {
				IdleLog.Error($"[DevCommand] Error running webhook tests: {ex.Message}");
				return 0;
			}
		}

		/// <summary>
		/// Starts a repeating test sequence that runs at regular intervals
		/// </summary>
		private static int HandleWebhookStartTestRepeater(CommandContext<CommandSender> context, int intervalSeconds) {
			try {
				bool started = WebhookTests.StartTestRepeater(intervalSeconds);

				if (started) {
					IdleLog.Info($"[DevCommand] Started webhook test repeater. Running tests every {intervalSeconds} seconds.");
				} else {
					IdleLog.Info("[DevCommand] Test repeater is already running. Stop it first if you want to restart with different settings.");
				}
				return 1;
			} catch (Exception ex) {
				IdleLog.Error($"[DevCommand] Error starting webhook test repeater: {ex.Message}");
				return 0;
			}
		}

		/// <summary>
		/// Stops the repeating test sequence
		/// </summary>
		private static int HandleWebhookStopTestRepeater(CommandContext<CommandSender> context) {
			try {
				bool stopped = WebhookTests.StopTestRepeater();

				if (stopped) {
					IdleLog.Info("[DevCommand] Webhook test repeater has been stopped.");
				} else {
					IdleLog.Info("[DevCommand] No test repeater is currently running.");
				}
				return 1;
			} catch (Exception ex) {
				IdleLog.Error($"[DevCommand] Error stopping webhook test repeater: {ex.Message}");
				return 0;
			}
		}

		/// <summary>
		/// Shows the current status of the webhook system
		/// </summary>
		private static int HandleWebhookStatus(CommandContext<CommandSender> context) {
			try {
				bool isRunning = WebhookTests.IsRepeaterRunning();
				int queuedCount = WebhookManager.GetQueuedRequestCount();

				var statusMessage = new System.Text.StringBuilder();
				statusMessage.AppendLine("Webhook System Status:");

				if (isRunning) {
					int interval = WebhookTests.GetRepeaterInterval();
					statusMessage.AppendLine($"- Automatic testing: Running (every {interval} seconds)");
				} else {
					statusMessage.AppendLine("- Automatic testing: Not running");
				}

				statusMessage.AppendLine($"- Pending requests: {queuedCount}");

				IdleLog.Info($"[DevCommand] Webhook status:\n{statusMessage}");
				return 1;
			} catch (Exception ex) {
				IdleLog.Error($"[DevCommand] Error checking webhook status: {ex.Message}");
				return 0;
			}
		}

		/// <summary>
		/// Shows metrics and statistics about webhook performance
		/// </summary>
		private static int HandleWebhookShowMetrics(CommandContext<CommandSender> context) {
			try {
				string report = WebhookMetrics.GetReport();
				IdleLog.Info($"[DevCommand] Webhook metrics:\n{report}");
				return 1;
			} catch (Exception ex) {
				IdleLog.Error($"[DevCommand] Error getting webhook metrics: {ex.Message}");
				return 0;
			}
		}

		/// <summary>
		/// Resets all webhook performance metrics
		/// </summary>
		private static int HandleWebhookResetMetrics(CommandContext<CommandSender> context) {
			try {
				WebhookMetrics.Reset();
				IdleLog.Info("[DevCommand] All webhook performance metrics have been reset to zero.");
				return 1;
			} catch (Exception ex) {
				IdleLog.Error($"[DevCommand] Error resetting webhook metrics: {ex.Message}");
				return 0;
			}
		}
	}
}