using System;
using System.Text.RegularExpressions;

namespace IdlePlus.API.Utility {
	
	/// <summary>
	/// Represents a unique identifier consisting of a namespace and a key.
	/// Ensures uniqueness across different namespaces, even if keys are duplicated.
	/// </summary>
	public sealed class NamespacedKey : System.IEquatable<NamespacedKey> {

		private static readonly Regex AllowedPattern = new Regex(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);
		private static readonly Regex FullPattern = new Regex(@"^([a-zA-Z0-9_]+):([a-zA-Z0-9_]+)$", RegexOptions.Compiled);
		
		/// <summary>
		/// The namespace of this <see cref="NamespacedKey"/>.
		/// Used to categorize and distinguish keys across different projects.
		/// </summary>
		public readonly string Namespace;
		/// <summary>
		/// The specific key within the given namespace.
		/// </summary>
		public readonly string Key;
		/// <summary>
		/// A unique identifier combining the <see cref="Namespace"/> and <see cref="Key"/>,
		/// ensuring uniqueness even if the same key exists in different namespaces.
		/// </summary>
		public readonly string Identifier;

		/// <summary>
		/// Creates a NamespacedKey from a given namespace and key, validating their format.
		/// </summary>
		/// <param name="ns">The namespace.</param>
		/// <param name="key">The key.</param>
		/// <exception cref="System.ArgumentException">Thrown if the namespace or key is invalid.</exception>
		public NamespacedKey(string ns, string key) {
			if (string.IsNullOrEmpty(ns) || !AllowedPattern.IsMatch(ns))
				throw new ArgumentException("Namespace can only contain letters, numbers, and underscores.", nameof(ns));
			if (string.IsNullOrEmpty(key) || !AllowedPattern.IsMatch(key))
				throw new ArgumentException("Key can only contain letters, numbers, and underscores.", nameof(key));
			
			this.Namespace = ns;
			this.Key = key;
			this.Identifier = $"{Namespace}:{Key}";
		}
		
		/// <summary>
		/// Creates a NamespacedKey from a single formatted string (namespace:key).
		/// </summary>
		/// <param name="identifier">A formatted string in the form "namespace:key".</param>
		/// <exception cref="ArgumentException">Thrown if the identifier is not in the correct format.</exception>
		public NamespacedKey(string identifier) {
			if (string.IsNullOrEmpty(identifier))
				throw new ArgumentException("Identifier cannot be null or empty.", nameof(identifier));
			var match = FullPattern.Match(identifier);
			if (!match.Success)
				throw new ArgumentException("Identifier must be in the format 'namespace:key', with both parts " +
				                            "containing only letters, numbers, and underscores.", nameof(identifier));

			this.Namespace = match.Groups[1].Value;
			this.Key = match.Groups[2].Value;
			this.Identifier = identifier;
		}

		// Factory Methods
		
		public static NamespacedKey Of(string ns, string key) => new NamespacedKey(ns, key);
		public static NamespacedKey Of(string identifier) => new NamespacedKey(identifier);
		
		// Equals & ToString Methods
		
		public override string ToString() => Identifier;
		
		public override bool Equals(object obj) => Equals(obj as NamespacedKey);

		public bool Equals(NamespacedKey other) => 
			other != null && Namespace == other.Namespace && Key == other.Key;
		
		public override int GetHashCode() {
			unchecked {
				var hash = 17;
				hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
				hash = hash * 31 + (Key?.GetHashCode() ?? 0);
				return hash;
			}
		}

		public static bool operator ==(NamespacedKey left, NamespacedKey right) => Equals(left, right);
		public static bool operator !=(NamespacedKey left, NamespacedKey right) => !Equals(left, right);
	}
}