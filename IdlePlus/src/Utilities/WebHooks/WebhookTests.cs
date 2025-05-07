using System.Collections.Generic;
using System;

namespace IdlePlus.Utilities {
	/// <summary>
	/// Contains test cases for webhook functionality with support for repeated execution.
	/// </summary>
	public static class WebhookTests {
		private static object _runningRepeater = null;
		private static readonly object _repeaterLock = new object();

		/// <summary>
		/// Runs test cases for sending webhook requests once.
		/// </summary>
		public static void RunTests() {
			IdleLog.Info("[WebhookTests] Starting webhook tests...");

			// Test 1: Standard Minigame start event
			IdleLog.Debug("[WebhookTests] Test 1: Minigame start event");
			WebhookManager.AddSendWebhook(
				WebhookType.Minigame,
				new Dictionary<string, string>
				{
					{ "action", "start" },
					{ "type", "Gathering" }
				}
			);

			// Test 2: Minigame stop event
			IdleLog.Debug("[WebhookTests] Test 2: Minigame stop event");
			WebhookManager.AddSendWebhook(
				WebhookType.Minigame,
				new Dictionary<string, string>
				{
					{ "action", "stop" },
					{ "type", "Gathering" }
				}
			);

			// Test 3: Minigame start with JSON data
			IdleLog.Debug("[WebhookTests] Test 3: Minigame start with JSON data");
			string eventJson = "{\"difficulty\":\"hard\",\"duration\":180}";
			WebhookManager.AddSendWebhook(
				WebhookType.Minigame,
				new Dictionary<string, string>
				{
					{ "action", "start" },
					{ "type", "Gathering" }
				},
				eventJson
			);

			// Log statistics
			IdleLog.Info("[WebhookTests] All webhook tests queued.");
		}

		/// <summary>
		/// Runs a single test with the specified parameters.
		/// </summary>
		/// <param name="type">The webhook type to test.</param>
		/// <param name="pathParams">The path parameters for the request.</param>
		/// <param name="jsonData">Optional JSON data for the request body.</param>
		public static void RunSingleTest(WebhookType type, Dictionary<string, string> pathParams, string jsonData = null) {
			WebhookManager.AddSendWebhook(
				type,
				pathParams,
				jsonData
			);

			IdleLog.Info("[WebhookTests] Single test queued.");
		}

		/// <summary>
		/// Starts a repeating task that runs webhook tests at the specified interval.
		/// </summary>
		/// <param name="intervalSeconds">The interval in seconds between test runs.</param>
		/// <returns>True if the repeater was started; false if it's already running.</returns>
		public static bool StartTestRepeater(int intervalSeconds = 5) {
			lock (_repeaterLock) {
				if (_runningRepeater != null) {
					IdleLog.Info($"[WebhookTests] Test repeater is already running");
					return false;
				}

				IdleLog.Info($"[WebhookTests] Starting test repeater with {intervalSeconds}s interval");

				_runningRepeater = IdleTasks.Repeat(0, intervalSeconds, task => {
					RunTests();
					IdleLog.Info("[WebhookTests] Test run complete.");
				});

				return true;
			}
		}

		/// <summary>
		/// Stops the currently running test repeater if it exists.
		/// </summary>
		/// <returns>True if a repeater was stopped; false if none was running.</returns>
		public static bool StopTestRepeater() {
			lock (_repeaterLock) {
				if (_runningRepeater == null) {
					IdleLog.Info("[WebhookTests] No test repeater is currently running");
					return false;
				}

				IdleLog.Info("[WebhookTests] Stopping test repeater");

				try {
					var idleTask = _runningRepeater as IdleTasks.IdleTask;
					idleTask.Cancel();
					IdleLog.Info("[WebhookTests] Stopped repeater using IdleTask.Cancel() method");

				} catch (Exception ex) {
					IdleLog.Error($"[WebhookTests] Error stopping repeater: {ex.Message}");
					IdleLog.Debug($"[WebhookTests] Repeater type: {_runningRepeater.GetType().FullName}");
				}

				_runningRepeater = null;
				return true;
			}
		}

		/// <summary>
		/// Checks if the test repeater is currently running.
		/// </summary>
		/// <returns>True if the repeater is running; otherwise, false.</returns>
		public static bool IsRepeaterRunning() {
			lock (_repeaterLock) {
				return _runningRepeater != null;
			}
		}

		/// <summary>
		/// Gets the current interval of the running repeater, or 0 if none is running.
		/// </summary>
		/// <returns>The interval in seconds, or 0 if no repeater is running.</returns>
		public static int GetRepeaterInterval() {
			lock (_repeaterLock) {
				if (_runningRepeater == null)
					return 0;

				try {
					// Use reflection to read the RepeatTime property
					var repeatTimeProperty = _runningRepeater.GetType().GetProperty("RepeatTime");
					if (repeatTimeProperty != null) {
						return (int)repeatTimeProperty.GetValue(_runningRepeater);
					}
				} catch (Exception ex) {
					IdleLog.Error($"[WebhookTests] Error getting repeater interval: {ex.Message}");
				}

				return 0;
			}
		}
	}
}