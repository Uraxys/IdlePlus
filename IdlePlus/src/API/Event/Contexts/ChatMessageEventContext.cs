using IdlePlus.API.Utility;
using IdlePlus.API.Utility.Data;
using Networking.Chat;

namespace IdlePlus.API.Event.Contexts {
	public class ChatMessageEventContext : EventContext {

		/// <summary>
		/// The <see cref="ChannelType"/> the message was sent in.
		/// </summary>
		public ChannelType Channel { get; }
		/// <summary>
		/// The message.
		/// </summary>
		public string Message { get; }
		/// <summary>
		/// The name of the sender, or <c>null</c> if it's a system message.
		/// </summary>
		public string Sender { get; }
		/// <summary>
		/// The guild tag of the sender, or <c>null</c> if there isn't any.
		/// </summary>
		private string GuildTag { get; }
		
		// Icons
		
		/// <summary>
		/// The <see cref="GameMode"/> of the sender.
		/// </summary>
		public GameMode GameMode { get; }
		/// <summary>
		/// If the sender has premium.
		/// </summary>
		public bool Premium { get; }
		/// <summary>
		/// If the sender has gilded.
		/// </summary>
		public bool Gilded { get; }
		/// <summary>
		/// If the sender is a moderator.
		/// </summary>
		public bool Moderator { get; }

		public ChatMessageEventContext(ChatboxMessage message) {
			this.Channel = (ChannelType)message.ChannelId;
			this.GameMode = (GameMode)message.GameMode;
			this.Premium = message.IsPremium;
			this.Gilded = message.IsGilded;
			this.Moderator = message.IsModerator;

			if (this.GameMode == GameMode.NotSelected ||
			    !IsPlayerMessage(message.Message, out var tag, out var name, out var content)) {
				this.Message = message.Message;
				this.Sender = null;
				this.GuildTag = null;
				return;
			}

			this.Sender = name;
			this.Message = content;
			this.GuildTag = tag;
		}
		
		public override string ToString() {
			return "ChatMessageEventContext { " +
			       $"Channel = {Channel}, " +
			       $"Message = \"{Message}\", " +
			       $"Sender = \"{Sender}\", " +
			       $"GuildTag = \"{GuildTag}\", " +
			       $"GameMode = {GameMode}, " +
			       $"Premium = {Premium}, " +
			       $"Gilded = {Gilded}, " +
			       $"Moderator = {Moderator} }}";
		}

		// Internal
		
		internal static bool IsPlayerMessage(string input, out string tag, out string name, out string message) {
			tag = null;
			name = null;
			message = null;
			
			var reader = new GeneralStringReader(input);
			if (!reader.CanRead(11)) return false;
			// Check the time "[xx:xx:xx]".
			if (reader.Next() != '[' || reader.Next(2) != ':' || reader.Next(2) != ':' || 
			    reader.Next(2) != ']') return false;
			// Check the space.
			if (reader.Next() != ' ') return false;
			
			// Check if we have a guild tag or not.
			if (reader.Peek() == '[') {
				reader.Skip();
				// If we don't have a closing bracket then it isn't a player
				// message.
				if (reader.Peek(3) != ']' || reader.Peek(4) != ' ') return false;
				tag = reader.ReadStr(3);
				reader.Skip(2);
			}
			
			// Get the player name.
			var nameLength = reader.IndexOf(':') - reader.Index;
			if (nameLength <= 0) return false; // Name can't be 0.
			name = reader.ReadStr(nameLength);
			
			reader.Skip(2); // Skipping the ": ".
			message = reader.ReadStr();
			return true;
		}
	}
}