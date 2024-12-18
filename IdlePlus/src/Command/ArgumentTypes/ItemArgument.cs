using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET;
using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;
using Databases;
using IdlePlus.Utilities.Collections;

namespace IdlePlus.Command.ArgumentTypes {
	public class ItemArgument : ArgumentType<Item> {
		
		private static readonly DynamicCommandExceptionType Exception = new DynamicCommandExceptionType(o => new LiteralMessage($"Unknown item {o}"));
		private static readonly IEnumerable<string> ExampleValues = new[] { "gold_bar", "coal_ore", "iron_ore" };
		
		private static Trie _sortedItems;
		private static Dictionary<string, Item> _itemMap;

		private readonly int _max;

		public static ItemArgument Of(int max = 1000) {
			return new ItemArgument(max);
		}
		
		private static void CacheSortItems() {
			if (_sortedItems != null) return;
			_sortedItems = new Trie(true);
			_itemMap = new Dictionary<string, Item>();
			foreach (var entry in ItemDatabase.ItemList) {
				_sortedItems.Insert(entry.Value.Name);
				_itemMap[entry.Value.Name] = entry.Value;
			}
		}

		private ItemArgument(int max) {
			this._max = max;
			CacheSortItems();
		}

		public override Item Parse(IStringReader reader) {
			var name = reader.ReadUnquotedString();
			var result = _sortedItems.ExactMatch(name);
			if (result != null) return _itemMap[result];
			throw Exception.CreateWithContext(reader, name);
		}

		public override Task<Suggestions> ListSuggestions<TSource>(CommandContext<TSource> context, SuggestionsBuilder builder) {
			return Task.Run(() => {
				var result = _sortedItems.Search(builder.RemainingLowerCase);
				foreach (var item in result) {
					builder.Suggest(item);
					if (builder.SuggestionCount >= _max) return builder.Build();
				}
				return builder.Build();
			});
		}

		public override IEnumerable<string> Examples => ExampleValues;
	}
}