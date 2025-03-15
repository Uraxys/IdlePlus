using System;
using System.Collections.Generic;
using Il2CppSystem.Net.Http;
using IdlePlus.Settings;

namespace IdlePlus.Utilities
{
    public static class WebhookManager
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Sends a generic webhook request.
        /// </summary>
        /// <param name="webhookType">The type of webhook to send.</param>
        /// <param name="pathParams">
        /// A dictionary with placeholder values for the URL path.
        /// For example, for a minigame start webhook, you could pass:
        /// { "action", "start" } and { "type", minigame.EventType }.
        /// </param>
        /// <param name="requestData">Optional request data (for POST requests). 
        /// This can be either a JSON string (used as-is) or an object that will be serialized to JSON.</param>
        #pragma warning disable CS1998
        public static async void FireWebhookAsync(WebhookType webhookType, Dictionary<string, string> pathParams = null, object requestData = null)
        {
            WebhookConfig config = WebhookConfigProvider.GetConfig(webhookType);
            if (config == null)
            {
                IdleLog.Info($"No configuration found for webhook type: {webhookType}");
                return;
            }

            if (!IsWebhookEnabled(webhookType))
            {
                return;
            }

            string fullUrl = BuildFullUrl(config, pathParams);
            IdleLog.Info($"[Webhook] Sending {config.RequestMethod} request to: {fullUrl}");

            HttpMethod method = GetHttpMethod(config.RequestMethod);
            var request = new HttpRequestMessage(method, fullUrl);

            AddAuthorizationHeader(request);

            if (requestData != null && method == HttpMethod.Post)
            {
                string jsonData = JsonSerializer.ToJsonString(requestData);
                request.Content = new StringContent(jsonData, Il2CppSystem.Text.Encoding.UTF8, "application/json");
            }

            try
            {
                var response = client.SendAsync(request).GetAwaiter().GetResult();
                IdleLog.Info($"Response status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                IdleLog.Error($"[Webhook] Error sending {config.RequestMethod} request: {ex.Message}");
            }
        }
        #pragma warning restore CS1998

        private static string BuildFullUrl(WebhookConfig config, Dictionary<string, string> pathParams)
        {
            string baseUrl = ModSettings.Hooks.BackendHookServer.Value;
            string urlPath = config.UrlPath;

            if (pathParams != null)
            {
                foreach (var param in pathParams)
                {
                    urlPath = urlPath.Replace($"{{{param.Key}}}", param.Value);
                }
            }

            return $"{baseUrl.TrimEnd('/')}/{urlPath.TrimStart('/')}";
        }

        private static HttpMethod GetHttpMethod(string requestMethod)
        {
            return requestMethod.Equals("GET", StringComparison.OrdinalIgnoreCase)
                ? HttpMethod.Get
                : requestMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)
                    ? HttpMethod.Post
                    : new HttpMethod(requestMethod);
        }

        private static void AddAuthorizationHeader(HttpRequestMessage request)
        {
            string token = ModSettings.Hooks.BackendHookBarrer.Value;
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.TryAddWithoutValidation("Authorization", token);
            }
        }

        /// <summary>
        /// Checks if a webhook is enabled based on the corresponding ToggleSetting.
        /// </summary>
        private static bool IsWebhookEnabled(WebhookType webhookType)
        {
            if (ModSettings.Hooks.WebhookToggles.TryGetValue(webhookType, out var toggle))
            {
                return toggle.Value;
            }
            return false;
        }
    }
}
