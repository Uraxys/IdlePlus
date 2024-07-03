using IdlePlus.Settings;
using Newtonsoft.Json.Linq;

namespace IdlePlus.IdleClansAPI {
	public class MarketEntry {
		
		public readonly int AveragePrice;
		public readonly int LowestSellPrice;
		public readonly long LowestSellVolume;
		public readonly int HighestBuyPrice;
		public readonly long HighestBuyVolume;

		public MarketEntry(JObject data) {
			AveragePrice = (int)data["dailyAveragePrice"];
			LowestSellPrice = (int)data["lowestSellPrice"];
			LowestSellVolume = (long)data["lowestPriceVolume"];
			HighestBuyPrice = (int)data["highestBuyPrice"];
			HighestBuyVolume = (long)data["highestPriceVolume"];
		}

		public int GetSellBuyPrice() {
			if (LowestSellPrice > 0) return LowestSellPrice;
			if (HighestBuyPrice > 0) return HighestBuyPrice;
			return -1;
		}
		
		public int GetBuySellPrice() {
			if (HighestBuyPrice > 0) return HighestBuyPrice;
			if (LowestSellPrice > 0) return LowestSellPrice;
			return -1;
		}
		
		public int GetSellPrice() {
			if (LowestSellPrice <= 0) return -1;
			return LowestSellPrice;
		}

		public int GetBuyPrice() {
			if (HighestBuyPrice <= 0) return -1;
			return HighestBuyPrice;
		}

		public int GetAveragePrice() {
			if (AveragePrice <= 0) return -1;
			return AveragePrice;
		}
		
		// Get price depending on setting.
		public int GetPriceDependingOnSetting() {
			switch (ModSettings.MarketValue.DisplayType.Value) {
				case 0: return GetSellBuyPrice();
				case 1: return GetBuySellPrice();
				case 2: return GetSellPrice();
				case 3: return GetBuyPrice();
				case 4: return GetAveragePrice();
				default: return -1;
			}
		}
	}
}