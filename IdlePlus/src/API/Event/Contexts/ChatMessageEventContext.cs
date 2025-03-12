using IdlePlus.API.Utility.Data;
using IdlePlus.API.Utility.Game;
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
		/// <para>The <see cref="GameMode"/> of the sender.</para>
		/// If this is a system message, then the game mode will be <see cref="GameMode.NotSelected"/>.
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
			    !PlayerUtils.IsPlayerMessage(message.Message, out var tag, out var name, out var content)) {
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
	}
}