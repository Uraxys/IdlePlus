using System.Linq;

namespace IdlePlus.API.Utility.Game {
	public static class PlayerUtils {

		/// <summary>
		/// Checks if the given string is a valid username of a player.
		/// </summary>
		/// <param name="name">The username to check.</param>
		/// <returns>True if it's valid, false if it isn't.</returns>
		public static bool IsValidUsername(string name) {
			if (name.Length < 3 || name.Length > 20) return false;
			return name.All(char.IsLetterOrDigit);
		}

		/// <summary>
		/// Determines whether the provided <paramref name="input"/> is a player message, 
		/// extracting the clan tag, username, and message if valid, or setting them to
		/// <c>null</c> if not.
		/// </summary>
		/// <param name="input">The input string to check.</param>
		/// <param name="tag">Outputs the extracted clan tag if valid, otherwise, <c>null</c>.</param>
		/// <param name="name">Outputs the extracted username if valid, otherwise, <c>null</c>.</param>
		/// <param name="message">Outputs the extracted message content if valid, otherwise, <c>null</c>.</param>
		/// <returns><c>true</c> if the input is a valid player message, otherwise, <c>false</c>.</returns>
		public static bool IsPlayerMessage(string input, out string tag, out string name, out string message) =>
			IsPlayerMessageIndex(input, out var _, out tag, out var _, out name, out var _, out message);

		/// <summary>
		/// Determines whether the provided <paramref name="input"/> is a player message, 
		/// extracting the clan tag, username, and message if valid, or setting them to
		/// <c>null</c> if not.
		/// </summary>
		/// <param name="input">The input string to check.</param>
		/// <param name="tagIndex">The index of the tag.</param>
		/// <param name="tag">Outputs the extracted clan tag if valid, otherwise, <c>null</c>.</param>
		/// <param name="nameIndex">The index of the name.</param>
		/// <param name="name">Outputs the extracted username if valid, otherwise, <c>null</c>.</param>
		/// <param name="messageIndex">The index of the message.</param>
		/// <param name="message">Outputs the extracted message content if valid, otherwise, <c>null</c>.</param>
		/// <returns><c>true</c> if the input is a valid player message, otherwise, <c>false</c>.</returns>
		public static bool IsPlayerMessageIndex(string input, out int tagIndex,out string tag, out int nameIndex,
			out string name, out int messageIndex, out string message) {
			tagIndex = -1;
			tag = null;
			nameIndex = -1;
			name = null;
			messageIndex = -1;
			message = null;
			
			var reader = new GeneralStringReader(input);

			// Check the time "[xx:xx:xx] "
			if (!reader.CanRead(11)) return false;
			if (reader.Next() != '[' || reader.Next(2) != ':' || reader.Next(2) != ':' || 
			    reader.Next(2) != ']') return false;
			if (reader.Next() != ' ') return false;
			
			// Check if we have a guild tag or not.
			if (reader.CanRead(6) && reader.Peek() == '[') {
				reader.Skip();
				
				// Mark the tag index.
				tagIndex = reader.Index;
				
				// If we don't have a closing bracket then it isn't a player
				// message.
				if (reader.Peek(3) != ']' || reader.Peek(4) != ' ') return false;
				tag = reader.ReadStr(3);
				reader.Skip(2);
			}
			
			// Get the player name.
			var nameLength = reader.IndexOf(':') - reader.Index;
			if (nameLength <= 0) return false; // Name can't be 0.
			nameIndex = reader.Index;
			name = reader.ReadStr(nameLength);

			if (!reader.CanRead(2)) return false;
			reader.Skip(2); // Skipping the ": ".
			// Get the message.
			messageIndex = reader.Index;
			message = reader.ReadStr();
			return true;
		}
	}
}