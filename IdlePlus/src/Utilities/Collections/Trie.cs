using System.Collections.Generic;

namespace IdlePlus.Utilities.Collections {
	public class Trie {
		
		private readonly TrieNode _root = new TrieNode();
		private readonly bool _sorted;

		public Trie(bool sorted = false) {
			this._sorted = sorted;
		}

		public void Insert(string entry) {
			var current = this._root;
			foreach (var c in entry) {
				if (!current.Children.ContainsKey(c)) current.Children[c] = new TrieNode();
				current = current.Children[c];

				if (!this._sorted) {
					if (!current.Entries.Contains(entry)) current.Entries.Add(entry);
					continue;
				}
				
				if (current.Entries.Contains(entry)) continue;
				var index = current.Entries.BinarySearch(entry);
				if (index < 0) index = ~index;
				current.Entries.Insert(index, entry);
			}
			current.IsEndOfWord = true;
		}
		
		public List<string> Search(string prefix) {
			var current = this._root;
			foreach (var c in prefix) {
				if (!current.Children.TryGetValue(c, out var child)) return new List<string>();
				current = child;
			}
			return new List<string>(current.Entries);
		}
		
		public string ExactMatch(string prefix) {
			var current = this._root;
			foreach (var c in prefix) {
				if (!current.Children.TryGetValue(c, out var child)) return null;
				current = child;
			}
			return current.IsEndOfWord ? current.Entries[0] : null;
		}
		
		/*public void PrintMemoryFootprint() {
			var bytes = GetMemoryFootprint();
			var sizeInKb = bytes / 1024d;
			var sizeInMb = sizeInKb / 1024d;
			IdleLog.Info($"Trie uses about {bytes} bytes, making it {sizeInKb:F2} KB or {sizeInMb:F2} MB.");
		}
		
		public int GetMemoryFootprint() {
			return this.GetMemoryFootprint(this._root);
		}

		private int GetMemoryFootprint(TrieNode node) {
			// Base size of the TrieNode
			int size = 32; // Object header + field references + alignment.

			// Add size of the Dictionary<char, TrieNode>
			size += 48; // Base overhead of Dictionary.
			size += node.Children.Count * 16; // Memory per key-value pair.

			// Add size of the List<string>
			size += 24; // Base overhead of List<string>.
			size += node.Entries.Count * 8; // References to strings.
			foreach (var entry in node.Entries) {
				size += 24; // Base size of each string object.
				size += 2 * entry.Length; // Size of string characters.
			}

			// Recursively calculate size for all children
			foreach (var child in node.Children.Values) {
				size += GetMemoryFootprint(child);
			}

			return size;
		}*/
	}
	
	internal class TrieNode {
		public readonly Dictionary<char, TrieNode> Children = new Dictionary<char, TrieNode>();
		public bool IsEndOfWord;
		public readonly List<string> Entries = new List<string>();
	}
}