using System.Collections.Generic;

namespace IdlePlus.Utilities {
	/// <summary>
	/// Enum defining the available webhook types.
	/// </summary>
	/// <remarks>
	/// Add new webhook types here. Each type should have its corresponding configuration
	/// in the WebhookConfigProvider._configs dictionary.
	/// </remarks>
	public enum WebhookType {
		/// <summary>Minigame/clan event webhooks.</summary>
		Minigame,
		ClanAction
		// When adding a new webhook type:
		// 1. Add it here as a new enum value
		// 2. Add its configuration to the _configs dictionary in WebhookConfigProvider
		// Toggle settings will be automatically created based on this enum
	}

	/// <summary>
	/// Provides configuration settings for each webhook type.
	/// </summary>
	public static class WebhookConfigProvider {
		// Dictionary holding configuration for each webhook type.
		// 
		// URL TEMPLATE PLACEHOLDERS:
		// You can use the following placeholders in your URL paths:
		// - Custom parameters from your code: {action}, {type}, etc.
		// - Built-in metadata parameters:
		//   {playerName} - Current player's username
		//   {gameMode} - Player's current game mode (Default, Ironman, etc.)
		//   {clanName} - Player's clan name, or "None" if not in a clan
		//   {timestamp} - Current Unix timestamp (seconds since 1970-01-01)
		//   {clientVersion} - Current mod version
		//
		// NOTES:
		// - For POST/PUT requests, all metadata will also be included in the request body
		//   under the "metadata" property.
		// - Path parameters passed to AddSendWebhook will be included in the "params" property.
		// - You can use any parameter name in your URL template, as long as you provide the
		//   corresponding value in the pathParams dictionary when calling AddSendWebhook.
		private static readonly Dictionary<WebhookType, WebhookConfig> _configs = new Dictionary<WebhookType, WebhookConfig>
		{
            // Minigame webhook configuration
            // Example usage: WebhookManager.AddSendWebhook(WebhookType.Minigame, 
            //                  new Dictionary<string, string> { { "action", "start" }, { "type", "fishing" } });
            {
				WebhookType.Minigame, new WebhookConfig {
					RequestMethod = "POST",
					UrlPath = "/minigame/{action}/{type}",
					SettingsName = "Minigames (Clan Events)"
				}
			},
			{
				WebhookType.ClanAction, new WebhookConfig {
					RequestMethod = "POST",
					UrlPath = "/clan",
					SettingsName = "Clan Actions (Application received, new skill ticket, etc)"
				}
			}
            
            // TEMPLATE FOR ADDING NEW WEBHOOK TYPES:
            //
            // {
            //     WebhookType.YourNewType, new WebhookConfig {
            //         RequestMethod = "POST", // or "GET", "PUT", etc.
            //         UrlPath = "/your-endpoint/{param1}/{param2}",
            //         SettingsName = "Human-Readable Name for Settings UI"
            //     }
            // }
        };

		/// <summary>
		/// Retrieves the configuration for the specified webhook type.
		/// </summary>
		/// <param name="type">The webhook type.</param>
		/// <returns>The corresponding <see cref="WebhookConfig"/> if found; otherwise, null.</returns>
		public static WebhookConfig GetConfig(WebhookType type) {
			return _configs.TryGetValue(type, out var config) ? config : null;
		}

		/// <summary>
		/// Validates a webhook configuration.
		/// </summary>
		/// <param name="type">The webhook type to validate.</param>
		/// <returns>True if the configuration is valid; otherwise, false.</returns>
		public static bool ValidateConfig(WebhookType type) {
			if (!_configs.TryGetValue(type, out var config))
				return false;

			// Check for required fields
			if (string.IsNullOrEmpty(config.RequestMethod) || string.IsNullOrEmpty(config.UrlPath))
				return false;

			// Additional validation could be added here

			return true;
		}

		/// <summary>
		/// Gets all available webhook types.
		/// </summary>
		/// <returns>An array of all webhook types.</returns>
		public static WebhookType[] GetAvailableTypes() {
			var types = new WebhookType[_configs.Count];
			_configs.Keys.CopyTo(types, 0);
			return types;
		}
	}

	/// <summary>
	/// Represents the configuration for a webhook.
	/// </summary>
	public class WebhookConfig {
		/// <summary>
		/// Gets or sets the HTTP request method (e.g., "GET" or "POST").
		/// </summary>
		/// <remarks>
		/// Common values: "GET", "POST", "PUT", "DELETE"
		/// Only POST and PUT methods support sending a request body.
		/// </remarks>
		public string RequestMethod { get; set; }

		/// <summary>
		/// Gets or sets the URL path template with placeholders (e.g., "/minigame/{action}/{type}").
		/// </summary>
		/// <remarks>
		/// Placeholders are enclosed in curly braces and will be replaced with values from:
		/// 1. The pathParams dictionary passed to AddSendWebhook
		/// 2. Automatically collected metadata like {playerName}, {timestamp}, etc.
		/// </remarks>
		public string UrlPath { get; set; }

		/// <summary>
		/// Gets or sets the display name for the webhook as shown in the settings.
		/// </summary>
		/// <remarks>
		/// This should be a human-readable name that describes the webhook's purpose.
		/// </remarks>
		public string SettingsName { get; set; }
	}
}