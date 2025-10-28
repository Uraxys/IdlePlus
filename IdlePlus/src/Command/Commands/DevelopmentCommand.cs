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
		
		internal static void Register(CommandDispatcher<CommandSender> registry) {
			var command = Literal.Of("dev");
				
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