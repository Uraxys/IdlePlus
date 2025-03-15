using System;
// using System.Threading.Tasks;
using Il2CppSystem.Net.Http;
using IdlePlus.Settings;
using IdlePlus.Utilities;
using Newtonsoft.Json;

namespace IdlePlus.Patches.Minigame
{
    public static class WebhookHelper
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Sends a generic webhook request.
        /// </summary>
        /// <param name="webhookId">The ID of the webhook (e.g., "minigamestart", "minigamestop", etc.).</param>
        /// <param name="requestType">The HTTP request type (e.g., "GET", "POST").</param>
        /// <param name="endpoint">The endpoint, e.g., "/minigame/start/{minigameType}"</param>
        /// <param name="requestData">Optional request data (currently unused for GET requests).</param>
        #pragma warning disable CS1998 
        public static async void FireWebhookAsync(string webhookId, string requestType, string endpoint, object requestData = null)
        {
            // Check if the webhook is enabled in the settings
            if (!IsWebhookEnabled(webhookId))
            {
                return;
            }

            string baseUrl = ModSettings.Hooks.BackendHookServer.Value;
            string fullUrl = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

            IdleLog.Info($"[Backend] Sending {requestType} request to: {fullUrl}");

            // Determine the HTTP method
            HttpMethod method;
            if (requestType.Equals("GET", StringComparison.OrdinalIgnoreCase))
                method = HttpMethod.Get;
            else if (requestType.Equals("POST", StringComparison.OrdinalIgnoreCase))
                method = HttpMethod.Post;
            else
                method = new HttpMethod(requestType);

            var request = new HttpRequestMessage(method, fullUrl);

            // Add the authorization header if a token is provided
            string token = ModSettings.Hooks.BackendHookBarrer.Value;
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.TryAddWithoutValidation("Authorization", token);
            }

            // If requestData is provided and it's a POST, serialize it as JSON
            if (requestData != null && method == HttpMethod.Post)
            {
                string jsonData = JsonConvert.SerializeObject((Il2CppSystem.Object)requestData);
                request.Content = new StringContent(jsonData, Il2CppSystem.Text.Encoding.UTF8, "application/json");
            }

            try
            {
                // Send the request (fire-and-forget)
                _ = client.SendAsync(request);
            }
            catch (Exception ex)
            {
                IdleLog.Error($"[Backend] Error sending {requestType} request: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a webhook is enabled based on settings.
        /// Expects a boolean property named "{webhookId}Enabled" in ModSettings.Hooks.
        /// </summary>
        private static bool IsWebhookEnabled(string webhookId)
        {
            // Build the property name, e.g., "minigamestartEnabled"
            string propertyName = webhookId + "Enabled";

            var hooksType = typeof(ModSettings.Hooks);
            var prop = hooksType.GetProperty(propertyName);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                return (bool)prop.GetValue(null);
            }
            return false;
        }
    }
}
