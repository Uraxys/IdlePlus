using System.Collections.Generic;

namespace IdlePlus.Utilities
{
    // Enum defining the available webhook types.
    public enum WebhookType
    {
        Minigame
    }

    // Provides webhook configuration based on the webhook type.
    public static class WebhookConfigProvider
    {
        private static readonly Dictionary<WebhookType, WebhookConfig> _configs = new Dictionary<WebhookType, WebhookConfig>
        {
            {
                WebhookType.Minigame, new WebhookConfig {
                    RequestMethod = "GET",
                    UrlPath = "/minigame/{action}/{type}",
                    SettingsName = "Minigames (Clan Events)"
                }
            }
            
        };

        public static WebhookConfig GetConfig(WebhookType type)
        {
            return _configs.TryGetValue(type, out var config) ? config : null;
        }
    }

    // Configuration class for each webhook.
    public class WebhookConfig
    {
        /// <summary>
        /// The HTTP request method (e.g., "GET" or "POST").
        /// </summary>
        public string RequestMethod { get; set; }
        
        /// <summary>
        /// The URL path template with placeholders (e.g., "/minigame/{action}/{type}").
        /// </summary>
        public string UrlPath { get; set; }
        
        /// <summary>
        /// The display name for the webhook as shown in the settings.
        /// </summary>
        public string SettingsName { get; set; }
    }
}
