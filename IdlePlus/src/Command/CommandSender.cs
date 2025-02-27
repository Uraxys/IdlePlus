using ChatboxLogic;
using IdlePlus.API.Utility.Data;
using IdlePlus.Utilities.Extensions;

namespace IdlePlus.Command {
	public class CommandSender {
		
		private static ChatboxManager ChatManager => ChatboxManager.Instance;
		private static ChannelType CurrentChannel => (ChannelType) ChatManager.ActiveChannel.Channel;

		/// <summary>
		/// Sends a message in chat for this client.
		/// </summary>
		/// <param name="message">The message to send in the chat.</param>
		/// <param name="mode">The mode icon to display, or <c>GameMode.NotSelected</c>
		/// if this is a system message.</param>
		/// <param name="premium">If the premium icon should be displayed.</param>
		/// <param name="gilded">If the gilded icon should be displayed.</param>
		/// <param name="moderator">If the moderator icon should be displayed.</param>
		public void SendMessage(string message, GameMode mode = GameMode.NotSelected, bool premium = false, 
			bool gilded = false, bool moderator = false) {
			SendMessage(CurrentChannel.ToChannelId(), message, mode, premium, gilded, moderator);
		}
		
		private static void SendMessage(ChannelId channelId, string message, GameMode gameMode, bool premium, bool gilded,
			bool moderator) {
			
			var channelInstance = ChatManager._chatboxContentInstances[channelId];
			var messageEntryObj = channelInstance.GetChild(0);
			var messageEntry = messageEntryObj.Use<ChatboxMessageEntry>();
			
			messageEntry.transform.SetAsLastSibling();
			messageEntry.Setup(ChatManager._serialization, message, channelId, gameMode, premium, gilded, 
				moderator);
		}
	}
}