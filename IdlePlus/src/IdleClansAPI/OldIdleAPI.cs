using System;
using System.Collections.Generic;
using Databases;
using IdleClansApi;
using IdlePlus.Utilities;
using Il2CppSystem.Net;
using Il2CppSystem.Net.Http;
using Il2CppSystem.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdlePlus.IdleClansAPI {
	
	// TODO: Refactor, I want a handler for every API endpoint, easy to use and understand.
	public static class OldIdleAPI {
		
		private const string MarketPricesUrl = "https://query.idleclans.com/api/PlayerMarket/items/prices/latest?includeAveragePrice=true";
		private static bool _initialized;

		public static Dictionary<int, MarketEntry> MarketPrices { get; private set; } = new Dictionary<int, MarketEntry>();
		public static Action<bool> OnMarketPricesFetched;
		
		public static MarketEntry GetMarketEntry(Item item) {
			return MarketPrices.TryGetValue(item.ItemId, out var price) ? price : null;
		}
		
		public static bool IsInitialized() {
			return _initialized;
		}
		
		public static void UpdateMarketPrices() {
#pragma warning disable CS0162 // Unreachable code detected
			if (IdlePlus.PerformanceTest) return;
#pragma warning restore CS0162 // Unreachable code detected
			
			var start = DateTime.Now.Ticks;
			IdleClansAPIManager.HttpClient.GetAsync(MarketPricesUrl)
				.ContinueWith((Action<Task>) delegate(Task t1) {
					var webTask = new Task<HttpResponseMessage>(t1.Pointer);
					if (webTask.ExceptionRecorded) {
						IdleLog.Error("Failed to fetch item prices.", webTask.Exception);
						return;
					}
					
					var result = webTask.Result;
					if (result.StatusCode != HttpStatusCode.OK) {
						IdleLog.Error($"Failed to fetch item prices, status code: {result.StatusCode}");
						return;
					}

					// Read the fetched data.
					result.Content.ReadAsStringAsync().ContinueWith((Action<Task>)delegate(Task t2) {
						var readTask = new Task<string>(t2.Pointer);
						if (readTask.ExceptionRecorded) {
							IdleLog.Error("Failed to read item prices.", readTask.Exception);
							return;
						}
						
						var end = DateTime.Now.Ticks;
						var data = readTask.Result;
						
						// Parse the fetched data.
						try {
							var prices = new Dictionary<int, MarketEntry>();
							var array = new JArray(JsonConvert.DeserializeObject(data).Pointer);

							for (var i = 0; i < array.Count; i++) {
								var item = new JObject(array[i].Pointer);
								var itemId = (int)item["itemId"];
								prices[itemId] = new MarketEntry(item);
							}
							
							IdleLog.Info($"Updated market prices in {(end - start) / 10_000.0}ms\n" +
							                 $"Prices: old={MarketPrices.Count} new={prices.Count}");
							MarketPrices = prices;
							
							var first = !_initialized;
							_initialized = true;
							OnMarketPricesFetched?.Invoke(first);
						} catch (Exception e) {
							IdleLog.Error("Failed to update market prices!", e);
						}
					});
				});
		}
	}
}