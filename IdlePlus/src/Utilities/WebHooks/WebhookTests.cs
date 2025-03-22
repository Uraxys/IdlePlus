using System.Collections.Generic;
using IdlePlus.Utilities;
using System;

namespace IdlePlus.Utilities
{
    /// <summary>
    /// Contains test cases for webhook functionality with support for repeated execution.
    /// </summary>
    public static class WebhookTests
    {
        private static object _runningRepeater = null;
        private static readonly object _repeaterLock = new object();

        /// <summary>
        /// Runs test cases for sending webhook requests once.
        /// </summary>
        public static void RunTests()
        {
            IdleLog.Info("[WebhookTests] Starting webhook tests...");

            // Test 1: Call with an Il2CppSystem-compatible JSON string.
            IdleLog.Debug("[WebhookTests] Test 1: Il2CppSystem-compatible JSON string");
            WebhookManager.AddSendWebhook(
                WebhookType.MarketData,
                new Dictionary<string, string>
                {
                    { "action", "testIl2cppString" }
                },
                "[{\"dummy\":true}]"
            );

            // Test 2: Call with null JSON data (should simply send a GET request).
            IdleLog.Debug("[WebhookTests] Test 2: Null JSON data");
            WebhookManager.AddSendWebhook(
                WebhookType.MarketData,
                new Dictionary<string, string>
                {
                    { "action", "testGet" }
                },
                null
            );

            // Test 3: Call with a valid JSON string.
            IdleLog.Debug("[WebhookTests] Test 3: Valid JSON string");
            string validJson = "{\"price\":123,\"currency\":\"USD\"}";
            WebhookManager.AddSendWebhook(
                WebhookType.MarketData,
                new Dictionary<string, string>
                {
                    { "action", "update" }
                },
                validJson
            );

            // Test 4: Call with an invalid JSON string (expected to fail).
            IdleLog.Debug("[WebhookTests] Test 4: Invalid JSON string (expected to fail)");
            WebhookManager.AddSendWebhook(
                WebhookType.MarketData,
                new Dictionary<string, string>
                {
                    { "action", "testInvalid" }
                },
                "asdf"
            );

            // Log statistics
            IdleLog.Info("[WebhookTests] All webhook tests queued.");
        }

        /// <summary>
        /// Runs a single test with the specified parameters.
        /// </summary>
        public static void RunSingleTest(WebhookType type, Dictionary<string, string> pathParams, string jsonData = null)
        {
            // IdleLog.Info($"[WebhookTests] Running single test: Type={type}, Params={string.Join(", ", pathParams.Select(kv => $"{kv.Key}={kv.Value}"))}");

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
        public static bool StartTestRepeater(int intervalSeconds = 5)
        {
            lock (_repeaterLock)
            {
                if (_runningRepeater != null)
                {
                    IdleLog.Info($"[WebhookTests] Test repeater is already running");
                    return false;
                }

                IdleLog.Info($"[WebhookTests] Starting test repeater with {intervalSeconds}s interval");

                _runningRepeater = IdleTasks.Repeat(0, intervalSeconds, task =>
                {
                    // Netzwerkprüfung entfernt - vereinfacht für bessere Kompatibilität
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
        public static bool StopTestRepeater()
        {
            lock (_repeaterLock)
            {
                if (_runningRepeater == null)
                {
                    IdleLog.Info("[WebhookTests] No test repeater is currently running");
                    return false;
                }

                IdleLog.Info("[WebhookTests] Stopping test repeater");

                try
                {
                    // Based on the IdleTasks implementation, we know the object is an IdleTask with Cancel method
                    var idleTask = _runningRepeater as IdleTasks.IdleTask;
                    if (idleTask != null)
                    {
                        idleTask.Cancel();
                        IdleLog.Info("[WebhookTests] Stopped repeater using IdleTask.Cancel() method");
                    }
                    else
                    {
                        // Fallback to reflection if the cast didn't work
                        var cancelMethod = _runningRepeater.GetType().GetMethod("Cancel");
                        if (cancelMethod != null)
                        {
                            cancelMethod.Invoke(_runningRepeater, null);
                            IdleLog.Info("[WebhookTests] Stopped repeater using reflection to call Cancel() method");
                        }
                        else
                        {
                            IdleLog.Info("[WebhookTests] Could not find a way to stop the repeater task");
                        }
                    }
                }
                catch (Exception ex)
                {
                    IdleLog.Error($"[WebhookTests] Error stopping repeater: {ex.Message}");
                    IdleLog.Debug($"[WebhookTests] Repeater type: {_runningRepeater.GetType().FullName}");
                }

                // Regardless of whether we could stop it properly, clear our reference
                _runningRepeater = null;
                return true;
            }
        }

        /// <summary>
        /// Checks if the test repeater is currently running.
        /// </summary>
        /// <returns>True if the repeater is running; otherwise, false.</returns>
        public static bool IsRepeaterRunning()
        {
            lock (_repeaterLock)
            {
                return _runningRepeater != null;
            }
        }

        /// <summary>
        /// Gets the current interval of the running repeater, or 0 if none is running.
        /// </summary>
        /// <returns>The interval in seconds, or 0 if no repeater is running.</returns>
        public static int GetRepeaterInterval()
        {
            lock (_repeaterLock)
            {
                if (_runningRepeater == null)
                    return 0;

                try
                {
                    // Verwenden Sie Reflection, um die RepeatTime-Eigenschaft zu lesen
                    var repeatTimeProperty = _runningRepeater.GetType().GetProperty("RepeatTime");
                    if (repeatTimeProperty != null)
                    {
                        return (int)repeatTimeProperty.GetValue(_runningRepeater);
                    }
                }
                catch (Exception ex)
                {
                    IdleLog.Error($"[WebhookTests] Error getting repeater interval: {ex.Message}");
                }

                return 0;
            }
        }
    }
}