using System;
using System.Collections.Generic;

namespace IdlePlus.Utilities.Collections {
	public class ExpiringDictionary<TKey, TValue> {
		
		private readonly Dictionary<TKey, CacheEntry> _items = new Dictionary<TKey, CacheEntry>();
		private readonly TimeSpan _expirationTime;

		public ExpiringDictionary(TimeSpan? expiration = null) {
			this._expirationTime = expiration ?? TimeSpan.FromMinutes(5);
		}

		public void Set(TKey key, TValue value) {
			this._items[key] = new CacheEntry {
				Value = value,
				Expiration = DateTime.UtcNow.Add(this._expirationTime)
			};
		}

		public bool TryGet(TKey key, out TValue value) {
			if (this._items.TryGetValue(key, out var item)) {
				if (item.Expiration > DateTime.UtcNow) {
					value = item.Value;
					return true;
				}
				this._items.Remove(key);
			}

			value = default;
			return false;
		}
		
		private class CacheEntry {
			public TValue Value { get; set; }
			public DateTime Expiration { get; set; }
		}
	}
}