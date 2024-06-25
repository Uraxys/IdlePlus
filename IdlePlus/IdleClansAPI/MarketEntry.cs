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
		
		public int GetSellPrice() {
			if (LowestSellPrice <= 0) return -1;
			return LowestSellPrice;
		}

		public int GetBuyPrice() {
			if (HighestBuyPrice <= 0) return -1;
			return HighestBuyPrice;
		}
	}
}