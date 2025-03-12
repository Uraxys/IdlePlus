using System.Collections.Generic;
using System.Linq;

namespace IdlePlus.Utilities.Strings {
	public class AhoCorasick {

		private class TrieNode {
			public readonly Dictionary<char, TrieNode> Children = new Dictionary<char, TrieNode>();
			public TrieNode FailureLink;
			public readonly List<string> Outputs = new List<string>();
		}
		
		private TrieNode _root = new TrieNode();
		
		public void Insert(string word) {
			var node = this._root;
			foreach (char ch in word) {
				if (!node.Children.ContainsKey(ch)) node.Children[ch] = new TrieNode();
				node = node.Children[ch];
			}
			node.Outputs.Add(word);
		}

		public void Clear() {
			this._root = new TrieNode();
		}
		
		public void BuildFailureLinks() {
			var queue = new Queue<TrieNode>();
			foreach (var child in this._root.Children.Values) {
				child.FailureLink = this._root; 
				queue.Enqueue(child);
			}

			while (queue.Count > 0) {
				var current = queue.Dequeue();

				foreach (var entry in current.Children) {
					char ch = entry.Key;
					TrieNode child = entry.Value;

					// Find the failure link
					TrieNode failure = current.FailureLink;
					while (failure != null && !failure.Children.ContainsKey(ch)) 
						failure = failure.FailureLink;

					if (failure == null) child.FailureLink = this._root;
					else {
						child.FailureLink = failure.Children[ch];
						child.Outputs.AddRange(child.FailureLink.Outputs);
					}

					queue.Enqueue(child);
				}
			}
		}
		
		public List<AhoCorasickResult> Search(string text, bool unique = false) {
			var results = new List<AhoCorasickResult>();
			var node = this._root;

			for (int i = 0; i < text.Length; i++) {
				char ch = text[i];

				// Follow failure links if the character is not in the Trie
				while (node != null && !node.Children.ContainsKey(ch)) node = node.FailureLink;
				// If we found a valid transition, follow it
				node = node == null ? this._root : node.Children[ch];
				// If this node contains matches, store results
				foreach (var match in node.Outputs) {
					results.Add(new AhoCorasickResult(match, i - match.Length + 1, (i - match.Length + 1) + match.Length));
					//results.Add((match, i - match.Length + 1));
				}
			}

			// If we don't care about duplicates then we can return here.
			if (!unique) return results;
			
			// Welp, okay, we care about uniqueness, meaning we don't allow
			// words inside words.
			results.Sort((a, b) =>
				a.StartIndex != b.StartIndex
					? a.StartIndex.CompareTo(b.StartIndex)
					: b.Word.Length.CompareTo(a.Word.Length));
			
			// Only one word at the time.
			var filteredMatches = new List<AhoCorasickResult>();
			int lastEnd = -1;
			foreach (var entry in results.Where(entry => entry.StartIndex >= lastEnd)) {
				filteredMatches.Add(entry);
				lastEnd = entry.StartIndex + entry.Word.Length;
			}
			
			return filteredMatches;
		}
		
		public void PrintMemoryFootprint() {
			var bytes = GetMemoryFootprint();
			var sizeInKb = bytes / 1024d;
			var sizeInMb = sizeInKb / 1024d;
			IdleLog.Info($"AhoCorasick uses about {bytes} bytes, making it {sizeInKb:F2} KB or {sizeInMb:F2} MB.");
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
			size += node.Outputs.Count * 8; // References to strings.
			foreach (var entry in node.Outputs) {
				size += 24; // Base size of each string object.
				size += 2 * entry.Length; // Size of string characters.
			}

			// Recursively calculate size for all children
			foreach (var child in node.Children.Values) {
				size += GetMemoryFootprint(child);
			}

			return size;
		}
	}

	public class AhoCorasickResult {
		
		public readonly string Word;
		
		public readonly int StartIndex;
		public readonly int EndIndex;
		public readonly int Length;

		private int _mutableStartIndex;
		public int MutableStartIndex {
			get => _mutableStartIndex;
			set {
				_mutableStartIndex = value;
				MutableLength = MutableEndIndex - MutableStartIndex;
			}
		}

		private int _mutableEndIndex;
		public int MutableEndIndex {
			get => _mutableEndIndex;
			set {
				_mutableEndIndex = value;
				MutableLength = MutableEndIndex - MutableStartIndex;
			}
		}
		
		public int MutableLength { get; private set; }
		
		public AhoCorasickResult(string word, int startIndex, int endIndex) {
			this.Word = word;
			this.StartIndex = startIndex;
			this.EndIndex = endIndex;
			this.Length = endIndex - startIndex;

			this._mutableStartIndex = startIndex;
			this._mutableEndIndex = endIndex;
		}
	}
}