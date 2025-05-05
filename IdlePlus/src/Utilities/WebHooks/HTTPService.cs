using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdlePlus.Settings;

namespace IdlePlus.Utilities {
	/// <summary>
	/// Handles HTTP requests for webhooks with support for timeouts, retries, and error handling.
	/// </summary>
	public class HttpService {
		private static readonly HttpClient _client;
		private static readonly HttpService _instance = new HttpService();
		private static readonly object _lockObject = new object();
		private static bool _isInitialized = false;

		/// <summary>
		/// Gets the singleton instance of the HttpService.
		/// </summary>
		public static HttpService Instance => _instance;

		// Static constructor to initialize the HttpClient
		static HttpService() {
			try {
				_client = new HttpClient {
					Timeout = TimeSpan.FromSeconds(10)
				};

				// Add default headers if needed
				// _client.DefaultRequestHeaders.Add("User-Agent", "IdlePlus Webhook Client");
			} catch (Exception ex) {
				IdleLog.Error($"[HttpService] Error initializing HttpClient: {ex.Message}");
				throw; // Rethrow to prevent silent failures in static initialization
			}
		}

		private HttpService() {
			// Private constructor to enforce singleton pattern
		}

		/// <summary>
		/// Initializes the HTTP service with settings from ModSettings.
		/// </summary>
		public void Initialize() {
			if (_isInitialized)
				return;

			lock (_lockObject) {
				if (_isInitialized)
					return;

				try {
					// Any initialization code that depends on game settings
					// For example, setting up default headers based on game settings

					_isInitialized = true;
					IdleLog.Debug("[HttpService] Initialized successfully");
				} catch (Exception ex) {
					IdleLog.Error($"[HttpService] Initialization error: {ex.Message}");
				}
			}
		}

		/// <summary>
		/// Sends an HTTP request to the specified URL with the given method and content.
		/// </summary>
		/// <param name="url">The URL to send the request to.</param>
		/// <param name="method">The HTTP method to use (GET, POST, etc.).</param>
		/// <param name="jsonContent">The JSON content to include in the request body (for POST requests).</param>
		/// <param name="timeoutSeconds">Timeout in seconds for this specific request.</param>
		/// <returns>True if the request was successful; otherwise, false.</returns>
		public async Task<bool> SendRequestAsync(string url, string method, string jsonContent, int timeoutSeconds = 10) {
			if (!_isInitialized) {
				Initialize();
			}

			if (string.IsNullOrEmpty(url)) {
				IdleLog.Error("[HttpService] URL cannot be null or empty");
				return false;
			}

			try {
				using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds))) {
					HttpMethod httpMethod = GetHttpMethod(method);
					var request = new HttpRequestMessage(httpMethod, url);

					AddAuthorizationHeader(request);

					if (jsonContent != null && (httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put)) {
						request.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
					}

					IdleLog.Debug($"[HttpService] Sending {httpMethod} request to {url}");
					var response = await _client.SendAsync(request, cts.Token);

					// Log response details
					IdleLog.Info($"[HttpService] Response Status: {response.StatusCode} for {url}");

					if (!response.IsSuccessStatusCode) {
						string responseContent = await response.Content.ReadAsStringAsync();
						IdleLog.Error($"[HttpService] Request failed with status {response.StatusCode}. Response: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}");
					}

					return response.IsSuccessStatusCode;
				}
			} catch (TaskCanceledException) {
				IdleLog.Error($"[HttpService] Request to {url} timed out");
				return false;
			} catch (HttpRequestException ex) {
				if (ex.InnerException is WebException webEx &&
					webEx.Status == WebExceptionStatus.NameResolutionFailure) {
					IdleLog.Error($"[HttpService] DNS resolution failed for {url}. Check network connection and URL");
				} else {
					IdleLog.Error($"[HttpService] HTTP request error: {ex.Message}");
				}
				return false;
			} catch (Exception ex) {
				IdleLog.Error($"[HttpService] Error sending {method} request to {url}: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Returns the HttpMethod corresponding to the given request method string.
		/// </summary>
		/// <param name="requestMethod">The HTTP method as a string.</param>
		/// <returns>The corresponding HttpMethod object.</returns>
		private HttpMethod GetHttpMethod(string requestMethod) {
			if (string.IsNullOrEmpty(requestMethod)) {
				IdleLog.Info("[HttpService] Request method is null or empty, defaulting to GET");
				return HttpMethod.Get;
			}

			switch (requestMethod.ToUpperInvariant()) {
				case "GET":
					return HttpMethod.Get;
				case "POST":
					return HttpMethod.Post;
				case "PUT":
					return HttpMethod.Put;
				case "DELETE":
					return HttpMethod.Delete;
				case "HEAD":
					return HttpMethod.Head;
				default:
					// For less common methods
					return new HttpMethod(requestMethod);
			}
		}

		/// <summary>
		/// Adds the Authorization header to the HTTP request if a token is provided.
		/// </summary>
		/// <param name="request">The HTTP request to add the header to.</param>
		private void AddAuthorizationHeader(HttpRequestMessage request) {
			try {
				string token = ModSettings.Hooks.BackendHookBarrer.Value;
				if (!string.IsNullOrEmpty(token)) {
					request.Headers.TryAddWithoutValidation("Authorization", token);
				}
			} catch (Exception ex) {
				IdleLog.Error($"[HttpService] Error adding authorization header: {ex.Message}");
			}
		}

		/// <summary>
		/// Builds the full URL from the base URL and the path with parameters.
		/// </summary>
		/// <param name="baseUrl">The base URL.</param>
		/// <param name="urlPath">The path with parameters.</param>
		/// <returns>The complete URL.</returns>
		public string BuildFullUrl(string baseUrl, string urlPath) {
			if (string.IsNullOrEmpty(baseUrl)) {
				IdleLog.Error("[HttpService] Base URL cannot be null or empty");
				return urlPath; // Return at least the path if we have it
			}

			if (string.IsNullOrEmpty(urlPath)) {
				return baseUrl; // Return just the base URL if path is empty
			}

			// Ensure base URL doesn't end with a slash and urlPath doesn't start with one
			return $"{baseUrl.TrimEnd('/')}/{urlPath.TrimStart('/')}";
		}
	}
}