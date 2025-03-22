using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdlePlus.Settings;

namespace IdlePlus.Utilities
{
    public static class WebhookManager
    {
        private static readonly WebhookQueue<WebhookRequest> _webhookQueue = new WebhookRequestQueue();
        private static readonly int MAX_RETRIES = 3;

        /// <summary>
        /// Enqueues a webhook request (with a JSON string payload) for background processing.
        /// </summary>
        public static void AddSendWebhook(WebhookType webhookType, Dictionary<string, string> pathParams, string jsonRequestData = null)
        {
            if (!TryGetEnabledConfig(webhookType, out var config))
            {
                IdleLog.Debug($"[Webhook] Webhook type {webhookType} is not enabled or not configured.");
                return;
            }

            var request = new WebhookRequest
            {
                WebhookType = webhookType,
                PathParams = pathParams,
                JsonRequestData = jsonRequestData,
                RetryCount = 0
            };

            _webhookQueue.Enqueue(request);
            IdleLog.Debug($"[Webhook] Request for {webhookType} added to queue.");
        }

        /// <summary>
        /// Enqueues a webhook request (with an Il2CppSystem.Object payload) for background processing.
        /// </summary>
        public static void AddSendWebhook(WebhookType webhookType, Dictionary<string, string> pathParams, Il2CppSystem.Object il2cppRequestData)
        {
            if (!TryGetEnabledConfig(webhookType, out var config))
            {
                IdleLog.Debug($"[Webhook] Webhook type {webhookType} is not enabled or not configured.");
                return;
            }

            try
            {
                var json = SerializeIl2CppRequestData(il2cppRequestData);
                if (json != null)
                {
                    var request = new WebhookRequest
                    {
                        WebhookType = webhookType,
                        PathParams = pathParams,
                        JsonRequestData = json,
                        RetryCount = 0
                    };

                    _webhookQueue.Enqueue(request);
                    IdleLog.Debug($"[Webhook] Request for {webhookType} added to queue.");
                }
            }
            catch (Exception ex)
            {
                IdleLog.Error($"[Webhook] Error preparing webhook {webhookType}: {ex.Message}");
            }
        }

        /// <summary>
        /// Implementation of the WebhookQueue that processes webhook requests.
        /// </summary>
        private class WebhookRequestQueue : WebhookQueue<WebhookRequest>
        {
            protected override async Task ProcessItemAsync(WebhookRequest request)
            {
                await ProcessWebhookRequest(request);
            }
        }

        /// <summary>
        /// Processes the webhook request with retry logic.
        /// </summary>
        private static async Task ProcessWebhookRequest(WebhookRequest request)
        {
            try
            {
                var config = WebhookConfigProvider.GetConfig(request.WebhookType);
                if (config == null || !IsWebhookEnabled(request.WebhookType))
                {
                    IdleLog.Error($"[Webhook] Configuration for {request.WebhookType} not found or disabled.");
                    return;
                }

                if (!IsValidJson(request.JsonRequestData) && request.JsonRequestData != null)
                {
                    IdleLog.Error($"[Webhook] Invalid JSON: {request.JsonRequestData}");
                    return;
                }

                string baseUrl = ModSettings.Hooks.BackendHookServer.Value;
                string urlPath = ReplacePlaceholders(config.UrlPath, request.PathParams);
                string fullUrl = HttpService.Instance.BuildFullUrl(baseUrl, urlPath);

                bool success = await HttpService.Instance.SendRequestAsync(
                    fullUrl, 
                    config.RequestMethod, 
                    request.JsonRequestData
                );

                if (!success && request.RetryCount < MAX_RETRIES)
                {
                    // Retry logic
                    request.RetryCount++;
                    IdleLog.Error($"[Webhook] Retry {request.RetryCount}/{MAX_RETRIES} for {request.WebhookType}");
                    
                    // Wait before retrying (exponential backoff)
                    int delayMs = 500 * (int)Math.Pow(2, request.RetryCount);
                    await Task.Delay(delayMs);
                    
                    // Re-queue the request
                    _webhookQueue.Enqueue(request);
                }
                else if (!success)
                {
                    IdleLog.Error($"[Webhook] Maximum retries reached for {request.WebhookType}");
                }
            }
            catch (Exception ex)
            {
                IdleLog.Error($"[Webhook] Error processing webhook: {ex.Message}");
            }
        }

        /// <summary>
        /// Replaces placeholders in the URL path with actual values.
        /// </summary>
        private static string ReplacePlaceholders(string urlPath, Dictionary<string, string> pathParams)
        {
            string result = urlPath;
            foreach (var param in pathParams)
            {
                result = result.Replace($"{{{param.Key}}}", param.Value);
            }
            return result;
        }

        /// <summary>
        /// Validates if the JSON data is properly formatted.
        /// </summary>
        private static bool IsValidJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return true; // Null is allowed for GET requests
                
            var trimmedJson = json.Trim();
            return (trimmedJson.StartsWith("{") && trimmedJson.EndsWith("}")) || 
                   (trimmedJson.StartsWith("[") && trimmedJson.EndsWith("]"));
        }

        /// <summary>
        /// Serializes an Il2CppSystem.Object into a JSON string.
        /// </summary>
        private static string SerializeIl2CppRequestData(Il2CppSystem.Object il2cppRequestData)
        {
            try
            {
                return JsonHelper.Serialize(il2cppRequestData);
            }
            catch (Exception ex)
            {
                IdleLog.Error($"[Webhook] Error serializing Il2Cpp object: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks whether the webhook is enabled based on the configured toggle.
        /// </summary>
        private static bool IsWebhookEnabled(WebhookType webhookType)
        {
            if (ModSettings.Hooks.WebhookToggles.TryGetValue(webhookType, out var toggle))
            {
                return toggle.Value;
            }
            return false;
        }

        /// <summary>
        /// Tries to get the configuration for the given webhook type and checks if it is enabled.
        /// </summary>
        private static bool TryGetEnabledConfig(WebhookType webhookType, out WebhookConfig config)
        {
            config = WebhookConfigProvider.GetConfig(webhookType);
            return config != null && IsWebhookEnabled(webhookType);
        }

        /// <summary>
        /// Represents a webhook request with retry information.
        /// </summary>
        public class WebhookRequest
        {
            public WebhookType WebhookType;
            public Dictionary<string, string> PathParams;
            public string JsonRequestData;
            public int RetryCount;
        }

        /// <summary>
        /// Gets the number of webhook requests currently in the processing queue.
        /// </summary>
        /// <returns>The number of pending webhook requests.</returns>
        public static int GetQueuedRequestCount()
        {
            return _webhookQueue.Count;
        }
    }
}