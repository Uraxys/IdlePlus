using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdlePlus.Settings;
using Player;
using Guilds;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace IdlePlus.Utilities {
	/// <summary>
	/// Manages the creation and processing of webhook requests.
	/// </summary>
	public static class WebhookManager {
		private static readonly WebhookQueue<WebhookRequest> _webhookQueue = new WebhookRequestQueue();
		private static readonly int MAX_RETRIES = 3;

		// Methods that can have a request body
		private static readonly HashSet<string> MethodsWithBody = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"POST", "PUT"
		};

		/// <summary>
		/// Adds a webhook request to the processing queue.
		/// </summary>
		/// <param name="webhookType">The type of webhook to send.</param>
		/// <param name="pathParams">Path parameters to replace in the URL template.</param>
		/// <param name="jsonRequestData">Optional JSON data for the request body (for POST, PUT methods).</param>
		/// <exception cref="ArgumentException">Thrown when invalid JSON data is provided.</exception>
		/// <remarks>
		/// This method validates settings, prepares the request and enqueues it for asynchronous processing.
		/// Path parameters can replace placeholders in the URL template, e.g., {action} will be replaced with the value of the "action" key.
		/// </remarks>
		public static void AddSendWebhook(WebhookType webhookType, Dictionary<string, string> pathParams, string jsonRequestData = null) {
			if (!TryGetEnabledConfig(webhookType, out var config)) {
				IdleLog.Debug($"[WebhookManager] Webhook type {webhookType} is not enabled or not configured.");
				return;
			}

			try {
				string requestMethod = config.RequestMethod.ToUpperInvariant();
				bool supportsBody = MethodsWithBody.Contains(requestMethod);

				// For methods without body support, jsonRequestData should be null
				if (!supportsBody && !string.IsNullOrEmpty(jsonRequestData)) {
					jsonRequestData = null;
				}

				// Validate JSON format if data was provided
				if (!string.IsNullOrEmpty(jsonRequestData) && !JsonHelper.IsValidJson(jsonRequestData)) {
					IdleLog.Error($"[WebhookManager] Invalid JSON format for {webhookType}: {jsonRequestData}");
					return; // Abort if JSON is invalid
				}

				// Collect metadata for path replacement and body if needed
				var metadata = CollectMetadata();

				// Process URL path with metadata replacements
				string processedPath = ReplacePlaceholders(config.UrlPath, pathParams, metadata);

				// Enrich with metadata if the method supports a body
				string finalJsonData = supportsBody ? EnrichWithMetadata(jsonRequestData, pathParams, metadata) : null;

				var request = new WebhookRequest {
					WebhookType = webhookType,
					PathParams = pathParams,
					JsonRequestData = finalJsonData,
					ProcessedUrlPath = processedPath,
					RetryCount = 0
				};

				_webhookQueue.Enqueue(request);
				IdleLog.Debug($"[WebhookManager] Request for {webhookType} added to queue with method {requestMethod}.");
			} catch (Exception ex) {
				IdleLog.Error($"[WebhookManager] Error preparing webhook {webhookType}: {ex.Message}");
			}
		}

		/// <summary>
		/// Adds a webhook request with an Il2CppSystem.Object payload to the processing queue.
		/// </summary>
		/// <param name="webhookType">The type of webhook to send.</param>
		/// <param name="pathParams">Path parameters to replace in the URL template.</param>
		/// <param name="il2cppRequestData">An Il2CppSystem.Object to be serialized to JSON.</param>
		/// <exception cref="ArgumentException">Thrown when the object cannot be serialized to valid JSON.</exception>
		/// <remarks>
		/// This method converts the Il2Cpp object to JSON and then calls AddSendWebhook with the JSON data.
		/// </remarks>
		public static void AddSendWebhook(WebhookType webhookType, Dictionary<string, string> pathParams, Il2CppSystem.Object il2cppRequestData) {
			try {
				string json = JsonHelper.Serialize(il2cppRequestData);
				if (json != null) {
					AddSendWebhook(webhookType, pathParams, json);
				}
			} catch (Exception ex) {
				IdleLog.Error($"[WebhookManager] Error preparing webhook {webhookType}: {ex.Message}");
			}
		}

		/// <summary>
		/// Collects metadata about the current game state.
		/// </summary>
		/// <returns>A dictionary containing metadata about the player, game mode, clan, etc.</returns>
		private static Dictionary<string, string> CollectMetadata() {
			var metadata = new Dictionary<string, string>();

			// Player information - using null conditional and null coalescing operators
			metadata["playerName"] = PlayerData.Instance?.Username ?? "Unknown";
			metadata["gameMode"] = PlayerData.Instance?.GameMode.ToString() ?? "Unknown";

			// Clan information - chain of null conditional operators
			metadata["clanName"] = GuildManager.Instance?.OurGuild?.Name ?? "None";

			// Unix Timestamp (seconds since 1/1/1970)
			var unixTimestamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			metadata["timestamp"] = unixTimestamp.ToString();

			// Client version
			metadata["clientVersion"] = IdlePlus.ModVersion;

			return metadata;
		}

		/// <summary>
		/// Enriches the provided data with metadata.
		/// </summary>
		/// <param name="jsonData">The original JSON data string.</param>
		/// <param name="pathParams">Path parameters used in the request.</param>
		/// <param name="metadata">Collected metadata to add to the JSON.</param>
		/// <returns>A new JSON string with the added metadata.</returns>
		private static string EnrichWithMetadata(string jsonData, Dictionary<string, string> pathParams, Dictionary<string, string> metadata) {
			// Parse existing JSON data or create new object
			JObject payload = !string.IsNullOrEmpty(jsonData) ? JObject.Parse(jsonData) : new JObject();

			// Add metadata
			var metadataObj = new JObject();
			foreach (var item in metadata) {
				metadataObj[item.Key] = new JValue(item.Value);
			}
			payload["metadata"] = metadataObj;

			// Add path parameters if not already present
			if (payload["params"] == null && pathParams?.Count > 0) {
				var paramsObj = new JObject();
				foreach (var param in pathParams) {
					paramsObj[param.Key] = new JValue(param.Value);
				}
				payload["params"] = paramsObj;
			}

			return payload.ToString(Formatting.None);
		}

		/// <summary>
		/// Replaces placeholders in the URL path with values from pathParams and metadata.
		/// </summary>
		/// <param name="urlPath">The URL path template with placeholders.</param>
		/// <param name="pathParams">The path parameters to replace placeholders.</param>
		/// <param name="metadata">Additional metadata that can be used for replacements.</param>
		/// <returns>The processed URL path with placeholders replaced.</returns>
		private static string ReplacePlaceholders(string urlPath, Dictionary<string, string> pathParams, Dictionary<string, string> metadata) {
			string result = urlPath;

			// Create a merged dictionary with pathParams taking precedence over metadata
			var replacements = new Dictionary<string, string>(metadata);

			// Add path parameters (will overwrite any metadata with the same key)
			if (pathParams != null) {
				foreach (var param in pathParams) {
					replacements[param.Key] = param.Value;
				}
			}

			// Replace all placeholders in one pass
			foreach (var replacement in replacements) {
				result = result.Replace($"{{{replacement.Key}}}", Uri.EscapeDataString(replacement.Value));
			}

			return result;
		}

		/// <summary>
		/// Internal queue handler for webhook requests.
		/// </summary>
		private class WebhookRequestQueue : WebhookQueue<WebhookRequest> {
			protected override async Task ProcessItemAsync(WebhookRequest request) {
				await ProcessWebhookRequest(request);
			}
		}

		/// <summary>
		/// Processes a webhook request, handling retries and error cases.
		/// </summary>
		/// <param name="request">The webhook request to process.</param>
		private static async Task ProcessWebhookRequest(WebhookRequest request) {
			var stopwatch = Stopwatch.StartNew();
			WebhookMetrics.RegisterStart(request.WebhookType);
			bool success = false;

			try {
				var config = WebhookConfigProvider.GetConfig(request.WebhookType);
				if (config == null || !IsWebhookEnabled(request.WebhookType)) {
					IdleLog.Error($"[WebhookManager] Configuration for {request.WebhookType} not found or disabled.");
					return;
				}

				string baseUrl = ModSettings.Hooks.BackendHookServer.Value;
				string fullUrl = HttpService.Instance.BuildFullUrl(baseUrl, request.ProcessedUrlPath);
				string requestMethod = config.RequestMethod;

				success = await HttpService.Instance.SendRequestAsync(
					fullUrl,
					requestMethod,
					request.JsonRequestData
				);

				if (!success && request.RetryCount < MAX_RETRIES) {
					// Retry logic
					request.RetryCount++;
					IdleLog.Error($"[WebhookManager] Retry {request.RetryCount}/{MAX_RETRIES} for {request.WebhookType}");

					// Wait before retrying (exponential backoff)
					int delayMs = 500 * (int)Math.Pow(2, request.RetryCount);
					await Task.Delay(delayMs);

					// Re-queue the request
					_webhookQueue.Enqueue(request);
				} else if (!success) {
					IdleLog.Error($"[WebhookManager] Maximum retries reached for {request.WebhookType}");
				} else {
					IdleLog.Info($"[WebhookManager] Successfully processed webhook {request.WebhookType}");
				}
			} catch (Exception ex) {
				IdleLog.Error($"[WebhookManager] Error processing webhook: {ex.Message}");
			} finally {
				stopwatch.Stop();
				WebhookMetrics.RegisterCompletion(request.WebhookType, success, stopwatch.ElapsedMilliseconds, request.RetryCount);
			}
		}

		/// <summary>
		/// Checks if a webhook type is enabled in the settings.
		/// </summary>
		/// <param name="webhookType">The webhook type to check.</param>
		/// <returns>True if the webhook is enabled; otherwise, false.</returns>
		private static bool IsWebhookEnabled(WebhookType webhookType) {
			return ModSettings.Hooks.WebhookToggles.TryGetValue(webhookType, out var toggle) && toggle.Value;
		}

		/// <summary>
		/// Tries to get the configuration for an enabled webhook type.
		/// </summary>
		/// <param name="webhookType">The webhook type to get configuration for.</param>
		/// <param name="config">When this method returns, contains the webhook configuration if found; otherwise, null.</param>
		/// <returns>True if the webhook type is enabled and has valid configuration; otherwise, false.</returns>
		private static bool TryGetEnabledConfig(WebhookType webhookType, out WebhookConfig config) {
			config = WebhookConfigProvider.GetConfig(webhookType);
			return config != null && IsWebhookEnabled(webhookType);
		}

		/// <summary>
		/// Data class for webhook requests.
		/// </summary>
		public class WebhookRequest {
			/// <summary>The type of webhook to send.</summary>
			public WebhookType WebhookType;

			/// <summary>Path parameters for URL template substitution.</summary>
			public Dictionary<string, string> PathParams;

			/// <summary>JSON data for the request body.</summary>
			public string JsonRequestData;

			/// <summary>The processed URL path with placeholders replaced.</summary>
			public string ProcessedUrlPath;

			/// <summary>Current retry count for this request.</summary>
			public int RetryCount;
		}

		/// <summary>
		/// Gets the number of webhook requests currently in the queue.
		/// </summary>
		/// <returns>The number of queued requests.</returns>
		public static int GetQueuedRequestCount() {
			return _webhookQueue.Count;
		}

		/// <summary>
		/// Checks if a webhook type is enabled in the settings.
		/// </summary>
		/// <param name="webhookType">The webhook type to check.</param>
		/// <returns>True if the webhook is enabled; otherwise, false.</returns>
		public static bool IsWebhookTypeEnabled(WebhookType webhookType) {
			return ModSettings.Hooks.WebhookToggles.TryGetValue(webhookType, out var toggle) && toggle.Value;
		}
	}

	/// <summary>
	/// Provides performance metrics for webhook processing.
	/// </summary>
	public static class WebhookMetrics {
		private static readonly object _lockObject = new object();
		private static int _totalWebhooksSent = 0;
		private static int _totalWebhooksSucceeded = 0;
		private static int _totalWebhooksFailed = 0;
		private static readonly Dictionary<WebhookType, int> _countByType = new Dictionary<WebhookType, int>();
		private static readonly Dictionary<WebhookType, long> _totalProcessingTimeByType = new Dictionary<WebhookType, long>();
		private static readonly Dictionary<WebhookType, int> _retryCountByType = new Dictionary<WebhookType, int>();

		/// <summary>
		/// Registers the start of a webhook processing operation.
		/// </summary>
		/// <param name="type">The webhook type being processed.</param>
		public static void RegisterStart(WebhookType type) {
			lock (_lockObject) {
				_totalWebhooksSent++;

				if (!_countByType.ContainsKey(type))
					_countByType[type] = 0;
				_countByType[type]++;
			}
		}

		/// <summary>
		/// Registers the completion of a webhook processing operation.
		/// </summary>
		/// <param name="type">The webhook type that was processed.</param>
		/// <param name="success">Whether the processing was successful.</param>
		/// <param name="processingTimeMs">The processing time in milliseconds.</param>
		/// <param name="retryCount">The number of retries that were needed.</param>
		public static void RegisterCompletion(WebhookType type, bool success, long processingTimeMs, int retryCount) {
			lock (_lockObject) {
				if (success)
					_totalWebhooksSucceeded++;
				else
					_totalWebhooksFailed++;

				if (!_totalProcessingTimeByType.ContainsKey(type))
					_totalProcessingTimeByType[type] = 0;
				_totalProcessingTimeByType[type] += processingTimeMs;

				if (!_retryCountByType.ContainsKey(type))
					_retryCountByType[type] = 0;
				_retryCountByType[type] += retryCount;
			}
		}

		/// <summary>
		/// Gets a report with performance metrics.
		/// </summary>
		/// <returns>A string containing the performance report.</returns>
		public static string GetReport() {
			lock (_lockObject) {
				var report = new System.Text.StringBuilder();
				report.AppendLine("=== Webhook Performance Report ===");
				report.AppendLine($"Total Webhooks: {_totalWebhooksSent}");
				report.AppendLine($"Success Rate: {(_totalWebhooksSent > 0 ? (_totalWebhooksSucceeded * 100.0 / _totalWebhooksSent).ToString("F2") : "0.00")}%");
				report.AppendLine();
				report.AppendLine("By Type:");

				foreach (var type in _countByType.Keys) {
					int count = _countByType[type];
					long totalTime = _totalProcessingTimeByType.ContainsKey(type) ? _totalProcessingTimeByType[type] : 0;
					int retries = _retryCountByType.ContainsKey(type) ? _retryCountByType[type] : 0;

					report.AppendLine($"  {type}:");
					report.AppendLine($"    Count: {count}");
					report.AppendLine($"    Avg Time: {(count > 0 ? (totalTime / count).ToString() : "0")}ms");
					report.AppendLine($"    Retries: {retries}");
				}

				return report.ToString();
			}
		}

		/// <summary>
		/// Resets all statistics.
		/// </summary>
		public static void Reset() {
			lock (_lockObject) {
				_totalWebhooksSent = 0;
				_totalWebhooksSucceeded = 0;
				_totalWebhooksFailed = 0;
				_countByType.Clear();
				_totalProcessingTimeByType.Clear();
				_retryCountByType.Clear();
			}
		}
	}
}